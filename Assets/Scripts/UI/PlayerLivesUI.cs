using UnityEngine;
using UnityEngine.UI;
using DefenderRemake.Player;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Displays player lives as a row of icons.
    /// Hides icons beyond the current life count.
    /// </summary>
    public class PlayerLivesUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField, Tooltip("PlayerHealth component on the Player")]
        private PlayerHealth playerHealth;

        [Header("Life Icons")]
        [SerializeField, Tooltip("Assign each life icon Image here (max 3 by default)")]
        private Image[] lifeIcons;

        private void Start()
        {
            if (playerHealth == null) return;

            // Subscribe to the lives changed event — no polling needed
            playerHealth.OnLivesChanged += UpdateIcons;

            // Initialise display
            UpdateIcons(playerHealth.CurrentLives);
        }

        private void OnDestroy()
        {
            // Always unsubscribe to prevent memory leaks
            if (playerHealth != null)
                playerHealth.OnLivesChanged -= UpdateIcons;
        }

        private void UpdateIcons(int currentLives)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                    lifeIcons[i].enabled = i < currentLives;
            }
        }
    }
}
