using UnityEngine;
using DefenderRemake.Systems;
using System.Collections.Generic;

namespace DefenderRemake.UI
{
    public class MapBoundaryWalls3D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Reference to the player's MapBounds3D script")]
        private MapBounds3D mapBounds;
        [SerializeField, Tooltip("The material for the grid walls")]
        private Material gridMaterial;

        [Header("Settings")]
        [SerializeField, Tooltip("How many patches to divide each wall into. 3 = 3x3 grid (9 patches per wall). Higher = smaller localized patches!")]
        [Range(1, 10)]
        private int gridSubdivisions = 4;
        
        [SerializeField, Tooltip("Toggle this if you prefer the look of the back-face of your material!")]
        private bool flipWalls180 = false;
        
        [SerializeField, Tooltip("Distance at which a specific patch starts fading in")]
        private float visibilityDistance = 60f;
        
        [SerializeField, Tooltip("How large each grid square should be inside the material itself")]
        private float gridTileSize = 10f;

        // Data structures for hyper-efficient rendering
        private List<Renderer> _patchRenderers = new List<Renderer>();
        private List<Transform> _patchTransforms = new List<Transform>();
        private MaterialPropertyBlock _propBlock;
        private string _colorPropName = "";
        private Material _sharedMaterialInstance;

        private void Start()
        {
            if (mapBounds == null || gridMaterial == null) return;

            _propBlock = new MaterialPropertyBlock();

            // Create exactly 1 shared material instance in memory for all patches!
            _sharedMaterialInstance = new Material(gridMaterial);
            
            // Auto-detect color property (Check BaseColor FIRST for URP support to prevent popping!)
            if (_sharedMaterialInstance.HasProperty("_BaseColor")) _colorPropName = "_BaseColor";
            else if (_sharedMaterialInstance.HasProperty("_TintColor")) _colorPropName = "_TintColor";
            else if (_sharedMaterialInstance.HasProperty("_Color")) _colorPropName = "_Color";

            float mx = mapBounds.minX; float Mx = mapBounds.maxX;
            float my = mapBounds.minY; float My = mapBounds.maxY;
            float mz = mapBounds.minZ; float Mz = mapBounds.maxZ;

            float sizeX = Mx - mx;
            float sizeY = My - my;
            float sizeZ = Mz - mz;
            Vector3 center = new Vector3((mx + Mx) / 2f, (my + My) / 2f, (mz + Mz) / 2f);

            // Left Wall (-X)
            BuildPatchedWall(new Vector3(mx, center.y, center.z), Quaternion.LookRotation(Vector3.left), sizeZ, sizeY, "Left");
            // Right Wall (+X)
            BuildPatchedWall(new Vector3(Mx, center.y, center.z), Quaternion.LookRotation(Vector3.right), sizeZ, sizeY, "Right");
            // Bottom Wall (-Y)
            BuildPatchedWall(new Vector3(center.x, my, center.z), Quaternion.LookRotation(Vector3.down), sizeX, sizeZ, "Bottom");
            // Top Wall (+Y)
            BuildPatchedWall(new Vector3(center.x, My, center.z), Quaternion.LookRotation(Vector3.up), sizeX, sizeZ, "Top");
            // Back Wall (-Z)
            BuildPatchedWall(new Vector3(center.x, center.y, mz), Quaternion.LookRotation(Vector3.back), sizeX, sizeY, "Back");
            // Front Wall (+Z)
            BuildPatchedWall(new Vector3(center.x, center.y, Mz), Quaternion.LookRotation(Vector3.forward), sizeX, sizeY, "Front");
        }

        private Mesh CreateSoftEdgedQuadMesh(int resolution = 15)
        {
            Mesh mesh = new Mesh();
            mesh.name = "SoftEdgeQuad";

            Vector3[] vertices = new Vector3[resolution * resolution];
            Vector2[] uvs = new Vector2[resolution * resolution];
            Color[] colors = new Color[resolution * resolution];
            int[] tris = new int[(resolution - 1) * (resolution - 1) * 6];

            int triIndex = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = y * resolution + x;
                    
                    // Map x and y to percentages (0.0 to 1.0)
                    float pctX = (float)x / (resolution - 1);
                    float pctY = (float)y / (resolution - 1);

                    // Vertices span from -0.5 to 0.5
                    vertices[i] = new Vector3(pctX - 0.5f, pctY - 0.5f, 0);
                    uvs[i] = new Vector2(pctX, pctY);

                    // To make a soft square, we calculate distance to the closest edge (0.0 to 1.0 space)
                    float distToEdge = Mathf.Min(Mathf.Min(pctX, 1f - pctX), Mathf.Min(pctY, 1f - pctY));
                    
                    // The outer 15% of the patch will fade out smoothly, leaving the center a massive solid square
                    float borderThickness = 0.15f;
                    float alpha = Mathf.Clamp01(distToEdge / borderThickness);
                    
                    // Smoothstep makes the soft edge fade buttery smooth
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    
                    // Apply to Vertex Color
                    colors[i] = new Color(alpha, alpha, alpha, alpha);

                    // Build Triangles (Alternating Chessboard pattern to fix diagonal corner artifacts!)
                    if (x < resolution - 1 && y < resolution - 1)
                    {
                        int bottomLeft = i;
                        int bottomRight = i + 1;
                        int topLeft = i + resolution;
                        int topRight = i + resolution + 1;

                        if ((x + y) % 2 == 0)
                        {
                            // Diagonal from Bottom-Left to Top-Right
                            tris[triIndex++] = bottomLeft;
                            tris[triIndex++] = topLeft;
                            tris[triIndex++] = topRight;

                            tris[triIndex++] = bottomLeft;
                            tris[triIndex++] = topRight;
                            tris[triIndex++] = bottomRight;
                        }
                        else
                        {
                            // Diagonal from Top-Left to Bottom-Right
                            tris[triIndex++] = bottomLeft;
                            tris[triIndex++] = topLeft;
                            tris[triIndex++] = bottomRight;

                            tris[triIndex++] = bottomRight;
                            tris[triIndex++] = topLeft;
                            tris[triIndex++] = topRight;
                        }
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = tris;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void BuildPatchedWall(Vector3 centerPosition, Quaternion rotation, float width, float height, string wallName)
        {
            // Create a temporary parent to easily calculate local offsets
            GameObject wallParent = new GameObject($"Wall_{wallName}");
            wallParent.transform.parent = this.transform;
            wallParent.transform.position = centerPosition;
            
            if (flipWalls180)
            {
                rotation = rotation * Quaternion.Euler(0, 180, 0);
            }
            wallParent.transform.rotation = rotation;

            float patchWidth = width / gridSubdivisions;
            float patchHeight = height / gridSubdivisions;

            Mesh softMesh = CreateSoftEdgedQuadMesh();

            for (int x = 0; x < gridSubdivisions; x++)
            {
                for (int y = 0; y < gridSubdivisions; y++)
                {
                    float localX = (-width / 2f) + (patchWidth / 2f) + (x * patchWidth);
                    float localY = (-height / 2f) + (patchHeight / 2f) + (y * patchHeight);

                    GameObject patch = new GameObject($"Patch_{x}_{y}");
                    MeshFilter mf = patch.AddComponent<MeshFilter>();
                    Renderer r = patch.AddComponent<MeshRenderer>();
                    
                    mf.mesh = softMesh;

                    // Use SetParent with false so it perfectly inherits the wall's rotation!
                    patch.transform.SetParent(wallParent.transform, false);
                    patch.transform.localPosition = new Vector3(localX, localY, 0);
                    patch.transform.localScale = new Vector3(patchWidth, patchHeight, 1f);

                    r.sharedMaterial = _sharedMaterialInstance; // Share the exact same material for batching!

                    // Set auto-tiling via PropertyBlock so we don't break the shared material!
                    r.GetPropertyBlock(_propBlock);
                    if (_sharedMaterialInstance.HasProperty("_MainTex"))
                        _propBlock.SetVector("_MainTex_ST", new Vector4(patchWidth / gridTileSize, patchHeight / gridTileSize, 0, 0));
                    else if (_sharedMaterialInstance.HasProperty("_BaseMap"))
                        _propBlock.SetVector("_BaseMap_ST", new Vector4(patchWidth / gridTileSize, patchHeight / gridTileSize, 0, 0));
                    r.SetPropertyBlock(_propBlock);

                    // Start totally invisible
                    r.enabled = false;

                    _patchRenderers.Add(r);
                    _patchTransforms.Add(patch.transform);
                }
            }
        }

        private void Update()
        {
            if (mapBounds == null || string.IsNullOrEmpty(_colorPropName)) return;

            Vector3 playerPos = mapBounds.transform.position;
            Color baseColor = _sharedMaterialInstance.GetColor(_colorPropName);

            // Loop through every single patch
            for (int i = 0; i < _patchRenderers.Count; i++)
            {
                // Calculate how close the player is to the EXACT center of this specific patch
                float dist = Vector3.Distance(playerPos, _patchTransforms[i].position);

                if (dist <= visibilityDistance)
                {
                    _patchRenderers[i].enabled = true;
                    
                    // Fade in as they get closer
                    float alpha = 1f - (dist / visibilityDistance);
                    float clampedAlpha = Mathf.Clamp01(alpha);
                    
                    // Additive shaders treat Black as 100% transparent and ignore the Alpha channel!
                    // To get a buttery smooth fade, we must fade the actual RGB colors towards Black.
                    Color fadedColor = new Color(
                        baseColor.r * clampedAlpha, 
                        baseColor.g * clampedAlpha, 
                        baseColor.b * clampedAlpha, 
                        baseColor.a * clampedAlpha
                    );

                    // Use MaterialPropertyBlock to change THIS patch's color without breaking performance!
                    _patchRenderers[i].GetPropertyBlock(_propBlock);
                    _propBlock.SetColor(_colorPropName, fadedColor);
                    _patchRenderers[i].SetPropertyBlock(_propBlock);
                }
                else
                {
                    // Too far away, completely disable renderer to save performance
                    if (_patchRenderers[i].enabled)
                    {
                        _patchRenderers[i].enabled = false;
                    }
                }
            }
        }
    }
}
