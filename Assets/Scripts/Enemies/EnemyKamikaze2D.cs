using UnityEngine;

namespace DefenderRemake.Enemies
{
    /// <summary>
    /// A medium enemy that idles slowly but charges directly at the player 
    /// at high speed once the player gets too close.
    /// </summary>
    public class EnemyKamikaze2D : EnemyBase2D
    {
        [Header("Kamikaze Settings")]
        [SerializeField, Tooltip("Speed before spotting the player")] 
        private float idleSpeed = 2f;
        
        [SerializeField, Tooltip("Speed when diving at the player")] 
        private float chargeSpeed = 9f;
        
        [SerializeField, Tooltip("How close the player must be to trigger the charge")] 
        private float detectionRadius = 12f;

        private Transform _playerTransform;
        private bool _isCharging = false;

        private float _currentMoveDirX = -1f;
        private float _nextWanderTime;

        protected override void Awake()
        {
            base.Awake();
            
            var players = FindObjectsByType<DefenderRemake.Player.PlayerController2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (players.Length > 0) _playerTransform = players[0].transform;

            _nextWanderTime = Time.time + Random.Range(1f, 3f);
        }

        protected override void Update()
        {
            base.Update(); // Handles sprite flipping
            if (Time.timeScale == 0f) return;

            if (_playerTransform == null)
            {
                // No player? Just cruise
                rb.linearVelocity = new Vector2(_currentMoveDirX * idleSpeed, 0f);
                return;
            }

            if (_playerTransform.gameObject.activeInHierarchy)
            {
                // Player is ALIVE. 
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                
                if (!_isCharging && dist <= detectionRadius)
                {
                    _isCharging = true;
                }

                if (_isCharging)
                {
                    // Fly directly at the player's current position at high speed
                    Vector2 dir = (_playerTransform.position - transform.position).normalized;
                    rb.linearVelocity = dir * chargeSpeed;
                }
                else
                {
                    // Intelligent Patrol: Move towards the player horizontally, and slowly match altitude
                    float xDiff = _playerTransform.position.x - transform.position.x;
                    if (Mathf.Abs(xDiff) > 0.2f)
                    {
                        _currentMoveDirX = Mathf.Sign(xDiff);
                    }
                    float yDiff = _playerTransform.position.y - transform.position.y;
                    float yVel = Mathf.Clamp(yDiff, -1f, 1f) * idleSpeed;

                    rb.linearVelocity = new Vector2(_currentMoveDirX * idleSpeed, yVel);
                }
            }
            else
            {
                // Player is DEAD. Wander randomly.
                _isCharging = false;

                if (Time.time > _nextWanderTime)
                {
                    _currentMoveDirX = Random.value > 0.5f ? 1f : -1f;
                    _nextWanderTime = Time.time + Random.Range(1.5f, 4f);
                }

                // Move horizontally while applying a sine wave vertical motion
                float yVel = Mathf.Cos(Time.time * 2f) * 3f * 2f;
                rb.linearVelocity = new Vector2(_currentMoveDirX * idleSpeed, yVel);
            }
        }
    }
}
