using UnityEngine;

namespace Sketch.Grid.MapArea
{
    public class DefaultMapAreaFactory : AMapAreaFactory
    {
        public override T CreateMapArea<T>(Vector2Int pos, int areaSize, float globalScale)
        {
            return (T)new BaseMapArea(new Bounds((Vector2)pos * globalScale * areaSize, areaSize * globalScale * Vector2.one));
        }
    }
}