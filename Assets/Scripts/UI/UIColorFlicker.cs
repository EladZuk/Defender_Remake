using UnityEngine;
using UnityEngine.UI;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Cycles a UI Image's color through a series of retro colors over time.
    /// Attach this to the Background grid image (requires a transparent background).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIColorFlicker : MonoBehaviour
    {
        [Header("Color Settings")]
        [SerializeField, Tooltip("Colors to cycle through")]
        private Color[] cycleColors = new Color[] 
        {
            new Color(0f, 1f, 1f), // Cyan
            new Color(1f, 0f, 1f), // Magenta
            new Color(0f, 0f, 1f), // Blue
            new Color(0f, 1f, 0f)  // Green
        };

        [SerializeField, Tooltip("How fast the colors cycle")]
        private float cycleSpeed = 1f;

        private Image _image;
        private float _t = 0f;
        private int _colorIndex = 0;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Update()
        {
            if (cycleColors == null || cycleColors.Length < 2) return;

            _t += Time.deltaTime * cycleSpeed;

            if (_t >= 1f)
            {
                _t = 0f;
                _colorIndex = (_colorIndex + 1) % cycleColors.Length;
            }

            int nextIndex = (_colorIndex + 1) % cycleColors.Length;
            _image.color = Color.Lerp(cycleColors[_colorIndex], cycleColors[nextIndex], _t);
        }
    }
}
