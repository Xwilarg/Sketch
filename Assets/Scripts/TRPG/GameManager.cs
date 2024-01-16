using Sketch.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.TRPG
{
    public class GameManager : MonoBehaviour
    {
        [Header("Map")]

        [SerializeField]
        private GameObject _obstaclePrefab;

        [SerializeField]
        private GameObject _filterPrefab;

        [SerializeField]
        private Material _highlightMat;

        [SerializeField]
        private GameObject _pathPrefab;

        [SerializeField]
        private Sprite _straightPath, _cornerPath;

        [Header("Player")]

        [SerializeField]
        private GameObject _playerPrefab;

        [SerializeField]
        private GameObject _ghostPrefab;

        // Instance
        private GameObject _player;
        private Vector2Int _playerPos;
        private GameObject _playerGhost;

        // List of all tiles of the board
        private TileData[,] _tiles;
        private readonly List<Vector2Int> _availableMoves = new();

        // Size of the grid
        private const int _maxSize = 20;
        // Max movement range
        private const int _range = 10;
        // LOS range
        private const float _visionRange = 20f;

        // Chance to spawn a rock
        private const float _rockChance = .15f;
        // Number of enemies to spawn
        private const int _enemyCount = 5;
        // If the distance between an enemy and a player is inferior to that, he won't spawn there
        private const float _maxDistWithPlayer = 6f;

        private Camera _cam;

        private readonly List<GameObject> _helpTiles = new();
        private List<TileDirection> _dirInfo;
        private readonly List<GameObject> _dirInstances = new();

        // Containers to clean hierarchy
        private Transform _wallContainer, _enemyContainer, _hintContainer, _pathContainer;

        // Moving player

        // List of position to reach the destination
        private readonly List<Vector2Int> _pathToMouse = new();
        // Is the playing moving
        private bool _isMoving;
        // Internal timer to move
        private float _moveTimer;
        private Vector2 _lastPos;

        /// <summary>
        /// Last mouse position, used to generate path
        /// </summary>
        private Vector2Int? _lastMousePos;

        private void Awake()
        {
            _cam = Camera.main;

            _wallContainer = new GameObject("Walls").transform;
            _enemyContainer = new GameObject("Enemies").transform;
            _hintContainer = new GameObject("Hint tiles").transform;
            _pathContainer = new GameObject("Paths").transform;

            _tiles = new TileData[_maxSize, _maxSize];

            // We create all tiles and place obstacles randomly
            for (int y = 0; y < _maxSize; y++)
            {
                for (int x = 0; x < _maxSize; x++)
                {
                    GameObject obstacle = null;

                    if (Random.Range(0f, 1f) < _rockChance)
                    {
                        obstacle = Instantiate(_obstaclePrefab, new Vector2(x, y), Quaternion.identity);
                        obstacle.transform.parent = _wallContainer;
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

            // Add enemies on top of that
            var c = _enemyCount;
            while (c > 0)
            {
                var rand = new Vector2Int(Random.Range(0, _maxSize), Random.Range(0, _maxSize));

                if (_tiles[rand.y, rand.x].Obstacle == null && Vector2.Distance(_playerPos, rand) > _maxDistWithPlayer) // We are not too close from the player
                {
                    var enemy = Instantiate(_player, (Vector2)rand, Quaternion.identity);
                    enemy.transform.parent = _enemyContainer;
                    enemy.GetComponent<SpriteRenderer>().color = Color.red;
                    _tiles[rand.y, rand.x] = new TileData()
                    {
                        Obstacle = enemy
                    };

                    c--;
                }
            }

            Camera.onPostRender += OnPostRenderCallback;
            DisplayHintTiles();
        }

        private void OnDestroy()
        {
            Camera.onPostRender -= OnPostRenderCallback;
        }

        private void Update()
        {
            if (_isMoving)
            {
                _moveTimer += Time.deltaTime * 5f;

                _player.transform.position = Vector2.Lerp(_lastPos, _pathToMouse[0], Mathf.Clamp01(_moveTimer));

                if (_moveTimer >= 1f)
                {
                    _lastPos = _pathToMouse[0];
                    _pathToMouse.RemoveAt(0);
                    _moveTimer = 0f;

                    if (!_pathToMouse.Any())
                    {
                        _playerPos = new(Mathf.RoundToInt(_player.transform.position.x), Mathf.RoundToInt(_player.transform.position.y));
                        DisplayHintTiles();
                        _isMoving = false;
                    }
                }
            }
            if (!_isMoving)
            {
                DisplayPath();
            }
        }

        private void DisplayPath()
        {
            var mousePos = GetMousePosI();
            if (_lastMousePos == null || mousePos != _lastMousePos)
            {
                // Clear path tiles
                foreach (var go in _dirInstances) Destroy(go);
                _dirInstances.Clear();
                _pathToMouse.Clear();
                Destroy(_playerGhost);

                _lastMousePos = mousePos;

                // Display path
                var nextPos = _dirInfo.FirstOrDefault(x => x.Position == _lastMousePos);
                if (nextPos != null) // Mouse is a valid position we can move to
                {
                    _pathToMouse.Add(_lastMousePos.Value);
                    _playerGhost = Instantiate(_ghostPrefab, (Vector2)_lastMousePos, Quaternion.identity);

                    // Don't display path over the ghost so we directy go to the next one
                    Vector2 lastDir = nextPos.Position - nextPos.From;
                    nextPos = _dirInfo.First(x => x.Position == nextPos.From);
                    _pathToMouse.Add(nextPos.Position);

                    while (nextPos.From != nextPos.Position) // While we have tiles to display
                    {
                        if (OptionsManager.Instance.ShowPath) // If options say we don't show path we can skip all these calculations
                        {
                            // Path is straight is we are the last element or if the Vector Dot between our last direction and current one gives -1 or 1
                            var dir = nextPos.Position - nextPos.From;
                            var isStraight = lastDir == null || Mathf.Abs(Vector2.Dot(lastDir, dir)) == 1f;

                            Quaternion rot = Quaternion.identity;
                            if (isStraight)
                            {
                                if (dir == Vector2.up || dir == Vector2.down)
                                {
                                    rot = Quaternion.Euler(0f, 0f, 90f);
                                }
                            }
                            else
                            {
                                if ((lastDir == Vector2.left && dir == Vector2.up) || (dir == Vector2.right && lastDir == Vector2.down))
                                {
                                    rot = Quaternion.Euler(0f, 0f, 90f);
                                }
                                else if ((lastDir == Vector2.right && dir == Vector2.up) || (dir == Vector2.left && lastDir == Vector2.down))
                                {
                                    rot = Quaternion.Euler(0f, 0f, 180f);
                                }
                                else if ((lastDir == Vector2.right && dir == Vector2.down) || (dir == Vector2.left && lastDir == Vector2.up))
                                {
                                    rot = Quaternion.Euler(0f, 0f, -90f);
                                }
                            }

                            var tile = Instantiate(_pathPrefab, (Vector2)nextPos.Position, rot);
                            tile.transform.parent = _pathContainer;
                            var sr = tile.GetComponent<SpriteRenderer>();
                            sr.sprite = isStraight ? _straightPath : _cornerPath;
                            sr.color = Color.black;

                            _dirInstances.Add(tile);
                            lastDir = dir;
                        }

                        nextPos = _dirInfo.First(x => x.Position == nextPos.From);
                        _pathToMouse.Add(nextPos.Position);
                    }

                    // Since the parsing above go from destination to player, we need to reverse the array to it contains path from the player to the mouse
                    _pathToMouse.Reverse();
                }
            }
        }

        /// <summary>
        /// Get mouse position as a Vector2Int
        /// </summary>
        /// <returns>Mouse position, rounded to the closest tile</returns>
        private Vector2Int GetMousePosI()
        {
            var pos = Camera.main.ScreenToWorldPoint(CursorUtils.Position);
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }

        private void OnPostRenderCallback(Camera c)
        {
            if (OptionsManager.Instance.ShowLos)
            {
                GL.PushMatrix();

                GL.LoadOrtho();

                _highlightMat.SetPass(0);

                Vector2? prevPos = null;

                for (float i = Mathf.PI / 4; i < 3 * Mathf.PI / 4; i += .001f)
                {
                    var from = (Vector2)_player.transform.position;

                    GL.Begin(GL.TRIANGLES); // Performances :thinking:
                    Vector2 pos;

                    var mousePos = _cam.ScreenToWorldPoint(CursorUtils.Position);
                    var angleRad = Mathf.Atan2(mousePos.y - from.y, mousePos.x - from.x);

                    var angle = angleRad + i - Mathf.PI / 2;

                    var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var hit = Physics2D.Raycast(from, dir, _visionRange);
                    if (hit.collider == null)
                    {
                        pos = from + dir * _visionRange;
                    }
                    else
                    {
                        pos = hit.point;
                    }

                    if (prevPos != null)
                    {
                        DrawTriangle(from, prevPos.Value, pos);
                    }
                    prevPos = pos;
                    GL.End();
                }
                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Display hint tiles around the player to show where he can move
        /// </summary>
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
            tile.transform.parent = _hintContainer;
            tile.GetComponent<SpriteRenderer>().color = new(0f, 0f, 1f, .5f);
            _helpTiles.Add(tile);

            _lastMousePos = null;

            _dirInfo = new()
            {
                new(_playerPos, _playerPos, int.MaxValue)
            };

            foreach (var pos in DisplayRangeTile(_playerPos, _range, _dirInfo))
            {
                var helpTile = Instantiate(_filterPrefab, (Vector2)pos, Quaternion.identity);
                helpTile.transform.parent = _hintContainer;
                helpTile.GetComponent<SpriteRenderer>().color = new(0f, 1f, 0f, .5f);
                _helpTiles.Add(helpTile);
                _availableMoves.Add(pos);
            }
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

        private readonly Vector2Int[] _directions = new[]
        {
            Vector2Int.left, Vector2Int.right,
            Vector2Int.up, Vector2Int.down
        };

        private IEnumerable<Vector2Int> DisplayRangeTile(Vector2Int pos, int moveLeft, List<TileDirection> path)
        {
            if (moveLeft > 0)
            {
                foreach (var d in _directions)
                {
                    var nextPos = pos + d;

                    if (IsInBounds(nextPos.x, nextPos.y) && _tiles[nextPos.y, nextPos.x].Obstacle == null)
                    {
                        if (!path.Any(x => x.Position == nextPos))
                        {
                            path.Add(new(nextPos, pos, moveLeft - 1));

                            yield return nextPos;
                            foreach (var p in DisplayRangeTile(nextPos, moveLeft - 1, path))
                            {
                                yield return p;
                            }

                            
                        }
                        else if (moveLeft > path.First(x => x.Position == nextPos).Score)
                        {
                            path[path.IndexOf(path.First(x => x.Position == nextPos))] = new(nextPos, pos, moveLeft - 1);
                            foreach (var p in DisplayRangeTile(nextPos, moveLeft - 1, path))
                            {
                                yield return p;
                            }
                        }
                    }
                }
            }
        }

        private bool IsInBounds(int x, int y)
            => x >= 0 && x < _maxSize && y >= 0 && y < _maxSize;

        public void OnClick(InputAction.CallbackContext value)
        {
            if (value.performed && !_isMoving)
            {
                var posI = GetMousePosI();

                if (_availableMoves.Contains(posI))
                {
                    _lastPos = _player.transform.position;
                    _isMoving = true;
                }
            }
        }
    }
}
