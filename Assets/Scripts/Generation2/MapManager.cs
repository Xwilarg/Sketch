using Sketch.Generation2.Area;
using Sketch.Generation2.Parsing;
using Sketch.Generation2.Runtime;
using Sketch.Grid;
using System.Linq;
using UnityEngine;

namespace Sketch.Generation2
{
    public class MapManager : MonoBehaviour
    {
        [Header("Room configuration")]
        [SerializeField]
        [Tooltip("Available rooms we can spawn")]
        private TextAsset[] _rooms;

        [SerializeField]
        [Tooltip("Size in pixel of a tile")]
        private int _tilePixelSize;

        [Header("Room prefabs")]
        [SerializeField]
        private GameObject _floorPrefab;
        [SerializeField]
        private GameObject _wallPrefab;

        [Header("Area debug")]
        [SerializeField]
        private GameObject _textAreaHint;

        [SerializeField]
        private GameObject _lrAreaPrefab;

        private TextRoomData[] _availableRooms;
        private GridManager<MapArea> _grid;

        private void Awake()
        {
            _availableRooms = _rooms.SelectMany(RoomParser.Parse).ToArray();

            _grid = new(_tilePixelSize, 20, new MapAreaFactory(_lrAreaPrefab, _textAreaHint));

            // Place the first room
            var startingRoom = _availableRooms[0];
            var mapArea = _grid.GetOrCreateMapArea(Vector2Int.zero);
            CreateRoom(mapArea, startingRoom, Vector2Int.zero);
        }

        private RuntimeRoom CreateRoom(MapArea mapArea, TextRoomData data, Vector2Int worldPos)
        {
            var rr = new RuntimeRoom(mapArea);
            DrawRoom(rr, data, worldPos);
            mapArea.Rooms.Add(rr);
            return rr;
        }

        private void DrawRoom(RuntimeRoom rr, TextRoomData data, Vector2Int worldPos)
        {
            for (var dy = 0; dy < data.Height; dy++)
            {
                for (var dx = 0; dx < data.Width; dx++)
                {
                    if (data.Data[dx, dy] == TileType.NONE)
                    {
                        continue; // Tile outside of the room, we ignore it
                    }
                    var xPos = worldPos.x + dx;
                    var yPos = worldPos.y + dy;
                    var p = new Vector2Int(xPos, yPos);
                    if (!_grid.Has(p)) // We didn't already place the tile and it's a wall
                    {
                        GameObject instance;
                        if (data.Data[dx, dy] == TileType.WALL)
                        {
                            instance = rr.AddWall(_wallPrefab, _grid.LocalToGlobal(p), p);
                        }
                        else if (data.Data[dx, dy] == TileType.DOOR)
                        {
                            instance = rr.AddDoor(_wallPrefab, _grid.LocalToGlobal(p), p);
                        }
                        else
                        {
                            instance = rr.AddFloor(_floorPrefab, _grid.LocalToGlobal(p), p);
                        }
                        _grid.RegisterTile(p, new InstantiatedTileData() {
                            SR = instance.GetComponent<SpriteRenderer>(),
                            Tile = data.Data[dx, dy],
                            RR = rr
                        });
                    }
                }
            }
        }
    }
}