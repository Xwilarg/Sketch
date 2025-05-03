using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Generation
{
    public class MapArea
    {
        public MapArea(string name, GameObject lrPrefab, Vector2 minBound, Vector2 maxBound)
        {
            RoomRoot = new GameObject($"Rooms {name}").transform;
            RoomRoot.transform.position = (Vector2)(maxBound - minBound) / 2f;

            // Add lines debug to show areas
            _lrs = new LineRenderer[]
            {
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>()
            };
            _lrs[0].SetPositions(new Vector3[] { (Vector2) minBound, new Vector2(maxBound.x, minBound.y) });
            _lrs[1].SetPositions(new Vector3[] { (Vector2)minBound, new Vector2(minBound.x, maxBound.y) });
            _lrs[2].SetPositions(new Vector3[] { new Vector2(minBound.x, maxBound.y), (Vector2)maxBound });
            _lrs[3].SetPositions(new Vector3[] { new Vector2(maxBound.x, minBound.y), (Vector2)maxBound });

            Toggle(false);
        }

        public void Toggle(bool value)
        {
            foreach (var lr in _lrs) lr.gameObject.SetActive(value);
        }

        // Parent object so everything isn't thrown up at the root
        public Transform RoomRoot { private set; get; }
        public List<RuntimeRoom> Rooms { get; } = new();

        public List<Vector2Int> NextDoors { get; } = new();

        private readonly LineRenderer[] _lrs;
    }
}