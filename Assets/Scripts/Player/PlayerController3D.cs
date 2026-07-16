using UnityEngine;

namespace DefenderRemake.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController3D : MonoBehaviour
    {
        [Header("Movement Dynamics")]
        [SerializeField] private float moveForce = 60f;
        [SerializeField] private float maxSpeed = 30f;
        [SerializeField] private float stoppingDrag = 4f;
        [SerializeField] private float thrustingDrag = 0.5f;

        [Header("Mouse Aim (Classic)")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField, Tooltip("Invert the Y axis (up/down)")]
        private bool invertY = false;
        [SerializeField, Tooltip("Max pitch angle (looking up/down) to prevent flipping upside down")]
        private float maxPitchLimit = 85f;

        [Header("Visual Banking (Ship Model)")]
        [SerializeField, Tooltip("The visual model of the ship to tilt (assign a child object)")]
        private Transform shipVisualModel;
        
        [SerializeField, Tooltip("How much the ship rolls when yawing (turning left/right)")]
        private float maxRollAngle = 45f;
        [SerializeField, Tooltip("How quickly the ship snaps to its roll angle")]
        private float bankSpeed = 5f;

        private Rigidbody _rb;
        private Vector3 _moveInput;
        
        // Accumulated rotation tracking
        private float _yaw;
        private float _pitch;

        // Visual roll tracking
        private float _currentRoll;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false; 
            _rb.constraints = RigidbodyConstraints.FreezeRotation; // We manually apply rotation to the Rigidbody
            _rb.interpolation = RigidbodyInterpolation.Interpolate; // Fixes physics/camera desync jitter!

            // Lock the mouse to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleMouseAim();
            ApplyRotation(); // Applied in Update for buttery smooth camera sync
            
            // Read 2D input (WASD / Left Stick)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            _moveInput = new Vector3(horizontal, 0f, vertical).normalized;
            
            HandleVisualBanking();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        private void HandleMouseAim()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            if (invertY) mouseY = -mouseY;

            _yaw += mouseX;
            _pitch -= mouseY;

            // Clamp pitch so the player can't do a full vertical loop and flip the camera
            _pitch = Mathf.Clamp(_pitch, -maxPitchLimit, maxPitchLimit);
        }

        private void ApplyRotation()
        {
            // Apply directly to transform in Update instead of Rigidbody in FixedUpdate to eliminate mouse turning jitter
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void ApplyMovement()
        {
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _rb.linearDamping = stoppingDrag;
            }
            else
            {
                _rb.linearDamping = thrustingDrag;
                
                // Calculate thrust direction relative to where the ship is pointing
                Vector3 forceDirection = transform.TransformDirection(_moveInput);
                _rb.AddForce(forceDirection * moveForce, ForceMode.Force);
            }

            // Clamp max speed
            if (_rb.linearVelocity.magnitude > maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
            }
        }

        private void HandleVisualBanking()
        {
            if (shipVisualModel == null) return;

            // Calculate how fast we are turning left/right with the mouse
            float yawSpeed = Input.GetAxis("Mouse X");
            
            // Target roll based on yaw input (mouse turning) + strafing input (A/D)
            float targetRoll = -(yawSpeed + _moveInput.x) * maxRollAngle;
            targetRoll = Mathf.Clamp(targetRoll, -maxRollAngle, maxRollAngle);

            _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, Time.deltaTime * bankSpeed);

            // Apply the roll purely visually to the child model
            shipVisualModel.localRotation = Quaternion.Euler(0f, 0f, _currentRoll);
        }

        private void OnDestroy()
        {
            // Unlock cursor if the player dies or scene changes
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
