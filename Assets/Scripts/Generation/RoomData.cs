using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Generation
{
    public class RuntimeRoom
    {
        public RuntimeRoom(int id, Transform parent, float pixelSize, GameObject lrPrefab, Material normalMat, Material importantMat, GameObject filterTile)
        {
            Container = new GameObject($"Room {id}").transform;
            Container.transform.parent = parent;

            _pixelSize = pixelSize;
            _lrPrefab = lrPrefab;
            _normalMat = normalMat;
            _importantMat = importantMat;
            _filterTile = filterTile;
        }

        // GameObject that contains all the instanciated data
        public Transform Container { get; }

        // Size of a tile
        private float _pixelSize;

        // Prefabs to render stuff
        private GameObject _lrPrefab;
        private Material _normalMat, _importantMat;
        private GameObject _filterTile;
        private readonly List<GameObject> _instanciatedHints = new();

        // Runtime tiles
        // Border tiles are shared between rooms so there are mostly for organization purpose
        public List<GameObject> Walls = new();
        public List<Vector2Int> Doors = new();
        public List<Vector2Int> Floors = new();

        public void LateInit()
        {
            _center = new(Floors.Sum(p => p.x) / Floors.Count, Floors.Sum(p => p.y) / Floors.Count);
        }

        // Line renderers that link rooms
        private readonly Dictionary<Vector2, (LineRenderer LR, RuntimeRoom RR)> LRs = new();

        // Rooms that have a door that lead to this one
        private readonly List<RuntimeRoom> _adjacentRooms = new();
        public void AddAdjacentRoom(RuntimeRoom room)
        {
            if (_adjacentRooms.Contains(room))
            {
                Debug.LogWarning("Trying to add an adjacent room when it was already added");
                return;
            }

            _adjacentRooms.Add(room);

            Debug.Log(Container == null);
            var go = Object.Instantiate(_lrPrefab, Container.transform);
            var lr = go.GetComponent<LineRenderer>();
            lr.SetPositions(new[]
            {
                (Vector3)_center * _pixelSize, (Vector3)room._center * _pixelSize
            });
            LRs.Add(room._center, (lr, room));
        }

        public void Highlight()
        {
            foreach (var lr in LRs)
            {
                lr.Value.LR.material = _importantMat;// Highlight our line renderers...
                lr.Value.RR.LRs.First(x => x.Key == _center).Value.LR.material = _importantMat; // ...and the ones going to us
            }
            foreach (var pos in Floors) // Highlights tiles in the room
            {
                var go = Object.Instantiate(_filterTile, Container);
                go.transform.position = (Vector2)pos * _pixelSize;
                go.GetComponent<SpriteRenderer>().color = new(0F, 0f, 1f, .5f);
                _instanciatedHints.Add(go);
            }
        }

        public void UnHighlight()
        {
            foreach (var lr in LRs)
            {
                lr.Value.LR.material = _normalMat;
                lr.Value.RR.LRs.First(x => x.Key == _center).Value.LR.material = _normalMat;
            }
            foreach (var t in _instanciatedHints)
            {
                Object.Destroy(t);
            }
            _instanciatedHints.Clear();
        }

        private Vector2 _center;
    }

    public record RoomData
    {
        public int Width;
        public int Height;

        public TileType[,] Data;

        public Vector2Int[] Doors;
    }
}