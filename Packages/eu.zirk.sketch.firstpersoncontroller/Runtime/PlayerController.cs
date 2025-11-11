using Sketch.Common;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Sketch.FPS
{
    public class PlayerController : MonoBehaviour
    {
        #region Serialized fields
        [Header("Configuration")]
        [SerializeField]
        private float _mouvementSpeed = 5f;

        [SerializeField]
        private float _horizontalSensitivity = .1f;

        [SerializeField]
        private float _verticalSensitivity = .1f;

        [SerializeField]
        private float _controllerSensitivity = 750f;

        [SerializeField]
        private float _runningMultiplier = 1.5f;

        [SerializeField]
        private float _jumpForce = 2f;

        [SerializeField]
        private float _gravityMultiplier = .75f;

        [Header("Physics")]
        [SerializeField]
        private bool _enablePhysics;

        [Header("Data")]
        [SerializeField]
        private Transform _head;
        private float _headRotation;

        [SerializeField]
        private PlayerInput _pInput;

        [SerializeField]
        private TriggerArea _triggerArea;

        [SerializeField]
        private TMP_Text _interactionText;

        /*[SerializeField]
        private RectTransform _stamina;
        private float _staminaLeft = 1f;
        private float _timerStaminaReload = 0f;*/
        #endregion Serialized fields

        #region Member variables
        // Base controls
        private CharacterController _controller;
        private bool _isSprinting;
        private float _verticalSpeed;

        private Vector3 _baseSpawnPos;

        // Last controller input
        private Vector2? _lastControllerRot;

        // Mobile interactions
        private bool? _mobileIsMoving;
        private Vector2 _touchRef;
        private Vector2 _touchPos;

        // Mouvements
        protected Vector2 _mov;

        // Interactions
        private readonly List<IInteractable> _interactions = new();
        public IEnumerable<IInteractable> InteractionByDistance => _interactions.OrderBy(x => Vector2.Distance(x.GameObject.transform.position, _triggerArea.transform.position));
        #endregion Member variables

        // Overrides behaviors
        public virtual bool IsActive => true;
        public virtual bool CanSprint => true;

        // Movement callbacks
        protected UnityEvent<bool> OnSprintStateChanges { get; } = new();
        protected UnityEvent OnJumpDone { get; } = new();

        protected virtual void Awake()
        {
            if (_pInput == null) Debug.LogWarning("PInput not assigned, mobile controls won't be available");
            if (_triggerArea == null) Debug.LogWarning("Trigger Area not assigned, interactions won't be available");
            if (_interactionText == null) Debug.LogWarning("Interaction Text not assigned, interaction hints won't be available");

            _controller = GetComponent<CharacterController>();
            _baseSpawnPos = transform.position;
            if (_interactionText != null) _interactionText.gameObject.SetActive(false);

            if (_triggerArea != null)
            {
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
                        RemoveInteraction(i);
                    }
                });
            }
        }

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected virtual void Update()
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

            // Push objects on the way
            if (_enablePhysics)
            {
                var hits = Physics.SphereCastAll(transform.position, _controller.radius, desiredMove,
                                   .1f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                foreach (var hit in hits)
                {
                    var hitRb = hit.collider.GetComponent<Rigidbody>();
                    if (hitRb != null)
                    {
                        hitRb.AddForce(desiredMove, ForceMode.Force);
                    }
                }
            }

            Vector3 moveDir = Vector3.zero;
            if (IsActive)
            {
                moveDir.x = desiredMove.x * _mouvementSpeed * (CanSprint && _isSprinting/* && _staminaLeft > 0f*/ ? _runningMultiplier : 1f);
                moveDir.z = desiredMove.z * _mouvementSpeed * (CanSprint && _isSprinting/* && _staminaLeft > 0f*/ ? _runningMultiplier : 1f);
            }

            if (_controller.isGrounded && _verticalSpeed < 0f) // We are on the ground and not jumping
            {
                moveDir.y = -.1f; // Stick to the ground
                _verticalSpeed = -_gravityMultiplier;
            }
            else
            {
                // We are currently jumping, reduce our jump velocity by gravity and apply it
                _verticalSpeed += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
                moveDir.y += _verticalSpeed;
            }

            _controller.Move(moveDir * Time.deltaTime);

            if (transform.position.y < -10f)
            {
                transform.position = _baseSpawnPos;
                _verticalSpeed = 0f;
            }

            // If we can interact with anything, we check if target changed
            if (_interactions.Count > 1) UpdateInteractionText();

            // If we are playing on a controller, we update the rotation
            if (_lastControllerRot != null)
            {
                OnLookInternal(_lastControllerRot.Value * Time.deltaTime * _controllerSensitivity);
            }

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

        public void RemoveInteraction(IInteractable i)
        {
            _interactions.RemoveAll(x => x.GameObject.GetInstanceID() == i.GameObject.GetInstanceID());
            UpdateInteractionText();
        }

        public virtual string GetInteractionText(string interactionVerb) => string.IsNullOrEmpty(interactionVerb) ? string.Empty : $"Press 'E' to {interactionVerb}";
        public virtual string GetDenyText(string denySentence) => denySentence;

        private void UpdateInteractionText()
        {
            if (_interactionText == null) return;

            var interactions = InteractionByDistance;
            var validInteraction = interactions.FirstOrDefault(x => x.CanInteract(this));
            if (validInteraction != null)
            {
                _interactionText.gameObject.SetActive(true);
                _interactionText.text = GetInteractionText(validInteraction.InteractionVerb(this));
            }
            else
            {
                var closestInvalid = interactions.FirstOrDefault();
                if (closestInvalid == null || closestInvalid.DenySentence(this) == null)
                {
                    _interactionText.gameObject.SetActive(false);
                }
                else
                {
                    _interactionText.gameObject.SetActive(true);
                    _interactionText.text = GetDenyText(closestInvalid.DenySentence(this));
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
            if (!IsActive) return;

            transform.rotation *= Quaternion.AngleAxis(rot.x * _horizontalSensitivity, Vector3.up);

            _headRotation -= rot.y * _verticalSensitivity; // Vertical look is inverted by default, hence the -=

            _headRotation = Mathf.Clamp(_headRotation, -89, 89);
            _head.transform.localRotation = Quaternion.AngleAxis(_headRotation, Vector3.right);
        }
        public void OnLook(InputAction.CallbackContext value)
        {
            var rot = value.ReadValue<Vector2>();
            _lastControllerRot = null;
            OnLookInternal(rot);
        }

        public void OnLookController(InputAction.CallbackContext value)
        {
            _lastControllerRot = value.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext value)
        {
            if (_controller.isGrounded && IsActive)
            {
                _verticalSpeed = _jumpForce;
                OnJumpDone.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext value)
        {
            _isSprinting = value.ReadValueAsButton();
            OnSprintStateChanges.Invoke(_isSprinting);
        }

        private void OnInteractInternal()
        {
            if (!IsActive) return;

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