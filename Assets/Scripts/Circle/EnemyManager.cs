using Sketch.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Sketch.Circle
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { private set; get; }

        private readonly List<CircleEnemy> _enemies = new();
        public IReadOnlyList<CircleEnemy> Enemies => _enemies.AsReadOnly();

        private Camera _cam;

        [SerializeField]
        private GameObject[] _spawnables;

        public void Remove(int index)
        {
            Destroy(_enemies[index].gameObject);
            _enemies.RemoveAt(index);

            SpawnOne();
        }

        private void Awake()
        {
            Instance = this;

            _cam = Camera.main;

            for (int i = 0; i < 10; i++)
            {
                SpawnOne();
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
            _enemies.Add(go.GetComponent<CircleEnemy>());
        }
    }
}