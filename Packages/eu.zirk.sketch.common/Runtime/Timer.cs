using UnityEngine;
using UnityEngine.Events;

namespace Sketch.Common
{
    public class Timer
    {
        public UnityEvent OnDone { set; get; } = new();
        public UnityEvent OnReset { set; get; } = new();

        private float _timer;
        private float _maxTime;

        public bool IsActive { private set; get; }

        public float TimerClamped => Mathf.Clamp(_timer, 0f, _maxTime);
        public float TimerClamped01 => _maxTime == 0f ? 0f : Mathf.Clamp01(_timer / _maxTime);

        public void Start(float maxTime)
        {
            if (IsActive) OnReset.Invoke();
            _timer = 0f;
            _maxTime = maxTime;
            IsActive = true;
        }

        public void Stop()
        {
            IsActive = false;
            OnDone.Invoke();
            OnReset.Invoke();
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            _timer += deltaTime;

            if (_timer >= _maxTime)
            {
                IsActive = false;
                OnDone.Invoke();
            }
        }
    }
}