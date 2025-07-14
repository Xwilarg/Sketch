using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Grid.MapArea
{
    public class BaseMapArea
    {

        public BaseMapArea(Bounds bounds)
        {
            Bounds = bounds;
        }

        public IEnumerable<KeyValuePair<Vector2Int, T>> Where<T>(Func<Vector2Int, T, bool> func) where T : ITileData
        {
            return Data.Where(x => func(x.Key, (T)x.Value)).Select(x => new KeyValuePair<Vector2Int, T>(x.Key, (T)x.Value));
        }

        public Bounds Bounds { private set; get; }
        internal Dictionary<Vector2Int, ITileData> Data { get; } = new();
    }
}