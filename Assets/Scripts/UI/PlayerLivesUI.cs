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
        [SerializeField, Tooltip("Persistent session data tracking lives across scenes")]
        private DefenderRemake.Data.GameSessionData sessionData;

        [Header("Life Icons")]
        [SerializeField, Tooltip("Assign each life icon Image here (max 3 by default)")]
        private Image[] lifeIcons;

        [Header("Visuals")]
        [SerializeField, Tooltip("Color of a lost life")]
        private Color deadColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        [SerializeField, Tooltip("Color of an active life")]
        private Color aliveColor = Color.white;

        private void Start()
        {
            if (sessionData == null)
            {
                Debug.LogWarning("PlayerLivesUI is missing GameSessionData reference!");
                return;
            }

            sessionData.OnLivesChanged += UpdateIcons;
            UpdateIcons(sessionData.Lives);
        }

        private void OnDestroy()
        {
            if (sessionData != null)
                sessionData.OnLivesChanged -= UpdateIcons;
        }

        private void UpdateIcons(int currentLives)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].color = (i < currentLives) ? aliveColor : deadColor;
                }
            }
        }
    }
}
