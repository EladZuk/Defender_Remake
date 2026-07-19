using UnityEngine;
using DefenderRemake.Systems;

namespace DefenderRemake.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class LaserProjectile3D : MonoBehaviour
    {
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

        public void Fire(Vector3 direction)
        {
            // Pull the exact speed from our centralized GameManager!
            float speed = 800f; 
            if (PersistentGameManager.Instance != null)
            {
                speed = isPlayerLaser ? PersistentGameManager.Instance.playerLaserSpeed : PersistentGameManager.Instance.enemyLaserSpeed;
            }

            _rb.linearVelocity = direction.normalized * speed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Optional: Spawn a particle hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }

            // TODO: Deal damage based on PersistentGameManager damage stats!

            // Destroy the laser itself
            Destroy(gameObject);
        }
    }
}
