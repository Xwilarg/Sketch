using System.Collections;
using UnityEngine;

namespace Sketch.Fishing
{
    public class FishController : MonoBehaviour
    {
        private Rigidbody2D _rb;

        private Vector2 _aimPosition;

        private HookController _target;

        private float _attackTimer; // Time between 2 attacks
        private float AttackTimerRef => Random.Range(1f, 2f);

        private float _attackDurationRef;
        private float _attackDurationTimer = -1f;

        private float _moveBackTimer = -1f;
        private const float _moveBackTimerRef = .2f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.velocity = transform.right * .4f;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("FISHING_Float"))
            {
                _rb.velocity = Vector2.zero;
                var dir = collision.transform.position - transform.position;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                _aimPosition = transform.position;

                _target = collision.GetComponent<HookController>();
                _attackTimer = AttackTimerRef;
            }
        }

        private IEnumerator BitAndWait()
        {
            yield return new WaitForSeconds(2f);
            _target.Hooked = null;
            _target = null;
            _rb.velocity = transform.right * .6f;
        }

        private void Update()
        {
            if (_target != null)
            {
                if (_target.Hooked != null && _target.Hooked != this)
                {
                    _target = null;
                    _rb.velocity = transform.right * .6f;
                }
                else if (_moveBackTimer > 0f) // Going back from bait to initial position
                {
                    _moveBackTimer -= Time.deltaTime;
                    if (_moveBackTimer <= 0f)
                    {
                        _attackTimer = AttackTimerRef;
                    }
                    transform.position = Vector2.Lerp(_aimPosition, _target.transform.position, Mathf.Clamp01(_moveBackTimer / _moveBackTimerRef));
                }
                else if (_attackTimer > 0f) // Waiting to attack
                {
                    _attackTimer -= Time.deltaTime;
                    if (_attackTimer <= 0f)
                    {
                        _attackDurationRef = Random.Range(.25f, .5f);
                        _attackDurationTimer = _attackDurationRef;
                    }
                }
                else if (_attackDurationTimer > 0f) // Going to bait
                {
                    _attackDurationTimer -= Time.deltaTime;
                    if (_attackDurationTimer <= 0f)
                    {
                        if (_target.TakeDamage())
                        {
                            _target.Hooked = this;
                            StartCoroutine(BitAndWait());
                        }
                        else
                        {
                            _moveBackTimer = _moveBackTimerRef;
                        }
                    }
                    transform.position = Vector2.Lerp(_target.transform.position, _aimPosition, Mathf.Clamp01(_attackDurationTimer / _attackDurationRef));
                }
            }

            if (Mathf.Max(Mathf.Abs(transform.position.x), Mathf.Abs(transform.position.y)) > 10f)
            {
                Destroy(gameObject); // TODO: Handle that properly
            }
        }
    }
}
