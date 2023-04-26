using UnityEngine;

namespace Sketch.Generation
{
    public record RoomData
    {
        public int Width;
        public int Height;

        public TileType[,] Data;

        public Vector2Int[] Doors;
    }
}