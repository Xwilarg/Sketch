using Sketch.Grid.MapArea;
using UnityEngine;

namespace Sketch.Generation.Area
{
    public class MapAreaFactory : AMapAreaFactory
    {
        private readonly GameObject _lrPrefab, _textHint;

        public MapAreaFactory(GameObject lrPrefab, GameObject textHint)
        {
            _lrPrefab = lrPrefab;
            _textHint = textHint;
        }

        public override T CreateMapArea<T>(Vector2Int p, int areaSize, float globalScale)
        {
            return (T)(AMapArea)new MapArea(p.x, p.y, _lrPrefab, _textHint, (Vector2)p * globalScale * areaSize, areaSize * globalScale);
        }
    }
}