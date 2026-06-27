using UnityEngine;
using UnityEngine.UI;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Drives the overheat bar Image fill and color.
    /// TODO: Wire up to WeaponSystem2D when the weapon branch is merged.
    /// Color states:
    ///   Cool       : cyanColor
    ///   Heating up : orange
    ///   Overheated : red
    /// </summary>
    public class HeatBarUI : MonoBehaviour
    {
        [Header("Bar References")]
        [SerializeField, Tooltip("The fill Image for the heat meter")]
        private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color coolColor = new Color(0f, 1f, 0.88f, 1f); // #00FFE0
        [SerializeField] private Color warmColor = new Color(1f, 0.5f, 0f, 1f);  // orange
        [SerializeField] private Color hotColor  = new Color(1f, 0.15f, 0.15f, 1f); // red

        [SerializeField, Tooltip("Heat threshold (0-100) above which color shifts to warm")]
        private float warmThreshold = 40f;

        [SerializeField, Tooltip("Heat threshold (0-100) above which color becomes fully red")]
        private float hotThreshold = 80f;

        // Called by WeaponSystem2D when it is available (weapon branch)
        public void SetHeat(float heat, bool isOverheated)
        {
            if (fillImage == null) return;

            fillImage.fillAmount = heat / 100f;

            if (isOverheated || heat >= hotThreshold)
                fillImage.color = hotColor;
            else if (heat >= warmThreshold)
                fillImage.color = Color.Lerp(coolColor, warmColor, (heat - warmThreshold) / (hotThreshold - warmThreshold));
            else
                fillImage.color = coolColor;
        }
    }
}
