using Sketch.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Generation
{
    public class DragInput : MonoBehaviour
    {
        private MapGenerator _generator;
        private Camera _cam;

        private Vector2? _clickPos;

        private void Awake()
        {
            _generator = GetComponent<MapGenerator>();
            _cam = Camera.main;
        }

        public void OnClick(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _clickPos = CursorUtils.Position;
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                if (_clickPos == CursorUtils.Position) // Click and not drag
                {
                    _generator.HandleClick(CursorUtils.Position);
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
