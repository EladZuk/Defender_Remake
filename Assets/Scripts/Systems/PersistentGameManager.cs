using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefenderRemake.Systems
{
    public class PersistentGameManager : MonoBehaviour
    {
        public static PersistentGameManager Instance { get; private set; }

        [Header("Player Session Stats")]
        public int currentLives = 3;
        public int currentScore = 0;
        public float carryoverHeatLevel = 0f;

        [Header("Global Weapon Stats")]
        [Tooltip("The speed of the player's 3D laser")]
        public float playerLaserSpeed = 800f;
        [Tooltip("How much damage the player laser does")]
        public int playerLaserDamage = 1;
        
        [Tooltip("The speed of the enemy 3D lasers")]
        public float enemyLaserSpeed = 400f;
        [Tooltip("How much damage the enemy lasers do to the player")]
        public int enemyLaserDamage = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddScore(int amount)
        {
            currentScore += amount;
            Debug.Log("Score: " + currentScore);
        }

        public void LoseLife()
        {
            currentLives--;
            if (currentLives <= 0)
            {
                Debug.Log("Game Over!");
                // Handle Game Over Logic (Load Game Over scene, or reset)
            }
        }
    }
}
