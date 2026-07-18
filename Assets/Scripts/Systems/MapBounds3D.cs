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
        [SerializeField] public float minX = -250f;
        [SerializeField] public float maxX = 250f;
        [SerializeField] public float minY = -250f;
        [SerializeField] public float maxY = 250f;
        [SerializeField] public float minZ = -250f;
        [SerializeField] public float maxZ = 250f;
        
        [Header("Collision Settings")]
        [SerializeField, Tooltip("How far away from the absolute edge the player is stopped (prevents clipping the walls)")]
        private float stopMargin = 5f;
        [SerializeField, Tooltip("How bouncy the walls are (0 = dead stop, 1 = full bounce)")]
        private float bounciness = 0.5f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        // We use FixedUpdate instead of LateUpdate to prevent fighting the physics engine (which causes jittering)
        private void FixedUpdate()
        {
            if (_rb == null) return;

            Vector3 pos = _rb.position;
            Vector3 vel = _rb.linearVelocity;
            bool hitWall = false;

            // X Axis Bounce
            if (pos.x <= minX + stopMargin && vel.x < 0) 
            { 
                pos.x = minX + stopMargin; 
                vel.x = -vel.x * bounciness; 
                hitWall = true; 
            }
            else if (pos.x >= maxX - stopMargin && vel.x > 0) 
            { 
                pos.x = maxX - stopMargin; 
                vel.x = -vel.x * bounciness; 
                hitWall = true; 
            }

            // Y Axis Bounce
            if (pos.y <= minY + stopMargin && vel.y < 0) 
            { 
                pos.y = minY + stopMargin; 
                vel.y = -vel.y * bounciness; 
                hitWall = true; 
            }
            else if (pos.y >= maxY - stopMargin && vel.y > 0) 
            { 
                pos.y = maxY - stopMargin; 
                vel.y = -vel.y * bounciness; 
                hitWall = true; 
            }

            // Z Axis Bounce
            if (pos.z <= minZ + stopMargin && vel.z < 0) 
            { 
                pos.z = minZ + stopMargin; 
                vel.z = -vel.z * bounciness; 
                hitWall = true; 
            }
            else if (pos.z >= maxZ - stopMargin && vel.z > 0) 
            { 
                pos.z = maxZ - stopMargin; 
                vel.z = -vel.z * bounciness; 
                hitWall = true; 
            }

            if (hitWall)
            {
                // Apply the corrected physics directly back to the rigidbody
                _rb.position = pos;
                _rb.linearVelocity = vel;
                
                // TODO: Trigger Assassin's Creed style digital wall shader/effect here in the future
            }
        }

        // Draws editor gizmos so you can actually see the arena limits in the Unity Scene view!
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Gizmos.DrawWireCube(center, size);
        }

        public float DistanceToNearestWall()
        {
            Vector3 pos = transform.position;
            float dx1 = pos.x - minX;
            float dx2 = maxX - pos.x;
            float dy1 = pos.y - minY;
            float dy2 = maxY - pos.y;
            float dz1 = pos.z - minZ;
            float dz2 = maxZ - pos.z;

            float minDistance = Mathf.Min(dx1, dx2, dy1, dy2, dz1, dz2);
            return Mathf.Max(0f, minDistance);
        }

        public struct WallHitInfo
        {
            public float distance;
            public Vector3 closestPointOnWall;
            public Vector3 wallNormal;
        }

        public WallHitInfo GetNearestWallInfo()
        {
            Vector3 pos = transform.position;
            float[] dists = { pos.x - minX, maxX - pos.x, pos.y - minY, maxY - pos.y, pos.z - minZ, maxZ - pos.z };
            
            int minIdx = 0;
            float minDist = dists[0];
            for (int i = 1; i < 6; i++)
            {
                if (dists[i] < minDist)
                {
                    minDist = dists[i];
                    minIdx = i;
                }
            }

            WallHitInfo info = new WallHitInfo();
            info.distance = Mathf.Max(0f, minDist);
            info.closestPointOnWall = pos;

            // Determine exact point and normal based on which wall is closest
            switch (minIdx)
            {
                case 0: info.closestPointOnWall.x = minX; info.wallNormal = Vector3.right; break;
                case 1: info.closestPointOnWall.x = maxX; info.wallNormal = Vector3.left; break;
                case 2: info.closestPointOnWall.y = minY; info.wallNormal = Vector3.up; break;
                case 3: info.closestPointOnWall.y = maxY; info.wallNormal = Vector3.down; break;
                case 4: info.closestPointOnWall.z = minZ; info.wallNormal = Vector3.forward; break;
                case 5: info.closestPointOnWall.z = maxZ; info.wallNormal = Vector3.back; break;
            }

            return info;
        }
    }
}
