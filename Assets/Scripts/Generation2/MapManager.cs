using Sketch.Generation2.Area;
using Sketch.Generation2.Parsing;
using Sketch.Generation2.Runtime;
using Sketch.Grid;
using System.Collections;
using System.Collections.Generic;
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

        [Header("Room highlight")]
        [SerializeField]
        private GameObject _filterTile;
        [SerializeField]
        private Material _importantMat, _normalMat;

        [Header("Distances")]
        [SerializeField]
        private GameObject _textDistancePrefab;
        [SerializeField]
        private GameObject _lrPrefab;

        [Header("Area debug")]
        [SerializeField]
        private GameObject _textAreaHint;
        [SerializeField]
        private GameObject _lrAreaPrefab;

        private TextRoomData[] _availableRooms;
        private GridManager<MapArea> _grid;

        private Camera _cam;
        private DragInput _dInput;

        // The room we clicked on
        private RuntimeRoom _highlightedRoom;


        private Vector2Int[] _directions = new[]
        {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        private void Awake()
        {
            _availableRooms = _rooms.SelectMany(RoomParser.Parse).ToArray();

            _grid = new(_tilePixelSize, 20, new MapAreaFactory(_lrAreaPrefab, _textAreaHint));

            _cam = Camera.main;
            _dInput = GetComponent<DragInput>();

            // Place the first room
            var startingRoom = _availableRooms[0];
            var mapArea = _grid.GetOrCreateMapArea(Vector2Int.zero);
            CreateRoom(mapArea, startingRoom, Vector2Int.zero);
        }

        private void Start()
        {
            StartCoroutine(WatchOverInstantiation());
        }

        public void HandleClick(Vector2 uiPos)
        {
            var pos = _cam.ScreenToWorldPoint(uiPos);

            if (_highlightedRoom != null)
            {
                _highlightedRoom.UnHighlight();
                _highlightedRoom = null;
            }

            var rounded = _grid.GlobalToLocal(pos);
            var room = _grid.GetAllMapAreas().SelectMany(x => x.Rooms).FirstOrDefault(x => x.IsClickedOn(rounded));
            if (room != null)
            {
                _highlightedRoom = room;
                room.Highlight(_grid);

                /*if (!room.HasDoors)
                {
                    AchievementManager.Instance.Unlock(AchievementID.GEN_noDoor);
                }*/
            }
        }


        private IEnumerator WatchOverInstantiation()
        {
            Vector2 oldPos = Vector2.one * 100f; // Trigger change at start
            var areas = new Dictionary<Vector2Int, MapArea>();
            bool needFillHoles = true;
            while (true)
            {
                // Only parse areas near the mouse
                var pos = _dInput.LastCameraPos;

                if (pos != oldPos)
                {
                    foreach (var a in areas.Values)
                    {
                        a.Toggle(false);
                    }
                    areas.Clear();
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            var area = _grid.GetOrCreateMapAreaFromWorld(pos, x, y);
                            if (true)//area.NextDoors.Count > 0 || area.Rooms.Count > 0)
                            {
                                areas.Add(new(x, y), area);
                            }
                        }
                    }
                    foreach (var a in areas.Values)
                    {
                        a.Toggle(true);
                    }

                    needFillHoles = true;
                    oldPos = pos;
                }

                var builtAnyRoom = false;
                foreach (var _ in InstantiationLoop(areas))
                {
                    builtAnyRoom = true;
                    yield return new WaitForEndOfFrame();
                }

                if (!builtAnyRoom && needFillHoles)
                {
                    yield return FillHoles(areas);
                    needFillHoles = false;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator FillHoles(Dictionary<Vector2Int, MapArea> areas)
        {
            var min = areas[-Vector2Int.one].MinBound / _grid.LocalToGlobalScale;
            var max = areas[Vector2Int.one].MaxBound / _grid.LocalToGlobalScale;

            List<Vector2Int> empty = new();

            for (int y = Mathf.RoundToInt(min.y); y < Mathf.RoundToInt(max.y); y++)
            {
                for (int x = Mathf.RoundToInt(min.x); x < Mathf.RoundToInt(max.x); x++)
                {
                    if (!_grid.Has(new(x, y)))
                    {
                        empty.Add(new(x, y));
                    }
                }
            }

            Dictionary<Vector2Int, GameObject> group = new();
            while (empty.Any())
            {
                yield return new WaitForEndOfFrame();
                group.Clear();

                var area = _grid.GetOrCreateMapAreaFromWorld(_grid.LocalToGlobal(empty[0]));
                var rr = CreateRuntimeRoom(area);

                var first = empty[0];
                var go = rr.AddFloor(_floorPrefab, _grid.LocalToGlobal(first), first);
                group.Add(first, go);
                empty.RemoveAt(0);

                yield return new WaitForEndOfFrame();

                // We get a group of all the adjacent tiles
                for (int i = 0; i < empty.Count; i++) // Ugly but I'll optimize that later
                {
                    if (group.Keys.Any(x => _directions.Any(d => d + empty[i] == x)))
                    {
                        go = rr.AddFloor(_floorPrefab, _grid.LocalToGlobal(empty[i]), empty[i]);
                        group.Add(empty[i], go);
                        empty.RemoveAt(i);
                        i = -1;
                        yield return new WaitForEndOfFrame();
                    }
                }

                if (group.Keys.Any(x => _directions.Any(d =>
                {
                    var p = d + x;
                    return p.x <= Mathf.RoundToInt(min.x) || p.x >= Mathf.RoundToInt(max.x) || p.y <= Mathf.RoundToInt(min.y) || p.y >= Mathf.RoundToInt(max.y);
                })))
                {
                    // Room is not finished and going oob
                    foreach (var r in group.Values)
                    {
                        Destroy(r);
                    }
                    area.Rooms.Remove(rr);
                    continue;
                }

                foreach (var f in group)
                {
                    _grid.RegisterTile(f.Key, new InstantiatedTileData()
                    {
                        RR = rr,
                        SR = f.Value.GetComponent<SpriteRenderer>(),
                        Tile = TileType.FLOOR
                    });
                }
                rr.LateInit(_grid);
                foreach (var t in group.Keys)
                {
                    foreach (var room in _directions.Where(d => _grid.Has(t + d)))
                    {
                        var tile = _grid.Get<InstantiatedTileData>(t + room);
                        var r = tile.RR;
                        if (false && r != null && !RuntimeRoom.Compare(r, rr) && r._doors.Contains(t + room))
                        {
                            r.AddAdjacentRoom(rr);
                            rr.AddAdjacentRoom(r);

                            UpdateAllDistances();
                        }
                    }
                }
            }
        }

        private IEnumerable<RuntimeRoom> InstantiationLoop(IDictionary<Vector2Int, MapArea> areas)
        {
            var doorAreas = areas.Values.Where(x => x.NextDoors.Count > 0).ToArray();

            // Attempt to place a room
            foreach (var da in doorAreas)
            {
                for (int i = da.NextDoors.Count - 1; i >= 0; i--)
                {
                    var (rr, door) = da.NextDoors[i];
                    var placementData = LookForValidRoom(door);

                    if (placementData != null)
                    {
                        // Area where the new room will be generated
                        var pos = new Vector2Int(door.x - placementData.DoorPos.x, door.y - placementData.DoorPos.y);
                        var newArea = _grid.GetOrCreateMapAreaFromWorld(_grid.LocalToGlobal(pos));

                        var newRoom = CreateRoom(newArea, placementData.Room, pos);

                        // Replace the door with a floor
                        var target = _grid.Get<InstantiatedTileData>(door);
                        target.Tile = TileType.FLOOR;
                        Destroy(target.SR.gameObject);
                        var go = target.RR.AddWall(_floorPrefab, _grid.LocalToGlobal(door), door);
                        target.SR = go.GetComponent<SpriteRenderer>();

                        // Make a link to the next room
                        rr.AddAdjacentRoom(newRoom);
                        newRoom.AddAdjacentRoom(rr);


                        yield return newRoom;
                    }
                    else
                    {
                        // We can't place any room there

                        // If a surrounding room exist
                        var surroundings =
                        _directions
                        .Select(x => door + x)
                        .Select(p =>
                        {
                            if (_grid.Has(p))
                            {
                                var itd = _grid.Get<InstantiatedTileData>(p);
                                if (itd.Tile == TileType.FLOOR) return itd.RR;
                                return null;
                            }
                            return null;
                        })
                        .Where(x => x != null && !RuntimeRoom.Compare(x, rr));

                        if (surroundings.Any())
                        {
                            // We can't place anything, but this door link to an existing room!
                            var adjRr = surroundings.First();

                            // Replace the door with a floor
                            var target = _grid.Get<InstantiatedTileData>(door);
                            target.Tile = TileType.FLOOR;
                            Destroy(target.SR.gameObject);
                            var go = target.RR.AddWall(_floorPrefab, _grid.LocalToGlobal(door), door);
                            target.SR = go.GetComponent<SpriteRenderer>();

                            // Make a link to the next room
                            rr.AddAdjacentRoom(adjRr);
                            adjRr.AddAdjacentRoom(rr);
                        }
                        else
                        {
                            // Nothing to do with this door, we replace it by a wall
                            var target = _grid.Get<InstantiatedTileData>(door);
                            target.Tile = TileType.WALL;
                            Destroy(target.SR.gameObject);
                            var go = target.RR.AddWall(_wallPrefab, _grid.LocalToGlobal(door), door);
                            target.SR = go.GetComponent<SpriteRenderer>();
                        }
                    }
                    da.NextDoors.RemoveAt(i);
                }
            }
        }

        /// <returns>Information about the room that will be generated, null if not possible</returns>
        private GeneratedRoom LookForValidRoom(Vector2Int localPos)
        {
            foreach (var availableRoom in _availableRooms.OrderBy(x => Random.value)) // For all rooms...
            {
                foreach (var door in availableRoom.Doors) // ...and for all doors...
                {
                    if (CanGenerateRoom(localPos, availableRoom, door))
                    {
                        return new()
                        {
                            Room = availableRoom,
                            DoorPos = door,
                        };
                    }
                }
            }
            return null;
        }
        private record GeneratedRoom
        {
            public TextRoomData Room { set; get; }
            public Vector2Int DoorPos { set; get; }
        }

        private bool CanGenerateRoom(Vector2Int localPos, TextRoomData availableRoom, Vector2Int door)
        {
            bool isSuperposition = true;

            // Iterate on all tiles
            for (int dy = 0; dy < availableRoom.Height; dy++)
            {
                for (int dx = 0; dx < availableRoom.Width; dx++)
                {
                    // Get current tile of the room we check, and the tile that is in the world
                    var globalPos = new Vector2Int(localPos.x - door.x + dx, localPos.y - door.y + dy);
                    var me = availableRoom.Data[dx, dy];
                    var other = !_grid.Has(globalPos) ? TileType.NONE : _grid.Get<InstantiatedTileData>(globalPos).Tile;

                    if (other != TileType.NONE && other != me) // We can't place the tile if we are a wall but there is already a wall there
                    {
                        return false;
                    }
                    if (other != me) // Make sure the 2 rooms aren't just on top of each other
                    {
                        isSuperposition = false;
                    }
                }
            }

            return !isSuperposition;
        }

        private RuntimeRoom CreateRoom(MapArea mapArea, TextRoomData data, Vector2Int worldPos)
        {
            var rr = CreateRuntimeRoom(mapArea);
            DrawRoom(rr, data, worldPos, mapArea);
            return rr;
        }

        private RuntimeRoom CreateRuntimeRoom(MapArea mapArea)
        {
            var rr = new RuntimeRoom(mapArea, _filterTile, _importantMat, _normalMat, _textAreaHint, _lrPrefab, _textDistancePrefab);
            mapArea.Rooms.Add(rr);
            return rr;
        }

        private void UpdateAllDistances()
        {
            var allRuntimes = _grid.GetAllMapAreas().SelectMany(x => x.Rooms); // TODO: Don't do on all?
            while (allRuntimes.Any(x => x.UpdateDistances()))
            { }
        }

        /// <summary>
        /// Draw the room on screen
        /// </summary>
        /// <param name="rr">Representation of the instantiated room</param>
        /// <param name="data">Raw data containing what need to be drawn</param>
        /// <param name="worldPos">World position of the bottom left corner of the room</param>
        /// <param name="mapArea">Grid area this room will be in</param>
        private void DrawRoom(RuntimeRoom rr, TextRoomData data, Vector2Int worldPos, MapArea mapArea)
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
                            mapArea.NextDoors.Add((rr, p));
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
            rr.LateInit(_grid);
        }
    }
}