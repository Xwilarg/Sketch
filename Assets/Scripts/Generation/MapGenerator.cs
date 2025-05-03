using Sketch.Achievement;
using Sketch.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
        private GameObject _floorPrefab;

        [SerializeField]
        [Tooltip("Size in pixel of _wallPrefab")]
        private int _tilePixelSize;

        // Prefab to display various info

        [SerializeField]
        private GameObject _lrPrefab, _lrAreaPrefab;

        [SerializeField]
        private Material _normalMat, _importantMat;

        [SerializeField]
        private GameObject _filterTile;

        [SerializeField]
        private GameObject _textHintPrefab;

        // Rooms we can instantiate
        private RoomData[] _availableRooms;

        private Camera _cam;
        private DragInput _dInput;

        // The room we clicked on
        private RuntimeRoom _highlightedRoom;

        // We split the world into areas for optimization purposes
        private readonly Dictionary<Vector2Int, MapArea> _areas = new();

        // All tiles instanciated
        // This is used as a grid to check if some tile is at a specific position
        private readonly Dictionary<Vector2Int, InstanciatedTileData> _tiles = new();

        /// <summary>
        /// Size used for MapArea
        /// </summary>
        private const int AreaSize = 10;

        private int _runtimeRoomId;

        public void ToggleAllLinks(bool value)
        {
            foreach (var rr in _areas.Values.SelectMany(x => x.Rooms))
            {
                rr.ToggleLinks(value);
            }
        }

        public void ToggleDistance(bool value)
        {
            foreach (var rr in _areas.Values.SelectMany(x => x.Rooms))
            {
                rr.ToggleDistances(value);
            }
        }

        private void Awake()
        {
            Instance = this;
            _dInput = GetComponent<DragInput>();

            _cam = Camera.main;
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

            // Create first room
            var startingRoom = _availableRooms[0];
            var mapArea = GetOrCreateMapArea(Vector2Int.zero);
            RuntimeRoom rr = MakeRR(mapArea);
            DrawRoom(startingRoom, 0, 0, rr);
            mapArea.NextDoors.AddRange(startingRoom.Doors);
        }

        private void Start()
        {
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
            var room = _areas.SelectMany(x => x.Value.Rooms).FirstOrDefault(x => x.Floors.Contains(rounded));
            if (room != null)
            {
                _highlightedRoom = room;
                room.Highlight();

                if (!room.HasDoors)
                {
                    AchievementManager.Instance.Unlock(AchievementID.GEN_noDoor);
                }
            }
        }

        /// <summary>
        /// Convert a coordinate in the world by one that can be used by <see cref="GetOrCreateMapArea(Vector2Int)"/>
        /// </summary>
        private Vector2Int GlobalToMapAreaCoordinate(Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x / AreaSize), Mathf.RoundToInt(v.y / AreaSize));
        }
        private MapArea GetOrCreateMapArea(Vector2Int p)
        {
            if (_areas.ContainsKey(p))
            {
                return _areas[p];
            }
            var area = new MapArea($"({p.x} ; {p.y})", _lrAreaPrefab, p * AreaSize, new Vector2Int(p.x + 1, p.y + 1) * AreaSize);
            _areas.Add(p, area);
            return area;
        }

        /// <summary>
        /// Create a new room to be instantiated on the world
        /// </summary>
        public RuntimeRoom MakeRR(MapArea ma)
        {
            var rr = new RuntimeRoom(_runtimeRoomId++, ma, _tilePixelSize / 100f, _lrPrefab, _normalMat, _importantMat, _filterTile, _textHintPrefab);
            ma.Rooms.Add(rr);
            return rr;

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

        private void AddRoomLinks(RuntimeRoom r1, RuntimeRoom r2)
        {
            r1.AddAdjacentRoom(r2);
            r2.AddAdjacentRoom(r1);

            var allRuntimes = _areas.SelectMany(x => x.Value.Rooms); // TODO: Don't do on all?
            while (allRuntimes.Any(x => x.UpdateDistances()))
            { }
        }

        // Keep track of the doors we area checking by areas
        private int _currentlyCheckedArea;
        private int _currentlyCheckedRoom;
        private int _roomMade;
        private IEnumerator Generate()
        {
            var directions = new[]
            {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };
            Vector2 oldPos = Vector2.one * 100f; // Trigger change at start
            var areas = new List<MapArea>();

            while (true) // Even if we are out of room, we keep that loop alive
            {
                // Only parse areas near the mouse
                var pos = _dInput.LastCameraPos;
                if (pos != oldPos)
                {
                    foreach (var a in areas)
                    {
                        a.Toggle(false);
                        //foreach (var d in a.NextDoors) _tiles[d].SR.color = Color.red;
                    }
                    areas.Clear();
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            var area = GetOrCreateMapArea(GlobalToMapAreaCoordinate(pos) + new Vector2Int(x, y));
                            if (true)//area.NextDoors.Count > 0 || area.Rooms.Count > 0)
                            {
                                areas.Add(area);
                            }
                        }
                    }
                    foreach (var a in areas)
                    {
                        a.Toggle(true);
                        //foreach (var d in a.NextDoors) _tiles[d].SR.color = Color.blue;
                    }
                }

                // Check doors to create new rooms
                _roomMade = 0;
                var doorAreas = areas.Where(x => x.NextDoors.Count > 0).ToArray();
                _currentlyCheckedArea = 0;
                _currentlyCheckedRoom = 0;

                while (_currentlyCheckedArea < doorAreas.Length && _currentlyCheckedRoom < doorAreas[_currentlyCheckedArea].NextDoors.Count)
                {
                    // Attempt to place a room
                    var target = doorAreas[_currentlyCheckedArea].NextDoors[_currentlyCheckedRoom];
                    var area = doorAreas[_currentlyCheckedArea];

                    yield return GenerateRoom(target.x, target.y, area);

                    // Fill doors
                    foreach (var rr in areas.SelectMany(x => x.Rooms))
                    {
                        foreach (var d in rr.Doors)
                        {
                            var door = _tiles.First(x => x.Value.Tile == TileType.DOOR);

                            // Remove doors that lead to a wall or another door
                            if (directions.Count(x => _tiles.ContainsKey(door.Key + x) && _tiles[door.Key + x].Tile != TileType.FLOOR && _tiles[door.Key + x].Tile != TileType.NONE) >= 3)
                            {
                                // DEBUG
                                door.Value.SR.color = Color.white;
                                door.Value.Tile = TileType.WALL;
                            }
                            // Same for inside doors
                            else if (directions.Count(x => _tiles.ContainsKey(door.Key + x) && _tiles[door.Key + x].Tile == TileType.FLOOR) == 2)
                            {
                                var adjacentRoom = _areas.SelectMany(x => x.Value.Rooms).First(x => x.Doors.Contains(door.Key));

                                AddRoomLinks(rr, adjacentRoom);

                                Destroy(door.Value.SR.gameObject);
                                door.Value.SR = null;

                                var floor = Instantiate(_floorPrefab, rr.Container);
                                floor.transform.position = (Vector2)door.Key * _tilePixelSize / 100f;
                                floor.name = $"Floor ({door.Key.x};{door.Key.y})";
                                door.Value.SR = floor.GetComponent<SpriteRenderer>();

                                door.Value.Tile = TileType.FLOOR;
                            }
                        }
                    }

                    _currentlyCheckedRoom++;
                    if (_currentlyCheckedRoom > doorAreas[_currentlyCheckedArea].NextDoors.Count)
                    {
                        _currentlyCheckedArea++;
                        _currentlyCheckedRoom = 0;
                    }
                }

                // If we created all rooms we could, we calculate spaces between rooms that could make rooms themselves
                if (false && _roomMade == 0 && OptionsManager.Instance.CalculateNewRooms) // TODO: Remove false
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

                        var rr = MakeRR(GetOrCreateMapArea(GlobalToMapAreaCoordinate((Vector2)group[0] * _tilePixelSize / 100f)));
                        foreach (var f in group)
                        {
                            var floor = Instantiate(_floorPrefab, rr.Container);
                            floor.transform.position = (Vector2)f * _tilePixelSize / 100f;
                            floor.name = $"Floor ({f.x};{f.y})";
                        }
                        rr.Floors.AddRange(group);
                        rr.LateInit();
                        foreach (var t in group)
                        {
                            _tiles.Add(t, new()
                            {
                                SR = null,
                                RR = rr,
                                Tile = TileType.FLOOR
                            });
                            foreach (var room in directions.Where(d => _tiles.ContainsKey(t + d)))
                            {
                                var r = _tiles[t + room].RR;
                                if (r != null && r.ID != rr.ID && r.Doors.Contains(t + room))
                                {
                                    AddRoomLinks(rr, _tiles[t + room].RR);
                                }
                            }
                        }
                        yield return new WaitForEndOfFrame();

                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Create a new room
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rr"></param>
        /// <param name="newArea">New area that contains this room</param>
        /// <param name="fromArea">Area that contains the door from which this room was generated</param>
        /// <returns></returns>
        private IEnumerator GenerateRoom(int x, int y, MapArea fromArea)
        {
            var pxlSize = _tilePixelSize / 100f;
            var realPos = new Vector2(x, y) * pxlSize;
            var bounds = _cam.CalculateBounds();
            if (realPos.x < bounds.min.x - pxlSize || realPos.x > bounds.max.x + pxlSize || realPos.y < bounds.min.y - pxlSize || realPos.y > bounds.max.y + pxlSize)
            {
                // We are outside of the bounds so no need to continue further in this direction
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
                        _roomMade++;
                        var newArea = GetOrCreateMapArea(GlobalToMapAreaCoordinate(new Vector2(x - door.x, y - door.y) * pxlSize));
                        DrawRoom(room, x - door.x, y - door.y, MakeRR(newArea));
                        newArea.NextDoors.AddRange(room.Doors.Select(d => new Vector2Int(x - door.x + d.x, y - door.y + d.y)));
                        fromArea.NextDoors.RemoveAt(_currentlyCheckedRoom);
                        yield break;
                    }
                }
            }
            // We can't do anything with that door
            var target = _tiles[new(x, y)];
            target.Tile = TileType.FLOOR;
            Destroy(target.SR.gameObject);
            target.SR = null;

            _roomMade++;
            var p = (Vector2)new(x, y) * pxlSize;
            var rr = MakeRR(GetOrCreateMapArea(GlobalToMapAreaCoordinate(p)));
            var floor = Instantiate(_floorPrefab, rr.Container);
            floor.transform.position = p;
            floor.name = $"Floor ({x};{y})";
            target.SR = floor.GetComponent<SpriteRenderer>();

            fromArea.NextDoors.RemoveAt(_currentlyCheckedRoom);
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
                    GameObject instance;
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
                            instance = Instantiate(_floorPrefab, rr.Container);
                            instance.transform.position = (Vector2)p * _tilePixelSize / 100f;
                            instance.name = $"Floor ({p.x};{p.y})";
                            rr.Floors.Add(p);
                        }
                        _tiles.Add(p, new() { SR = instance.GetComponent<SpriteRenderer>(), Tile = room.Data[dx, dy], RR = rr });
                    }
                }
            }

            rr.LateInit();
        }
    }
}
