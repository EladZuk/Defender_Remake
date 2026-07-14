using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DefenderRemake.Data;
using DefenderRemake.Enemies;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Handles wave progression, UI announcements, and randomized spawning of enemies/survivors.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [SerializeField] private List<WaveData> waves;
        [SerializeField] private float timeBetweenWaves = 4f;

        [Header("Prefabs")]
        [SerializeField] private GameObject slowShooterPrefab;
        [SerializeField] private GameObject kamikazePrefab;
        [SerializeField] private GameObject survivorPrefab;
        [SerializeField] private GameObject bossPrefab;
        
        [Header("UI Text Options")]
        [SerializeField] private string bossWarningText = "BOSS INCOMING";
        [SerializeField] private string waveSubtitleText = "KILL ENEMIES AND SAVE SURVIVORS!";

        [Header("Enemy Spawn Settings")]
        [SerializeField, Tooltip("How far off-screen enemies spawn relative to the player")]
        private float enemySpawnOffscreenDistance = 25f;
        [SerializeField, Tooltip("Minimum Y position for enemy spawns")]
        private float enemyMinY = -6f;
        [SerializeField, Tooltip("Maximum Y position for enemy spawns")]
        private float enemyMaxY = 6f;

        [Header("Survivor Spawn Settings")]
        [SerializeField, Tooltip("Minimum X position survivors can spawn at")]
        private float survivorMinX = -90f;
        [SerializeField, Tooltip("Maximum X position survivors can spawn at")]
        private float survivorMaxX = 90f;
        [SerializeField, Tooltip("Minimum Y position for survivor spawns")]
        private float survivorMinY = -6f;
        [SerializeField, Tooltip("Maximum Y position for survivor spawns")]
        private float survivorMaxY = 6f;
        
        // Event for the UI to listen to (MainText, SubtitleText, IsBossWarning)
        public static event System.Action<string, string, bool> OnWaveAnnounced;

        private int _currentWaveIndex = 0;
        private int _activeEnemies = 0;
        private Transform _playerTransform;

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _playerTransform = p.transform;
            
            if (waves != null && waves.Count > 0)
            {
                StartCoroutine(WaveRoutine());
            }
            else
            {
                Debug.LogWarning("WaveManager has no Waves assigned!");
            }
        }

        private IEnumerator WaveRoutine()
        {
            yield return new WaitForSeconds(2f);

            while (_currentWaveIndex < waves.Count)
            {
                WaveData currentWave = waves[_currentWaveIndex];
                
                // Announce Wave (Main text, Subtitle text, isBoss=false)
                OnWaveAnnounced?.Invoke(currentWave.waveName, waveSubtitleText, false);
                
                yield return new WaitForSeconds(3f); 

                // Spawn Survivors instantly across the entire map at start of wave
                for(int i = 0; i < currentWave.survivorCount; i++)
                {
                    SpawnEntity(survivorPrefab, true);
                }

                // Create a list of enemies to spawn
                List<GameObject> enemiesToSpawn = new List<GameObject>();
                for (int i = 0; i < currentWave.slowShooterCount; i++) enemiesToSpawn.Add(slowShooterPrefab);
                for (int i = 0; i < currentWave.kamikazeCount; i++) enemiesToSpawn.Add(kamikazePrefab);
                
                Shuffle(enemiesToSpawn);
                _activeEnemies = enemiesToSpawn.Count;

                // Spawn enemies one by one
                foreach (var enemyPrefab in enemiesToSpawn)
                {
                    SpawnEntity(enemyPrefab, false);
                    yield return new WaitForSeconds(currentWave.timeBetweenSpawns);
                }

                // Wait until all enemies are dead
                while (_activeEnemies > 0)
                {
                    // Clean up dead enemies from the count without relying on Unity Tags
                    var alive = FindObjectsByType<EnemyBase2D>(FindObjectsInactive.Exclude);
                    
                    // Filter out the boss just in case
                    int count = 0;
                    foreach(var enemy in alive)
                    {
                        if (!(enemy is BossController2D)) count++;
                    }
                    
                    _activeEnemies = count;
                    yield return new WaitForSeconds(1f);
                }

                // Wave Complete! Check for Boss
                if (currentWave.spawnsBossAtEnd)
                {
                    OnWaveAnnounced?.Invoke(bossWarningText, "", true); // isBoss=true
                    yield return new WaitForSeconds(4f);
                    SpawnEntity(bossPrefab, false);
                    
                    // The Boss halts normal wave progression until it kills the player
                    yield break; 
                }

                _currentWaveIndex++;
                if (_currentWaveIndex < waves.Count)
                {
                    yield return new WaitForSeconds(timeBetweenWaves);
                }
            }
        }

        private void SpawnEntity(GameObject prefab, bool isSurvivor)
        {
            if (prefab == null) return;

            float spawnX, spawnY;

            if (isSurvivor)
            {
                // Survivors scatter across the full map at ground level
                spawnX = Random.Range(survivorMinX, survivorMaxX);
                spawnY = Random.Range(survivorMinY, survivorMaxY);
            }
            else
            {
                // Enemies spawn just off-screen from the player
                float playerX = _playerTransform != null ? _playerTransform.position.x : 0f;
                int dir = Random.value > 0.5f ? 1 : -1;
                spawnX = playerX + (enemySpawnOffscreenDistance * dir);
                spawnY = Random.Range(enemyMinY, enemyMaxY);
            }

            Instantiate(prefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
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
