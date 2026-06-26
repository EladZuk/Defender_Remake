using UnityEngine;

namespace DefenderRemake.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
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
        [SerializeField, Tooltip("The SpriteRenderer to flip horizontally")] 
        private SpriteRenderer shipSprite;

        [Header("Dependencies")]
        [SerializeField, Tooltip("Optional reference to the BoostSystem. If null, tries to find it on this GameObject.")] 
        private BoostSystem boostSystem;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            
            // Enforce zero gravity for flying feel
            _rb.gravityScale = 0f;
            
            // Lock Z rotation so the ship doesn't spin wildly when hitting the tilemap
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            if (boostSystem == null)
            {
                boostSystem = GetComponent<BoostSystem>();
            }

            if (shipSprite == null)
            {
                shipSprite = GetComponentInChildren<SpriteRenderer>();
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
        }

        private void HandleVisualFlip()
        {
            if (shipSprite != null)
            {
                if (_moveInput.x > 0.1f)
                    shipSprite.flipX = false;
                else if (_moveInput.x < -0.1f)
                    shipSprite.flipX = true;
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
            Vector2 force = new Vector2(_moveInput.x, _moveInput.y) * (moveForce * currentBoost);
            
            _rb.AddForce(force, ForceMode2D.Force);

            // Clamp velocity to max speeds to prevent infinite acceleration
            Vector2 currentVel = _rb.linearVelocity; // Unity 6 uses linearVelocity
            
            float clampedX = Mathf.Clamp(currentVel.x, -maxHorizontalSpeed * currentBoost, maxHorizontalSpeed * currentBoost);
            float clampedY = Mathf.Clamp(currentVel.y, -maxVerticalSpeed * currentBoost, maxVerticalSpeed * currentBoost);
            
            _rb.linearVelocity = new Vector2(clampedX, clampedY);
        }
    }
}
