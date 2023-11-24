using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.TRPG
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _obstaclePrefab;

        [SerializeField]
        private GameObject _filterPrefab;

        private TileData[,] _tiles;

        private const int _maxSize = 20;
        private const int _range = 5;

        private Camera _cam;

        private readonly List<GameObject> _helpTiles = new();

        private void Awake()
        {
            _cam = Camera.main;

            _tiles = new TileData[_maxSize, _maxSize];
            for (int y = 0; y < _maxSize; y++)
            {
                for (int x = 0; x < _maxSize; x++)
                {
                    GameObject obstacle = null;

                    if (Random.Range(0f, 1f) < .1f)
                    {
                        obstacle = Instantiate(_obstaclePrefab, new Vector2(x, y), Quaternion.identity);
                        obstacle.transform.localScale = new(3.2f, 3.2f, 1f);
                    }

                    _tiles[y, x] = new TileData()
                    {
                        Obstacle = obstacle
                    };
                }
            }
        }

        private Vector2Int[] _directions = new[]
        {
            Vector2Int.left, Vector2Int.right,
            Vector2Int.up, Vector2Int.down
        };

        private void DisplayRangeTile(Vector2Int pos, int moveLeft, List<TileDirection> path)
        {
            if (moveLeft == 0)
            {
                return;
            }

            foreach (var d in _directions)
            {
                var nextPos = pos + d;

                if (IsInBounds(nextPos.x, nextPos.y) && _tiles[nextPos.y, nextPos.x].Obstacle == null)
                {
                    if (!path.Any(x => x.Position == nextPos))
                    {
                        path.Add(new(nextPos, pos, moveLeft - 1));

                        var helpTile = Instantiate(_filterPrefab, (Vector2)nextPos, Quaternion.identity);
                        helpTile.GetComponent<SpriteRenderer>().color = new(0f, 1f, 0f, .5f);
                        _helpTiles.Add(helpTile);

                        DisplayRangeTile(nextPos, moveLeft - 1, path);
                    }
                    else if (moveLeft > path.First(x => x.Position == nextPos).Score)
                    {
                        path[path.IndexOf(path.First(x => x.Position == nextPos))] = new(nextPos, pos, moveLeft - 1);
                        DisplayRangeTile(nextPos, moveLeft - 1, path);
                    }
                }
            }
        }

        private void ClearAllTiles()
        {
            foreach (var t in _helpTiles)
            {
                Destroy(t);
            }
            _helpTiles.Clear();
        }

        private bool IsInBounds(int x, int y)
            => x >= 0 && x < _maxSize && y >= 0 && y < _maxSize;

        public void OnClick(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                ClearAllTiles();

                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var posI = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));

                if (IsInBounds(posI.x, posI.y))
                {
                    var tile = Instantiate(_filterPrefab, (Vector2)posI, Quaternion.identity);
                    tile.GetComponent<SpriteRenderer>().color = new(0f, 0f, 1f, .5f);
                    _helpTiles.Add(tile);

                    if (_tiles[posI.y, posI.x].Obstacle == null)
                    {
                        List<TileDirection> tiles = new()
                        {
                            new(posI, posI, int.MaxValue)
                        };

                        DisplayRangeTile(posI, _range, tiles);
                    }
                }
            }
        }
    }
}
