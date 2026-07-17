using UnityEngine;

namespace DefenderRemake.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController3D : MonoBehaviour
    {
        [Header("Thrust Dynamics")]
        [SerializeField] private float moveForce = 150f;
        [SerializeField] private float maxSpeed = 40f;
        [SerializeField] private float stoppingDrag = 2f;
        [SerializeField] private float thrustingDrag = 0.5f;

        [Header("Mouse Aim Options (Arcade)")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private bool invertY = false;
        [SerializeField, Tooltip("Max pitch angle to prevent flipping upside down")]
        private float maxPitchLimit = 85f;

        [Header("Visual Banking (Ship Model)")]
        [SerializeField, Tooltip("The VisualWrapper object! (NOT the raw Blender model)")]
        private Transform shipVisualModel;
        
        [SerializeField, Tooltip("How much the ship visually rolls when turning left/right")]
        private float maxVisualRollAngle = 45f;
        [SerializeField, Tooltip("How quickly the ship visually snaps to its roll angle")]
        private float visualBankSpeed = 5f;

        private Rigidbody _rb;
        private BoostSystem _boostSystem;
        private Vector3 _moveInput;
        
        private float _currentVisualRoll;
        
        // Accumulated mouse inputs between physics frames
        private float _accumulatedMouseX;
        private float _accumulatedMouseY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _boostSystem = GetComponent<BoostSystem>();
            
            _rb.useGravity = false; 
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Lock physics rotation so we can cleanly control the math
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // 1. Accumulate input (Update runs faster than FixedUpdate, so we must add them up)
            _accumulatedMouseX += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            
            float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            if (invertY) my = -my;
            _accumulatedMouseY += my;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector3(horizontal, 0f, vertical).normalized;
            
            HandleVisualBanking();
        }

        private void FixedUpdate()
        {
            ApplyMouseAimPhysics();
            ApplyMovement();
        }

        private void ApplyMouseAimPhysics()
        {
            // 1. Calculate the rotation from the accumulated mouse input
            Quaternion yaw = Quaternion.AngleAxis(_accumulatedMouseX, Vector3.up);
            Quaternion pitch = Quaternion.AngleAxis(-_accumulatedMouseY, Vector3.right);
            
            // Reset accumulators for the next frame
            _accumulatedMouseX = 0f;
            _accumulatedMouseY = 0f;

            // 2. Auto-Leveling (Anti-Roll)
            // Calculate how much we need to counter-rotate the Z axis to stay upright
            float currentRoll = _rb.rotation.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;
            Quaternion level = Quaternion.AngleAxis(-currentRoll * 5f * Time.fixedDeltaTime, Vector3.forward);

            // 3. Apply the rotation physically! 
            // Using MoveRotation instead of transform.Rotate allows the physics engine 
            // to perfectly smooth out the camera movement without jittering!
            _rb.MoveRotation(_rb.rotation * yaw * pitch * level);
        }

        private void ApplyMovement()
        {
            float boostMult = _boostSystem != null ? _boostSystem.GetSpeedMultiplier() : 1f;

            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _rb.linearDamping = stoppingDrag;
            }
            else
            {
                _rb.linearDamping = thrustingDrag;
                Vector3 forceDirection = transform.TransformDirection(_moveInput);
                _rb.AddForce(forceDirection * (moveForce * boostMult), ForceMode.Acceleration);
            }

            float currentMax = maxSpeed * boostMult;
            if (_rb.linearVelocity.magnitude > currentMax)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * currentMax;
            }
        }

        private void HandleVisualBanking()
        {
            if (shipVisualModel == null) return;
            
            if (shipVisualModel == this.transform)
            {
                Debug.LogError("PlayerController3D: Ship Visual Model CANNOT be the Player_3D object itself!");
                return;
            }

            // Because the physical ship no longer has the nasty FPS Drill Spin, 
            // we get the exact same beautiful visual tilt applied 100% equally at every angle!
            float yawSpeed = Input.GetAxisRaw("Mouse X");
            float targetRoll = -(yawSpeed + _moveInput.x) * maxVisualRollAngle;
            targetRoll = Mathf.Clamp(targetRoll, -maxVisualRollAngle, maxVisualRollAngle);

            _currentVisualRoll = Mathf.Lerp(_currentVisualRoll, targetRoll, Time.deltaTime * visualBankSpeed);

            shipVisualModel.localRotation = Quaternion.Euler(0f, 0f, _currentVisualRoll);
        }

        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
