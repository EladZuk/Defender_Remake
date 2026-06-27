using UnityEngine;
using System;

namespace DefenderRemake.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Lives Settings")]
        [SerializeField, Tooltip("Number of lives the player starts with")]
        private int startingLives = 3;

        public int CurrentLives { get; private set; }

        // Subscribe to these from UI or game manager
        public event Action<int> OnLivesChanged;  // passes remaining lives
        public event Action OnPlayerDied;          // all lives gone

        private void Awake()
        {
            CurrentLives = startingLives;
        }

        public void LoseLife()
        {
            CurrentLives--;
            CurrentLives = Mathf.Max(CurrentLives, 0);
            OnLivesChanged?.Invoke(CurrentLives);

            if (CurrentLives <= 0)
                OnPlayerDied?.Invoke();
        }

        public void GainLife()
        {
            CurrentLives++;
            OnLivesChanged?.Invoke(CurrentLives);
        }

        // Called on respawn — resets to starting lives
        public void ResetLives()
        {
            CurrentLives = startingLives;
            OnLivesChanged?.Invoke(CurrentLives);
        }
    }
}
