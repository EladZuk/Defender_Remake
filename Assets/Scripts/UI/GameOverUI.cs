using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Handles the Game Over screen buttons.
    /// Attach this to the Game Over Panel in the Phase1_2D scene.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Data Reset")]
        [SerializeField, Tooltip("Reference to GameSessionData to wipe clean on restart")]
        private DefenderRemake.Data.GameSessionData sessionData;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button restartButton;
        [SerializeField] private UnityEngine.UI.Button mainMenuButton;

        private void Awake()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMenu);
        }

        /// <summary>
        /// Called by the "Restart" UI Button OnClick event.
        /// </summary>
        public void RestartGame()
        {
            if (sessionData != null)
            {
                Debug.Log("Wiping old session data for restart...");
                sessionData.ResetSession();
            }

            // Unpause the game
            Time.timeScale = 1f;

            // Reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Called by the "Main Menu" UI Button OnClick event.
        /// </summary>
        public void ReturnToMenu()
        {
            // Unpause the game so Main Menu works properly
            Time.timeScale = 1f;
            
            // Load the main menu
            SceneManager.LoadScene("MainMenu");
        }
    }
}
