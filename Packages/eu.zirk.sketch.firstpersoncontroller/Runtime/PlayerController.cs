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
        private PlayerControlInfo[] _controls;

        [SerializeField]
        private float _horizontalSensitivity = .1f;

        [SerializeField]
        private float _verticalSensitivity = .1f;

        [SerializeField]
        private float _controllerSensitivity = 750f;

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

        private int _controlIndex;
        private PlayerControlInfo CurrentControl => _controls[_controlIndex];

        // Last controller input
        private Vector2? _lastControllerRot;

        // Mobile interactions
        private bool? _mobileIsMoving;
        private Vector2 _touchRef;
        private Vector2 _touchPos;

        // Mouvements
        private Vector2 _mov;

        // Interactions
        private readonly List<IInteractable> _interactions = new();
        #endregion Member variables

        protected Vector3 Velocity { private set; get; }

        // Overrides behaviors
        /// <summary>
        /// Is the player active (if false, all controls are disabled)
        /// </summary>
        public virtual bool IsActive => true;
        /// <summary>
        /// Are we able to spring (only work if OnSprint is called)
        /// </summary>
        public virtual bool CanSprint => true;
        /// <summary>
        /// Apply a transformation on raw movement input
        /// </summary>
        public virtual Vector2 GetCurrentInputMovement(Vector2 movInput) => movInput;
        /// <summary>
        /// Apply a transformation on raw rotation input
        /// </summary>
        public virtual Vector2 GetCurrentInputRotation(Vector2 movInput) => movInput;
        /// <summary>
        /// Apply a transformation on character movement
        /// </summary>
        public virtual Vector3 GetCurrentPlayerMovement(Vector3 mov) => mov;

        public void SetControlIndex(int index)
        {
            _controlIndex = index;
        }

        public IEnumerable<IInteractable> InteractionByDistance => _interactions.OrderBy(x => Vector2.Distance(x.GameObject.transform.position, _triggerArea.transform.position));

        // Movement callbacks
        protected UnityEvent<bool> OnSprintStateChanges { get; } = new();
        protected UnityEvent OnJumpDone { get; } = new();

        protected Vector3 GetForwardMovement()
        {
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
                        SetMouvement(dir.normalized);
                    }
                    else
                    {
                        RotateHead(dir.normalized * 10f);
                    }
                }
            }

            var pos = GetCurrentInputMovement(_mov);
            return transform.forward * pos.y + transform.right * pos.x;
        }

        #region Unity methods

        protected virtual void Awake()
        {
            if (_pInput == null) Debug.LogWarning("PInput not assigned, mobile controls won't be available");
            if (_triggerArea == null) Debug.LogWarning("Trigger Area not assigned, interactions won't be available");
            if (_interactionText == null) Debug.LogWarning("Interaction Text not assigned, interaction hints won't be available");

            if (_controls.Length == 0)
            {
                Debug.LogWarning("Log control scheme found, creating a default one...");
                _controls = new PlayerControlInfo[] {
                    ScriptableObject.CreateInstance<PlayerControlInfo>()
                };
            }

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
            {
                Velocity = Vector3.zero;
                return;
            }

            var desiredMove = GetForwardMovement();

            // Get a normal for the surface that is being touched to move along it
            Physics.SphereCast(transform.position, _controller.radius, Vector3.down, out RaycastHit hitInfo,
                               _controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = GetCurrentPlayerMovement(Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized);

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
                moveDir.x = desiredMove.x * CurrentControl.MouvementSpeed * (CanSprint && _isSprinting/* && _staminaLeft > 0f*/ ? CurrentControl.RunningMultiplier : 1f);
                moveDir.z = desiredMove.z * CurrentControl.MouvementSpeed * (CanSprint && _isSprinting/* && _staminaLeft > 0f*/ ? CurrentControl.RunningMultiplier : 1f);
            }

            if (_controller.isGrounded && _verticalSpeed < 0f) // We are on the ground and not jumping
            {
                moveDir.y = -.1f; // Stick to the ground
                _verticalSpeed = -CurrentControl.GravityMultiplier;
            }
            else
            {
                // We are currently jumping, reduce our jump velocity by gravity and apply it
                _verticalSpeed += Physics.gravity.y * CurrentControl.GravityMultiplier * Time.deltaTime;
                moveDir.y += _verticalSpeed;
            }

            var p = transform.position;
            _controller.Move(moveDir * Time.deltaTime);
            Velocity = transform.position - p;

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
                RotateHead(_lastControllerRot.Value * Time.deltaTime * _controllerSensitivity);
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

        #endregion Unity methods

        /// <summary>
        /// Unregister an interaction in the list of those being in range to the player
        /// </summary>
        public void RemoveInteraction(IInteractable i)
        {
            _interactions.RemoveAll(x => x.GameObject.GetInstanceID() == i.GameObject.GetInstanceID());
            UpdateInteractionText();
        }

        /// <summary>
        /// Set player velocity, please note that this can be override easily when player move with keyboard/controller/other
        /// </summary>
        public void SetMouvement(Vector2 mouvement)
        {
            _mov = mouvement;
        }

        /// <summary>
        /// Return the text that is shown when interacting with something that can be interacted with
        /// </summary>
        /// <param name="interactionVerb">Verb used describing the option, for example for a door it would be "open"</param>
        public virtual string GetInteractionText(string interactionVerb) => string.IsNullOrEmpty(interactionVerb) ? string.Empty : $"Press 'E' to {interactionVerb}";
        /// <summary>
        /// Return the sentence when we can't interact with something
        /// For example being closed to a door that requires a key
        /// </summary>
        public virtual string GetDenyText(string denySentence) => denySentence;

        /// <summary>
        /// Update the interaction text
        /// </summary>
        public void UpdateInteractionText()
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

        /// <summary>
        /// Rotate the player by the given rotation
        /// If _head isn't set, the Y rotation is ignored
        /// </summary>
        protected void RotateHead(Vector2 rot)
        {
            if (!IsActive) return;

            rot = GetCurrentInputRotation(rot);

            transform.rotation *= Quaternion.AngleAxis(rot.x * _horizontalSensitivity, Vector3.up);

            if (_head != null)
            {
                _headRotation -= rot.y * _verticalSensitivity; // Vertical look is inverted by default, hence the -=

                _headRotation = Mathf.Clamp(_headRotation, -89, 89);
                _head.transform.localRotation = Quaternion.AngleAxis(_headRotation, Vector3.right);
            }
        }

        /// <summary>
        /// Make player jump
        /// </summary>
        /// <param name="jumpForceMultiplier">Force factor of the jump (based on _jumpForce), 1f mean a normal jump, 0.5f apply half the force</param>
        public void Jump(float jumpForceMultiplier = 1f)
        {
            _verticalSpeed = CurrentControl.JumpForce * jumpForceMultiplier;
            OnJumpDone.Invoke();
        }

        #region Inputs
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
                if (_mobileIsMoving == true) SetMouvement(Vector2.zero);
                _mobileIsMoving = null;
                if (_touchRef == _touchPos)
                {
                    OnInteractInternal();
                }
            }
        }

        public void OnMovement(InputAction.CallbackContext value)
        {
            SetMouvement(value.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext value)
        {
            var rot = value.ReadValue<Vector2>();
            _lastControllerRot = null;
            RotateHead(rot);
        }

        public void OnLookController(InputAction.CallbackContext value)
        {
            _lastControllerRot = value.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext value)
        {
            if (_controller.isGrounded && IsActive)
            {
                Jump();
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
        #endregion Inputs
    }
}