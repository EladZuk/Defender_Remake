using UnityEngine;
using TMPro;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Sweeps a wave of retro RGB colors across individual characters in a TextMeshProUGUI element.
    /// Attach this directly to the Title text object.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextFlicker : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField, Tooltip("Colors to cycle through")]
        private Color[] cycleColors = new Color[] 
        {
            new Color(1f, 0f, 1f), // Magenta
            new Color(0f, 1f, 1f), // Cyan
            new Color(1f, 1f, 0f), // Yellow
            new Color(1f, 0f, 0f)  // Red
        };

        [SerializeField, Tooltip("How fast the colors wave across the text")]
        private float waveSpeed = 2f;
        
        [SerializeField, Tooltip("The offset between each character (controls how wide the wave is)")]
        private float characterOffset = 0.5f;

        private TextMeshProUGUI _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (cycleColors == null || cycleColors.Length < 2) return;

            // Force an update to the mesh so we can read and modify the latest character data
            _textMesh.ForceMeshUpdate();
            TMP_TextInfo textInfo = _textMesh.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                // Skip spaces or invisible characters
                if (!textInfo.characterInfo[i].isVisible) continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                Color32[] newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Calculate color for this specific character based on time and index
                float t = (Time.unscaledTime * waveSpeed) + (i * characterOffset);
                float lerpVal = t % cycleColors.Length;
                
                int colorIndex1 = Mathf.FloorToInt(lerpVal);
                int colorIndex2 = (colorIndex1 + 1) % cycleColors.Length;
                float fraction = lerpVal - colorIndex1;

                Color charColor = Color.Lerp(cycleColors[colorIndex1], cycleColors[colorIndex2], fraction);
                Color32 color32 = charColor;

                // Set color for all 4 vertices of the character
                newVertexColors[vertexIndex + 0] = color32;
                newVertexColors[vertexIndex + 1] = color32;
                newVertexColors[vertexIndex + 2] = color32;
                newVertexColors[vertexIndex + 3] = color32;
            }

            // Push the modified vertex colors back to the TextMeshPro component
            _textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
