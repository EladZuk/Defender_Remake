using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Handles the main menu logic, such as starting the game and quitting.
    /// Attach this to an empty GameObject or a Canvas in the MainMenu scene.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField, Tooltip("The exact name of the gameplay scene to load")]
        private string gameplaySceneName = "Phase1_2D";

        [Header("Mute Button Settings")]
        [SerializeField] private Image muteButtonImage;
        [SerializeField] private Sprite unmutedSprite;
        [SerializeField] private Sprite mutedSprite;
        
        [SerializeField, Tooltip("Audio clip to play when mute button is clicked")]
        private AudioClip muteClickSound;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            // Initialize sprite
            UpdateMuteSprite();
        }

        /// <summary>
        /// Called by the "Start Game" UI Button OnClick event.
        /// </summary>
        public void StartGame()
        {
            Debug.Log("Starting Game: Loading " + gameplaySceneName);
            Time.timeScale = 1f; 
            SceneManager.LoadScene(gameplaySceneName);
        }

        /// <summary>
        /// Called by the "Quit" UI Button OnClick event.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        /// <summary>
        /// Called by a "Mute" UI Button OnClick event.
        /// Toggles the global volume and swaps the sprite.
        /// </summary>
        public void ToggleMute()
        {
            bool isMuted = AudioListener.volume == 0f;
            AudioListener.volume = isMuted ? 1f : 0f;
            
            // Play sound (if we just unmuted, it will be audible. If muted, it won't be, which is correct).
            if (muteClickSound != null && !isMuted) 
            {
                // Play ignoring the new volume if we want to hear it right before muting?
                // Actually, if they mute, they don't want sound. If they unmute, they want sound.
                _audioSource.PlayOneShot(muteClickSound);
            }

            UpdateMuteSprite();
            
            Debug.Log("Sound is now: " + (AudioListener.volume > 0f ? "ON" : "MUTED"));
        }

        private void UpdateMuteSprite()
        {
            if (muteButtonImage == null) return;

            bool isMuted = AudioListener.volume == 0f;
            muteButtonImage.sprite = isMuted ? mutedSprite : unmutedSprite;
        }
    }
}
