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
            var maxDist = Mathf.Max(bounds.size.x, bounds.size.y);
            var dist = Random.insideUnitCircle.normalized * maxDist;
            Instantiate(_fishPrefab, dist, Quaternion.Euler(0f, 0f, Random.value * 360f));
            yield return Spawn();
        }
    }
}
