using UnityEngine;

namespace DefenderRemake.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Dynamics")]
        [SerializeField, Tooltip("Base acceleration force applied to the ship")] 
        private float moveForce = 50f;
        
        [SerializeField, Tooltip("Maximum velocity the ship can reach horizontally")] 
        private float maxHorizontalSpeed = 15f;
        
        [SerializeField, Tooltip("Maximum velocity the ship can reach vertically")] 
        private float maxVerticalSpeed = 10f;
        
        [SerializeField, Tooltip("Drag applied when no input is pressed to simulate retro deceleration")] 
        private float stoppingDrag = 3f;

        [Header("Visuals")]
        [SerializeField, Tooltip("The child object containing the ship model to flip")] 
        private Transform shipModel;
        
        [SerializeField, Tooltip("Rotation speed when flipping direction")] 
        private float flipSpeed = 15f;

        [Header("Dependencies")]
        [SerializeField, Tooltip("Optional reference to the BoostSystem. If null, tries to find it on this GameObject.")] 
        private BoostSystem boostSystem;

        private Rigidbody _rb;
        private Vector2 _moveInput;
        private float _targetYRotation = 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            // Enforce 2D constraints on the 3D Rigidbody
            _rb.constraints = RigidbodyConstraints.FreezePositionZ | 
                              RigidbodyConstraints.FreezeRotationX | 
                              RigidbodyConstraints.FreezeRotationY | 
                              RigidbodyConstraints.FreezeRotationZ;
            
            _rb.useGravity = false; // It's a flying ship
            
            if (boostSystem == null)
            {
                boostSystem = GetComponent<BoostSystem>();
            }
        }

        private void Update()
        {
            HandleInput();
            HandleVisualFlip();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        private void HandleInput()
        {
            // Read raw input for snappy direction changes
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            _moveInput = new Vector2(horizontal, vertical).normalized;

            // Determine facing direction for visuals based on input
            if (horizontal > 0.1f)
                _targetYRotation = 0f;
            else if (horizontal < -0.1f)
                _targetYRotation = 180f;
        }

        private void HandleVisualFlip()
        {
            if (shipModel != null)
            {
                // Smoothly rotate the model 180 degrees on Y axis when changing direction
                Quaternion targetRot = Quaternion.Euler(0f, _targetYRotation, 0f);
                shipModel.localRotation = Quaternion.Slerp(shipModel.localRotation, targetRot, flipSpeed * Time.deltaTime);
            }
        }

        private void ApplyMovement()
        {
            // Determine active drag
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _rb.linearDamping = stoppingDrag; // Unity 6 uses linearDamping instead of drag
            }
            else
            {
                _rb.linearDamping = 0.5f; // Light drag while moving
            }

            // Calculate force including boost multiplier
            float currentBoost = boostSystem != null ? boostSystem.GetSpeedMultiplier() : 1f;
            Vector3 force = new Vector3(_moveInput.x, _moveInput.y, 0f) * (moveForce * currentBoost);
            
            _rb.AddForce(force, ForceMode.Acceleration);

            // Clamp velocity to max speeds to prevent infinite acceleration
            Vector3 currentVel = _rb.linearVelocity; // Unity 6 uses linearVelocity
            
            float clampedX = Mathf.Clamp(currentVel.x, -maxHorizontalSpeed * currentBoost, maxHorizontalSpeed * currentBoost);
            float clampedY = Mathf.Clamp(currentVel.y, -maxVerticalSpeed * currentBoost, maxVerticalSpeed * currentBoost);
            
            _rb.linearVelocity = new Vector3(clampedX, clampedY, 0f);
        }
    }
}
