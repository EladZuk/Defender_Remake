using UnityEngine;

namespace DefenderRemake.Data
{
    /// <summary>
    /// Configuration for a single wave of enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "Defender/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Title")]
        public string waveName = "WAVE 1";
        
        [Header("Enemy Counts")]
        public int slowShooterCount = 5;
        public int kamikazeCount = 2;
        
        [Header("Survivor Counts")]
        public int survivorCount = 3;

        [Header("Spawning Dynamics")]
        [Tooltip("Seconds between each individual enemy spawn")]
        public float timeBetweenSpawns = 2f;
        
        [Tooltip("Should the Boss spawn at the END of this wave?")]
        public bool spawnsBossAtEnd = false;
    }
}
