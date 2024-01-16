using Sketch.Achievement;
using Sketch.Common;
using Sketch.Translation;
using System.Collections;
using System.Linq;
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

        /// <summary>
        /// A fish will only try to bit the hook when this variable is true
        /// This allow to let the player rest a bit between 2 bits
        /// </summary>
        public bool IsReady { private set; get; } = true;

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

            var possibleFishes = _fishes.OrderBy(x => x.MaxSize);
            var index = 0;

            // Get a random fish but we have higher change to get a small one
            while (Random.Range(0, 2) == 0 || index == _fishes.Length - 1)
            {
                index++;
            }
            var info = _fishes[Random.Range(0, _fishes.Length)];

            controller.Info = info;

            var size = Random.Range(info.MinSize, info.MaxSize);
            controller.Size = size;
            go.transform.localScale = new(size, size, 1f);

            yield return Spawn();
        }

        public IEnumerator Congrats(FishController fish)
        {
            var size = Mathf.RoundToInt(fish.Size * 100);
            if (size >= 150)
            {
                AchievementManager.Instance.Unlock(AchievementID.FIS_150cm);
            }

            _congratsText.gameObject.SetActive(true);
            _congratsText.text = $"{Translate.Instance.Tr("youCaught")} {Translate.Instance.Tr(fish.Info.Name)}";
            _fishSizeText.text = $"{Translate.Instance.Tr("size")} {size}cm";
            yield return new WaitForSeconds(2f);
            _congratsText.gameObject.SetActive(false);
            IsReady = true;

        }

        public IEnumerator Rest()
        {
            IsReady = false;
            yield return new WaitForSeconds(2f);
            IsReady = true;
        }
    }
}
