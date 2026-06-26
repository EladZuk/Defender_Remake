using UnityEngine;

namespace DefenderRemake.Systems
{
    public class CameraController2D : MonoBehaviour
    {
        [Header("Tracking Settings")]
        [SerializeField, Tooltip("The target to follow (Player)")] 
        private Transform target;
        
        [SerializeField, Tooltip("How smoothly the camera follows the target")] 
        private float smoothSpeed = 10f;
        
        [SerializeField, Tooltip("Offset from the target. Y and Z remain constant based on this.")] 
        private Vector3 offset = new Vector3(0f, 5f, -20f);

        [Header("Wrapping Support")]
        [SerializeField, Tooltip("If the target moves more than this distance in one frame, the camera will snap instantly (prevents sweeping across the map on level wrap).")]
        private float wrapSnapDistance = 20f;

        private void LateUpdate()
        {
            if (target == null) return;

            // Desired position: Follow target's X, maintain our defined Y and Z offset
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, offset.y, offset.z);

            // Detect if the target just teleported (level wrap)
            if (Mathf.Abs(target.position.x - transform.position.x) > wrapSnapDistance)
            {
                // Snap instantly
                transform.position = desiredPosition;
            }
            else
            {
                // Smooth follow
                transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            }
        }
    }
}
