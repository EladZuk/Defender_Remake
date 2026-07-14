using UnityEngine;

namespace DefenderRemake.Data
{
    /// <summary>
    /// Static class to hold data that needs to persist across the 2D to 3D scene transition.
    /// Specifically, the captured screen snapshot from the exact moment of death.
    /// </summary>
    public static class TransitionData
    {
        public static Texture2D ScreenSnapshot;
    }
}
