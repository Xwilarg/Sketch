using Sketch.Common;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Sketch.Circle
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { private set; get; }

        private readonly List<PolygonCollider2D> _enemies = new();
        public IReadOnlyList<PolygonCollider2D> Enemies => _enemies.AsReadOnly();

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
            var bounds = _cam.CalculateBounds();
            var p = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));
            var go = Instantiate(_spawnables[Random.Range(0, _spawnables.Length)], p, Quaternion.identity);
            _enemies.Add(go.GetComponent<PolygonCollider2D>());
        }
    }
}