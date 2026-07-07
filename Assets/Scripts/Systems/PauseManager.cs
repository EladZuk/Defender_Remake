using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Handles pausing the game, freezing time, and bringing up the Pause Menu UI.
    /// Attach this to a GameController object or the UI Canvas in the gameplay scene.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField, Tooltip("The UI Panel containing the pause menu buttons")]
        private GameObject pauseMenuPanel;

        [Header("Scene Settings")]
        [SerializeField, Tooltip("The exact name of the Main Menu scene to load when quitting")]
        private string mainMenuSceneName = "MainMenu";

        private bool _isPaused = false;

        private void Start()
        {
            // Ensure the game starts unpaused and the menu is hidden
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        private void Update()
        {
            // Listen for the Escape key or P key to toggle pause
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (_isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        /// <summary>
        /// Freezes the game physics and displays the UI.
        /// </summary>
        public void PauseGame()
        {
            _isPaused = true;
            Time.timeScale = 0f; // Freezes physics, movement, and animations

            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
            
            Debug.Log("Game Paused");
        }

        /// <summary>
        /// Called by the "Resume" UI Button, or pressing Escape again.
        /// Unfreezes the game and hides the UI.
        /// </summary>
        public void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;

            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            
            Debug.Log("Game Resumed");
        }

        /// <summary>
        /// Called by the "Quit to Main Menu" UI Button.
        /// </summary>
        public void QuitToMainMenu()
        {
            // MUST reset time scale to 1 before loading a new scene!
            Time.timeScale = 1f; 
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
