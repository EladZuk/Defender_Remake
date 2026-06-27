using UnityEngine;
using TMPro;

namespace DefenderRemake.Gameplay
{
    /// <summary>
    /// Spawns "SAVED!" text that floats upward, flickers in retro RGB colors,
    /// and destroys itself after a short lifetime.
    /// </summary>
    public class SavedTextEffect : MonoBehaviour
    {
        [SerializeField, Tooltip("The TextMeshPro component to animate")]
        private TextMeshPro textMesh;
        
        [SerializeField, Tooltip("How fast the text floats upward")]
        private float floatSpeed = 2f;
        
        [SerializeField, Tooltip("How long before the text destroys itself")]
        private float lifetime = 1f;
        
        [SerializeField, Tooltip("How fast the text cycles through colors")]
        private float flickerSpeed = 20f;

        // Retro RGB palette — constrained to classic arcade colors
        private static readonly Color[] RetroColors = new Color[]
        {
            new Color(0f, 1f, 0.88f), // Cyan
            new Color(1f, 0f, 1f),    // Magenta
            new Color(1f, 1f, 1f),    // White
            new Color(0f, 1f, 0.27f), // Green
            new Color(1f, 0.92f, 0f), // Yellow
        };

        private float _deathTime;

        private void Start()
        {
            if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
            _deathTime = Time.time + lifetime;
        }

        private void Update()
        {
            // Float upward
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Flicker color
            if (textMesh != null)
            {
                int colorIndex = Mathf.FloorToInt(Time.time * flickerSpeed) % RetroColors.Length;
                textMesh.color = RetroColors[colorIndex];
            }

            // Self-destruct
            if (Time.time >= _deathTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
