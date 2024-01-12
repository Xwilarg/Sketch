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
        [SerializeField]
        [Tooltip("Available rooms we can spawn")]
        private TextAsset[] _rooms;

        [SerializeField]
        [Tooltip("Prefab to use as a wall")]
        private GameObject _wallPrefab;

        [SerializeField]
        [Tooltip("Size in pixel of _wallPrefab")]
        private int _tilePixelSize;

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

        private void Awake()
        {
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
            DrawRoom(startingRoom, 0, 0);
            _nextDoors.AddRange(startingRoom.Doors);
            StartCoroutine(Generate());
        }

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
            while (true) // Even if we are out of room, we keep that loop alive
            {
                while (_currentlyCheckedRoom < _nextDoors.Count)
                {
                    // Attempt to place a room
                    var target = _nextDoors[_currentlyCheckedRoom];
                    yield return GenerateRoom(target.x, target.y);

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
                            Destroy(door.Value.GameObject);
                            door.Value.GameObject = null;
                            door.Value.Tile = TileType.FLOOR;
                        }
                    }
                }
                _currentlyCheckedRoom = 0;
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator GenerateRoom(int x, int y)
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
                        DrawRoom(room, x - door.x, y - door.y);
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

        private void DrawRoom(RoomData room, int x, int y)
        {
            var c = new Vector2(x + room.Width / 2f, y + room.Height / 2f);
            var rr = new RuntimeRoom
            {
                Container = new GameObject($"Room {_runtimeRooms.Count + 1} ({c.x} ; {c.y})").transform,
                Data = room,
                Center = c
            };
            rr.Container.transform.parent = _roomsParent;

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
                            rr.Doors.Add(instance);
                        }
                        _tiles.Add(p, new() { GameObject = instance, Tile = room.Data[dx, dy] });
                    }
                }
            }

            _runtimeRooms.Add(rr);
        }
    }
}
