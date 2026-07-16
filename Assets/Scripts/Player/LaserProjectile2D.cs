using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace DefenderRemake.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class LaserProjectile2D : MonoBehaviour
    {
        [Header("Projectile Dynamics")]
        [SerializeField] private float speed = 30f;
        [SerializeField] private float lifeTime = 3f;

        private IObjectPool<LaserProjectile2D> _pool;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _sr = GetComponent<SpriteRenderer>();
        }

        public void SetPool(IObjectPool<LaserProjectile2D> pool)
        {
            _pool = pool;
        }

        public void Fire(Vector2 direction)
        {
            _rb.linearVelocity = direction.normalized * speed;

            // Flip sprite to match travel direction
            if (_sr != null)
                _sr.flipX = direction.x < 0f;

            StartCoroutine(LifeTimer());
        }

        private IEnumerator LifeTimer()
        {
            yield return new WaitForSeconds(lifeTime);
            ReleaseToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore the player's own collider
            if (other.CompareTag("Player")) return;

            // Ignore survivors floating in space
            if (other.GetComponent<DefenderRemake.Gameplay.SurvivorPickup2D>() != null) return;

            // Check if the object we hit can take damage
            var damageable = other.GetComponent<DefenderRemake.Systems.IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(1);
            }
            
            // Instantly destroy (pool) the laser upon hitting anything that isn't the player
            ReleaseToPool();
        }

        private void ReleaseToPool()
        {
            if (gameObject.activeInHierarchy && _pool != null)
            {
                _pool.Release(this);
            }
        }
    }
}
