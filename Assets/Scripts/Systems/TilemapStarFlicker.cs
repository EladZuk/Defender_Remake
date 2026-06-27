using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Attaches to the Background Tilemap.
    /// Unlocks tile colors and makes each star randomly flicker
    /// through a retro RGB palette at independent rates and phases.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class TilemapStarFlicker : MonoBehaviour
    {
        [Header("Flicker Settings")]
        [SerializeField, Tooltip("How fast the stars cycle through colors (higher = faster)")]
        private float flickerSpeed = 1.5f;

        [SerializeField, Tooltip("Minimum brightness a star can fade to (0 = fully invisible)")]
        [Range(0f, 1f)]
        private float minBrightness = 0.1f;

        [SerializeField, Tooltip("Maximum brightness a star reaches at peak")]
        [Range(0f, 1f)]
        private float maxBrightness = 1f;

        // Retro RGB palette — constrained to classic arcade colors
        private static readonly Color[] RetroColors = new Color[]
        {
            new Color(0f,    1f,    0.88f), // Cyan    #00FFE0
            new Color(1f,    0f,    1f),    // Magenta #FF00FF
            new Color(1f,    1f,    1f),    // White
            new Color(0f,    1f,    0.27f), // Green   #00FF44
            new Color(1f,    0.92f, 0f),    // Yellow  #FFEB00
        };

        private Tilemap _tilemap;

        // Per-star state — stored parallel to positions list
        private List<Vector3Int> _starPositions = new List<Vector3Int>();
        private float[] _phaseOffsets;   // Random time offset per star
        private float[] _speedMultipliers; // Random speed variance per star
        private int[] _colorIndices;     // Which retro color each star uses

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        private void Start()
        {
            CollectStarPositions();
        }

        private void CollectStarPositions()
        {
            _tilemap.CompressBounds();
            BoundsInt bounds = _tilemap.cellBounds;

            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (!_tilemap.HasTile(pos)) continue;

                // Unlock the tile color so we can change it at runtime
                _tilemap.SetTileFlags(pos, TileFlags.None);
                _starPositions.Add(pos);
            }

            int count = _starPositions.Count;
            _phaseOffsets    = new float[count];
            _speedMultipliers = new float[count];
            _colorIndices    = new int[count];

            for (int i = 0; i < count; i++)
            {
                _phaseOffsets[i]     = Random.Range(0f, Mathf.PI * 2f);
                _speedMultipliers[i] = Random.Range(0.5f, 2f);
                _colorIndices[i]     = Random.Range(0, RetroColors.Length);
            }
        }

        private void Update()
        {
            float time = Time.time;

            for (int i = 0; i < _starPositions.Count; i++)
            {
                // Sine wave between minBrightness and maxBrightness
                float t        = (Mathf.Sin(time * flickerSpeed * _speedMultipliers[i] + _phaseOffsets[i]) + 1f) * 0.5f;
                float brightness = Mathf.Lerp(minBrightness, maxBrightness, t);

                Color baseColor = RetroColors[_colorIndices[i]];
                _tilemap.SetColor(_starPositions[i], baseColor * brightness);
            }
        }
    }
}
