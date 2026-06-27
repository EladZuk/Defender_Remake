using UnityEngine;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Moves a UI element (the white view bracket) horizontally across the scanner bar
    /// to indicate the Main Camera's current view over the level.
    /// </summary>
    public class ScannerViewIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("The main gameplay camera (defaults to Camera.main)")]
        private Camera mainCamera;
        
        [SerializeField, Tooltip("The RectTransform of the scanner map (the RawImage)")]
        private RectTransform scannerMapRect;

        [Header("Scanner Settings")]
        [SerializeField, Tooltip("Must match Scanner Display Half Width on the ScannerCamera!")]
        private float scannerDisplayHalfWidth = 100f;

        private RectTransform _indicatorRect;

        private void Awake()
        {
            _indicatorRect = GetComponent<RectTransform>();
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            if (mainCamera == null || scannerMapRect == null) return;

            // 1. Get the camera's X position in the world (-levelHalfWidth to +levelHalfWidth)
            float camWorldX = mainCamera.transform.position.x;

            // 2. Normalize it to 0-1 range (0 = left edge, 1 = right edge)
            // InverseLerp automatically clamps, so if camera goes past the display width, the box stops at the edge
            float normalizedX = Mathf.InverseLerp(-scannerDisplayHalfWidth, scannerDisplayHalfWidth, camWorldX);

            // 3. Map the normalized 0-1 value to the pixel width of the scanner UI bar
            float mapWidth = scannerMapRect.rect.width;
            
            // This assumes the indicator UI element is anchored to the CENTER of the scannerMapRect (0.5, 0.5)
            float mappedX = Mathf.Lerp(-mapWidth / 2f, mapWidth / 2f, normalizedX);

            _indicatorRect.anchoredPosition = new Vector2(mappedX, _indicatorRect.anchoredPosition.y);
        }
    }
}
