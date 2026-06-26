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

        [Header("Level Bounds")]
        [SerializeField, Tooltip("Minimum X coordinate the camera can pan to")]
        private float minX = -100f;
        
        [SerializeField, Tooltip("Maximum X coordinate the camera can pan to")]
        private float maxX = 100f;

        private void LateUpdate()
        {
            if (target == null) return;

            // Desired position: Follow target's X, maintain our defined Y and Z offset
            float targetX = target.position.x + offset.x;
            
            // Clamp the targetX within the bounded arena limits
            targetX = Mathf.Clamp(targetX, minX, maxX);

            Vector3 desiredPosition = new Vector3(targetX, offset.y, offset.z);

            // Smooth follow
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
