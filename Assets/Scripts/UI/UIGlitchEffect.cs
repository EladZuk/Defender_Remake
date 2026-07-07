using UnityEngine;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Randomly glitches the horizontal position of a UI element to create a CRT stutter effect.
    /// Attach this to your Background Grid image.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIGlitchEffect : MonoBehaviour
    {
        [Header("Glitch Settings")]
        [SerializeField, Tooltip("Chance (0 to 1) per frame to glitch horizontally")]
        private float glitchChance = 0.05f;
        
        [SerializeField, Tooltip("How far it can jump horizontally in pixels")]
        private float glitchIntensity = 15f;

        private RectTransform _rectTransform;
        private Vector2 _originalPos;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPos = _rectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (Random.value < glitchChance)
            {
                float randomOffset = Random.Range(-glitchIntensity, glitchIntensity);
                _rectTransform.anchoredPosition = new Vector2(_originalPos.x + randomOffset, _originalPos.y);
            }
            else
            {
                // Snap back to perfectly centered/original position immediately
                _rectTransform.anchoredPosition = _originalPos;
            }
        }
    }
}
