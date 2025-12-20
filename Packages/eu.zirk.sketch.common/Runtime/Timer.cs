using UnityEngine;
using UnityEngine.Events;

namespace Sketch.Common
{
    public class Timer
    {
        public UnityEvent OnDone { set; get; } = new();

        private float _timer;
        private float _maxTime;
        private bool _isActive;

        public float TimerClamped => Mathf.Clamp(_timer, 0f, _maxTime);
        public float TimerClamped01 => Mathf.Clamp01(_timer / _maxTime);

        public void Start(float maxTime)
        {
            _timer = 0f;
            _maxTime = maxTime;
            _isActive = true;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive) return;

            _timer += deltaTime;

            if (_timer >= _maxTime)
            {
                OnDone.Invoke();
                _isActive = false;
            }
        }
    }
}