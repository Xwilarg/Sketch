using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Drawing
{
    public class DrawingManager : MonoBehaviour
    {
        public static DrawingManager Instance { private set; get; }

        [SerializeField]
        private LineRenderer _lr, _bufferLr;
        private LineShineAnim _bufferAnim;

        private Camera _cam;
        private readonly List<Vector3> _positions = new();
        private List<Vector3> _positionBuffer = new();
        private readonly float MinDistance = .1f;

        public float CurrLength { private set; get; }
        private const float MaxLength = 20f;

        private bool _isMousePressed;

        private List<ITargetShape> _shapes = new();

        public void Register(ITargetShape shape)
        {
            _shapes.Add(shape);
        }

        public void Unregister(ITargetShape shape)
        {
            _shapes.Remove(shape);
        }

        private void Awake()
        {
            Instance = this;

            // If objects are prefabs, we instantiate them
            if (_lr.gameObject.scene.name == null) _lr = Instantiate(_lr).GetComponent<LineRenderer>();
            if (_bufferLr.gameObject.scene.name == null) _bufferLr = Instantiate(_bufferLr).GetComponent<LineRenderer>();

            _cam = Camera.main;
            _bufferAnim = _bufferLr.GetComponent<LineShineAnim>();
        }
        // Check if 2 segments intersect
        // https://stackoverflow.com/a/9997374
        private bool Ccw(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
        }

        private bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return Ccw(a, c, d) != Ccw(b, c, d) && Ccw(a, b, c) != Ccw(a, b, d);
        }

        // Check if a point if inside a triangle
        // https://stackoverflow.com/a/2049593
        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, v1, v2);
            d2 = Sign(pt, v2, v3);
            d3 = Sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        // Drawing code

        public void CleanLines()
        {
            _positions.Clear();
            _positionBuffer.Clear();
            _lr.positionCount = 0;
            _bufferLr.positionCount = 0;
            CurrLength = 0;
        }

        private Vector2 ShapeCenter(List<Vector3> points)
        {
            return new Vector2(points.Sum(x => x.x) / points.Count, points.Sum(x => x.y) / points.Count);
        }

        private void OnDrawGizmos()
        {
            foreach (var enn in _shapes)
            {
                var points = enn.Collider.points;
                var p = enn.Position;
                for (int i = 1; i <= points.Length; i++)
                {
                    var a = p;
                    var b = p + (points[i - 1] * enn.Scale);
                    var c = p + ((i == points.Length ? points[0] : points[i]) * enn.Scale);
                    Debug.DrawLine(a, b, Color.red);
                    Debug.DrawLine(b, c, Color.red);
                    Debug.DrawLine(c, a, Color.red);
                }
            }

            if (_positions.Any())
            {
                foreach (var enn in _shapes)
                {
                    var center = ShapeCenter(_positions);
                    for (int i = 1; i <= _positions.Count; i++)
                    {
                        var a = center;
                        var b = _positions[i - 1];
                        var c = (i == _positions.Count ? _positions[0] : _positions[i]);
                        var color = PointInTriangle(enn.Position, a, b, c) ? Color.green : Color.grey;
                        Debug.DrawLine(a, b, color);
                        Debug.DrawLine(b, c, color);
                        Debug.DrawLine(c, a, color);
                    }
                }
            }
        }

        private bool IsTouchingLines(PolygonCollider2D enn)
        {
            var points = enn.points;
            var p = (Vector2)enn.transform.position;
            for (int i = 1; i <= points.Length; i++) // ...we get each triangle of the collider...
            {
                var a = p;
                var b = p + (points[i - 1] * enn.transform.localScale.x);
                var c = p + ((i == points.Length ? points[0] : points[i]) * enn.transform.localScale.x);
                Debug.DrawLine(a, b, Color.red);
                Debug.DrawLine(b, c, Color.red);
                Debug.DrawLine(c, a, Color.red);

                foreach (var point in _positions) // ...and then for each point of what we traced...
                {
                    if (PointInTriangle(point, a, b, c)) // ...and check if our mouse is inside
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void UpdateMousePress(bool isPressed)
        {
            _isMousePressed = isPressed;
        }

        public void UpdatePosition(Vector2 mousePosition)
        {
            if (_isMousePressed)
            {
                var position = _cam.ScreenToWorldPoint(mousePosition);
                position.z = 0;

                // Is the mouse inside a Katsis
                var isColliding = false;
                foreach (var enn in _shapes) // For each enemy...
                {
                    if (IsTouchingLines(enn.Collider))
                    {
                        CleanLines();
                        isColliding = true;
                        break;
                    }
                }

                // If not, we update the line on screen
                if (!isColliding)
                {
                    if (_positions.Any())
                    {
                        var dist = Vector2.Distance(_positions.Last(), position);
                        if (dist > MinDistance)
                        {
                            if (_positions.Count > 3)
                            {
                                var pointC = _positions.Last();
                                for (int i = 1; i < _positions.Count - 1; i++)
                                {
                                    if (Intersect(_positions[i - 1], _positions[i], pointC, position)) // We made a closed shape...
                                    {
                                        // Is a Katsis inside the circle
                                        for (int it = _shapes.Count - 1; it >= 0; it--) // ...for all enemies...
                                        {
                                            var enn = _shapes[it];
                                            var center = ShapeCenter(_positions);
                                            for (int y = 1; y <= _positions.Count; y++) // ...we get each triangle of what we drew...
                                            {
                                                var a = center;
                                                var b = _positions[y - 1];
                                                var c = y == _positions.Count ? _positions[0] : _positions[y];
                                                if (PointInTriangle(enn.Position, a, b, c)) // ...and check if the enemy is inside
                                                {
                                                    enn.GetCircled();
                                                    break;
                                                }
                                            }
                                        }

                                        // 2 lines interect, we bufferize them
                                        _positionBuffer = new(_positions)
                                        {
                                            position
                                        };
                                        _bufferLr.positionCount = _positionBuffer.Count;
                                        _bufferLr.SetPositions(_positionBuffer.ToArray());
                                        _bufferAnim?.StartTimer();
                                        _positions.Clear();
                                        CurrLength = 0;

                                        break;
                                    }
                                }
                            }

                            CurrLength += dist;
                            while (CurrLength > MaxLength)
                            {
                                var firstDist = Vector2.Distance(_positions[0], _positions[1]);
                                CurrLength -= firstDist;
                                _positions.RemoveAt(0);
                            }
                            _positions.Add(position);
                        }
                    }
                    else
                    {
                        _positions.Add(position);
                    }
                }
            }
            else if (_positions.Any()) // Mouse released
            {
                CleanLines();
            }

            _lr.positionCount = _positions.Count;
            _lr.SetPositions(_positions.ToArray());
        }
    }
}
