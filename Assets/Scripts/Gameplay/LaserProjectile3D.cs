using UnityEngine;
using DefenderRemake.Systems;

namespace DefenderRemake.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class LaserProjectile3D : MonoBehaviour
    {
        [Header("Laser Stats")]
        [SerializeField, Tooltip("Flight speed of the laser")] 
        private float speed = 800f;
        [SerializeField, Tooltip("Damage dealt by this laser")] 
        private int baseDamage = 1;

        [Header("Projectile Settings")]
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private GameObject hitEffectPrefab;
        
        [Header("Faction Settings")]
        [SerializeField] private bool isPlayerLaser = true;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            // It's a fast laser, we should use Continuous Dynamic collision to prevent passing through objects
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Start()
        {
            // Destroy after lifetime to prevent memory leaks if it flies into deep space
            Destroy(gameObject, lifetime);
        }

        private int _damage = 1;

        public void Fire(Vector3 direction, int overrideDamage = -1)
        {
            _rb = GetComponent<Rigidbody>();
            
            _damage = (overrideDamage > 0) ? overrideDamage : baseDamage;

            _rb.linearVelocity = direction * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleHit(collision.collider, collision.contacts[0].point, collision.contacts[0].normal);
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other, transform.position, -transform.forward);
        }

        private void HandleHit(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            }

            IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
            
            if (damageable != null)
            {
                bool hitPlayer = hitCollider.GetComponentInParent<DefenderRemake.Player.PlayerController3D>() != null;

                // Prevent Friendly Fire
                if (isPlayerLaser && hitPlayer) return;
                if (!isPlayerLaser && !hitPlayer) return;

                damageable.TakeDamage(_damage);
            }

            Destroy(gameObject);
        }
    }
}
