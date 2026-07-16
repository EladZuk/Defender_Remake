using UnityEngine;
using DefenderRemake.Player;
using DefenderRemake.Systems;
using System.Collections;

namespace DefenderRemake.Enemies
{
    /// <summary>
    /// A bomb dropped by the Boss. Homes in on the player, then explodes into an AoE field.
    /// Slows the player while they are inside. Includes a full visual expand/fade effect.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class SlowingBomb2D : MonoBehaviour
    {
        [Header("Bomb Settings")]
        [SerializeField] private float timeToExplode = 2f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float lingerTime = 4f;
        [SerializeField] private float homingSpeed = 4f;
        [SerializeField, Tooltip("How fast the bomb spins while homing (degrees per second)")]
        private float bombRotationSpeed = 360f;

        [Header("Slow Effect")]
        [SerializeField, Tooltip("Speed multiplier while inside the AoE (0.3 = 30% speed)")]
        private float playerSlowMultiplier = 0.3f;
        [SerializeField, Tooltip("How long the slow persists after leaving the AoE")]
        private float slowDuration = 0.5f;

        [Header("AoE Visual")]
        [SerializeField, Tooltip("A child GameObject with a SpriteRenderer (assign a circle sprite)")]
        private SpriteRenderer aoeVisual;
        [SerializeField, Tooltip("Color of the AoE field (alpha controls max opacity)")]
        private Color aoeColor = new Color(0.2f, 0.8f, 1f, 0.55f);
        [SerializeField, Tooltip("How fast the circle expands on explosion (units per second)")]
        private float expandSpeed = 12f;
        [SerializeField, Tooltip("How much the AoE pulses in size while active (0 = no pulse)")]
        private float pulseAmount = 0.08f;
        [SerializeField, Tooltip("Speed of the pulse oscillation")]
        private float pulseSpeed = 2.5f;
        [SerializeField, Tooltip("Fine-tune to make the visual exactly match the physics AoE (1 = auto-sized to collider)")]
        private float aoeVisualSizeMultiplier = 1f;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private bool _hasExploded = false;
        private bool _explodeStarted = false;
        private Coroutine _explosionTimer;
        private Transform _playerTransform;
        private DefenderRemake.Player.PlayerController2D _playerInsideAoe; // track who is inside so we can clear the slow

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _rb.gravityScale = 0f;

            var players = FindObjectsByType<PlayerController2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (players.Length > 0) _playerTransform = players[0].transform;

            // Hide the AoE visual until explosion
            if (aoeVisual != null)
            {
                aoeVisual.transform.localScale = Vector3.zero;
                aoeVisual.color = new Color(aoeColor.r, aoeColor.g, aoeColor.b, 0f);
            }

            _explosionTimer = StartCoroutine(ExplosionTimerRoutine());
        }

        private void Update()
        {
            // Visual rotation while homing and exploding
            float currentRotSpeed = _hasExploded ? bombRotationSpeed * 0.25f : bombRotationSpeed;
            transform.Rotate(0f, 0f, currentRotSpeed * Time.deltaTime);

            if (_hasExploded) return;

            // Homing logic
            if (_playerTransform != null && _playerTransform.gameObject.activeInHierarchy)
            {
                Vector2 dir = (_playerTransform.position - transform.position).normalized;
                _rb.linearVelocity = dir * homingSpeed;
            }
            else
            {
                _rb.linearVelocity = Vector2.down * (homingSpeed * 0.5f);
            }

            // Force explode if it hits the ground
            if (transform.position.y <= -7f && !_explodeStarted)
            {
                TriggerExplosion();
            }
        }

        private void TriggerExplosion()
        {
            if (_explodeStarted) return;
            _explodeStarted = true;
            if (_explosionTimer != null) StopCoroutine(_explosionTimer);
            StartCoroutine(ExplodeNowRoutine());
        }

        private IEnumerator ExplosionTimerRoutine()
        {
            yield return new WaitForSeconds(timeToExplode);
            TriggerExplosion();
        }

        private IEnumerator ExplodeNowRoutine()
        {
            _hasExploded = true;
            // Drift slowly in the direction it was already heading
            _rb.linearVelocity *= 0.25f;
            _rb.gravityScale = 0f;

            // Expand the physics collider immediately
            _col.isTrigger = true;
            if (_col is CircleCollider2D circle)
                circle.radius = explosionRadius;

            // --- Visual: Rapid Expand Phase ---
            // Target scale = collider diameter in world units, corrected for sprite's native pixel size
            float targetWorldDiameter = explosionRadius * 2f * aoeVisualSizeMultiplier;
            float spriteWorldSize = 1f; // default fallback
            if (aoeVisual != null && aoeVisual.sprite != null)
            {
                // Sprite's natural world size = pixel width / pixelsPerUnit
                spriteWorldSize = aoeVisual.sprite.rect.width / aoeVisual.sprite.pixelsPerUnit;
            }
            float targetScale = targetWorldDiameter / (spriteWorldSize > 0f ? spriteWorldSize : 1f);

            if (aoeVisual != null)
            {
                float currentScale = 0f;
                while (currentScale < targetScale)
                {
                    currentScale = Mathf.MoveTowards(currentScale, targetScale, expandSpeed * Time.deltaTime);
                    aoeVisual.transform.localScale = Vector3.one * currentScale;
                    float t = currentScale / targetScale;
                    aoeVisual.color = new Color(aoeColor.r, aoeColor.g, aoeColor.b, aoeColor.a * t);
                    yield return null;
                }
            }

            // --- Visual: Pulse + Fade Phase (runs for lingerTime) ---
            float elapsed = 0f;
            float baseScale = targetScale; // Use same PPU-corrected scale
            while (elapsed < lingerTime)
            {
                elapsed += Time.deltaTime;
                float fadeOut = 1f - (elapsed / lingerTime); // 1→0 over linger time

                if (aoeVisual != null)
                {
                    float pulse = 1f + pulseAmount * Mathf.Sin(elapsed * pulseSpeed * Mathf.PI * 2f);
                    aoeVisual.transform.localScale = Vector3.one * baseScale * pulse;
                    aoeVisual.color = new Color(aoeColor.r, aoeColor.g, aoeColor.b, aoeColor.a * fadeOut);
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_hasExploded) return;

            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<DefenderRemake.Player.PlayerController2D>();
                if (player != null)
                {
                    _playerInsideAoe = player;
                    player.ApplySlow(playerSlowMultiplier, slowDuration);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Player escaped the AoE cloud — clear the slow immediately
            if (other.CompareTag("Player") && _playerInsideAoe != null)
            {
                _playerInsideAoe.ClearSlow();
                _playerInsideAoe = null;
            }
        }

        private void OnDestroy()
        {
            // Bomb was destroyed while player was still inside — clear slow immediately
            if (_playerInsideAoe != null)
            {
                _playerInsideAoe.ClearSlow();
                _playerInsideAoe = null;
            }
        }
    }
}
