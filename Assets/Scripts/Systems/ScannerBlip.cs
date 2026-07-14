using UnityEngine;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Attaching this to an Enemy or the Player will automatically generate a 
    /// giant colored dot on the "ScannerOnly" layer so it appears clearly on the radar.
    /// </summary>
    public class ScannerBlip : MonoBehaviour
    {
        [Header("Blip Settings")]
        [SerializeField, Tooltip("Color of the dot on the radar")]
        private Color blipColor = Color.red;
        
        [SerializeField, Tooltip("How large the blip is on the radar map")]
        private float blipScale = 6f;
        
        [SerializeField, Tooltip("Assign a basic circle or square sprite here (like a default unity knob)")]
        private Sprite blipSprite;

        private void Start()
        {
            // Dynamically create the blip graphic so we don't clutter the prefabs
            GameObject blipObj = new GameObject("RadarBlipGraphic");
            blipObj.transform.SetParent(transform);
            blipObj.transform.localPosition = Vector3.zero;
            blipObj.transform.localScale = new Vector3(blipScale, blipScale, 1f);

            int layer = LayerMask.NameToLayer("ScannerOnly");
            if (layer > -1)
            {
                blipObj.layer = layer;
            }
            else
            {
                Debug.LogWarning("ScannerBlip: You need to create a 'ScannerOnly' layer in Unity Tags & Layers!");
            }

            var sr = blipObj.AddComponent<SpriteRenderer>();
            sr.sprite = blipSprite;
            sr.color = blipColor;
            sr.sortingOrder = 100; // Render on top of the map
        }
    }
}
