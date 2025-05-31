using Ink.Runtime;
using Sketch.Achievement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private bool _isAutoEnabled;

        private bool _didUseSkip;

        private void Awake()
        {
            Instance = this;

            _display.OnDisplayDone += (_sender, _e) =>
            {
                if (_story.currentChoices.Any())
                {
                    ResetVN();
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

                if (_isAutoEnabled)
                {
                    StartCoroutine(AutoNextDialogue());
                }
            };
        }

        private void Start()
        {
            ShowStory(_intro);
        }

        private IEnumerator AutoNextDialogue()
        {
            yield return new WaitForSeconds(1f);
            if (_isAutoEnabled)
            {
                DisplayNextDialogue();
            }
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

        private void ResetVN(bool resetUI = true)
        {
            _isSkipEnabled = false;
            _isAutoEnabled = false;

            if (resetUI)
            {
                _container.SetActive(true);
                if (_currentCharacter != null)
                {
                    _characterImage.gameObject.SetActive(true);
                }
                _choiceContainer.gameObject.SetActive(true);
            }
        }

        public void ShowStory(TextAsset asset)
        {
            Debug.Log($"[STORY] Playing {asset.name}");
            _currentCharacter = null;
            _story = new(asset.text);
            ResetVN();
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

                    case "ach-noskip":
                        if (content == "START") _didUseSkip = false;
                        else if (content == "STOP")
                        {
                            if (!_didUseSkip)
                            {
                                AchievementManager.Instance.Unlock(AchievementID.VIS_NoSkip);
                            }
                        }
                        else Debug.LogError($"[STORY] Invalid achievement value {content}");
                        break;

                    default:
                        Debug.LogError($"Unknown story key: {s[0]}");
                        break;
                }
            }
            _display.ToDisplay = text;
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

            if (_isSkipEnabled)
            {
                _didUseSkip = true;
            }
        }

        public void ToggleAuto()
        {
            _isAutoEnabled = !_isAutoEnabled;

            if (_isAutoEnabled && _display.IsDisplayDone && _story.canContinue && !_story.currentChoices.Any())
            {
                DisplayNextDialogue();
            }
        }

        public void ToggleHide()
        {
            _container.SetActive(!_container.activeInHierarchy);

            _characterImage.gameObject.SetActive(_container.activeInHierarchy);
            _choiceContainer.gameObject.SetActive(_container.activeInHierarchy);

            ResetVN(resetUI: false);
        }

        public void OnNextDialogue(InputAction.CallbackContext value)
        {
            if (value.performed && !_isSkipEnabled)
            {
                if (_container.activeInHierarchy)
                {
                    // If we click on a button, we don't advance the 
                    PointerEventData pointerEventData = new(EventSystem.current)
                    {
                        position = Mouse.current.position.ReadValue()
                    };
                    List<RaycastResult> raycastResultsList = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerEventData, raycastResultsList);
                    for (int i = 0; i < raycastResultsList.Count; i++)
                    {
                        if (raycastResultsList[i].gameObject.TryGetComponent<Button>(out var _))
                        {
                            return;
                        }
                    }

                    ResetVN();
                    DisplayNextDialogue();
                }
                else
                {
                    // Hide mode is active
                    _container.SetActive(true);
                }
            }
        }

        public void OnHide(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                ToggleHide();
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