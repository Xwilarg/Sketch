using Ink.Runtime;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sketch.VN
{
    public class VNManager : MonoBehaviour
    {
        public static bool QuickRetry = false;
        public static VNManager Instance { private set; get; }

        [SerializeField]
        private TextDisplay _display;

        [SerializeField]
        private VNCharacterInfo[] _characters;
        private VNCharacterInfo _currentCharacter;

        private Story _story;

        [SerializeField]
        private GameObject _container;

        [SerializeField]
        private TextAsset _intro;

        [SerializeField]
        private GameObject _namePanel;

        [SerializeField]
        private TMP_Text _nameText;

        [SerializeField]
        private Image _characterImage;

        [SerializeField]
        private Transform _choiceContainer;

        [SerializeField]
        private GameObject _choicePrefab;

        private bool _isSkipEnabled;
        private float _skipTimer;
        private float _skipTimerRef = .1f;

        private void Awake()
        {
            Instance = this;
            ShowStory(_intro, null);

            _display.OnDisplayDone += (_sender, _e) =>
            {
                if (_story.currentChoices.Any())
                {
                    _isSkipEnabled = false;
                    foreach (var choice in _story.currentChoices)
                    {
                        var button = Instantiate(_choicePrefab, _choiceContainer);
                        button.GetComponentInChildren<TMP_Text>().text = choice.text;

                        var elem = choice;
                        button.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            _story.ChoosePath(elem.targetPath);
                            for (int i = 0; i < _choiceContainer.childCount; i++)
                                Destroy(_choiceContainer.GetChild(i).gameObject);
                            DisplayStory(_story.Continue());
                        });
                    }
                }
            };
        }

        public bool IsPlayingStory => _container.activeInHierarchy;

        private void Update()
        {
            if (_isSkipEnabled)
            {
                _skipTimer -= Time.deltaTime;
                if (_skipTimer < 0)
                {
                    _skipTimer = _skipTimerRef;
                    DisplayNextDialogue();
                }
            }
        }

        public void ShowStory(TextAsset asset, Action onDone)
        {
            Debug.Log($"[STORY] Playing {asset.name}");
            _currentCharacter = null;
            _story = new(asset.text);
            _isSkipEnabled = false;
            DisplayStory(_story.Continue());
        }

        private void DisplayStory(string text)
        {
            _container.SetActive(true);
            _namePanel.SetActive(false);

            foreach (var tag in _story.currentTags)
            {
                var s = tag.Split(' ');
                var content = string.Join(' ', s.Skip(1)).ToUpperInvariant();
                switch (s[0])
                {
                    case "speaker":
                        if (content == "NONE") _currentCharacter = null;
                        else
                        {
                            _currentCharacter = _characters.FirstOrDefault(x => x.Name.ToUpperInvariant() == content);
                            if (_currentCharacter == null)
                            {
                                Debug.LogError($"[STORY] Unable to find character {content}");
                            }
                        }

                        Debug.Log($"[STORY] Speaker set to {_currentCharacter?.Name}");
                        break;

                    case "skip":
                        if (content == "TRUE") _isSkipEnabled = true;
                        else if (content == "FALSE") _isSkipEnabled = false;
                        else Debug.LogError($"[STORY] Unable to find format {content}");
                        break;

                    default:
                        Debug.LogError($"Unknown story key: {s[0]}");
                        break;
                }
            }
            _display.ToDisplay = Regex.Replace(text, "\\*([^\\*]+)\\*", "<i>$1</i>"); ;
            if (_currentCharacter == null)
            {
                _namePanel.SetActive(false);
                _characterImage.gameObject.SetActive(false);
            }
            else
            {
                _namePanel.SetActive(true);
                _nameText.text = _currentCharacter.DisplayName;
                _characterImage.gameObject.SetActive(true);
                _characterImage.sprite = _currentCharacter.Image;
            }
        }

        public void DisplayNextDialogue()
        {
            if (!_container.activeInHierarchy)
            {
                return;
            }
            if (!_display.IsDisplayDone)
            {
                // We are slowly displaying a text, force the whole display
                _display.ForceDisplay();
            }
            else if (_story.canContinue && // There is text left to write
                !_story.currentChoices.Any()) // We are not currently in a choice
            {
                DisplayStory(_story.Continue());
            }
            else if (!_story.canContinue && !_story.currentChoices.Any())
            {
                _container.SetActive(false);
                SceneManager.LoadScene("Main");
            }
        }

        public void ToggleSkip()
        {
            _isSkipEnabled = !_isSkipEnabled;
        }

        public void OnNextDialogue(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                DisplayNextDialogue();
            }
        }

        public void OnSkip(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _isSkipEnabled = true;
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                _isSkipEnabled = false;
            }
        }
    }
}