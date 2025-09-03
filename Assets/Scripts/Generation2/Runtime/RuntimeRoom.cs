using Sketch.Generation2.Area;
using Sketch.Grid;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sketch.Generation2.Runtime
{
    public class RuntimeRoom
    {
        public static int _roomId = 0;

        public RuntimeRoom(MapArea area, GameObject filterTile, Material importantMat, Material normalMat)
        {
            _id = _roomId++;
            _container = new GameObject($"Room {_id}").transform;
            _container.transform.parent = area.RoomRoot;
            _filterTile = filterTile;
            _importantMat = importantMat;
            _normalMat = normalMat;
        }

        public GameObject AddWall(GameObject prefab, Vector2 globalPos, Vector2Int localPos)
        {
            var instance = Object.Instantiate(prefab, _container);
            instance.transform.position = globalPos;
            instance.name = $"Wall ({localPos.x};{localPos.y})";
            _walls.Add(instance);

            return instance;
        }

        public GameObject AddDoor(GameObject prefab, Vector2 globalPos, Vector2Int localPos)
        {
            var instance = Object.Instantiate(prefab, _container);
            instance.GetComponent<SpriteRenderer>().color = Color.red;
            instance.transform.position = globalPos;
            instance.name = $"Door ({localPos.x};{localPos.y})";
            _doors.Add(localPos);

            return instance;
        }

        public GameObject AddFloor(GameObject prefab, Vector2 globalPos, Vector2Int localPos)
        {
            var instance = Object.Instantiate(prefab, _container);
            instance.transform.position = globalPos;
            instance.name = $"Floor ({localPos.x};{localPos.y})";
            _floors.Add(localPos);

            return instance;
        }

        public bool IsClickedOn(Vector2Int pos)
            => _floors.Contains(pos);

        public void Highlight(GridManager<MapArea> grid)
        {
            foreach (var lr in _lrs)
            {
                lr.Value.LR.material = _importantMat;// Highlight our line renderers...
                lr.Value.RR._lrs.First(x => x.Key == _id).Value.LR.material = _importantMat; // ...and the ones going to us
            }
            foreach (var pos in _floors) // Highlights tiles in the room
            {
                var go = Object.Instantiate(_filterTile, _container);
                go.transform.position = grid.LocalToGlobal(pos);
                go.GetComponent<SpriteRenderer>().color = new(0f, 0f, 1f, .5f);
                _instantiatedHints.Add(go);
            }
            if (_hintDistanceInstance != null)
            {
                _hintDistanceInstance.color = Color.red;
            }
        }

        public void UnHighlight()
        {
            foreach (var lr in _lrs)
            {
                lr.Value.LR.material = _normalMat;
                lr.Value.RR._lrs.First(x => x.Key == _id).Value.LR.material = _normalMat;
            }
            foreach (var t in _instantiatedHints)
            {
                Object.Destroy(t);
            }
            _instantiatedHints.Clear();
            if (_hintDistanceInstance != null)
            {
                _hintDistanceInstance.color = Color.white;
            }
        }

        private int _id;
        private Transform _container;

        // Runtime tiles
        // Border tiles are shared between rooms so there are mostly for organization purpose
        private List<GameObject> _walls = new();
        private List<Vector2Int> _doors = new();
        private List<Vector2Int> _floors = new();

        // Previews
        private readonly List<GameObject> _instantiatedHints = new();
        private GameObject _filterTile;
        private Material _normalMat, _importantMat;
        // Room links and distances
        private readonly Dictionary<int, (LineRenderer LR, RuntimeRoom RR)> _lrs = new();
        private TMP_Text _hintDistanceInstance;
    }
}