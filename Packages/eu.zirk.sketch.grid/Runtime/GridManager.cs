using Sketch.Grid.MapArea;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Grid
{
    public class GridManager<MA> where MA : BaseMapArea
    {
        /// <param name="elementsize">The size of a tile, if your scale is 1 it'll be 100</param>
        /// <param name="areaSize">Amount of tiles we contains within a sub-group</param>
        public GridManager(float elementSize, int areaSize, AMapAreaFactory factory)
        {
            _elementSize = elementSize;
            _areaSize = areaSize;
            _factory = factory;
        }


        private readonly float _elementSize;
        private readonly int _areaSize;
        private AMapAreaFactory _factory;

        private float LocalToGlobalScale => _elementSize / 100f;

        // We split the world into areas for optimization purposes
        private readonly Dictionary<Vector2Int, MA> _areas = new();

        /// <summary>
        /// Convert a coordinate in the world by one that can be used by <see cref="GetOrCreateMapArea(Vector2Int)"/>
        /// </summary>
        private Vector2Int GlobalToMapAreaCoordinate(Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x / LocalToGlobalScale / _areaSize), Mathf.RoundToInt(v.y / LocalToGlobalScale / _areaSize));
        }

        public Bounds GetTile(Vector2 worldPos)
        {
            var rounded = new Vector2Int(Mathf.RoundToInt(worldPos.x / LocalToGlobalScale), Mathf.RoundToInt(worldPos.y / LocalToGlobalScale));

            return new Bounds((Vector2)rounded * LocalToGlobalScale, Vector2.one * LocalToGlobalScale);
        }

        public IEnumerable<MA> GetAllMapAreas()
            => _areas.Select(x => x.Value);

        public MA GetOrCreateMapAreaFromWorld(Vector2 worldP, int xOffset = 0, int yOffset = 0)
        {
            return GetOrCreateMapArea(GlobalToMapAreaCoordinate(worldP) + new Vector2Int(xOffset, yOffset));
        }

        public MA GetOrCreateMapArea(Vector2Int p)
        {
            if (_areas.ContainsKey(p))
            {
                return _areas[p];
            }
            var area = _factory.CreateMapArea<MA>(p, _areaSize, LocalToGlobalScale);
            _areas.Add(p, area);
            return area;
        }

        public void RegisterTile(Vector2Int p, ITileData data)
        {
            GetOrCreateMapArea(p / _areaSize).Data.Add(p, data);
        }

        public bool Has(Vector2Int p)
        {
            return _areas.ContainsKey(p / _areaSize) && _areas[p / _areaSize].Data.ContainsKey(p);
        }

        public T Get<T>(Vector2Int p) where T : ITileData
        {
            return (T)_areas[p / _areaSize].Data[p];
        }

        public void Set(Vector2Int p, ITileData data)
        {
            _areas[p / _areaSize].Data[p] = data;
        }
    }
}
