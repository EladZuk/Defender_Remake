using UnityEngine;

namespace DefenderRemake.Systems
{
    public class LevelWrapper : MonoBehaviour
    {
        [Header("Wrapping Bounds")]
        [SerializeField, Tooltip("The maximum X coordinate before the object wraps to the negative side.")] 
        private float levelBoundX = 150f;

        private void LateUpdate()
        {
            Vector3 pos = transform.position;

            // If we cross the positive bound, wrap to negative
            if (pos.x > levelBoundX)
            {
                pos.x -= (levelBoundX * 2);
                transform.position = pos;
            }
            // If we cross the negative bound, wrap to positive
            else if (pos.x < -levelBoundX)
            {
                pos.x += (levelBoundX * 2);
                transform.position = pos;
            }
        }
    }
}
