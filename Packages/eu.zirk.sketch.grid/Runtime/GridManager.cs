using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

namespace Sketch.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField]
        private bool _showDebugGizmos;

        [SerializeField]
        private float ElementSize = 100f;
        private float LocalToGlobalScale => ElementSize / 100f;

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos) return;

            var cam = Camera.main;
            var pos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var rounded = new Vector2Int(Mathf.RoundToInt(pos.x / LocalToGlobalScale), Mathf.RoundToInt(pos.y / LocalToGlobalScale));
            var start = ((Vector2)rounded * LocalToGlobalScale) - (Vector2.one * (LocalToGlobalScale / 2f));
            var end = ((Vector2)rounded * LocalToGlobalScale) + (Vector2.one * (LocalToGlobalScale / 2f));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(start, new Vector2(end.x, start.y));
            Gizmos.DrawLine(start, new Vector2(start.x, end.y));
            Gizmos.DrawLine(end, new Vector2(end.x, start.y));
            Gizmos.DrawLine(end, new Vector2(start.x, end.y));
        }
    }
}
