using Sketch.Common;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Sketch.Fishing
{
    public class FishSpawner : MonoBehaviour
    {
        public static FishSpawner Instance { get; private set; }

        [SerializeField]
        private GameObject _fishPrefab;

        [SerializeField]
        private CatchMinigame _minigame;

        [SerializeField]
        private TMP_Text _congratsText, _fishSizeText;

        [SerializeField]
        private FishInfo[] _fishes;

        private Camera _cam;

        private void Awake()
        {
            Instance = this;
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
            var go = Instantiate(_fishPrefab, dist, Quaternion.AngleAxis(angle, Vector3.forward));
            var controller = go.GetComponent<FishController>();
            controller.Minigame = _minigame;
            var info = _fishes[Random.Range(0, _fishes.Length)];
            controller.Info = info;

            var size = Random.Range(info.MinSize, info.MaxSize);
            controller.Size = size;
            go.transform.localScale = new(size, size, 1f);

            yield return Spawn();
        }

        public IEnumerator Congrats(FishController fish)
        {
            _congratsText.gameObject.SetActive(true);
            _congratsText.text = $"You caught a {fish.Info.Name.ToLowerInvariant()}";
            _fishSizeText.text = $"Size: {Mathf.RoundToInt(fish.Size * 100)}cm";
            yield return new WaitForSeconds(2f);
            _congratsText.gameObject.SetActive(false);

        }
    }
}
