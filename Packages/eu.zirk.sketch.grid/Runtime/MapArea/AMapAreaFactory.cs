using UnityEngine;

namespace Sketch.Grid.MapArea
{
    public abstract class AMapAreaFactory
    {
        public abstract T CreateMapArea<T>(Vector2Int pos, int areaSize, float globalScale) where T : AMapArea;
    }
}