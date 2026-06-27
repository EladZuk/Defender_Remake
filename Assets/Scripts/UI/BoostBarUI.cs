using UnityEngine;
using UnityEngine.UI;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Displays the Boost meter as a fill bar with color states.
    /// Color shifts: Cyan (full) -> White (boosting) -> Red flash (depleted/locked).
    /// Attach to a UI GameObject alongside a Slider or Image (filled).
    /// </summary>
    public class BoostBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("The BoostSystem on the Player")]
        private BoostSystem boostSystem;

        [SerializeField, Tooltip("The fill Image of the boost bar")]
        private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color fullColor = new Color(0f, 1f, 0.878f); // Neon cyan #00FFE0
        [SerializeField] private Color boostingColor = Color.white;
        [SerializeField] private Color lockedColor = Color.red;

        private void Update()
        {
            if (boostSystem == null || fillImage == null) return;

            // Update fill amount (0 to 1)
            fillImage.fillAmount = boostSystem.CurrentMeter / 100f;

            // Update color based on state
            if (boostSystem.IsLockedOut)
                fillImage.color = lockedColor;
            else if (boostSystem.IsBoosting)
                fillImage.color = boostingColor;
            else
                fillImage.color = fullColor;
        }
    }
}
