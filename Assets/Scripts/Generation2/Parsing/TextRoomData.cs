using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Generation2.Parsing
{
    public record TextRoomData
    {
        public int Width { set; get; }
        public int Height { set; get; }
        public TileType[,] Data { set; get; }
        public IEnumerable<Vector2Int> Doors { set; get; }
    }
}