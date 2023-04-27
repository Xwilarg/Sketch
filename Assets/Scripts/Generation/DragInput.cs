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
                _clickPos = Mouse.current.position.ReadValue();
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
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
