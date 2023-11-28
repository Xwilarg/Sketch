using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Common
{
    public static class CursorUtils
    {
        public static Vector2 Position
            => Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
    }
}
