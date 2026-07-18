using UnityEngine;
using DefenderRemake.Systems;
using System.Collections.Generic;

namespace DefenderRemake.Gameplay
{
    public class AsteroidFieldGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Reference to the map bounds so we know where the edges are")]
        private MapBounds3D mapBounds;
        [SerializeField, Tooltip("List of asteroid or space debris prefabs to spawn")]
        private List<GameObject> asteroidPrefabs;

        [Header("Settings")]
        [SerializeField, Tooltip("Total number of asteroids to spawn along the edges")]
        private int asteroidCount = 300;
        [SerializeField, Tooltip("How thick the asteroid wall should be (variance from the absolute edge)")]
        private float wallThickness = 10f;
        [SerializeField, Tooltip("Minimum scale for an asteroid")]
        private float minScale = 1f;
        [SerializeField, Tooltip("Maximum scale for an asteroid")]
        private float maxScale = 5f;

        private void Start()
        {
            if (mapBounds == null || asteroidPrefabs == null || asteroidPrefabs.Count == 0)
            {
                Debug.LogWarning("AsteroidFieldGenerator: Missing MapBounds or Prefabs!");
                return;
            }

            GenerateAsteroidCage();
        }

        private void GenerateAsteroidCage()
        {
            float mx = mapBounds.minX; float Mx = mapBounds.maxX;
            float my = mapBounds.minY; float My = mapBounds.maxY;
            float mz = mapBounds.minZ; float Mz = mapBounds.maxZ;

            for (int i = 0; i < asteroidCount; i++)
            {
                // Randomly pick which of the 6 walls to spawn on
                int wallIndex = Random.Range(0, 6);
                Vector3 pos = Vector3.zero;

                // Randomly place it somewhere on that wall, with a little random thickness offset
                float offset = Random.Range(-wallThickness / 2f, wallThickness / 2f);

                switch (wallIndex)
                {
                    case 0: // Left Wall
                        pos = new Vector3(mx + offset, Random.Range(my, My), Random.Range(mz, Mz));
                        break;
                    case 1: // Right Wall
                        pos = new Vector3(Mx + offset, Random.Range(my, My), Random.Range(mz, Mz));
                        break;
                    case 2: // Bottom Wall
                        pos = new Vector3(Random.Range(mx, Mx), my + offset, Random.Range(mz, Mz));
                        break;
                    case 3: // Top Wall
                        pos = new Vector3(Random.Range(mx, Mx), My + offset, Random.Range(mz, Mz));
                        break;
                    case 4: // Back Wall
                        pos = new Vector3(Random.Range(mx, Mx), Random.Range(my, My), mz + offset);
                        break;
                    case 5: // Front Wall
                        pos = new Vector3(Random.Range(mx, Mx), Random.Range(my, My), Mz + offset);
                        break;
                }

                // Pick a random prefab
                GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)];
                
                // Spawn it
                GameObject asteroid = Instantiate(prefab, pos, Random.rotation, this.transform);
                
                // Add the universal floating script so the debris feels alive!
                asteroid.AddComponent<FloatingSpaceProp3D>();
                
                // Randomize scale
                float scale = Random.Range(minScale, maxScale);
                asteroid.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
