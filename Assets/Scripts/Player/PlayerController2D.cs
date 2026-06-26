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
        
        [SerializeField, Tooltip("Drag applied when no input is pressed to simulate floaty retro deceleration")] 
        private float stoppingDrag = 3f;

        [Header("Level Bounds")]
        [SerializeField, Tooltip("Lowest point the player can fly")]
        private float minY = -8f;
        
        [SerializeField, Tooltip("Highest point the player can fly")]
        private float maxY = 8f;

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
            
            // Enforce zero gravity for floaty flying feel
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
            EnforceBounds();
        }

        private void HandleInput()
        {
            // We use GetAxis for a slight input delay, adding to the floaty, heavy feel of the ship
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
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
            // Determine active drag for the floaty feel
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _rb.linearDamping = stoppingDrag; // Glides to a halt
            }
            else
            {
                _rb.linearDamping = 0.5f; // Light drag while thrusting
            }

            // Calculate force including boost multiplier
            float currentBoost = boostSystem != null ? boostSystem.GetSpeedMultiplier() : 1f;
            Vector2 force = new Vector2(_moveInput.x, _moveInput.y) * (moveForce * currentBoost);
            
            _rb.AddForce(force, ForceMode2D.Force);

            // Clamp velocity to max speeds to prevent infinite acceleration
            Vector2 currentVel = _rb.linearVelocity;
            
            float clampedX = Mathf.Clamp(currentVel.x, -maxHorizontalSpeed * currentBoost, maxHorizontalSpeed * currentBoost);
            float clampedY = Mathf.Clamp(currentVel.y, -maxVerticalSpeed * currentBoost, maxVerticalSpeed * currentBoost);
            
            _rb.linearVelocity = new Vector2(clampedX, clampedY);
        }

        private void EnforceBounds()
        {
            Vector3 pos = transform.position;
            
            if (pos.y > maxY)
            {
                pos.y = maxY;
                if (_rb.linearVelocity.y > 0) _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            }
            else if (pos.y < minY)
            {
                pos.y = minY;
                if (_rb.linearVelocity.y < 0) _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            }

            transform.position = pos;
        }
    }
}
