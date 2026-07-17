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
            HandleMouseAim();

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
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            if (invertY) mouseY = -mouseY;

            // 1. TRUE AERODYNAMIC FLIGHT
            // We apply Pitch and Yaw locally. This permanently solves the FPS "Drill Spin" bug at the poles
            // because you are always turning relative to the nose of the ship.
            transform.Rotate(Vector3.up, mouseX, Space.Self);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);

            // 2. SOFT AUTO-LEVELING
            // Because local rotations cause natural roll drift, we gently pull the ship back upright.
            // We read the global Z rotation (roll) and gently counteract it.
            float currentRoll = transform.eulerAngles.z;
            if (currentRoll > 180f) currentRoll -= 360f;
            
            // Counter-rotate the Z axis to stay level with the horizon
            transform.Rotate(Vector3.forward, -currentRoll * 5f * Time.deltaTime, Space.Self);
            
            // NOTE: We do not clamp pitch! You can fly full loops like a real space fighter.
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
