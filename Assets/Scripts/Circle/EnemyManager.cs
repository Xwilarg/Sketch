using Sketch.Achievement;
using Sketch.Common;
using Sketch.Drawing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Circle
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { private set; get; }

        private readonly List<CircleEnemy> _enemies = new();

        private Camera _cam;

        private PlayerInput _pInput;
        private int _amountCircled;
        private bool _isMousePressed;

        [SerializeField]
        private GameObject[] _spawnables;

        private void Awake()
        {
            Instance = this;

            _cam = Camera.main;
            _pInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnOne();
            }
        }

        private void Update()
        {
            _amountCircled = 0;
            var mousePos = CursorUtils.GetPosition(_pInput).Value;
            DrawingManager.Instance.UpdatePosition(mousePos, _isMousePressed);
        }

        public void OnClick(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _isMousePressed = true;
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                _isMousePressed = false;
            }
        }

        public void Remove(CircleEnemy enn)
        {
            DrawingManager.Instance.Unregister(enn);
            Destroy(enn.gameObject);
            _enemies.Remove(enn);

            SpawnOne();

            _amountCircled++;
            if (_amountCircled >= 3)
            {
                AchievementManager.Instance.Unlock(AchievementID.CIR_CircleN);
            }
        }

        private void SpawnOne()
        {
            GameObject go = null;

            var bounds = _cam.CalculateBounds();
            while (go == null)
            {
                var p = new Vector2(Random.Range(bounds.min.x + 1f, bounds.max.x - 1f), Random.Range(bounds.min.y + 1f, bounds.max.y - 1f));

                if (Physics2D.OverlapCircle(p, 1f) == null)
                {
                    go = Instantiate(_spawnables[Random.Range(0, _spawnables.Length)], p, Quaternion.identity);
                }
            }
            var ce = go.GetComponent<CircleEnemy>();
            DrawingManager.Instance.Register(ce);
            _enemies.Add(ce);
        }
    }
}