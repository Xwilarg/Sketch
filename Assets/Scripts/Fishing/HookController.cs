using Sketch.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Fishing
{
    public class HookController : MonoBehaviour
    {
        private int _hp;
        private SpriteRenderer _sr;

        private FishController _hooked { set; get; }
        public FishController Hooked
        {
            set
            {
                IsTargeted = false;
                _hooked = value;
                if (value == null)
                {
                    _sr.color = Color.white;
                    _hp = Random.Range(2, 5);
                }
            }
            get => _hooked;
        }

        /// <summary>
        /// At least one fish is targeting this (but not hooked yet)
        /// </summary>
        public bool IsTargeted { set; private get; }

        private Camera _cam;

        private void Awake()
        {
            _hp = Random.Range(2, 5);
            _sr = GetComponent<SpriteRenderer>();
            _cam = Camera.main;
        }

        private void Update()
        {
            // If we are not hooked, the float follow the mouse
            if (_hooked == null && !IsTargeted)
            {
                var camBounds = CameraUtils.CalculateBounds(_cam);

                var pos = Input.touchCount > 0 ? Input.GetTouch(0).position : Mouse.current.position.ReadValue();
                var mousePos = _cam.ScreenToWorldPoint(pos);
                mousePos.z = 0f;

                // We keep the float inside the camera bounds
                if (mousePos.x < camBounds.min.x) mousePos.x = camBounds.min.x;
                else if (mousePos.x > camBounds.max.x) mousePos.x = camBounds.max.x;
                if (mousePos.y < camBounds.min.y) mousePos.y = camBounds.min.y;
                else if (mousePos.y > camBounds.max.y) mousePos.y = camBounds.max.y;

                transform.position = mousePos;
            }
        }

        public bool TakeDamage()
        {
            if (_hp == 0) return false;
            _hp--;
            StartCoroutine(DamageEffect());
            return _hp == 0;
        }

        private IEnumerator DamageEffect()
        {
            yield return new WaitForSeconds(.1f);
            _sr.color = new(1f, 1f, 1f, .5f);
            yield return new WaitForSeconds(.1f);
            if (_hp > 0)
            {
                _sr.color = Color.white;
            }
        }
    }
}
