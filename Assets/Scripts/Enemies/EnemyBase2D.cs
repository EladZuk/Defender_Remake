using UnityEngine;
using DefenderRemake.Systems;

namespace DefenderRemake.Enemies
{
    /// <summary>
    /// Base class for all 2D enemies. Handles 1-hit death, physics setup, 
    /// and killing the player on physical collision.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public abstract class EnemyBase2D : MonoBehaviour, IDamageable
    {
        [Header("Level Bounds")]
        [SerializeField, Tooltip("Lowest point the enemy can fly")]
        protected float minY = -8f;
        
        [SerializeField, Tooltip("Highest point the enemy can fly")]
        protected float maxY = 8f;

        [Header("Base Death Effects")]
        [SerializeField, Tooltip("VFX prefab spawned when destroyed")]
        protected GameObject explosionPrefab;
        
        [SerializeField] 
        protected int scoreValue = 100;

        protected Rigidbody2D rb;
        protected SpriteRenderer sr;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
            
            // Standardize 2D space physics for enemies
            rb.gravityScale = 0f; 
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        protected virtual void Update()
        {
            HandleSpriteFlip();
            EnforceBounds();
        }

        protected virtual void EnforceBounds()
        {
            Vector3 pos = transform.position;
            if (pos.y > maxY)
            {
                pos.y = maxY;
                if (rb.linearVelocity.y > 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
            else if (pos.y < minY)
            {
                pos.y = minY;
                if (rb.linearVelocity.y < 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
            transform.position = pos;
        }

        protected virtual void HandleSpriteFlip()
        {
            if (sr == null || Time.timeScale == 0f) return;

            // Flip the sprite based on horizontal velocity
            if (rb.linearVelocity.x > 0.1f)
            {
                // Moving right
                sr.flipX = true; // Assuming default sprite faces left
            }
            else if (rb.linearVelocity.x < -0.1f)
            {
                // Moving left
                sr.flipX = false;
            }
        }

        /// <summary>
        /// IDamageable implementation. 2D enemies are fragile (1-hit kill).
        /// </summary>
        public virtual void TakeDamage(int amount, bool killedByBoss = false)
        {
            Die();
        }

        protected virtual void Die()
        {
            // Release any captured survivors back into the wild
            var captured = GetComponentsInChildren<DefenderRemake.Gameplay.SurvivorPickup2D>();
            foreach (var survivor in captured)
            {
                survivor.Release();
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // TODO: Notify a GameStateManager/ScoreManager to add scoreValue

            Destroy(gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // If the enemy physically crashes into the player, destroy both
            if (other.CompareTag("Player"))
            {
                var playerHealth = other.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    // Pass true if this is the Boss hitting the player
                    playerHealth.TakeDamage(1, this is BossController2D); 
                }
                
                // Kamikaze/Shooter destroy themselves when ramming, Boss does not
                if (!(this is BossController2D))
                {
                    Die();
                }
            }
        }
    }
}
