using UnityEngine.Events;

namespace QuimblyJam.Utils
{
    public class Timer
    {
        public UnityEvent OnDone { set; get; } = new();

        private float _timer;
        private bool _isActive;

        public void Start(float time)
        {
            _timer = time;
            _isActive = true;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive) return;

            _timer -= deltaTime;

            if (_timer <= 0f)
            {
                OnDone.Invoke();
                _isActive = false;
            }
        }
    }
}