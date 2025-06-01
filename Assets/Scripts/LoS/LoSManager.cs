using Sketch.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.LoSManager
{
    public class LoSManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _wallPrefab;

        [SerializeField]
        private Material _highlightMat;

        private float _maxVisionRange;

        [SerializeField, Range(0f, 1f)]
        private float _wallChance;

        private PlayerInput _pInput;

        private Camera _cam;

        private Vector2 _lastPos;

        private void Awake()
        {
            _cam = Camera.main;
            _pInput = GetComponent<PlayerInput>();
            var bounds = CameraUtils.CalculateBounds(_cam);

            var offX = Random.value / 2f;
            var offY = Random.value / 2f;

            _maxVisionRange = Vector2.Distance(bounds.min, bounds.max);

            // We create all tiles and place obstacles randomly
            for (int y = Mathf.FloorToInt(bounds.min.y); y <= Mathf.CeilToInt(bounds.max.y); y++)
            {
                for (int x = Mathf.FloorToInt(bounds.min.x); x <= Mathf.CeilToInt(bounds.max.x); x++)
                {
                    if (x == 0 && y == 0) continue;

                    GameObject obstacle = null;

                    if (Mathf.PerlinNoise(offX + x / 5f, offY + y / 5f) < _wallChance)
                    {
                        obstacle = Instantiate(_wallPrefab, new Vector2(x, y), Quaternion.identity);
                        obstacle.transform.localScale = new(3.2f, 3.2f, 1f);
                    }
                }
            }

            Camera.onPostRender += OnPostRenderCallback;
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= OnPostRenderCallback;
        }

        private void OnPostRenderCallback(Camera c)
        {
            GL.PushMatrix();

            GL.LoadOrtho();

            _highlightMat.SetPass(0);

            Vector2? prevPos = null;

            var screenPos = CursorUtils.GetPosition(_pInput) ?? _lastPos;
            var from = (Vector2)_cam.ScreenToWorldPoint(screenPos);
            if (Physics2D.OverlapPoint(from) != null) // Mouse is inside a wall
            {
                screenPos = _lastPos;
                from = (Vector2)_cam.ScreenToWorldPoint(screenPos);
            }
            else
            {
                _lastPos = screenPos;
            }


            for (float i = 0f; i < 2 * Mathf.PI; i += .001f)
            {
                GL.Begin(GL.TRIANGLES); // Performances :thinking:
                Vector2 pos;

                var angleRad = Mathf.Atan2(from.y, from.x);

                var angle = angleRad + i - Mathf.PI / 2;

                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var hit = Physics2D.Raycast(from, dir, _maxVisionRange);
                if (hit.collider == null)
                {
                    pos = from + dir * _maxVisionRange;
                }
                else
                {
                    pos = hit.point;
                }

                if (prevPos != null)
                {
                    DrawTriangle(from, prevPos.Value, pos);
                }
                prevPos = pos;
                GL.End();
            }
            GL.PopMatrix();
        }

        private void DrawTriangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            GL.Vertex(WorldToViewport(point1));
            GL.Vertex(WorldToViewport(point2));
            GL.Vertex(WorldToViewport(point3));
            GL.Vertex(WorldToViewport(point1));
        }

        private Vector3 WorldToViewport(Vector2 pos)
        {
            Vector3 newPos = Camera.main.WorldToViewportPoint(pos);
            newPos.z = 0f;
            return newPos;
        }
    }
}
