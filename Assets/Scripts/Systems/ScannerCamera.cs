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

        [SerializeField, Tooltip("Half the total level height (match minY/maxY on PlayerController2D)")]
        private float levelHalfHeight = 8f;

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

            // Override the camera's projection matrix to EXACTLY fit the width and height
            // we want, ignoring aspect ratio. This guarantees the map fits perfectly
            // into the minimap UI box (with slight stretching if the RT aspect is off).
            Matrix4x4 orthoMatrix = Matrix4x4.Ortho(
                -scannerDisplayHalfWidth, scannerDisplayHalfWidth, 
                -levelHalfHeight, levelHalfHeight, 
                _cam.nearClipPlane, _cam.farClipPlane
            );
            _cam.projectionMatrix = orthoMatrix;

            // Center the camera on the level (X=0, Y=0)
            transform.position = new Vector3(0f, 0f, transform.position.z);

            // Background: solid black
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = Color.black;

            // Depth: render BEFORE the main camera (lower number = renders first)
            _cam.depth = -2;
        }

        // Called from inspector or setup — set culling mask to GameWorld layer only
        // so the scanner doesn't render UI or other non-world objects
        [ContextMenu("Set Culling Mask to GameWorld Layer")]
        private void SetCullingMask()
        {
            if (_cam == null) _cam = GetComponent<Camera>();
            int gameWorldLayer = LayerMask.NameToLayer("GameWorld");
            if (gameWorldLayer == -1)
            {
                Debug.LogWarning("ScannerCamera: 'GameWorld' layer not found. Create it in Tags & Layers first.");
                return;
            }
            _cam.cullingMask = 1 << gameWorldLayer;
            Debug.Log("ScannerCamera: Culling mask set to GameWorld layer.");
        }
    }
}
