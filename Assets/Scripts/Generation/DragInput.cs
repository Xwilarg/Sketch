using Sketch.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Generation
{
    public class DragInput : MonoBehaviour
    {
        private PlayerInput _pInput;

        private MapGenerator _generator;
        private Camera _cam;

        private Vector2? _clickPos;

        private void Awake()
        {
            _generator = GetComponent<MapGenerator>();
            _pInput = GetComponent<PlayerInput>();
            _cam = Camera.main;
        }

        public void OnClick(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _clickPos = CursorUtils.GetPosition(_pInput);
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                var pos = CursorUtils.GetPosition(_pInput);
                if (_clickPos == pos) // Click and not drag
                {
                    _generator.HandleClick(pos.Value);
                }

                _clickPos = null;
            }
        }

        public void OnMove(InputAction.CallbackContext value)
        {
            if (_clickPos != null)
            {
                _cam.transform.Translate(-value.ReadValue<Vector2>() / 100f);
            }
        }
    }
}
