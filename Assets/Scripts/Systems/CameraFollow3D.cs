using UnityEngine;

namespace DefenderRemake.Systems
{
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Transform target;
        
        [Header("Offset & Smoothing")]
        [SerializeField, Tooltip("Camera position relative to the player (e.g. 0, 3, -10 means exactly behind and slightly up)")]
        private Vector3 localOffset = new Vector3(0f, 3f, -10f);
        
        [SerializeField, Tooltip("How quickly the camera catches up to the player's position")]
        private float positionFollowSpeed = 15f;

        [SerializeField, Tooltip("How quickly the camera rotates to look exactly where the player is looking")]
        private float rotationFollowSpeed = 15f;

        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (target == null) return;

            // Target position is calculated in the player's local space
            Vector3 targetPosition = target.TransformPoint(localOffset);

            // SmoothDamp is framerate independent and eliminates micro-stutters that Lerp causes
            // A higher positionFollowSpeed in the inspector means a lower smooth time here
            float smoothTime = 1f / Mathf.Max(positionFollowSpeed, 0.1f);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);

            // The camera should always try to match the exact rotation (pitch/yaw) of the player
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * rotationFollowSpeed);
        }
    }
}
