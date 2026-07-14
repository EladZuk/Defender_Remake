using UnityEngine;
using System;

namespace DefenderRemake.Data
{
    /// <summary>
    /// Persistent data layer that survives scene transitions.
    /// This is how bots collected in Phase 1 carry over to Phase 2.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSessionData", menuName = "Defender/Game Session Data")]
    public class GameSessionData : ScriptableObject
    {
        public int SurvivorCount { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }

        public event Action<int> OnSurvivorCountChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnScoreChanged;

        private void OnEnable()
        {
            ResetSession();
        }

        public void ResetSession()
        {
            SurvivorCount = 0;
            Lives = 3;
            Score = 0;
            
            OnSurvivorCountChanged?.Invoke(SurvivorCount);
            OnLivesChanged?.Invoke(Lives);
            OnScoreChanged?.Invoke(Score);
        }

        public void AddSurvivor()
        {
            SurvivorCount++;
            OnSurvivorCountChanged?.Invoke(SurvivorCount);
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void LoseLife()
        {
            Lives--;
            if (Lives < 0) Lives = 0;
            OnLivesChanged?.Invoke(Lives);
        }
    }
}
