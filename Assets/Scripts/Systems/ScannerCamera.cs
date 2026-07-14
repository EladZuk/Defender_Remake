using UnityEngine;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Configures the Scanner Camera for the Defender-style HUD bar.
    /// Attach this to a second Camera in the scene.
    /// The camera renders the full level width into a RenderTexture
    /// which is displayed in a RawImage UI element in the HUD bar.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ScannerCamera : MonoBehaviour
    {
        [Header("Scanner Dimensions")]
        [SerializeField, Tooltip("How much of the map width to show on the scanner. Lower this to zoom in and hide edge walls.")]
        private float scannerDisplayHalfWidth = 100f;

        [SerializeField, Tooltip("Lowest point of the level (match minY on PlayerController2D)")]
        private float minY = -8f;

        [SerializeField, Tooltip("Highest point of the level (match maxY on PlayerController2D)")]
        private float maxY = 8f;

        [Header("Render Target")]
        [SerializeField, Tooltip("The RenderTexture asset this camera renders into")]
        private RenderTexture scannerRenderTexture;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            ConfigureCamera();
        }

        private void ConfigureCamera()
        {
            // Orthographic to get the flat 2D top-down view of the level
            _cam.orthographic = true;

            if (scannerRenderTexture != null)
            {
                _cam.targetTexture = scannerRenderTexture;
            }

            // Calculate true center and height based on min/max Y
            float levelHeight = maxY - minY;
            float levelHalfHeight = levelHeight / 2f;
            float centerY = minY + levelHalfHeight;

            // Override the camera's projection matrix to EXACTLY fit the width and height
            Matrix4x4 orthoMatrix = Matrix4x4.Ortho(
                -scannerDisplayHalfWidth, scannerDisplayHalfWidth, 
                -levelHalfHeight, levelHalfHeight, 
                _cam.nearClipPlane, _cam.farClipPlane
            );
            _cam.projectionMatrix = orthoMatrix;

            // Center the camera on the true vertical center of the level
            transform.position = new Vector3(0f, centerY, transform.position.z);

            // Background: solid black
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = Color.black;

            // Depth: render BEFORE the main camera (lower number = renders first)
            _cam.depth = -2;
        }

        // Called from inspector or setup — set culling mask to GameWorld and ScannerOnly layers
        [ContextMenu("Set Culling Mask to GameWorld & ScannerOnly")]
        private void SetCullingMask()
        {
            if (_cam == null) _cam = GetComponent<Camera>();
            int gameWorldLayer = LayerMask.NameToLayer("GameWorld");
            int scannerLayer = LayerMask.NameToLayer("ScannerOnly");
            
            if (gameWorldLayer == -1 || scannerLayer == -1)
            {
                Debug.LogWarning("ScannerCamera: 'GameWorld' or 'ScannerOnly' layer not found. Create them in Tags & Layers first.");
                return;
            }
            
            // Bitwise OR to include both layers in the mask
            _cam.cullingMask = (1 << gameWorldLayer) | (1 << scannerLayer);
            Debug.Log("ScannerCamera: Culling mask set to GameWorld + ScannerOnly.");
        }
    }
}
