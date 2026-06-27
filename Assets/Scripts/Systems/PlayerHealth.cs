using UnityEngine;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Manages player lives. Attach to the Player.
    /// Other scripts call TakeDamage() to decrement lives.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Lives Settings")]
        [SerializeField, Tooltip("Number of lives the player starts with")]
        private int startingLives = 3;

        [SerializeField, Tooltip("Duration of invincibility after taking a hit (seconds)")]
        private float invincibilityDuration = 2f;

        public int CurrentLives { get; private set; }
        public bool IsInvincible { get; private set; }

        // Event so UI and other systems can react without polling
        public System.Action<int> OnLivesChanged;
        public System.Action OnPlayerDied;

        private void Awake()
        {
            CurrentLives = startingLives;
        }

        public void TakeDamage()
        {
            if (IsInvincible) return;

            CurrentLives--;
            OnLivesChanged?.Invoke(CurrentLives);

            if (CurrentLives <= 0)
            {
                OnPlayerDied?.Invoke();
                // TODO: Trigger game over / boss phase logic via GameStateManager
                gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(InvincibilityRoutine());
            }
        }

        private System.Collections.IEnumerator InvincibilityRoutine()
        {
            IsInvincible = true;

            // Flash the sprite to signal invincibility
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            float elapsed = 0f;
            while (elapsed < invincibilityDuration)
            {
                if (sr != null) sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (sr != null) sr.enabled = true;
            IsInvincible = false;
        }
    }
}
