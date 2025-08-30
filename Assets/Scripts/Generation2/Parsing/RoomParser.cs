using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.Generation2.Parsing
{
    public static class RoomParser
    {
        private static TileType GetTileType(string[] rawData, TileType[,] data, int x, int y)
        {
            return y < 0 || y >= rawData.Length || x < 0 || x >= rawData[y].Length ? TileType.NONE : data[x, y];
        }

        // https://stackoverflow.com/a/42535
        private static TileType[,] Rotate(TileType[,] array, int width, int height)
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

        public static IEnumerable<TextRoomData> Parse(TextAsset r)
        {
            var txt = r.text.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var height = txt.Length;
            var width = txt.Max(e => e.Length);

            // Determine the type of each tile based on its character
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

            // A door is characterized by an empty floor surrounded by 2 walls
            // Now that we know each tile, we look for doors by locating empty spaces
            List<Vector2Int> doors = new();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == txt[y].Length)
                    {
                        break; // Next y iteration
                    }

                    if (data[x, y] != TileType.FLOOR) continue; // Can't be a door here

                    // Surrounding tile types
                    var up = GetTileType(txt, data, x, y - 1);
                    var down = GetTileType(txt, data, x, y + 1);
                    var left = GetTileType(txt, data, x - 1, y);
                    var right = GetTileType(txt, data, x + 1, y);

                    // A door will be like so:
                    // D is door and X wall
                    // ...
                    // XDX
                    // ...
                    if (
                        (up == TileType.WALL && down == TileType.WALL && ((left == TileType.NONE && right == TileType.FLOOR) || (left == TileType.FLOOR && right == TileType.NONE))) ||
                        (left == TileType.WALL && right == TileType.WALL && ((up == TileType.NONE && down == TileType.FLOOR) || (up == TileType.FLOOR && down == TileType.NONE))))
                    {
                        data[x, y] = TileType.DOOR;
                        doors.Add(new(x, y));
                    }
                }
            }

            var room = new TextRoomData()
            {
                Width = width,
                Height = height,
                Data = data,
                Doors = doors
            };
            yield return room;

            // Also add the rooms for each possible rotation
            for (int i = 0; i < 3; i++)
            {
                (height, width) = (width, height);
                var rot = Rotate(room.Data, room.Width, room.Height);
                doors = new();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (rot[x, y] == TileType.DOOR)
                        {
                            doors.Add(new(x, y));
                        }
                    }
                }
                room = new TextRoomData
                {
                    Width = width,
                    Height = height,
                    Data = rot,
                    Doors = doors.ToArray()
                };
                yield return room;
            }
        }
    }
}