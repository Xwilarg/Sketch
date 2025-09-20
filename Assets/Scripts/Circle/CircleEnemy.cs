using Sketch.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.Circle
{
    public class CircleEnemy : MonoBehaviour, ITargetShape
    {
        [SerializeField]
        private int _maxHealth = 5;

        private int _health;

        public PolygonCollider2D Collider { private set; get; }

        public Vector2 Position => transform.position;

        public float Scale => transform.localScale.x;

        public void GetCircled()
        {
            _health--;

            if (_health == 0) EnemyManager.Instance.Remove(this);
            else transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, _health / (float)_maxHealth);
        }

        private void Awake()
        {
            Collider = GetComponent<PolygonCollider2D>();
            _health = _maxHealth;
        }
    }
}