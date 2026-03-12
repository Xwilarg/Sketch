using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Sketch.VN
{
    [Serializable]
    public class CharacterImageOverlay
    {
        public string Tag;
        public Image Image;
        public bool IsSet { set; get; }
    }

    public class VNManager : MonoBehaviour
    {
        public static VNManager Instance { private set; get; }

        [Header("Mandatory fields")]
        [SerializeField, Tooltip("Text that will show your visual novel story")]
        private TextDisplay _display;

        [SerializeField, Tooltip("List of characters that are shown by your visual novel")]
        private VNCharacterInfo[] _characters;
        private VNCharacterInfo _currentCharacter;

        private Story _story;

        [Header("Displayed sprite")]
        [SerializeField, Tooltip("Where the image of the character will be shown")]
        private Image _characterImage;

        [SerializeField, Tooltip("Others elements that overlaps the character sprite (emotions, clothes, etc...)")]
        private CharacterImageOverlay[] _overlays;

        [Header("Background")]
        [SerializeField, Tooltip("Image containing the background")]
        private Image _backgroundImage;

        [SerializeField, Tooltip("Tags matching the different backgrounds")]
        private CharacterOverlayContentInfo[] _backgrounds;

        [Header("Interface")]
        [SerializeField, Tooltip("Object that contains all the others visual novel components")]
        private GameObject _container;

        [SerializeField, Tooltip("Pannel around the name text")]
        private GameObject _namePanel;

        [SerializeField, Tooltip("Text that show the name of the character")]
        private TMP_Text _nameText;

        [Header("Choices")]
        [SerializeField, Tooltip("Object that contains the choices")]
        private Transform _choiceContainer;

        [SerializeField, Tooltip("Prefab of the choices to spawn them in runtime")]
        private GameObject _choicePrefab;

        private bool _isSkipEnabled;
        private float _skipTimer;
        private float _skipTimerRef = .1f;

        private bool _isAutoEnabled;

        private Action _onDone;
        private Func<string, string, bool> _onTags;

        private CursorLockMode _lastCursorMode;

        private void Awake()
        {
            Instance = this;

            if (_container == null) // If user didn't give a container, we just use the one of the text instead
            {
                _container = _display.gameObject;
            }
            _container.SetActive(false);

            // Setup choice button spawn
            if (_choicePrefab != null && _choiceContainer != null)
            {
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
        }

        private IEnumerator AutoNextDialogue()
        {
            yield return new WaitForSeconds(1f);
            if (_isAutoEnabled)
            {
                DisplayNextDialogue();
            }
        }

        public bool IsActive => _container.activeInHierarchy;
        public bool IsStoryOngoing => _story != null && (_story.canContinue || (_story.currentChoices != null && _story.currentChoices.Any()));

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
                if (_characterImage != null && _currentCharacter != null)
                {
                    _characterImage.gameObject.SetActive(true);
                    foreach (var cio in _overlays.Where(x => x.IsSet)) cio.Image.gameObject.SetActive(true);
                }
                if (_choiceContainer != null)
                {
                    _choiceContainer.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Start showing a story to the player
        /// </summary>
        /// <param name="asset">Compiled Ink file containing the story</param>
        /// <param name="updateVariables">Method taking a VariablesState as parameter, allow to update the variables within the Ink file</param>
        /// <param name="onDone">Called once the story is done being read</param>
        /// <param name="onTags">Called upon an unknown tag is found, first parameter is the tag name and second is its value, function expect to return if the tag was treated or not</param>
        public void ShowStory(TextAsset asset, Action<VariablesState> updateVariables = null, Action onDone = null, Func<string, string, bool> onTags = null)
        {
            Debug.Log($"[STORY] Playing {asset.name}");
            _currentCharacter = null;
            _story = new(asset.text);
            updateVariables?.Invoke(_story.variablesState);
            _onDone = onDone;
            _onTags = onTags;
            ResetVN();

            _lastCursorMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;

            DisplayStory(_story.Continue());
        }

        private void DisplayStory(string text)
        {
            _container.SetActive(true);

            if (_nameText != null)
            {
                _namePanel?.SetActive(false);
                _nameText.text = string.Empty;
            }

            foreach (var tag in _story.currentTags)
            {
                var s = tag.Split(' ');
                var content = string.Join(' ', s.Skip(1)).ToUpperInvariant();
                switch (s[0])
                {
                    case "speaker":
                        if (_characters.Length > 0)
                        {
                            if (content == "NONE") _currentCharacter = null;
                            else
                            {
                                _currentCharacter = _characters.FirstOrDefault(x => x.Name.ToUpperInvariant() == content);
                                if (_currentCharacter == null)
                                {
                                    Debug.LogError($"[STORY] Unable to find character {content}");
                                }
                            }
                        }
                        else
                        {
                            if (content == "NONE") _currentCharacter = null;
                            else
                            {
                                _currentCharacter = new()
                                {
                                    DisplayName = content,
                                    Name = content
                                };
                            }
                        }
                        break;

                    case "background":
                        if (_backgroundImage == null)
                        {
                            Debug.LogError($"[STORY] Trying to set background when {nameof(_backgroundImage)} is not set");
                            break;
                        }

                        if (content == "NONE") _backgroundImage.gameObject.SetActive(false);
                        else
                        {
                            var bgSprite = _backgrounds.FirstOrDefault(x => x.Tag.ToUpperInvariant() == content);
                            if (bgSprite == null)
                            {
                                Debug.LogError($"[STORY] Unable to find background {content}");
                            }

                            _backgroundImage.gameObject.SetActive(true);
                            _backgroundImage.sprite = bgSprite.Image;
                        }
                        break;

                    case "skip":
                        if (content == "TRUE") _isSkipEnabled = true;
                        else if (content == "FALSE") _isSkipEnabled = false;
                        else Debug.LogError($"[STORY] Unable to find format {content}");
                        break;

                    default:
                        var overlayTag = _currentCharacter?.Overlays?.FirstOrDefault(x => x.ParentTag.ToLowerInvariant() == s[0]);
                        if (overlayTag != null)
                        {
                            var img = _overlays.FirstOrDefault(x => x.Tag.ToLowerInvariant() == overlayTag.ParentTag.ToLowerInvariant());
                            if (img == null)
                            {
                                Debug.LogError($"[STORY] {nameof(_overlays)} is missing an element for the tag {overlayTag.ParentTag}");
                                break;
                            }

                            var elem = overlayTag.OverlayContent.FirstOrDefault(x => x.Tag.ToUpperInvariant() == content);
                            if (elem == null)
                            {
                                Debug.LogError($"[STORY] character overlay info is missing content {content} for tag {overlayTag.ParentTag}");
                                break;
                            }

                            img.Image.sprite = elem.Image;
                            img.Image.gameObject.SetActive(true);
                            img.IsSet = true;
                        }
                        else
                        {
                            if (_onTags == null || !_onTags.Invoke(s[0], content))
                            {
                                Debug.LogError($"[STORY] Unknown tag {s[0]}");
                            }
                        }
                        break;
                }
            }
            _display.ToDisplay = text;
            if (_currentCharacter == null)
            {
                if (_nameText != null)
                {
                    _namePanel?.SetActive(false);
                    _nameText.text = string.Empty;
                }
                if (_characterImage != null)
                {
                    _characterImage.gameObject.SetActive(false);
                    foreach (var cio in _overlays) cio.Image.gameObject.SetActive(false);
                }
            }
            else
            {
                if (_nameText != null)
                {
                    _namePanel?.SetActive(true);
                    _nameText.text = _currentCharacter.DisplayName;
                }
                if (_characterImage != null)
                {
                    _characterImage.gameObject.SetActive(true);
                    _characterImage.sprite = _currentCharacter.Image;
                    foreach (var cio in _overlays.Where(x => x.IsSet)) cio.Image.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Display the next dialogue if available
        /// </summary>
        public void DisplayNextDialogue()
        {
            if (!IsActive)
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
            else if (!IsStoryOngoing)
            {
                _container.SetActive(false);
                _onDone?.Invoke();
                Cursor.lockState = _lastCursorMode;
            }
        }

        /// <summary>
        /// Toggle visual novel skip
        /// Skip allow to quickly go throught dialogues without needing user click to pass them
        /// </summary>
        public void ToggleSkip()
        {
            _isSkipEnabled = !_isSkipEnabled;
        }

        /// <summary>
        /// Toggle visual novel auto
        /// Auto slowly go throught dialogues without needing user click to pass them
        /// </summary>
        public void ToggleAuto()
        {
            _isAutoEnabled = !_isAutoEnabled;

            if (_isAutoEnabled && _display.IsDisplayDone && _story.canContinue && !_story.currentChoices.Any())
            {
                DisplayNextDialogue();
            }
        }

        /// <summary>
        /// Hide all the visual novel interface until the user click anywhere
        /// </summary>
        public void ToggleHide()
        {
            _container.SetActive(!_container.activeInHierarchy);

            if (_characterImage != null && _currentCharacter != null)
            {
                _characterImage.gameObject.SetActive(_container.activeInHierarchy);
                if (_container.activeInHierarchy)
                {
                    foreach (var cio in _overlays.Where(x => x.IsSet)) cio.Image.gameObject.SetActive(true);
                }
                else
                {
                    foreach (var cio in _overlays) cio.Image.gameObject.SetActive(false);
                }
            }
            if (_choiceContainer != null)
                _choiceContainer.gameObject.SetActive(_container.activeInHierarchy);

            ResetVN(resetUI: false);
        }

        public void OnNextDialogue(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                if (IsActive)
                {
                    if (_isSkipEnabled)
                    {
                        _isSkipEnabled = false;
                    }
                    else
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
                }
                else if (IsStoryOngoing)
                {
                    // Hide mode is active
                    ToggleHide();
                }
            }
        }

        public void OnHide(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started && IsStoryOngoing)
            {
                ToggleHide();
            }
        }

        public void OnSkip(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started && _container.activeInHierarchy)
            {
                _isSkipEnabled = !_isSkipEnabled;
            }
        }
    }
}