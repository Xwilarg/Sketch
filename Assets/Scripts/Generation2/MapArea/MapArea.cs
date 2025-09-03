using Sketch.Generation2.Runtime;
using Sketch.Grid.MapArea;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sketch.Generation2.Area
{
    public class MapArea : BaseMapArea
    {
        public MapArea(int x, int y, GameObject lrPrefab, GameObject textHint, Vector2 center, float size) : base(new Bounds(center, size * Vector2.one))
        {
            RoomRoot = new GameObject($"Rooms ({x} ; {y})").transform;

            MinBound = Bounds.min;
            MaxBound = Bounds.max;
            RoomRoot.transform.position = (Bounds.max - Bounds.min) / 2f;

            // Add lines debug to show areas
            _lrs = new LineRenderer[]
            {
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>(),
                Object.Instantiate(lrPrefab, RoomRoot).GetComponent<LineRenderer>()
            };
            _lrs[0].SetPositions(new Vector3[] { Bounds.min, new Vector2(Bounds.max.x, Bounds.min.y) });
            _lrs[1].SetPositions(new Vector3[] { Bounds.min, new Vector2(Bounds.min.x, Bounds.max.y) });
            _lrs[2].SetPositions(new Vector3[] { new Vector2(Bounds.min.x, Bounds.max.y), Bounds.max });
            _lrs[3].SetPositions(new Vector3[] { new Vector2(Bounds.max.x, Bounds.min.y), Bounds.max });
            _textHint = Object.Instantiate(textHint, RoomRoot);
            _textHint.transform.position = new Vector2(Bounds.min.x + .5f, Bounds.max.y - .5f);
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

        public List<(RuntimeRoom, Vector2Int)> NextDoors { get; } = new();

        private readonly LineRenderer[] _lrs;
        private GameObject _textHint;

        public Vector2 MinBound { get; }
        public Vector2 MaxBound { get; }
    }
}