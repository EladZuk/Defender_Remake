using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        [SerializeField, Tooltip("The background Image of the boost bar")]
        private Image backgroundImage;

        [SerializeField, Tooltip("The fill Image for the boost meter")]
        private Image fillImage;

        [SerializeField, Tooltip("The Text object for the Boost label (requires TextMeshPro)")]
        private TextMeshProUGUI boostText;

        [Header("Pulse Animation")]
        [SerializeField, Tooltip("How large the text scales up when boosted")]
        private float pulseScaleMultiplier = 1.5f;
        
        [SerializeField, Tooltip("How long the pulse lasts in seconds")]
        private float pulseDuration = 0.4f;

        [Header("Colors")]
        [SerializeField] private Color normalColor   = new Color(0f, 1f, 0.88f, 1f); // #00FFE0
        [SerializeField] private Color boostingColor = Color.white;
        [SerializeField] private Color lockedColor   = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color bgNormalColor = new Color(0.15f, 0.15f, 0.15f, 1f); // dark neutral
        [SerializeField] private Color bgLockedColor = new Color(0.6f, 0.05f, 0.05f, 1f);  // dark red

        private void Update()
        {
            if (boostSystem == null || fillImage == null) return;

            // Update fill amount (0–1)
            fillImage.fillAmount = boostSystem.CurrentMeter / 100f;

            // Update fill color based on state
            if (boostSystem.IsLockedOut)
            {
                fillImage.color = lockedColor;
                if (backgroundImage != null) backgroundImage.color = bgLockedColor;
            }
            else if (boostSystem.IsBoosting)
            {
                fillImage.color = boostingColor;
                if (backgroundImage != null) backgroundImage.color = bgNormalColor;
            }
            else
            {
                fillImage.color = normalColor;
                if (backgroundImage != null) backgroundImage.color = bgNormalColor;
            }
        }

        public void TriggerPulseAnimation()
        {
            if (boostText == null || !gameObject.activeInHierarchy) return;
            StopAllCoroutines();
            StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            Color originalColor = boostText.color;
            Vector3 originalScale = Vector3.one;
            
            boostText.color = normalColor; // Cyan
            boostText.transform.localScale = originalScale * pulseScaleMultiplier;
            
            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                
                boostText.color = Color.Lerp(normalColor, originalColor, t);
                boostText.transform.localScale = Vector3.Lerp(originalScale * pulseScaleMultiplier, originalScale, t);
                
                yield return null;
            }
            
            boostText.color = originalColor;
            boostText.transform.localScale = originalScale;
        }
    }
}
