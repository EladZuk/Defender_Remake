using UnityEngine;
using DefenderRemake.Data;

namespace DefenderRemake.Gameplay
{
    /// <summary>
    /// Attached to a Bot/Survivor GameObject.
    /// Detects player collision, increments the persistent survivor count,
    /// spawns the SAVED! visual effect, and destroys itself.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SurvivorPickup2D : MonoBehaviour
    {
        [Header("Data System")]
        [SerializeField, Tooltip("Reference to the GameSessionData asset in the project")]
        private GameSessionData sessionData;

        [Header("Effects")]
        [SerializeField, Tooltip("Prefab with SavedTextEffect to spawn on pickup")]
        private GameObject savedTextPrefab;

        [Header("Combat Buff")]
        [SerializeField, Tooltip("Duration of the fire-rate boost in seconds")]
        private float buffDuration = 5f;
        [SerializeField, Tooltip("Fire rate multiplier (e.g. 0.5 means twice as fast)")]
        private float fireRateMultiplier = 0.5f;

        private bool _isCollected = false;

        private void Awake()
        {
            // Foolproof: ensure the collider is a trigger so the player doesn't crash into it
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected) return;

            // Using CompareTag is zero-GC and fast
            if (other.CompareTag("Player"))
            {
                _isCollected = true;
                
                // Apply Combat Buff
                var weapon = other.GetComponent<DefenderRemake.Player.WeaponSystem2D>();
                if (weapon != null)
                {
                    weapon.ResetHeatAndBoost(buffDuration, fireRateMultiplier);
                }

                Collect();
            }
        }

        private void Collect()
        {
            // 1. Update persistent data
            if (sessionData != null)
            {
                sessionData.AddSurvivor();
            }
            else
            {
                Debug.LogWarning("SurvivorPickup2D: GameSessionData is not assigned!");
            }

            // 2. Spawn floating text effect
            if (savedTextPrefab != null)
            {
                // Spawn slightly above the bot
                Vector3 spawnPos = transform.position + Vector3.up * 0.75f;
                Instantiate(savedTextPrefab, spawnPos, Quaternion.identity);
            }

            // 3. Remove bot from scene
            Destroy(gameObject);
        }
    }
}
