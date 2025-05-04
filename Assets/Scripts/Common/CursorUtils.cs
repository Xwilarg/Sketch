using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Common
{
    public static class CursorUtils
    {
        public static Vector2? GetPosition(PlayerInput p)
        {
            if (Input.touchCount > 0) return Input.GetTouch(0).position;
            if (p.GetDevice<Mouse>() != null) return Mouse.current?.position?.ReadValue();
            return null;
        }
    }
}
