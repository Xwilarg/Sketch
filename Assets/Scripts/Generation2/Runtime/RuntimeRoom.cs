using Sketch.Generation2.Area;
using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Generation2.Runtime
{
    public class RuntimeRoom
    {
        public static int _roomId = 0;

        public RuntimeRoom(MapArea area)
        {
            _id = _roomId++;
            _container = new GameObject($"Room {_id}").transform;
            _container.transform.parent = _container;
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

        private int _id;
        private Transform _container;

        // Runtime tiles
        // Border tiles are shared between rooms so there are mostly for organization purpose
        private List<GameObject> _walls = new();
        private List<Vector2Int> _doors = new();
        private List<Vector2Int> _floors = new();
    }
}