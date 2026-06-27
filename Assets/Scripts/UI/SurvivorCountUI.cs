using UnityEngine;
using TMPro;
using DefenderRemake.Data;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Displays the current survivor count on the UI.
    /// Listens to the persistent GameSessionData asset.
    /// </summary>
    public class SurvivorCountUI : MonoBehaviour
    {
        [SerializeField, Tooltip("Reference to the GameSessionData asset in the project")]
        private GameSessionData sessionData;

        [SerializeField, Tooltip("The TextMeshPro element to update")]
        private TextMeshProUGUI countText;

        private void OnEnable()
        {
            if (sessionData != null)
            {
                // Subscribe to the data change event
                sessionData.OnSurvivorCountChanged += UpdateUI;
                
                // Force an initial update so the UI reflects the true state immediately
                UpdateUI(sessionData.SurvivorCount);
            }
            else
            {
                Debug.LogWarning("SurvivorCountUI: GameSessionData is not assigned!");
            }
        }

        private void OnDisable()
        {
            if (sessionData != null)
            {
                // Unsubscribe to prevent memory leaks when UI is disabled
                sessionData.OnSurvivorCountChanged -= UpdateUI;
            }
        }

        private void UpdateUI(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
