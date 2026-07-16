using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Drives the overheat bar Image fill and color.
    /// Color states:
    ///   Cool       : cyanColor
    ///   Heating up : orange
    ///   Overheated : red
    /// </summary>
    public class HeatBarUI : MonoBehaviour
    {
        [Header("Bar References")]
        [SerializeField, Tooltip("The background Image of the heat bar")]
        private Image backgroundImage;

        [SerializeField, Tooltip("The fill Image for the heat meter")]
        private Image fillImage;

        [SerializeField, Tooltip("The Text object for the Heat label (requires TextMeshPro)")]
        private TextMeshProUGUI heatText;

        [Header("Pulse Animation")]
        [SerializeField, Tooltip("How large the text scales up when boosted")]
        private float pulseScaleMultiplier = 1.5f;
        
        [SerializeField, Tooltip("How long the pulse lasts in seconds")]
        private float pulseDuration = 0.4f;

        [Header("Colors")]
        [SerializeField] private Color coolColor = new Color(0f, 1f, 0.88f, 1f); // #00FFE0
        [SerializeField] private Color warmColor = new Color(1f, 0.5f, 0f, 1f);  // orange
        [SerializeField] private Color hotColor  = new Color(1f, 0.15f, 0.15f, 1f); // red
        [SerializeField] private Color bgNormalColor = new Color(0.15f, 0.15f, 0.15f, 1f); // dark neutral
        [SerializeField] private Color bgHotColor    = new Color(0.6f, 0.05f, 0.05f, 1f);  // dark red

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
            {
                fillImage.color = hotColor;
                if (backgroundImage != null) backgroundImage.color = bgHotColor;
            }
            else if (heat >= warmThreshold)
            {
                fillImage.color = Color.Lerp(coolColor, warmColor, (heat - warmThreshold) / (hotThreshold - warmThreshold));
                if (backgroundImage != null) backgroundImage.color = bgNormalColor;
            }
            else
            {
                fillImage.color = coolColor;
                if (backgroundImage != null) backgroundImage.color = bgNormalColor;
            }
        }

        public void TriggerPulseAnimation()
        {
            if (heatText == null || !gameObject.activeInHierarchy) return;
            StopAllCoroutines();
            StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            Color originalColor = heatText.color;
            Vector3 originalScale = Vector3.one; // Assume default scale is 1
            
            // Pop up
            heatText.color = coolColor;
            heatText.transform.localScale = originalScale * pulseScaleMultiplier;
            
            // Shrink back
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                
                heatText.color = Color.Lerp(coolColor, originalColor, t);
                heatText.transform.localScale = Vector3.Lerp(originalScale * pulseScaleMultiplier, originalScale, t);
                
                yield return null;
            }
            
            heatText.color = originalColor;
            heatText.transform.localScale = originalScale;
        }
    }
}
