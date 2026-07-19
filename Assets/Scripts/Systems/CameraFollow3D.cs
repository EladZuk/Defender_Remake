using UnityEngine;

namespace DefenderRemake.Systems
{
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Transform target;
        
        [Header("Offset & Smoothing")]
        [SerializeField, Tooltip("Camera position relative to the player (X: left/right, Y: up/down, Z: forward/back)")]
        private Vector3 localOffset = new Vector3(0f, 2.5f, -7f);
        
        [SerializeField, Tooltip("How quickly the camera catches up to the player's position")]
        private float positionFollowSpeed = 15f;

        [SerializeField, Tooltip("How quickly the camera rotates to look where the player is aiming")]
        private float rotationFollowSpeed = 15f;

        [SerializeField, Tooltip("How far AHEAD of the ship the camera looks. Higher number = ship sits lower on screen.")]
        private float lookAheadDistance = 40f;

        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (target == null) 
            {
                Debug.LogWarning("CameraFollow3D: Target is missing! Please drag your Player_3D object into the Target slot in the Inspector.");
                return;
            }

            // 1. POSITION: Move to the local offset
            Vector3 targetPosition = target.TransformPoint(localOffset);
            float smoothTime = 1f / Mathf.Max(positionFollowSpeed, 0.1f);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);

            // 2. ROTATION: Look past the ship at a target in the distance
            Vector3 lookTarget = target.position + (target.forward * lookAheadDistance);
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, target.up);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationFollowSpeed);
        }
    }
}
