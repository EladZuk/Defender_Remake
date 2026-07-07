using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Adds retro polish to UI Buttons:
    /// - Changes the button's TEXT color on hover.
    /// - Squashes visually on click.
    /// - Plays an audio clip on click.
    /// Attach this alongside a Unity UI Button component.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RetroButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Header("Hover Color Settings")]
        [SerializeField, Tooltip("The Text object inside this button")]
        private TextMeshProUGUI buttonText;
        
        [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Light grey text by default
        [SerializeField] private Color hoverColor = Color.white;

        [Header("Click Settings")]
        [SerializeField, Tooltip("How much to shrink the button when clicked")]
        private float clickScaleMultiplier = 0.9f;
        
        [SerializeField, Tooltip("Audio clip to play when clicked")]
        private AudioClip clickSound;
        
        [SerializeField, Tooltip("Local AudioSource to play the click (leave empty to auto-add one)")]
        private AudioSource audioSource;

        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;

            // Ensure we have an AudioSource to play the clip
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            // Set initial text color
            if (buttonText != null)
            {
                buttonText.color = normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buttonText != null)
            {
                buttonText.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (buttonText != null)
            {
                buttonText.color = normalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Visual Squish
            StartCoroutine(SquashRoutine());

            // Audio Cue
            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }
        }

        private IEnumerator SquashRoutine()
        {
            transform.localScale = _originalScale * clickScaleMultiplier;
            yield return new WaitForSecondsRealtime(0.1f);
            transform.localScale = _originalScale;
        }
    }
}
