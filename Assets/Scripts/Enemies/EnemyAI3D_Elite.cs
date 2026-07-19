using UnityEngine;
using System.Collections;
using DefenderRemake.Gameplay;

namespace DefenderRemake.Enemies
{
    public class EnemyAI3D_Elite : EnemyBase3D
    {
        [Header("Flight Settings")]
        [SerializeField] private float strafeSpeed = 10f;
        [SerializeField] private float turnSpeed = 1.5f;
        [SerializeField, Tooltip("How close it wants to stay to the player")]
        private float preferredDistance = 80f;

        [Header("Combat Barrage Settings")]
        [SerializeField] private LaserProjectile3D heavyLaserPrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float barrageCooldown = 4f;
        [SerializeField] private int burstFireCount = 5;
        [SerializeField] private float burstFireDelay = 0.15f;
        [SerializeField, Tooltip("Damage dealt by this enemy's laser. Overrides GameManager if > 0.")]
        private int laserDamage = 5;
        [SerializeField, Tooltip("How wide the enemy's aim tolerance is in degrees before they fire.")]
        private float firingConeAngle = 20f;
        
        private Transform _playerTarget;
        private bool _isFiringBarrage = false;
        private float _nextBarrageTime;

        protected override void Awake()
        {
            base.Awake();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTarget = player.transform;
            }
        }

        private void FixedUpdate()
        {
            if (_playerTarget == null) return;

            // 1. Aim at player with dynamic banking
            Vector3 directionToPlayer = (_playerTarget.position - transform.position).normalized;
            
            float turnAmount = Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up);
            float targetRoll = Mathf.Clamp(-turnAmount * 1.5f, -60f, 60f); // Elite banks harder
            
            Quaternion lookRot = Quaternion.LookRotation(directionToPlayer);
            Quaternion rollRot = Quaternion.Euler(0, 0, targetRoll);
            Quaternion finalRotation = lookRot * rollRot;

            rb.MoveRotation(Quaternion.Slerp(rb.rotation, finalRotation, turnSpeed * Time.fixedDeltaTime));

            // 2. Strafe/Distance Maintenance (Arcade thrust)
            float distance = Vector3.Distance(transform.position, _playerTarget.position);
            Vector3 flightDirection = Vector3.zero;

            if (distance > preferredDistance + 20f)
            {
                // Too far, move forward
                flightDirection = transform.forward;
            }
            else if (distance < preferredDistance - 20f)
            {
                // Too close, back away
                flightDirection = -transform.forward;
            }
            else
            {
                // In sweet spot, strafe slowly
                flightDirection = transform.right * (Mathf.Sin(Time.time * 0.5f));
            }

            // Apply absolute arcade velocity so they don't drift on ice
            rb.linearVelocity = flightDirection * strafeSpeed;

            // 3. Combat
            HandleShooting();
        }

        private void HandleShooting()
        {
            if (_isFiringBarrage || Time.time < _nextBarrageTime || heavyLaserPrefab == null || firePoints == null || firePoints.Length == 0) return;

            float angle = Vector3.Angle(transform.forward, (_playerTarget.position - transform.position));
            if (angle < firingConeAngle)
            {
                StartCoroutine(FireBarrageRoutine());
            }
        }

        private IEnumerator FireBarrageRoutine()
        {
            _isFiringBarrage = true;

            for (int i = 0; i < burstFireCount; i++)
            {
                if (_playerTarget != null)
                {
                    foreach (var fp in firePoints)
                    {
                        if (fp != null)
                        {
                            Vector3 direction = (_playerTarget.position - fp.position).normalized;
                            var laser = Instantiate(heavyLaserPrefab, fp.position, fp.rotation);
                            laser.Fire(direction, laserDamage);
                        }
                    }
                }
                
                yield return new WaitForSeconds(burstFireDelay);
            }

            _nextBarrageTime = Time.time + barrageCooldown;
            _isFiringBarrage = false;
        }
        
        public override void TakeDamage(int amount, bool killedByBoss = false)
        {
            base.TakeDamage(amount, killedByBoss);
            // Elites might have enraged logic here in the future
        }
    }
}
