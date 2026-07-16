using UnityEngine;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Clamps an object's position to keep it within the 3D arena walls.
    /// Can optionally trigger a visual "Animus Wall" effect later.
    /// </summary>
    public class MapBounds3D : MonoBehaviour
    {
        [Header("Arena Limits")]
        [SerializeField] private float minX = -50f;
        [SerializeField] private float maxX = 50f;
        [SerializeField] private float minZ = -50f;
        [SerializeField] private float maxZ = 50f;
        
        [Tooltip("The fixed height the player flies at")]
        [SerializeField] private float lockedY = 0f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            Vector3 pos = transform.position;
            bool hitWall = false;

            // Clamp X
            if (pos.x < minX) { pos.x = minX; hitWall = true; }
            else if (pos.x > maxX) { pos.x = maxX; hitWall = true; }

            // Clamp Z
            if (pos.z < minZ) { pos.z = minZ; hitWall = true; }
            else if (pos.z > maxZ) { pos.z = maxZ; hitWall = true; }

            // Lock Y (for an open 2D-style arena in 3D space)
            pos.y = lockedY;

            if (hitWall)
            {
                // Kill velocity heading into the wall
                if (_rb != null)
                {
                    Vector3 vel = _rb.linearVelocity;
                    if (pos.x <= minX && vel.x < 0) vel.x = 0;
                    if (pos.x >= maxX && vel.x > 0) vel.x = 0;
                    if (pos.z <= minZ && vel.z < 0) vel.z = 0;
                    if (pos.z >= maxZ && vel.z > 0) vel.z = 0;
                    vel.y = 0;
                    _rb.linearVelocity = vel;
                }
                
                // TODO: Trigger Assassin's Creed style digital wall shader/effect here in the future
            }

            transform.position = pos;
        }

        // Draws editor gizmos so you can actually see the arena limits in the Unity Scene view!
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minX + maxX) / 2f, lockedY, (minZ + maxZ) / 2f);
            Vector3 size = new Vector3(maxX - minX, 1f, maxZ - minZ);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
