using UnityEngine;

namespace DefenderRemake.Enemies
{
    /// <summary>
    /// A simple enemy that flies slowly across the screen and periodically shoots 
    /// a projectile aimed at the player.
    /// </summary>
    public class EnemyShooter2D : EnemyBase2D
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField, Tooltip("How much it wobbles up and down")] 
        private float waveAmplitude = 1.5f;
        [SerializeField] private float waveFrequency = 2f;

        [Header("Combat Settings")]
        [SerializeField] private EnemyProjectile2D projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField, Tooltip("Time in seconds between shots")]
        private float fireRate = 2.5f;
        
        [SerializeField, Tooltip("Fires if player is within this distance from any angle")]
        private float fireRadius = 8f;
        
        [SerializeField, Tooltip("Fires if player is on the same horizontal line (within this Y distance)")]
        private float horizontalAlignmentTolerance = 1.5f;
        
        private Transform _playerTransform;
        private float _nextFireTime;
        private float _currentMoveDirX = -1f;
        private float _nextWanderTime;

        protected override void Awake()
        {
            base.Awake();
            
            var players = FindObjectsByType<DefenderRemake.Player.PlayerController2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (players.Length > 0) _playerTransform = players[0].transform;
            
            // Randomize the first shot so a group of them don't all fire at the exact same frame
            _nextFireTime = Time.time + Random.Range(0.5f, fireRate); 
            _nextWanderTime = Time.time + Random.Range(1f, 3f);
        }

        protected override void Update()
        {
            base.Update(); // Handles sprite flipping based on velocity
            if (Time.timeScale == 0f) return;

            // Determine which way to fly
            if (_playerTransform != null && _playerTransform.gameObject.activeInHierarchy)
            {
                // Hunt the player - with a deadzone to prevent rapid flipping when directly above/below
                float xDiff = _playerTransform.position.x - transform.position.x;
                if (Mathf.Abs(xDiff) > 0.2f)
                {
                    _currentMoveDirX = Mathf.Sign(xDiff);
                }
            }
            else
            {
                // Player is dead, wander randomly
                if (Time.time > _nextWanderTime)
                {
                    _currentMoveDirX = Random.value > 0.5f ? 1f : -1f;
                    _nextWanderTime = Time.time + Random.Range(1.5f, 4f);
                }
            }

            // Fly horizontally while bobbing up and down using velocity
            float yVel = Mathf.Cos(Time.time * waveFrequency) * waveAmplitude * waveFrequency;
            rb.linearVelocity = new Vector2(_currentMoveDirX * moveSpeed, yVel);

            if (Time.time >= _nextFireTime && _playerTransform != null)
            {
                float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
                float yDiff = Mathf.Abs(transform.position.y - _playerTransform.position.y);

                // Fire if player is close, OR if player is horizontally aligned
                if (distToPlayer <= fireRadius || yDiff <= horizontalAlignmentTolerance)
                {
                    Shoot();
                }
            }
        }

        private void Shoot()
        {
            _nextFireTime = Time.time + fireRate;
            
            if (projectilePrefab == null || firePoint == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Cannot shoot! projectilePrefab or firePoint is completely unassigned in the Inspector!");
                return;
            }

            // Determine direction: Aim at player if alive, otherwise shoot straight ahead
            Vector2 fireDir = Vector2.left;
            if (_playerTransform != null)
            {
                fireDir = (_playerTransform.position - firePoint.position).normalized;
            }
            else
            {
                // If no player, shoot in the direction we are facing
                fireDir = sr != null && sr.flipX ? Vector2.right : Vector2.left;
            }

            Vector3 spawnPos = firePoint.position;
            spawnPos.z = 0f; // Guarantee it spawns on the 2D plane
            var laser = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            laser.Fire(fireDir);
        }
    }
}
