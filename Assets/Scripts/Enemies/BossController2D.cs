using UnityEngine;
using System.Collections;
using DefenderRemake.Systems;

namespace DefenderRemake.Enemies
{
    /// <summary>
    /// The unbeatable boss of the 2D Phase. 
    /// Slowly tracks the player, shoots lasers, and drops slowing bombs.
    /// After rageTime seconds, enters a RAGE PHASE — charges or fires a kill-shot.
    /// </summary>
    public class BossController2D : EnemyBase2D
    {
        [Header("Boss Movement")]
        [SerializeField] private float moveSpeed = 1.5f;

        [Header("Boss Arsenal")]
        [SerializeField] private EnemyProjectile2D laserPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 3f;
        
        [Space]
        [SerializeField] private SlowingBomb2D bombPrefab;
        [SerializeField] private Transform bombDropPoint;
        [SerializeField] private float bombDropRate = 5f;

        [Header("Rage Phase")]
        [SerializeField, Tooltip("Seconds of fighting before rage phase triggers")]
        private float rageTime = 20f;
        [SerializeField, Tooltip("How long the red warning flash lasts before the attack")]
        private float rageWarningDuration = 2f;
        [SerializeField, Tooltip("Ram charge speed during rage")]
        private float rageChargeSpeed = 18f;
        [SerializeField, Tooltip("Kill-shot laser speed multiplier over normal laser")]
        private float killShotSpeedMultiplier = 4f;
        [SerializeField, Tooltip("Color the boss glows during rage warning")]
        private Color rageColor = new Color(1f, 0.2f, 0f); // fiery orange-red

        private Transform _playerTransform;
        private float _nextFireTime;
        private float _nextBombTime;
        private float _rageTimer;
        private bool _isRaging = false;
        private bool _rageTriggered = false;
        private float _currentMoveDirX = -1f;
        private SpriteRenderer _sr;

        protected override void Awake()
        {
            base.Awake();
            _sr = GetComponentInChildren<SpriteRenderer>();

            var players = FindObjectsByType<DefenderRemake.Player.PlayerController2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (players.Length > 0) _playerTransform = players[0].transform;

            _nextFireTime = Time.time + fireRate;
            _nextBombTime = Time.time + bombDropRate;
            _rageTimer = 0f;
        }

        protected override void Update()
        {
            base.Update();
            if (Time.timeScale == 0f) return;
            if (_isRaging || _rageTriggered) return; // Rage coroutine takes over movement

            if (_playerTransform != null)
            {
                if (_playerTransform.gameObject.activeInHierarchy)
                {
                    // Count up rage timer while the player is alive and fighting
                    _rageTimer += Time.deltaTime;
                    if (_rageTimer >= rageTime && !_rageTriggered)
                    {
                        _rageTriggered = true;
                        StartCoroutine(RagePhaseRoutine());
                        return;
                    }

                    // Normal tracking
                    Vector2 dir = (_playerTransform.position - transform.position).normalized;
                    rb.linearVelocity = dir * moveSpeed;
                    _currentMoveDirX = Mathf.Sign(dir.x);

                    // Only fire weapons if on-screen
                    if (Mathf.Abs(transform.position.x - _playerTransform.position.x) < 18f)
                    {
                        if (Time.time >= _nextFireTime) FireLaser();
                        if (Time.time >= _nextBombTime) DropBomb();
                    }
                    else
                    {
                        _nextFireTime = Time.time + 1f;
                        _nextBombTime = Time.time + 1.5f;
                    }
                }
                else
                {
                    // Player is dead — cruise forward, pause rage timer
                    rb.linearVelocity = new Vector2(_currentMoveDirX * moveSpeed, 0f);
                }
            }
        }

        private IEnumerator RagePhaseRoutine()
        {
            _isRaging = true;
            rb.linearVelocity = Vector2.zero;

            // --- WARNING FLASH: pulse between rage color and white ---
            float elapsed = 0f;
            Color originalColor = _sr != null ? _sr.color : Color.white;
            while (elapsed < rageWarningDuration)
            {
                elapsed += Time.deltaTime;
                if (_sr != null)
                {
                    float t = Mathf.PingPong(elapsed * 6f, 1f);
                    _sr.color = Color.Lerp(rageColor, Color.white, t);
                }
                yield return null;
            }

            // Lock in rage color
            if (_sr != null) _sr.color = rageColor;

            if (_playerTransform == null || !_playerTransform.gameObject.activeInHierarchy)
            {
                _isRaging = false;
                yield break;
            }

            // --- RANDOMLY CHOOSE: RAM or KILL-SHOT ---
            if (Random.value > 0.5f)
            {
                yield return StartCoroutine(RageChargeRoutine());
            }
            else
            {
                yield return StartCoroutine(RageKillShotRoutine());
            }

            // Reset so boss resumes normal movement and can rage again later
            if (_sr != null) _sr.color = Color.white;
            _isRaging = false;
            _rageTriggered = false;
            _rageTimer = 0f;
        }

        private IEnumerator RageChargeRoutine()
        {
            if (_playerTransform == null) yield break;

            float chargeTime = 0f;
            // Continuously home in on the player for up to 4 seconds
            while (chargeTime < 4f)
            {
                chargeTime += Time.deltaTime;

                if (_playerTransform.gameObject.activeInHierarchy)
                {
                    // Re-aim every frame — true homing charge
                    Vector2 chargeDir = (_playerTransform.position - transform.position).normalized;
                    rb.linearVelocity = chargeDir * rageChargeSpeed;

                    float dist = Vector2.Distance(transform.position, _playerTransform.position);
                    if (dist < 2f)
                    {
                        // Direct hit — deal damage immediately
                        var damageable = _playerTransform.GetComponent<IDamageable>();
                        damageable?.TakeDamage(999, true);
                        break;
                    }
                }

                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }

        private IEnumerator RageKillShotRoutine()
        {
            // Brief pause for drama
            yield return new WaitForSeconds(0.4f);

            if (laserPrefab != null && firePoint != null && _playerTransform != null)
            {
                Vector2 fireDir = (_playerTransform.position - firePoint.position).normalized;
                Vector3 spawnPos = firePoint.position;
                spawnPos.z = 0f;

                var laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
                laser.FireKillShot(fireDir, killShotSpeedMultiplier, _playerTransform);
            }

            // Wait long enough for the homing laser to reach the player
            yield return new WaitForSeconds(3f);
        }

        private void FireLaser()
        {
            _nextFireTime = Time.time + fireRate;
            if (laserPrefab == null || firePoint == null) return;

            Vector2 fireDir = Vector2.left;
            if (_playerTransform != null)
                fireDir = (_playerTransform.position - firePoint.position).normalized;

            Vector3 spawnPos = firePoint.position;
            spawnPos.z = 0f;
            var laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
            laser.Fire(fireDir, true);
        }

        private void DropBomb()
        {
            _nextBombTime = Time.time + bombDropRate;
            if (bombPrefab == null || bombDropPoint == null) return;

            Vector3 spawnPos = bombDropPoint.position;
            spawnPos.z = 0f;
            Instantiate(bombPrefab, spawnPos, Quaternion.identity);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ram contact — kill the player with boss flag to trigger transition
            if (_isRaging && other.CompareTag("Player"))
            {
                var damageable = other.GetComponent<IDamageable>();
                damageable?.TakeDamage(999, true); // Boss kill flag = true
            }
        }

        public override void TakeDamage(int amount, bool killedByBoss = false)
        {
            if (!_isRaging)
                StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (_sr != null)
            {
                _sr.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                // Hard reset to white unless the boss entered rage mode during the flash
                if (!_isRaging && _sr != null) _sr.color = Color.white;
            }
        }
    }
}


