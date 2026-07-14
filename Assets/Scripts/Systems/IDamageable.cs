namespace DefenderRemake.Systems
{
    /// <summary>
    /// Interface for any entity that can take damage from weapons.
    /// Used by both Enemies and the Player in the 2D Phase.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount, bool killedByBoss = false);
    }
}
