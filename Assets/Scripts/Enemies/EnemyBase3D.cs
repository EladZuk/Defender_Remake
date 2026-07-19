using UnityEngine;
using DefenderRemake.Systems;
using DefenderRemake.Gameplay;

namespace DefenderRemake.Enemies
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public abstract class EnemyBase3D : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] protected int maxHealth = 10;
        [SerializeField] protected int scoreValue = 100;

        [Header("Death Settings")]
        [SerializeField] protected GameObject explosionPrefab;
        [SerializeField] protected GameObject survivorDropPrefab;
        [SerializeField, Range(0f, 1f)] protected float dropChance = 0.15f;

        protected int currentHealth;
        protected Rigidbody rb;
        protected EnergyShield3D energyShield; // Optional, can be null

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            
            // Lock rotations on non-relevant axes so physics doesn't spin them wildly if hit
            // Wait, for 3D space flight, we usually lock constraints, but let's let the AI scripts handle torque
            
            energyShield = GetComponentInChildren<EnergyShield3D>(true);
            if (energyShield == null)
            {
                Debug.LogError($"[ENEMY SETUP ERROR] {gameObject.name} is missing the EnergyShield3D script in its hierarchy!");
            }
            
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(int amount, bool killedByBoss = false)
        {
            // 1. Shield Interception
            int damageToHull = amount;
            if (energyShield != null)
            {
                damageToHull = energyShield.AbsorbDamage(amount);
            }

            // 2. Hull Damage
            if (damageToHull > 0)
            {
                currentHealth -= damageToHull;
                
                if (currentHealth <= 0)
                {
                    Die();
                }
            }
        }

        protected virtual void Die()
        {
            // Visuals
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Score
            if (PersistentGameManager.Instance != null)
            {
                PersistentGameManager.Instance.AddScore(scoreValue);
            }

            // Drops
            if (survivorDropPrefab != null && Random.value <= dropChance)
            {
                Instantiate(survivorDropPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            HandleRamming(collision.gameObject);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            HandleRamming(other.gameObject);
        }

        protected virtual void HandleRamming(GameObject hitObject)
        {
            // If we physically ram the player
            if (hitObject.CompareTag("Player"))
            {
                var playerDamageable = hitObject.GetComponentInParent<IDamageable>();
                if (playerDamageable != null)
                {
                    // Deal massive ramming damage
                    playerDamageable.TakeDamage(25);
                }
            }
        }
    }
}
