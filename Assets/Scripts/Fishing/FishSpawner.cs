using Sketch.Common;
using System.Collections;
using UnityEngine;

namespace Sketch.Fishing
{
    public class FishSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _fishPrefab;

        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
            var bounds = _cam.CalculateBounds();
            var maxDist = Mathf.Max(bounds.max.x, bounds.max.y);
            var dist = Random.insideUnitCircle.normalized * maxDist;
            var angle = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg + 180f + Random.Range(-45f, 45f);
            Instantiate(_fishPrefab, dist, Quaternion.AngleAxis(angle, Vector3.forward));
            yield return Spawn();
        }
    }
}
