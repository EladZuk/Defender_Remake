using UnityEngine;
using UnityEngine.UI;
using DefenderRemake.Player;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Drives the boost bar Image fill and color.
    /// Color states:
    ///   Normal (refilling) : cyanColor
    ///   Boosting           : white (bright burst)
    ///   Locked out         : lockedOutColor (red flash)
    /// </summary>
    public class BoostBarUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField, Tooltip("The BoostSystem on the Player")]
        private BoostSystem boostSystem;

        [Header("Bar References")]
        [SerializeField, Tooltip("The fill Image for the boost meter")]
        private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor   = new Color(0f, 1f, 0.88f, 1f); // #00FFE0
        [SerializeField] private Color boostingColor = Color.white;
        [SerializeField] private Color lockedColor   = new Color(1f, 0.2f, 0.2f, 1f);

        private void Update()
        {
            if (boostSystem == null || fillImage == null) return;

            // Update fill amount (0–1)
            fillImage.fillAmount = boostSystem.CurrentMeter / 100f;

            // Update color based on state
            if (boostSystem.IsLockedOut)
                fillImage.color = lockedColor;
            else if (boostSystem.IsBoosting)
                fillImage.color = boostingColor;
            else
                fillImage.color = normalColor;
        }
    }
}
