using System;
using UnityEngine;

namespace Sketch.TRPG
{
    public class TileDirection : IEquatable<TileDirection>
    {
        public TileDirection(Vector2Int position, Vector2Int from, float score)
            => (Position, From, Score) = (position, from, score);

        public Vector2Int Position { get; }
        public Vector2Int From { get; }
        public float Score { get; }

        public bool Equals(TileDirection other)
            => Position == other.Position;
    }
}
