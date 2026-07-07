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

        public event Action<int> OnSurvivorCountChanged;

        private void OnEnable()
        {
            // Automatically resets when Play Mode starts in Editor,
            // or when the game launches in a standalone build.
            ResetSession();
        }

        public void ResetSession()
        {
            SurvivorCount = 0;
            OnSurvivorCountChanged?.Invoke(SurvivorCount);
        }

        public void AddSurvivor()
        {
            SurvivorCount++;
            OnSurvivorCountChanged?.Invoke(SurvivorCount);
        }
    }
}
