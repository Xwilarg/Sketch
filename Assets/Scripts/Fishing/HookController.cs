using System.Collections;
using UnityEngine;

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
                _hooked = value;
                if (value == null)
                {
                    _sr.color = Color.white;
                    _hp = Random.Range(2, 5);
                }
            }
            get => _hooked;
        }

        private void Awake()
        {
            _hp = Random.Range(2, 5);
            _sr = GetComponent<SpriteRenderer>();
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
