using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Generation
{
    public class MapArea
    {
        public MapArea(string name, GameObject lrPrefab, Vector2Int minBound, Vector2Int maxBound)
        {
            RoomRoot = new GameObject($"Rooms {name}").transform;

            // Add lines debug to show areas
            Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>().SetPositions(new Vector3[] { (Vector2)minBound, new Vector2(maxBound.x, minBound.y) });
            Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>().SetPositions(new Vector3[] { (Vector2)minBound, new Vector2(minBound.x, maxBound.y) });
            Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>().SetPositions(new Vector3[] { new Vector2(minBound.x, maxBound.y), (Vector2)maxBound });
            Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>().SetPositions(new Vector3[] { new Vector2(maxBound.x, minBound.y), (Vector2)maxBound });
        }

        // Parent object so everything isn't thrown up at the root
        public Transform RoomRoot;
        public List<RuntimeRoom> Rooms = new();
    }
}