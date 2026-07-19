using UnityEngine;
using DefenderRemake.Systems;
using System.Collections.Generic;

namespace DefenderRemake.Gameplay
{
    [System.Serializable]
    public class EnvironmentPropConfig
    {
        [Tooltip("The 3D model/prefab to spawn")]
        public GameObject propPrefab;
        
        [Tooltip("How many of these specific props to spawn in the arena")]
        public int spawnAmount = 10;
        
        [Tooltip("Minimum scale of the prop")]
        public float minScale = 1f;
        
        [Tooltip("Maximum scale of the prop")]
        public float maxScale = 5f;
        
        [Tooltip("How far from the EXACT center they spawn (0 = everywhere, 50 = leaves a 50-unit safe zone in the middle)")]
        public float minDistanceFromCenter = 20f;
    }

    public class EnvironmentPropScatterer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Reference to the map bounds to know how big the arena is")]
        private MapBounds3D mapBounds;

        [Header("Configurations")]
        [SerializeField, Tooltip("Configure each environment prop type individually here!")]
        private List<EnvironmentPropConfig> propConfigs;

        private void Start()
        {
            if (mapBounds == null)
            {
                Debug.LogWarning("EnvironmentPropScatterer: Missing MapBounds reference!");
                return;
            }

            if (propConfigs == null || propConfigs.Count == 0)
            {
                Debug.LogWarning("EnvironmentPropScatterer: No prop configurations setup!");
                return;
            }

            ScatterProps();
        }

        private void ScatterProps()
        {
            float mx = mapBounds.minX; float Mx = mapBounds.maxX;
            float my = mapBounds.minY; float My = mapBounds.maxY;
            float mz = mapBounds.minZ; float Mz = mapBounds.maxZ;
            
            Vector3 center = new Vector3((mx + Mx) / 2f, (my + My) / 2f, (mz + Mz) / 2f);

            foreach (var config in propConfigs)
            {
                if (config.propPrefab == null) continue;

                for (int i = 0; i < config.spawnAmount; i++)
                {
                    Vector3 spawnPos = Vector3.zero;
                    bool validPositionFound = false;

                    // Try to find a valid position 100 times to prevent an infinite loop
                    for (int attempt = 0; attempt < 100; attempt++)
                    {
                        spawnPos = new Vector3(
                            Random.Range(mx + 10f, Mx - 10f), // Don't spawn exactly on the wall
                            Random.Range(my + 10f, My - 10f),
                            Random.Range(mz + 10f, Mz - 10f)
                        );

                        if (Vector3.Distance(spawnPos, center) >= config.minDistanceFromCenter)
                        {
                            validPositionFound = true;
                            break;
                        }
                    }

                    if (validPositionFound)
                    {
                        // Spawn
                        GameObject prop = Instantiate(config.propPrefab, spawnPos, Random.rotation, this.transform);
                        
                        // Add floating logic
                        if (prop.GetComponent<FloatingSpaceProp3D>() == null)
                        {
                            prop.AddComponent<FloatingSpaceProp3D>();
                        }

                        // Attach MapBounds3D so they bounce!
                        if (prop.GetComponent<MapBounds3D>() == null)
                        {
                            MapBounds3D propBounds = prop.AddComponent<MapBounds3D>();
                            // Copy the master arena limits so the prop bounces off the invisible walls!
                            propBounds.minX = mx; propBounds.maxX = Mx;
                            propBounds.minY = my; propBounds.maxY = My;
                            propBounds.minZ = mz; propBounds.maxZ = Mz;
                        }

                        // Scale
                        float s = Random.Range(config.minScale, config.maxScale);
                        prop.transform.localScale = new Vector3(s, s, s);
                    }
                }
            }
        }
    }
}
