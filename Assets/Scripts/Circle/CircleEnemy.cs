using UnityEngine;

namespace Sketch.Circle
{
    public class CircleEnemy : MonoBehaviour
    {
        [SerializeField]
        private int _maxHealth = 5;

        private int _health;
        public int Health
        {
            set
            {
                _health = value;
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, value / (float)_maxHealth);
            }
            get => _health;
        }

        public PolygonCollider2D Collider { private set; get; }

        private void Awake()
        {
            Collider = GetComponent<PolygonCollider2D>();
            Health = _maxHealth;
        }
    }
}