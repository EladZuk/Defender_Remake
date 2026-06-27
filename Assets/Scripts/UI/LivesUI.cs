using UnityEngine;
using UnityEngine.UI;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Displays player lives as individual icon images in the HUD.
    /// Attach to a UI GameObject. Drag in the LifeIcon prefab and a 
    /// Horizontal Layout Group container.
    /// </summary>
    public class LivesUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("The PlayerHealth script on the Player")]
        private PlayerHealth playerHealth;

        [SerializeField, Tooltip("Prefab for a single life icon (Image)")]
        private GameObject lifeIconPrefab;

        [SerializeField, Tooltip("Container with Horizontal Layout Group for icons")]
        private Transform iconsContainer;

        private void Start()
        {
            if (playerHealth == null)
            {
                Debug.LogWarning("[LivesUI] No PlayerHealth assigned.");
                return;
            }

            // Subscribe to the event — only runs when lives actually change
            playerHealth.OnLivesChanged += RefreshIcons;

            // Draw initial state
            RefreshIcons(playerHealth.CurrentLives);
        }

        private void OnDestroy()
        {
            // Always unsubscribe to prevent memory leaks
            if (playerHealth != null)
                playerHealth.OnLivesChanged -= RefreshIcons;
        }

        private void RefreshIcons(int currentLives)
        {
            if (iconsContainer == null || lifeIconPrefab == null) return;

            // Clear existing icons
            foreach (Transform child in iconsContainer)
                Destroy(child.gameObject);

            // Spawn one icon per remaining life
            for (int i = 0; i < currentLives; i++)
                Instantiate(lifeIconPrefab, iconsContainer);
        }
    }
}
