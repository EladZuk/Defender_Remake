using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DefenderRemake.Enemies;

namespace DefenderRemake.Systems
{
    [System.Serializable]
    public class WaveData3D
    {
        public string waveName = "WAVE 1";
        public int regularCount = 5;
        public int kamikazeCount = 2;
        public int eliteCount = 0;
        public float timeBetweenSpawns = 1.5f;
    }

    public class WaveManager3D : MonoBehaviour
    {
        [Header("Arena Configuration")]
        [SerializeField] private MapBounds3D mapBounds;
        [SerializeField, Tooltip("How far from the player enemies spawn")]
        private float safeSpawnDistance = 80f;

        [Header("Wave Progression")]
        [SerializeField] private List<WaveData3D> waves;
        [SerializeField] private float timeBetweenWaves = 5f;

        [Header("Prefabs")]
        [SerializeField] private GameObject regularEnemyPrefab;
        [SerializeField] private GameObject kamikazeEnemyPrefab;
        [SerializeField] private GameObject eliteEnemyPrefab;
        
        [Header("UI Text Options")]
        [SerializeField] private string waveSubtitleText = "DEFEND THE SECTOR!";

        public static event System.Action<string, string, bool> OnWaveAnnounced;

        private int _currentWaveIndex = 0;
        private int _activeEnemies = 0;
        private Transform _playerTransform;

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _playerTransform = p.transform;
            
            if (mapBounds == null)
            {
                Debug.LogWarning("WaveManager3D missing MapBounds!");
                return;
            }

            if (waves != null && waves.Count > 0)
            {
                StartCoroutine(WaveRoutine());
            }
        }

        private IEnumerator WaveRoutine()
        {
            yield return new WaitForSeconds(3f);

            while (_currentWaveIndex < waves.Count)
            {
                WaveData3D currentWave = waves[_currentWaveIndex];
                
                // Announce Wave (Main text, Subtitle text, isBoss=false)
                bool isBossWave = currentWave.eliteCount > 0;
                OnWaveAnnounced?.Invoke(currentWave.waveName, waveSubtitleText, isBossWave);
                
                yield return new WaitForSeconds(3f); 

                List<GameObject> enemiesToSpawn = new List<GameObject>();
                for (int i = 0; i < currentWave.regularCount; i++) enemiesToSpawn.Add(regularEnemyPrefab);
                for (int i = 0; i < currentWave.kamikazeCount; i++) enemiesToSpawn.Add(kamikazeEnemyPrefab);
                for (int i = 0; i < currentWave.eliteCount; i++) enemiesToSpawn.Add(eliteEnemyPrefab);
                
                Shuffle(enemiesToSpawn);
                _activeEnemies = enemiesToSpawn.Count;

                foreach (var enemyPrefab in enemiesToSpawn)
                {
                    SpawnEnemy(enemyPrefab);
                    yield return new WaitForSeconds(currentWave.timeBetweenSpawns);
                }

                // Wait until all enemies are dead
                while (_activeEnemies > 0)
                {
                    var alive = FindObjectsByType<EnemyBase3D>(FindObjectsInactive.Exclude);
                    _activeEnemies = alive.Length;
                    yield return new WaitForSeconds(1f);
                }

                _currentWaveIndex++;
                if (_currentWaveIndex < waves.Count)
                {
                    yield return new WaitForSeconds(timeBetweenWaves);
                }
            }
            
            // Victory sequence can go here!
            OnWaveAnnounced?.Invoke("SECTOR CLEARED", "ALL HOSTILES DESTROYED", false);
        }

        private void SpawnEnemy(GameObject prefab)
        {
            if (prefab == null) return;

            Vector3 spawnPos = Vector3.zero;
            bool valid = false;

            // Try 50 times to find a spawn point inside the map but far away from the player
            for (int i = 0; i < 50; i++)
            {
                spawnPos = new Vector3(
                    Random.Range(mapBounds.minX + 20f, mapBounds.maxX - 20f),
                    Random.Range(mapBounds.minY + 20f, mapBounds.maxY - 20f),
                    Random.Range(mapBounds.minZ + 20f, mapBounds.maxZ - 20f)
                );

                if (_playerTransform == null || Vector3.Distance(spawnPos, _playerTransform.position) >= safeSpawnDistance)
                {
                    valid = true;
                    break;
                }
            }

            if (valid)
            {
                var enemy = Instantiate(prefab, spawnPos, Random.rotation);
                
                // Ensure they have the bounds assigned so they bounce!
                var bounds = enemy.GetComponent<MapBounds3D>();
                if (bounds == null) bounds = enemy.AddComponent<MapBounds3D>();
                bounds.minX = mapBounds.minX; bounds.maxX = mapBounds.maxX;
                bounds.minY = mapBounds.minY; bounds.maxY = mapBounds.maxY;
                bounds.minZ = mapBounds.minZ; bounds.maxZ = mapBounds.maxZ;
            }
        }

        private void Shuffle(List<GameObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GameObject temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
