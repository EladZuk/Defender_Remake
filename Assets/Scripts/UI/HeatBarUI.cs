using UnityEngine;
using UnityEngine.UI;
using DefenderRemake.Player;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Drives the overheat bar Image fill and color.
    /// Safe to add now — null-checks WeaponSystem2D reference.
    /// Wire up the reference when the weapon branch is merged.
    /// Color states:
    ///   Cool       : coolColor  (cyan/green)
    ///   Heating up : warmColor  (orange)
    ///   Overheated : hotColor   (red)
    /// </summary>
    public class HeatBarUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField, Tooltip("The WeaponSystem2D on the Player — assign when weapon branch is merged")]
        private WeaponSystem2D weaponSystem;

        [Header("Bar References")]
        [SerializeField, Tooltip("The fill Image for the heat meter")]
        private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color coolColor      = new Color(0f, 1f, 0.88f, 1f); // #00FFE0
        [SerializeField] private Color warmColor      = new Color(1f, 0.5f, 0f, 1f);  // orange
        [SerializeField] private Color hotColor       = new Color(1f, 0.15f, 0.15f, 1f); // red

        [SerializeField, Tooltip("Heat threshold above which color starts shifting to warm")]
        private float warmThreshold = 40f;

        [SerializeField, Tooltip("Heat threshold above which color becomes fully red")]
        private float hotThreshold = 80f;

        private void Update()
        {
            // Null-safe: does nothing until WeaponSystem2D is assigned
            if (weaponSystem == null || fillImage == null) return;

            float heat = weaponSystem.CurrentHeat;

            // Update fill amount (0–1)
            fillImage.fillAmount = heat / 100f;

            // Lerp color based on heat level
            if (heat >= hotThreshold || weaponSystem.IsOverheated)
                fillImage.color = hotColor;
            else if (heat >= warmThreshold)
                fillImage.color = Color.Lerp(coolColor, warmColor, (heat - warmThreshold) / (hotThreshold - warmThreshold));
            else
                fillImage.color = coolColor;
        }
    }
}
