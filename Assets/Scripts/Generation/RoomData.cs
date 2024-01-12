using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Generation
{
    public class RuntimeRoom
    {
        public Transform Container;
        public RoomData Data;

        public GameObject LRPrefab;
        public float PixelSize;

        public Material NormalMat, ImportantMat;

        // Border tiles are shared between rooms so there are mostly for organization purpose
        public List<GameObject> Walls = new();
        public List<Vector2Int> Doors = new();
        public List<Vector2Int> Floors = new();

        private readonly List<LineRenderer> _lrs = new();

        private readonly List<RuntimeRoom> _adjacentRooms = new();
        public void AddAdjacentRoom(RuntimeRoom room)
        {
            _adjacentRooms.Add(room);

            var go = Object.Instantiate(LRPrefab, Container.transform);
            var lr = go.GetComponent<LineRenderer>();
            lr.SetPositions(new[]
            {
                (Vector3)Center * PixelSize, (Vector3)room.Center * PixelSize
            });
            _lrs.Add(lr);
        }

        public void Highlight()
        {
            foreach (var lr in _lrs)
            {
                lr.material = ImportantMat;
            }
        }

        public void UnHighlight()
        {
            foreach (var lr in _lrs)
            {
                lr.material = NormalMat;
            }
        }

        public Vector2 Center;
    }

    public record RoomData
    {
        public int Width;
        public int Height;

        public TileType[,] Data;

        public Vector2Int[] Doors;
    }
}