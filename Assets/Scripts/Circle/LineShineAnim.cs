using UnityEngine;

namespace Sketch.Circle
{
    public class LineShineAnim : MonoBehaviour
    {
        private LineRenderer _lr;
        private bool _hasStart;
        private float _timer;
        private int _state;

        private float _speedMult = 10f;

        private Color[] _states = new[]
        {
            Color.white,
            Color.red,
            new Color(1f, 1f, 1f, 0f)
        };

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
        }

        public void StartTimer()
        {
            _state = 0;
            _timer = 0f;
            _hasStart = true;
        }

        public void StopTimer()
        {
            _hasStart = false;
        }

        private void Update()
        {
            if (_hasStart)
            {
                _timer += Time.deltaTime * _speedMult;
                var a = _states[_state];
                var b = _states[_state + 1];
                var c = new Color(Mathf.Lerp(a.r, b.r, _timer), Mathf.Lerp(a.g, b.g, _timer), Mathf.Lerp(a.b, b.b, _timer), Mathf.Lerp(a.a, b.a, _timer));
                _lr.startColor = c;
                _lr.endColor = c;
                if (_timer >= 1f)
                {
                    _timer = 0f;
                    _state++;
                    if (_state == _states.Length - 1)
                    {
                        _hasStart = false;
                    }
                }
            }
        }
    }
}