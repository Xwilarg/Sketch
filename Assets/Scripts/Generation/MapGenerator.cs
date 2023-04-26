using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Generation
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField]
        private TextAsset[] _rooms;

        [SerializeField]
        private TextAsset _startingRoom;

        [SerializeField]
        private GameObject _wallPrefab;

        [SerializeField]
        private int _tilePixelSize;

        private Transform _roomsParent;

        private RoomData[] _availableRooms;

        private readonly Dictionary<Vector2Int, InstanciatedTileData> _tiles = new();

        private void Awake()
        {
            _roomsParent = new GameObject("Rooms").transform;
            _availableRooms = _rooms.Select(r => // Convert all room text assets to RoomData
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
                        }
                    }
                }
                return new RoomData
                {
                    Width = width,
                    Height = height,
                    Data = data
                };
            }).ToArray();
            GenerateRoom(5);
        }

        private void GenerateRoom(int count)
        {

        }

        private void DrawRoom(RoomData room, int x, int y)
        {
            for (var dy = 0; dy < room.Height; dy++)
            {
                for (var dx = 0; dx < room.Width; dx++)
                {
                    var xPos = x + dx;
                    var yPos = y + dy;
                    var p = new Vector2Int(dx, dy);
                    Assert.True(!_tiles.ContainsKey(p) || _tiles[p].Tile == room.Data[dy, dx]);
                    GameObject instance = null;
                    if (!_tiles.ContainsKey(p) && room.Data[dy, dx] == TileType.WALL) // We didn't already place the tile and it's a wall
                    {
                        instance = Instantiate(_wallPrefab, _roomsParent);
                        instance.transform.position = (Vector2)p / _tilePixelSize / 100f;
                    }
                    _tiles.Add(p, new() { GameObject =  instance, Tile = room.Data[dy, dx] });
                }
            }
        }
    }
}
