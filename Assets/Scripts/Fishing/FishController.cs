using UnityEngine;

namespace Sketch.Fishing
{
    public class FishController : MonoBehaviour
    {
        public CatchMinigame Minigame { private get; set; }
        public FishInfo Info { get; set; }
        public float Size { get; set; }

        private Rigidbody2D _rb;

        private Vector2 _aimPosition; // Where the fish is at when looking at the bait
        private Vector2 _targetPosition; // Target position including an offset

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

                var targetDir = (_aimPosition - (Vector2)_target.transform.position).normalized;
                _targetPosition += targetDir / 8f;

                _attackTimer = AttackTimerRef;
            }
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
                    transform.position = Vector2.Lerp(_aimPosition, _targetPosition, Mathf.Clamp01(_moveBackTimer / _moveBackTimerRef));
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
                            Minigame.OnDone = (bool status) =>
                            {
                                Minigame.gameObject.SetActive(false);
                                _target.Hooked = null;
                                if (status)
                                {
                                    FishSpawner.Instance.StartCoroutine(FishSpawner.Instance.Congrats(this));
                                    Destroy(gameObject);
                                }
                                else
                                {
                                    _target = null;
                                    _rb.velocity = transform.right * .6f;
                                }

                            };
                            Minigame.gameObject.SetActive(true);
                        }
                        else
                        {
                            _moveBackTimer = _moveBackTimerRef;
                        }
                    }
                    transform.position = Vector2.Lerp(_targetPosition, _aimPosition, Mathf.Clamp01(_attackDurationTimer / _attackDurationRef));
                }
            }

            if (Mathf.Max(Mathf.Abs(transform.position.x), Mathf.Abs(transform.position.y)) > 10f)
            {
                Destroy(gameObject); // TODO: Handle that properly
            }
        }
    }
}
