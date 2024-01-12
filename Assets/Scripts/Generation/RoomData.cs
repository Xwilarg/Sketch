using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Generation
{
    public class RuntimeRoom
    {
        public Transform Container;
        public RoomData Data;

        // Border tiles are shared between rooms so there are mostly for organization purpose
        public List<GameObject> Walls = new();
        public List<GameObject> Doors = new();

        public Vector2 Center;
    }

    public record RoomData
    {
        public int Width;
        public int Height;

        public TileType[,] Data;

        public Vector2Int[] Doors;
    }
}