using Sketch.Common;
using Sketch.FPS.Player;
using Sketch.Player;
using Sketch.Translation;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.FPS
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private PlayerInfo _info;

        [SerializeField]
        private Transform _head;
        private float _headRotation;

        [SerializeField]
        private PlayerInput _pInput;

        [SerializeField]
        private TMP_Text _interactionText;

        private TriggerArea _triggerArea;

        /*[SerializeField]
        private RectTransform _stamina;
        private float _staminaLeft = 1f;
        private float _timerStaminaReload = 0f;*/

        private CharacterController _controller;
        private bool _isSprinting;
        private float _verticalSpeed;

        private Vector2 _mov;

        private Vector3 _baseSpawnPos;

        // Mobile interactions
        private bool? _mobileIsMoving;
        private Vector2 _touchRef;
        private Vector2 _touchPos;

        private List<IInteractable> _interactions = new();
        public IEnumerable<IInteractable> InteractionByDistance => _interactions.OrderBy(x => Vector2.Distance(x.GameObject.transform.position, _triggerArea.transform.position));

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _baseSpawnPos = transform.position;
            _interactionText.gameObject.SetActive(false);

            _triggerArea = GetComponentInChildren<TriggerArea>();
            _triggerArea.OnTriggerEnterEvent.AddListener((Collider c) =>
            {
                if (c.TryGetComponent<IInteractable>(out var i))
                {
                    _interactions.Add(i);
                    UpdateInteractionText();
                }
            });
            _triggerArea.OnTriggerExitEvent.AddListener((Collider c) =>
            {
                if (c.gameObject.TryGetComponent<IInteractable>(out var i))
                {
                    _interactions.RemoveAll(x => x.GameObject.GetInstanceID() == i.GameObject.GetInstanceID());
                    UpdateInteractionText();
                }
            });
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (!_controller.enabled)
                return;

            // Mobile controls
            if (_mobileIsMoving != null)
            {
                var mousePos = CursorUtils.GetPosition(_pInput);
                if (mousePos != null)
                {
                    _touchPos = mousePos.Value;
                    var dir = _touchPos - _touchRef;
                    if (_mobileIsMoving.Value)
                    {
                        _mov = dir.normalized;
                    }
                    else
                    {
                        OnLookInternal(dir.normalized * 10f);
                    }
                }
            }

            var pos = _mov;
            Vector3 desiredMove = transform.forward * pos.y + transform.right * pos.x;

            // Get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, _controller.radius, Vector3.down, out RaycastHit hitInfo,
                               _controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            Vector3 moveDir = Vector3.zero;
            moveDir.x = desiredMove.x * _info.ForceMultiplier * (_isSprinting/* && _staminaLeft > 0f*/ ? _info.SpeedRunningMultiplicator : 1f);
            moveDir.z = desiredMove.z * _info.ForceMultiplier * (_isSprinting/* && _staminaLeft > 0f*/ ? _info.SpeedRunningMultiplicator : 1f);

            if (_controller.isGrounded && _verticalSpeed < 0f) // We are on the ground and not jumping
            {
                moveDir.y = -.1f; // Stick to the ground
                _verticalSpeed = -_info.GravityMultiplicator;
            }
            else
            {
                // We are currently jumping, reduce our jump velocity by gravity and apply it
                _verticalSpeed += Physics.gravity.y * _info.GravityMultiplicator * Time.deltaTime;
                moveDir.y += _verticalSpeed;
            }

            _controller.Move(moveDir * _info.MovementSpeed * Time.deltaTime);

            if (transform.position.y < -10f)
            {
                transform.position = _baseSpawnPos;
                _verticalSpeed = 0f;
            }

            // If we can interact with anything, we check if target changed
            if (_interactions.Count > 1) UpdateInteractionText();

            /*
            if (_isSprinting && _staminaLeft > 0f && desiredMove.magnitude > 0f)
            {
                _timerStaminaReload = 1f;
                _staminaLeft = Mathf.Clamp01(_staminaLeft - Time.deltaTime * .5f);
            }
            else if (_timerStaminaReload > 0f)
            {
                _timerStaminaReload -= Time.deltaTime;
            }
            else if (_staminaLeft < 1f)
            {
                _staminaLeft = Mathf.Clamp01(_staminaLeft + Time.deltaTime * .1f);
            }
            _stamina.gameObject.SetActive(_staminaLeft < 1f);
            _stamina.localScale = new Vector3(_staminaLeft, 1f, 1f);
            */
        }

        private void UpdateInteractionText()
        {
            var interactions = InteractionByDistance;
            var validInteraction = interactions.FirstOrDefault(x => x.CanInteract(this));
            if (validInteraction != null)
            {
                _interactionText.gameObject.SetActive(true);
                _interactionText.text = Translate.Instance.Tr("FPS_interactionText", Translate.Instance.Tr(validInteraction.InteractionVerb));
            }
            else
            {
                var closestInvalid = interactions.FirstOrDefault();
                if (closestInvalid == null || closestInvalid.DenySentence == null)
                {
                    _interactionText.gameObject.SetActive(false);
                }
                else
                {
                    _interactionText.gameObject.SetActive(true);
                    _interactionText.text = Translate.Instance.Tr(closestInvalid.DenySentence);
                }
            }
        }

        public void OnMobileDrag(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                var mousePos = CursorUtils.GetPosition(_pInput);
                _mobileIsMoving = mousePos.Value.x < Screen.width / 2f;
                _touchRef = mousePos.Value;
                _touchPos = mousePos.Value;
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                if (_mobileIsMoving == true) _mov = Vector2.zero;
                _mobileIsMoving = null;
                if (_touchRef == _touchPos)
                {
                    OnInteractInternal();
                }
            }
        }

        public void OnMovement(InputAction.CallbackContext value)
        {
            _mov = value.ReadValue<Vector2>();
        }

        private void OnLookInternal(Vector2 rot)
        {
            transform.rotation *= Quaternion.AngleAxis(rot.x * _info.HorizontalLookMultiplier, Vector3.up);

            _headRotation -= rot.y * _info.VerticalLookMultiplier; // Vertical look is inverted by default, hence the -=

            _headRotation = Mathf.Clamp(_headRotation, -89, 89);
            _head.transform.localRotation = Quaternion.AngleAxis(_headRotation, Vector3.right);
        }
        public void OnLook(InputAction.CallbackContext value)
        {
            var rot = value.ReadValue<Vector2>();
            OnLookInternal(rot);
        }

        public void OnJump(InputAction.CallbackContext value)
        {
            if (_controller.isGrounded)
            {
                _verticalSpeed = _info.JumpForce;
            }
        }

        public void OnSprint(InputAction.CallbackContext value)
        {
            _isSprinting = value.ReadValueAsButton();
        }

        private void OnInteractInternal()
        {
            var closestInteraction = InteractionByDistance.Where(x => x.CanInteract(this)).FirstOrDefault();
            if (closestInteraction != null)
            {
                closestInteraction.Interact(this);
                _interactions.RemoveAll(x => x.GameObject == null);
                UpdateInteractionText();
            }
        }
        public void OnInteract(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                OnInteractInternal();
            }
        }
    }
}