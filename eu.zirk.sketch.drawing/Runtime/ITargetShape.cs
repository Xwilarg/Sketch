using UnityEngine;

namespace Sketch.Drawing
{
    public interface ITargetShape
    {
        public PolygonCollider2D Collider { get; }
        public Vector2 Position { get; }

        public float Scale { get; }

        public void GetCircled();
    }
}