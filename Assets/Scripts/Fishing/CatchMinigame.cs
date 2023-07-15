using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Sketch.Fishing
{
    public class CatchMinigame : MonoBehaviour
    {
        public Action<bool> OnDone { private get; set; }

        [SerializeField]
        private RectTransform _fish, _cursor;

        private float _height;

        private float _max;

        private float _target;

        private float _timer;
        private float _maxTimer;

        private void OnEnable()
        {
            _height = ((RectTransform)transform).rect.height;
            _max = -_height + _fish.rect.height;
            _target = Random.Range(0f, _max);
            _timer = 0f;
            _maxTimer = 3f;
        }

        private void Update()
        {
            var dir = Time.deltaTime * (_fish.anchoredPosition.y < _target ? 1f : -1f) * 300f;
            _fish.anchoredPosition = new(_fish.anchoredPosition.x, _fish.anchoredPosition.y + dir);

            if (Mathf.Abs(_fish.anchoredPosition.y - _target) < 10f)
            {
                _target = Random.Range(0f, _max);
            }

            var pos = Input.touchCount > 0 ? Input.GetTouch(0).position.y : Mouse.current.position.ReadValue().y;
            _cursor.position = new(_cursor.position.x, pos + _cursor.rect.height / 2f);

            _timer += (_fish.anchoredPosition.y > _cursor.anchoredPosition.y || _fish.anchoredPosition.y + _cursor.rect.height - _fish.rect.height < _cursor.anchoredPosition.y ? -1f : 1f) * Time.deltaTime;
            _maxTimer -= Time.deltaTime * .5f;
            if (_timer >= _maxTimer)
            {
                OnDone(true);
            }
            else if (_timer <= -_maxTimer)
            {
                OnDone(false);
            }
        }
    }
}