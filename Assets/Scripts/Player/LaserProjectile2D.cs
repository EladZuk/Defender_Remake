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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Fast moving projectile
        }

        public void SetPool(IObjectPool<LaserProjectile2D> pool)
        {
            _pool = pool;
        }

        public void Fire(Vector2 direction)
        {
            _rb.linearVelocity = direction.normalized * speed;
            StartCoroutine(LifeTimer());
        }

        private IEnumerator LifeTimer()
        {
            yield return new WaitForSeconds(lifeTime);
            ReleaseToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore the player
            if (other.CompareTag("Player")) return;

            // TODO: Deal damage if the other object is an enemy
            
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
