using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DefenderRemake.Data;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Displays the 2D ScreenSnapshot on a full-screen quad and applies a violent 
    /// horizontal CRT stutter glitch (matching the Main Menu UI style) before destroying 
    /// itself to reveal the 3D world.
    /// </summary>
    public class WarpTransitionEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("How far in front of the camera the mesh sits.")]
        private float distanceFromCamera = 5f;
        [SerializeField, Tooltip("How long the glitch effect lasts in seconds.")]
        private float glitchDuration = 3f;
        [SerializeField, Tooltip("The 3D Scene to load after glitching finishes.")]
        private string nextSceneName = "Phase2_3D";
        
        [Header("Glitch Intensities")]
        [SerializeField, Tooltip("Starting chance per frame to glitch horizontally")]
        private float startGlitchChance = 0.1f;
        [SerializeField, Tooltip("Ending chance per frame to glitch horizontally")]
        private float endGlitchChance = 0.9f;
        [Space]
        [SerializeField, Tooltip("Starting max distance it can jump horizontally (UV space, e.g. 0.01)")]
        private float startGlitchJump = 0.01f;
        [SerializeField, Tooltip("Ending max distance it can jump horizontally (UV space, e.g. 0.2)")]
        private float endGlitchJump = 0.1f;

        private GameObject _screenQuad;
        private Material _glitchMaterial;

        private void Start()
        {
            if (TransitionData.ScreenSnapshot == null)
            {
                Debug.LogWarning("No ScreenSnapshot found in TransitionData! Using fallback.");
                TransitionData.ScreenSnapshot = new Texture2D(2, 2);
                TransitionData.ScreenSnapshot.SetPixels(new Color[] { Color.magenta, Color.magenta, Color.magenta, Color.magenta });
                TransitionData.ScreenSnapshot.Apply();
            }

            GenerateScreenQuad();
            StartCoroutine(GlitchSequence());
        }

        private void GenerateScreenQuad()
        {
            Camera cam = Camera.main;
            if (cam == null) cam = GetComponent<Camera>();

            _screenQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _screenQuad.name = "GlitchScreenQuad";
            
            // Remove the collider
            Destroy(_screenQuad.GetComponent<Collider>());

            // Parent to camera
            _screenQuad.transform.SetParent(transform);
            _screenQuad.transform.localRotation = Quaternion.identity;
            
            // Calculate full screen size
            float height = 2f * distanceFromCamera * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * cam.aspect;
            
            _screenQuad.transform.localScale = new Vector3(width, height, 1f);
            
            // Position exactly in front - DOCKED EDGES
            _screenQuad.transform.localPosition = new Vector3(0f, 0f, distanceFromCamera);

            // Apply texture
            _glitchMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _glitchMaterial.mainTexture = TransitionData.ScreenSnapshot;
            // Set wrap mode to repeat so the edges wrap around when UVs shift (like a broken CRT)
            TransitionData.ScreenSnapshot.wrapMode = TextureWrapMode.Repeat;
            _screenQuad.GetComponent<MeshRenderer>().sharedMaterial = _glitchMaterial;
        }

        private IEnumerator GlitchSequence()
        {
            float elapsed = 0f;
            Color[] glitchColors = { Color.red, Color.green, Color.blue, Color.white };

            while (elapsed < glitchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / glitchDuration;
                
                // Exponential curve so it starts light and then suddenly gets violent
                float easeInT = t * t * t; 

                float currentGlitchChance = Mathf.Lerp(startGlitchChance, endGlitchChance, easeInT);
                float currentMaxJump = Mathf.Lerp(startGlitchJump, endGlitchJump, easeInT);

                // 1. Horizontal Stutter Glitch via UV Offset (Keeps the mesh docked at edges)
                if (Random.value < currentGlitchChance)
                {
                    // Rapidly shift UVs left/right
                    float randomU = Random.Range(-currentMaxJump, currentMaxJump);
                    _glitchMaterial.mainTextureOffset = new Vector2(randomU, 0f);
                    
                    // 2. RGB Flash Glitch
                    if (Random.value > 0.5f)
                    {
                        _glitchMaterial.color = glitchColors[Random.Range(0, glitchColors.Length)];
                    }
                }
                else
                {
                    // Snap back to normal
                    _glitchMaterial.mainTextureOffset = Vector2.zero;
                    _glitchMaterial.color = Color.white;
                }

                yield return null;
            }

            // Boom! The 2D screen is destroyed, leaving only the 3D world.
            Destroy(_screenQuad);
            
            // Load the 3D Phase
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
