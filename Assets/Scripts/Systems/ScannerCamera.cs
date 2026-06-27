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
        [Header("Level Dimensions")]
        [SerializeField, Tooltip("Half the total level width (match LevelBoundX on CameraController2D)")]
        private float levelHalfWidth = 150f;

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

            // Ortho size = half height of the level
            _cam.orthographicSize = levelHalfHeight;

            // The camera's aspect ratio is driven by the RenderTexture dimensions.
            // Set via RenderTexture asset (e.g. 900 x 48) to get a wide scanner view.
            if (scannerRenderTexture != null)
                _cam.targetTexture = scannerRenderTexture;

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
