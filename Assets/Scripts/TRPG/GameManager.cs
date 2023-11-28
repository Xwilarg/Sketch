using Sketch.Common;
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

        [SerializeField]
        private Material _highlightMat;

        [SerializeField]
        private GameObject _playerPrefab;

        // Instance
        private GameObject _player;
        private Vector2Int _playerPos;

        // List of all tiles of the board
        private TileData[,] _tiles;
        private readonly List<Vector2Int> _availableMoves = new();

        private const int _maxSize = 20;
        private const int _range = 5;
        private const float _visionRange = 20f;

        private Camera _cam;

        private readonly List<GameObject> _helpTiles = new();

        private void Awake()
        {
            _cam = Camera.main;

            _tiles = new TileData[_maxSize, _maxSize];

            // We create all tiles and place obstacles randomly
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

            // Find a random pos near the center of the board to spawn the player
            var quarter = Mathf.RoundToInt(_maxSize / 4f);
            while (_player == null)
            {
                _playerPos = new Vector2Int(Random.Range(quarter, 3 * quarter), Random.Range(quarter, 3 * quarter));
                if (_tiles[_playerPos.x, _playerPos.y].Obstacle == null)
                {
                    _player = Instantiate(_playerPrefab, (Vector2)_playerPos, Quaternion.identity);
                }
            }

            Camera.onPostRender += OnPostRenderCallback;
            DisplayHintTiles();
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= OnPostRenderCallback;
        }

        private void OnPostRenderCallback(Camera c)
        {
            GL.PushMatrix();

            GL.LoadOrtho();

            _highlightMat.SetPass(0);

            Vector2? prevPos = null;

            for (float i = Mathf.PI / 4; i < 3 * Mathf.PI / 4; i += .001f)
            {
                GL.Begin(GL.TRIANGLES); // Performances :thinking:
                Vector2 pos;

                var mousePos = _cam.ScreenToWorldPoint(CursorUtils.Position);
                var angleRad = Mathf.Atan2(mousePos.y - _playerPos.y, mousePos.x - _playerPos.x);

                var angle = angleRad + i - Mathf.PI / 2;

                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var hit = Physics2D.Raycast(_playerPos, dir, _visionRange);
                if (hit.collider == null)
                {
                    pos = _playerPos + dir * _visionRange;
                }
                else
                {
                    pos = hit.point;
                }

                if (prevPos != null)
                {
                    DrawTriangle(_playerPos, prevPos.Value, pos);
                }
                prevPos = pos;
                GL.End();
            }
            GL.PopMatrix();
        }

        private void DisplayHintTiles()
        {
            // Clear previous data
            foreach (var t in _helpTiles)
            {
                Destroy(t);
            }
            _helpTiles.Clear();
            _availableMoves.Clear();

            var tile = Instantiate(_filterPrefab, (Vector2)_playerPos, Quaternion.identity);
            tile.GetComponent<SpriteRenderer>().color = new(0f, 0f, 1f, .5f);
            _helpTiles.Add(tile);

            List<TileDirection> tiles = new()
            {
                new(_playerPos, _playerPos, int.MaxValue)
            };

            DisplayRangeTile(_playerPos, _range, tiles);
        }

        private void DrawTriangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            GL.Vertex(WorldToViewport(point1));
            GL.Vertex(WorldToViewport(point2));
            GL.Vertex(WorldToViewport(point3));
            GL.Vertex(WorldToViewport(point1));
        }

        private Vector3 WorldToViewport(Vector2 pos)
        {
            Vector3 newPos = Camera.main.WorldToViewportPoint(pos);
            newPos.z = 0f;
            return newPos;
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
                        _availableMoves.Add(nextPos);

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

        private bool IsInBounds(int x, int y)
            => x >= 0 && x < _maxSize && y >= 0 && y < _maxSize;

        public void OnClick(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                var pos = Camera.main.ScreenToWorldPoint(CursorUtils.Position);
                var posI = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));

                if (_availableMoves.Contains(posI))
                {
                    _player.transform.position = (Vector2)posI;
                    _playerPos = posI;
                    DisplayHintTiles();
                }
            }
        }
    }
}
