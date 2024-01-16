using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Generation
{
    public class RuntimeRoom
    {
        // GameObject that contains all the instanciated data
        public Transform Container;

        // Size of a tile
        public float PixelSize;

        // Prefabs to render stuff
        public GameObject LRPrefab;
        public Material NormalMat, ImportantMat;
        public GameObject FilterTile;
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

            var go = Object.Instantiate(LRPrefab, Container.transform);
            var lr = go.GetComponent<LineRenderer>();
            lr.SetPositions(new[]
            {
                (Vector3)_center * PixelSize, (Vector3)room._center * PixelSize
            });
            LRs.Add(room._center, (lr, room));
        }

        public void Highlight()
        {
            foreach (var lr in LRs)
            {
                lr.Value.LR.material = ImportantMat;// Highlight our line renderers...
                lr.Value.RR.LRs.First(x => x.Key == _center).Value.LR.material = ImportantMat; // ...and the ones going to us
            }
            foreach (var pos in Floors) // Highlights tiles in the room
            {
                var go = Object.Instantiate(FilterTile, Container);
                go.transform.position = (Vector2)pos * PixelSize;
                go.GetComponent<SpriteRenderer>().color = new(0F, 0f, 1f, .5f);
                _instanciatedHints.Add(go);
            }
        }

        public void UnHighlight()
        {
            foreach (var lr in LRs)
            {
                lr.Value.LR.material = NormalMat;
                lr.Value.RR.LRs.First(x => x.Key == _center).Value.LR.material = NormalMat;
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