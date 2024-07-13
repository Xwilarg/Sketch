using Sketch.Common;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sketch.Fishing
{
    public class CatchMinigame : MonoBehaviour
    {
        public Action<bool> OnDone { private get; set; }

        [SerializeField]
        private RectTransform _fish, _cursor, _overallProgress;

        [SerializeField]
        [Tooltip("Text to inform the player the minigame is starting")]
        private GameObject _catchIt;

        private float _height;

        private float _max;

        private float _target;

        /// <summary>
        /// Our current timer, need to reach <see cref="_maxTimer"/> to win the minigame
        /// </summary>
        private float _timer;

        /// <summary>
        /// When reaching this duration, we win the minigame
        /// If we reach minus this duration we loose
        /// </summary>
        private float _maxTimer;

        /// <summary>
        /// Fish the cursor representing the fish is going by in the minigale
        /// </summary>
        const float _fishBaseSpeed = 300f;

        public FishController Fish { set; private get; }

        private void OnEnable()
        {
            _height = ((RectTransform)transform).rect.height;
            _max = -_height + _fish.rect.height;
            _target = Random.Range(0f, _max);
            _timer = 0f;
            _maxTimer = 3f;
            _overallProgress.localScale = new(1f, .5f, 1f);
            _cursor.position = new(_cursor.position.x, _height / 2f + _cursor.rect.height);
            StartCoroutine(StartMinigame());
        }

        private IEnumerator StartMinigame()
        {
            _catchIt.SetActive(true);
            yield return new WaitForSeconds(1f);
            _catchIt.SetActive(false);
        }

        private void Update()
        {
            if (_catchIt.activeInHierarchy)
            {
                // Player text info is still active, we wait for it to be gone
                return;
            }

            var dir = Time.deltaTime * (_fish.anchoredPosition.y < _target ? 1f : -1f) * _fishBaseSpeed * Fish.Size;
            _fish.anchoredPosition = new(_fish.anchoredPosition.x, _fish.anchoredPosition.y + dir);

            if (Mathf.Abs(_fish.anchoredPosition.y - _target) < 10f)
            {
                _target = Random.Range(0f, _max);
            }

            var pos = CursorUtils.Position.y;
            _cursor.position = new(_cursor.position.x, pos + _cursor.rect.height / 2f);

            _timer += (_fish.anchoredPosition.y - _fish.rect.height > _cursor.anchoredPosition.y || _fish.anchoredPosition.y + _cursor.rect.height < _cursor.anchoredPosition.y ? -1f : 1f) * Time.deltaTime;
            _maxTimer -= Time.deltaTime * .5f;
            if (_timer >= _maxTimer)
            {
                OnDone(true);
            }
            else if (_timer <= -_maxTimer)
            {
                OnDone(false);
            }
            else
            {
                var size = ((_timer / _maxTimer) + 1f) / 2f;
                _overallProgress.localScale = new(1f, size, 1f);
            }
        }
    }
}