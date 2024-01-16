using Sketch.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Generation
{
    public class MapGenerator : MonoBehaviour
    {
        public static MapGenerator Instance { private set; get; }

        [SerializeField]
        [Tooltip("Available rooms we can spawn")]
        private TextAsset[] _rooms;

        [SerializeField]
        [Tooltip("Prefab to use as a wall")]
        private GameObject _wallPrefab;

        [SerializeField]
        [Tooltip("Size in pixel of _wallPrefab")]
        private int _tilePixelSize;

        // Prefab to display various info

        [SerializeField]
        private GameObject _lrPrefab;

        [SerializeField]
        private Material _normalMat, _importantMat;

        [SerializeField]
        private GameObject _filterTile;

        // Parent object so everything isn't thrown up at the root
        private Transform _roomsParent;

        // Rooms we can instanciate
        private RoomData[] _availableRooms;

        private Camera _cam;

        private readonly List<Vector2Int> _nextDoors = new();

        // All tiles instanciated
        // This is used as a grid to check if some tile is at a specific position
        private readonly Dictionary<Vector2Int, InstanciatedTileData> _tiles = new();

        // All the rooms instanciated
        // This is used if we need to do stuff between rooms
        private readonly List<RuntimeRoom> _runtimeRooms = new();

        private RuntimeRoom _highlightedRoom;

        public void ToggleAllLinks(bool value)
        {
            foreach (var rr in _runtimeRooms)
            {
                rr.ToggleLinks(value);
            }
        }

        private void Awake()
        {
            Instance = this;

            _cam = Camera.main;
            _roomsParent = new GameObject("Rooms").transform;
            _availableRooms = _rooms.SelectMany(r => // Convert all room text assets to RoomData
            {
                var txt = r.text.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var height = txt.Length;
                var width = txt.Max(e => e.Length);
                var data = new TileType[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        TileType t;
                        if (x >= txt[y].Length || txt[y][x] == ' ')
                        {
                            t = TileType.NONE;
                        }
                        else if (txt[y][x] == '.')
                        {
                            t = TileType.FLOOR;
                        }
                        else
                        {
                            t = TileType.WALL;
                        }
                        data[x, y] = t;
                    }
                }

                TileType GetTileType(int x, int y)
                {
                    return y < 0 || y >= txt.Length || x < 0 || x >= txt[y].Length ? TileType.NONE : data[x, y];
                }

                List<Vector2Int> _doors = new();
                // Go over the tiles again to see if we see a door
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x == txt[y].Length)
                        {
                            break; // Next y iteration
                        }

                        // A door is charactirized by an empty floor surrounded by 2 walls on a side, and a wall / empty tile on the other one
                        var up = GetTileType(x, y - 1);
                        var down = GetTileType(x, y + 1);
                        var left = GetTileType(x - 1, y);
                        var right = GetTileType(x + 1, y);
                        if (data[x, y] == TileType.FLOOR &&
                            (up == TileType.WALL && down == TileType.WALL && ((left == TileType.NONE && right == TileType.FLOOR) || (left == TileType.FLOOR && right == TileType.NONE)) ||
                            left == TileType.WALL && right == TileType.WALL && ((up == TileType.NONE && down == TileType.FLOOR) || (up == TileType.FLOOR && down == TileType.NONE))))
                        {
                            data[x, y] = TileType.DOOR;
                            _doors.Add(new(x, y));
                        }
                    }
                }
                List<RoomData> rooms = new()
                {
                    new()
                    {
                        Width = width,
                        Height = height,
                        Data = data,
                        Doors = _doors.ToArray()
                    }
                };
                // Also add the rooms for each possible rotation
                for (int i = 0; i < 3; i++)
                {
                    (height, width) = (width, height);
                    var last = rooms.Last();
                    var rot = Rotate(last.Data, last.Width, last.Height);
                    _doors = new();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (rot[x, y] == TileType.DOOR)
                            {
                                _doors.Add(new(x, y));
                            }
                        }
                    }
                    var first = new RoomData
                    {
                        Width = width,
                        Height = height,
                        Data = rot,
                        Doors = _doors.ToArray()
                    };
                    rooms.Add(first);
                }
                return rooms;
            }).ToArray();
            var startingRoom = _availableRooms[0];
            RuntimeRoom rr = new(_runtimeRooms.Count + 1, _roomsParent, _tilePixelSize / 100f, _lrPrefab, _normalMat, _importantMat, _filterTile);
            DrawRoom(startingRoom, 0, 0, rr);
            _nextDoors.AddRange(startingRoom.Doors);
            StartCoroutine(Generate());
        }

        public void HandleClick(Vector2 uiPos)
        {
            var pos = _cam.ScreenToWorldPoint(uiPos);

            if (_highlightedRoom != null)
            {
                _highlightedRoom.UnHighlight();
                _highlightedRoom = null;
            }

            var rounded = new Vector2Int(Mathf.RoundToInt(pos.x / (_tilePixelSize / 100f)), Mathf.RoundToInt(pos.y / (_tilePixelSize / 100f)));
            var room = _runtimeRooms.FirstOrDefault(x => x.Floors.Contains(rounded));
            if (room != null)
            {
                _highlightedRoom = room;
                room.Highlight();
            }
        }

        public RuntimeRoom MakeRR()
            => new(_runtimeRooms.Count + 1, _roomsParent, _tilePixelSize / 100f, _lrPrefab, _normalMat, _importantMat, _filterTile);

        // https://stackoverflow.com/a/42535
        private TileType[,] Rotate(TileType[,] array, int width, int height)
        {
            var ret = new TileType[height, width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    ret[y, x] = array[width - x - 1, y];
                }
            }

            return ret;
        }

        private int _currentlyCheckedRoom;
        private IEnumerator Generate()
        {
            var directions = new[]
            {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };
            RuntimeRoom rr = null;
            while (true) // Even if we are out of room, we keep that loop alive
            {
                int roomMade = 0;
                while (_currentlyCheckedRoom < _nextDoors.Count)
                {
                    if (rr == null || !rr.IsEmpty)
                    {
                        rr = MakeRR();
                        roomMade++;
                    }

                    // Attempt to place a room
                    var target = _nextDoors[_currentlyCheckedRoom];

                    yield return GenerateRoom(target.x, target.y, rr);

                    // Fill doors
                    foreach (var door in _tiles.Where(x => x.Value.Tile == TileType.DOOR))
                    {
                        // Remove doors that lead to a wall or another door
                        if (directions.Count(x => _tiles.ContainsKey(door.Key + x) && _tiles[door.Key + x].Tile != TileType.FLOOR && _tiles[door.Key + x].Tile != TileType.NONE) >= 3)
                        {
                            // DEBUG
                            door.Value.GameObject.GetComponent<SpriteRenderer>().color = Color.white;
                            door.Value.Tile = TileType.WALL;
                        }
                        // Same for inside doors
                        else if (directions.Count(x => _tiles.ContainsKey(door.Key + x) && _tiles[door.Key + x].Tile == TileType.FLOOR) == 2)
                        {
                            var adjacentRoom = _runtimeRooms.First(x => x.Doors.Contains(door.Key));

                            rr.AddAdjacentRoom(adjacentRoom);
                            adjacentRoom.AddAdjacentRoom(rr);

                            Destroy(door.Value.GameObject);
                            door.Value.GameObject = null;
                            door.Value.Tile = TileType.FLOOR;
                        }
                    }
                }
                _currentlyCheckedRoom = 0;

                if (roomMade == 0 && OptionsManager.Instance.CalculateNewRooms)
                {
                    var camBounds = CameraUtils.CalculateBounds(_cam);
                    var min = camBounds.min * _tilePixelSize / 10f;
                    var max = camBounds.max * _tilePixelSize / 10f;

                    List<Vector2Int> empty = new();

                    for (int y = Mathf.RoundToInt(min.y); y < Mathf.RoundToInt(max.y); y++)
                    {
                        for (int x = Mathf.RoundToInt(min.x); x < Mathf.RoundToInt(max.x); x++)
                        {
                            if (!_tiles.ContainsKey(new(x, y)))
                            {
                                empty.Add(new(x, y));
                            }
                        }
                    }

                    List<Vector2Int> group = new();
                    while (empty.Any())
                    {
                        group.Clear();

                        var first = empty[0];
                        group.Add(first);
                        empty.RemoveAt(0);

                        // We get a group of all the adjacent tiles
                        for (int i = 0; i < empty.Count; i++) // Ugly but I'll optimize that later
                        {
                            if (group.Any(x => directions.Any(d => d + empty[i] == x)))
                            {
                                group.Add(empty[i]);
                                empty.RemoveAt(i);
                                i = -1;
                            }
                        }

                        if (group.Any(x => directions.Any(d =>
                        {
                            var p = d + x;
                            return p.x <= Mathf.RoundToInt(min.x) || p.x >= Mathf.RoundToInt(max.x) || p.y <= Mathf.RoundToInt(min.y) || p.y >= Mathf.RoundToInt(max.y);
                        })))
                        {
                            // Room is not finished and going oob
                            continue;
                        }

                        if (rr == null || !rr.IsEmpty)
                        {
                            rr = MakeRR();
                        }
                        rr.Floors.AddRange(group);
                        rr.LateInit();
                        foreach (var t in group)
                        {
                            _tiles.Add(t, new()
                            {
                                GameObject = null,
                                RR = rr,
                                Tile = TileType.FLOOR
                            });
                            foreach (var room in directions.Where(d => _tiles.ContainsKey(t + d)))
                            {
                                var r = _tiles[t + room].RR;
                                if (r != null && r.ID != rr.ID && r.Doors.Contains(t + room))
                                {
                                    rr.AddAdjacentRoom(_tiles[t + room].RR);
                                    _tiles[t + room].RR.AddAdjacentRoom(rr);
                                }
                            }
                        }
                        _runtimeRooms.Add(rr);

                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator GenerateRoom(int x, int y, RuntimeRoom rr)
        {
            var pxlSize = _tilePixelSize / 100f;
            var realPos = new Vector2(x, y) * pxlSize;
            var bounds = _cam.CalculateBounds();
            if (realPos.x < bounds.min.x - pxlSize || realPos.x > bounds.max.x + pxlSize || realPos.y < bounds.min.y - pxlSize || realPos.y > bounds.max.y + pxlSize)
            {
                // We are outside of the bounds so no need to continue further in this direction
                _currentlyCheckedRoom++;
                yield break;
            }

            foreach (var room in _availableRooms.OrderBy(x => UnityEngine.Random.value)) // For all rooms...
            {
                foreach (var door in room.Doors) // For all doors...
                {
                    bool isValid = true;
                    bool isSuperposition = true;
                    for (int dy = 0; dy < room.Height; dy++)
                    {
                        for (int dx = 0; dx < room.Width; dx++)
                        {
                            var globalPos = new Vector2Int(x - door.x + dx, y - door.y + dy);
                            var me = room.Data[dx, dy];
                            var other = !_tiles.ContainsKey(globalPos) ? TileType.NONE : _tiles[globalPos].Tile;
                            if (other != TileType.NONE && other != me) // We can't place the tile if we are a wall but there is already a wall there
                            {
                                // So let's check the next room
                                isValid = false;
                                break;
                            }
                            if (other != me) // Make sure the 2 rooms aren't just on top of each other
                            {
                                isSuperposition = false;
                            }
                        }
                        if (!isValid)
                        {
                            break;
                        }
                    }
                    if (isValid && !isSuperposition)
                    {
                        // Place the room
                        yield return new WaitForEndOfFrame();
                        DrawRoom(room, x - door.x, y - door.y, rr);
                        _nextDoors.AddRange(room.Doors.Select(d => new Vector2Int(x - door.x + d.x, y - door.y + d.y)));
                        _nextDoors.RemoveAt(_currentlyCheckedRoom);
                        yield break;
                    }
                }
            }
            // We can't do anything with that door
            var target = _tiles[new(x, y)];
            target.Tile = TileType.FLOOR;
            Destroy(target.GameObject);
            target.GameObject = null;
            _nextDoors.RemoveAt(_currentlyCheckedRoom);
        }

        /// <summary>
        /// Draw a room
        /// </summary>
        /// <param name="room">Information about the room to draw</param>
        /// <param name="x">X position of the drawing</param>
        /// <param name="y">Y position of the drawing</param>
        /// <param name="rr">
        /// Empty RuntimeRoom object that will be filled in this method
        /// This allow the parent method to then use it to fill the missing data
        /// </param>
        /// <returns></returns>
        private void DrawRoom(RoomData room, int x, int y, RuntimeRoom rr)
        {
            for (var dy = 0; dy < room.Height; dy++)
            {
                for (var dx = 0; dx < room.Width; dx++)
                {
                    if (room.Data[dx, dy] == TileType.NONE)
                    {
                        continue; // Tile outside of the room, we ignore it
                    }
                    var xPos = x + dx;
                    var yPos = y + dy;
                    var p = new Vector2Int(xPos, yPos);
                    GameObject instance = null;
                    if (!_tiles.ContainsKey(p)) // We didn't already place the tile and it's a wall
                    {
                        if (room.Data[dx, dy] == TileType.WALL)
                        {
                            instance = Instantiate(_wallPrefab, rr.Container);
                            instance.transform.position = (Vector2)p * _tilePixelSize / 100f;
                            instance.name = $"Wall ({p.x};{p.y})";
                            rr.Walls.Add(instance);
                        }
                        else if (room.Data[dx, dy] == TileType.DOOR) // DEBUG
                        {
                            instance = Instantiate(_wallPrefab, rr.Container);
                            instance.GetComponent<SpriteRenderer>().color = Color.red;
                            instance.transform.position = (Vector2)p * _tilePixelSize / 100f;
                            instance.name = $"Door ({p.x};{p.y})";
                            rr.Doors.Add(p);
                        }
                        else
                        {
                            rr.Floors.Add(p);
                        }
                        _tiles.Add(p, new() { GameObject = instance, Tile = room.Data[dx, dy], RR = rr });
                    }
                }
            }

            rr.LateInit();
            _runtimeRooms.Add(rr);
        }
    }
}
