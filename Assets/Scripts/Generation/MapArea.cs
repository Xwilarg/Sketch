using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sketch.Generation
{
    public class MapArea
    {
        public MapArea(int x, int y, GameObject lrPrefab, GameObject textHint, Vector2 minBound, Vector2 maxBound)
        {
            RoomRoot = new GameObject($"Rooms ({x} ; {y})").transform;
            RoomRoot.transform.position = (maxBound - minBound) / 2f;

            // Add lines debug to show areas
            _lrs = new LineRenderer[]
            {
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>()
            };
            _lrs[0].SetPositions(new Vector3[] { minBound, new Vector2(maxBound.x, minBound.y) });
            _lrs[1].SetPositions(new Vector3[] { minBound, new Vector2(minBound.x, maxBound.y) });
            _lrs[2].SetPositions(new Vector3[] { new Vector2(minBound.x, maxBound.y), maxBound });
            _lrs[3].SetPositions(new Vector3[] { new Vector2(maxBound.x, minBound.y), maxBound });
            _textHint = Object.Instantiate(textHint, RoomRoot);
            _textHint.transform.position = minBound + new Vector2(.5f, -.5f);
            _textHint.GetComponent<TMP_Text>().text = $"{x};{y}";

            Toggle(false);
        }

        public void Toggle(bool value)
        {
            foreach (var lr in _lrs) lr.gameObject.SetActive(value);
            _textHint.SetActive(value);
        }

        // Parent object so everything isn't thrown up at the root
        public Transform RoomRoot { private set; get; }
        public List<RuntimeRoom> Rooms { get; } = new();

        public List<Vector2Int> NextDoors { get; } = new();

        private readonly LineRenderer[] _lrs;
        private GameObject _textHint;
    }
}