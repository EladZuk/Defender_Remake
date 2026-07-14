using UnityEngine;
using UnityEngine.SceneManagement;
using DefenderRemake.Data;
using System.Collections;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Handles major state changes like Player Death, Game Over, and Phase Transitions.
    /// Should be placed in the scene and assigned the GameSessionData object.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Persistent Data")]
        [SerializeField, Tooltip("Required to track lives and score across scenes")]
        private GameSessionData sessionData;

        [Header("Scene Names")]
        [SerializeField, Tooltip("Name of the 3D transition scene to load when boss delivers the final kill")]
        private string transitionSceneName = "Transition";

        [Header("UI References")]
        [SerializeField, Tooltip("Panel containing the Game Over UI to activate on death")]
        private GameObject gameOverUIPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Ensure UI is hidden at start
            if (gameOverUIPanel != null) gameOverUIPanel.SetActive(false);
        }

        /// <summary>
        /// Called by the PlayerController2D when it is destroyed.
        /// </summary>
        public void PlayerDied(bool killedByBoss, DefenderRemake.Player.PlayerController2D player)
        {
            StartCoroutine(DeathSequence(killedByBoss, player));
        }

        private IEnumerator DeathSequence(bool killedByBoss, DefenderRemake.Player.PlayerController2D player)
        {
            if (sessionData == null)
            {
                Debug.LogWarning("GameStateManager is missing GameSessionData! Restarting scene as fallback.");
                SceneManager.LoadScene("Phase1_2D");
                yield break;
            }

            // If the boss just got the final kill, transition IMMEDIATELY
            if (killedByBoss && sessionData.Lives <= 1)
            {
                sessionData.LoseLife();
                
                bool sceneExists = SceneExistsInBuild(transitionSceneName);
                if (sceneExists)
                {
                    Debug.Log($"Boss delivered the killing blow! Transitioning to: {transitionSceneName}");
                    
                    // Freeze time and capture the exact frame before transitioning
                    Time.timeScale = 0f;
                    yield return new WaitForEndOfFrame();
                    
                    Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                    tex.Apply();
                    
                    TransitionData.ScreenSnapshot = tex;
                    
                    // Restore time so the next scene plays normally
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(transitionSceneName);
                }
                else
                {
                    Debug.LogWarning($"Transition scene '{transitionSceneName}' not found in Build Settings! Showing Game Over UI as fallback.");
                    ShowGameOverUI();
                }
                yield break;
            }

            // Normal death sequence (enemies, or not the last life)
            // Wait 2 seconds so the explosion effect finishes playing
            yield return new WaitForSeconds(2f);

            sessionData.LoseLife();

            if (sessionData.Lives > 0)
            {
                // Still have lives — always respawn
                Debug.Log($"Life lost. Remaining lives: {sessionData.Lives}. Respawning...");
                if (player != null) player.Respawn();
            }
            else
            {
                // Regular enemy got the last kill — Game Over UI
                Debug.Log("GAME OVER! Activating UI.");
                ShowGameOverUI();
            }
        }
        private void ShowGameOverUI()
        {
            if (gameOverUIPanel != null)
            {
                gameOverUIPanel.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                Debug.LogWarning("No GameOver UI panel assigned! Falling back to MainMenu.");
                SceneManager.LoadScene("MainMenu");
            }
        }

        private bool SceneExistsInBuild(string sceneName)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName) return true;
            }
            return false;
        }
    }
}
