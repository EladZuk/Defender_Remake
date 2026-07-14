using UnityEngine;
using DefenderRemake.Systems;
using System.Collections;

namespace DefenderRemake.Enemies
{
    /// <summary>
    /// A simple projectile fired by enemies. 
    /// Travels in a straight line and kills the player on impact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyProjectile2D : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 4f;
        [SerializeField, Tooltip("Set this to your Terrain/Ground layer so lasers stop on cover")]
        private LayerMask terrainLayer;

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private bool _isBossLaser = false;
        private Transform _homingTarget;
        private float _homingSpeed;

        private void Awake()
        {
            EnsureComponents();
            _rb.gravityScale = 0f;
        }

        private void Update()
        {
            // Homing logic — only active for kill-shot lasers
            if (_homingTarget == null || !_homingTarget.gameObject.activeInHierarchy) return;

            Vector2 dir = ((Vector2)_homingTarget.position - _rb.position).normalized;
            _rb.linearVelocity = dir * _homingSpeed;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void EnsureComponents()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();

            // FORCE setup to fix common Unity prefab issues:
            if (_rb != null) 
            {
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            var col = GetComponent<Collider2D>();
            if (col != null) 
            {
                col.isTrigger = true; // Required for OnTriggerEnter2D to work!
            }
            
            if (_sr != null) 
            {
                _sr.sortingOrder = 50; // Guarantee it renders on top of the background
            }
            
            // Force it onto the 2D plane so the camera sees it and physics register
            Vector3 pos = transform.position;
            pos.z = 0f;
            transform.position = pos;
        }

        public void Fire(Vector2 direction, bool isBossLaser = false)
        {
            EnsureComponents();

            _isBossLaser = isBossLaser;
            
            Vector2 dir = direction.normalized;
            _rb.linearVelocity = dir * speed;

            // Rotate the whole transform to face the fire direction
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Destroy(gameObject, lifeTime); // Auto-cleanup
        }

        /// <summary>
        /// Fires a blazing fast homing kill-shot during the boss rage phase.
        /// Always carries the boss-kill flag to trigger the 3D transition on hit.
        /// </summary>
        public void FireKillShot(Vector2 direction, float speedMultiplier, Transform target)
        {
            EnsureComponents();

            _isBossLaser = true;
            _homingTarget = target;
            _homingSpeed = speed * speedMultiplier;

            Vector2 dir = direction.normalized;
            _rb.linearVelocity = dir * _homingSpeed;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Always ignore other enemies
            if (other.GetComponent<EnemyBase2D>() != null) return;

            // If it hits the player, deal damage then vanish
            if (other.CompareTag("Player"))
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                    damageable.TakeDamage(1, _isBossLaser);

                Destroy(gameObject);
                return;
            }

            // If it hits terrain/cover, stop here — gives the player a hiding spot
            if (((1 << other.gameObject.layer) & terrainLayer) != 0)
            {
                Destroy(gameObject);
                return;
            }

            // Everything else (bomb AoE, survivors, pickups) — pass straight through
        }
    }
}
