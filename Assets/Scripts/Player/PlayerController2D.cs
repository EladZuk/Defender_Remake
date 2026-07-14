using UnityEngine;
using DefenderRemake.Systems;

namespace DefenderRemake.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour, IDamageable
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

        [Space]
        [SerializeField, Tooltip("ON = instant direction response (snappy). OFF = built-in Unity input smoothing (floaty).")]
        private bool useRawInput = false;

        [Header("Level Bounds")]
        [SerializeField, Tooltip("Lowest point the player can fly")]
        private float minY = -8f;
        
        [SerializeField, Tooltip("Highest point the player can fly")]
        private float maxY = 8f;

        [Header("Visuals")]
        [SerializeField, Tooltip("Sprite renderer of the ship - only used to face the correct direction via scale flip")]
        private SpriteRenderer shipSprite;

        [Header("Dependencies")]
        [SerializeField, Tooltip("Optional reference to the BoostSystem. If null, tries to find it on this GameObject.")] 
        private BoostSystem boostSystem;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private float _slowMultiplier = 1f;
        private Coroutine _slowCoroutine;

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
            // Toggle between raw (snappy) and smoothed (floaty) input via Inspector checkbox
            float horizontal = useRawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");
            float vertical   = useRawInput ? Input.GetAxisRaw("Vertical")   : Input.GetAxis("Vertical");

            _moveInput = new Vector2(horizontal, vertical).normalized;
        }

        private void HandleVisualFlip()
        {
            // Flip the entire transform (not just the sprite) so child objects
            // like BoostEffect automatically move to the correct rear side
            if (_moveInput.x > 0.1f)
                transform.localScale = new Vector3(1f, 1f, 1f);
            else if (_moveInput.x < -0.1f)
                transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        private void ApplyMovement()
        {
            Vector2 effectiveInput = _moveInput;

            // Auto-thrust forward if boosting and not holding any horizontal keys
            if (boostSystem != null && boostSystem.IsBoosting && Mathf.Abs(effectiveInput.x) < 0.01f)
            {
                effectiveInput.x = Mathf.Sign(transform.localScale.x);
            }

            // Determine active drag for the floaty feel
            if (effectiveInput.sqrMagnitude < 0.01f)
            {
                _rb.linearDamping = stoppingDrag; // Glides to a halt
            }
            else
            {
                _rb.linearDamping = 0.5f; // Light drag while thrusting
            }

            // Calculate force including boost multiplier and slow debuff
            float currentBoost = boostSystem != null ? boostSystem.GetSpeedMultiplier() : 1f;
            float effectiveMultiplier = currentBoost * _slowMultiplier;
            Vector2 force = new Vector2(effectiveInput.x, effectiveInput.y) * (moveForce * effectiveMultiplier);
            
            _rb.AddForce(force, ForceMode2D.Force);

            // Clamp velocity to max speeds to prevent infinite acceleration
            Vector2 currentVel = _rb.linearVelocity;
            
            float clampedX = Mathf.Clamp(currentVel.x, -maxHorizontalSpeed * effectiveMultiplier, maxHorizontalSpeed * effectiveMultiplier);
            float clampedY = Mathf.Clamp(currentVel.y, -maxVerticalSpeed * effectiveMultiplier, maxVerticalSpeed * effectiveMultiplier);
            
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

        public bool isInvulnerable { get; private set; }

        public void TakeDamage(int amount, bool killedByBoss = false)
        {
            if (isInvulnerable) return;
            Die(killedByBoss);
        }

        private void Die(bool killedByBoss)
        {
            Debug.Log($"PLAYER HAS DIED! Killed by Boss: {killedByBoss}");
            
            // For now, just disable the ship so it disappears and can't shoot
            gameObject.SetActive(false);

            if (DefenderRemake.Systems.GameStateManager.Instance != null)
            {
                DefenderRemake.Systems.GameStateManager.Instance.PlayerDied(killedByBoss, this);
            }
            else
            {
                Debug.LogWarning("No GameStateManager found in the scene to handle death!");
            }
        }

        public void Respawn()
        {
            // Reset position to center map
            transform.position = Vector3.zero;
            _rb.linearVelocity = Vector2.zero;
            
            // Turn back on
            gameObject.SetActive(true);

            // Fix the weapon lock bug
            var weapon = GetComponent<WeaponSystem2D>();
            if (weapon != null) weapon.FullReset();
            
            // Start I-frames
            StartCoroutine(InvulnerabilityRoutine());
        }

        /// <summary>
        /// Called by SlowingBomb2D when the player is inside the AoE field.
        /// </summary>
        public void ApplySlow(float multiplier, float duration)
        {
            if (_slowCoroutine != null) StopCoroutine(_slowCoroutine);
            _slowCoroutine = StartCoroutine(SlowRoutine(multiplier, duration));
        }

        /// <summary>
        /// Immediately removes any active slow — called when the player exits the AoE or the bomb is destroyed.
        /// </summary>
        public void ClearSlow()
        {
            if (_slowCoroutine != null)
            {
                StopCoroutine(_slowCoroutine);
                _slowCoroutine = null;
            }
            _slowMultiplier = 1f;
        }

        private System.Collections.IEnumerator SlowRoutine(float multiplier, float duration)
        {
            _slowMultiplier = multiplier;
            yield return new WaitForSeconds(duration);
            _slowMultiplier = 1f;
            _slowCoroutine = null;
        }

        private System.Collections.IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            float duration = 3f;
            float blinkInterval = 0.1f;
            
            // Flicker the sprite to show invulnerability
            while (duration > 0f)
            {
                if (shipSprite != null) shipSprite.enabled = !shipSprite.enabled;
                yield return new WaitForSeconds(blinkInterval);
                duration -= blinkInterval;
            }
            
            if (shipSprite != null) shipSprite.enabled = true;
            isInvulnerable = false;
        }
    }
}
