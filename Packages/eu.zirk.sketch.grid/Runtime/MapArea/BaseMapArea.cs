using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Grid.MapArea
{
    public class BaseMapArea
    {

        public BaseMapArea(Bounds bounds)
        {
            Bounds = bounds;
        }

        public Bounds Bounds { private set; get; }
        internal Dictionary<Vector2Int, ITileData> Data { get; } = new();
    }
}