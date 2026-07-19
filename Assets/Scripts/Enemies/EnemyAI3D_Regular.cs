using UnityEngine;
using DefenderRemake.Gameplay;

namespace DefenderRemake.Enemies
{
    public class EnemyAI3D_Regular : EnemyBase3D
    {
        [Header("Flight & AI Settings")]
        [SerializeField] private float flightSpeed = 20f;
        [SerializeField] private float turnSpeed = 2f;
        
        [Header("Combat Settings")]
        [SerializeField] private LaserProjectile3D laserPrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private float attackRange = 150f;
        [SerializeField, Tooltip("Damage dealt by this enemy's laser. Overrides GameManager if > 0.")]
        private int laserDamage = 1;
        [SerializeField, Tooltip("How wide the enemy's aim tolerance is in degrees before they fire.")]
        private float firingConeAngle = 30f;
        
        private Transform _playerTarget;
        private float _nextFireTime;

        protected override void Awake()
        {
            base.Awake();
            // In a real scenario, we'd use a robust targeting system, but finding by tag is fine for single-player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTarget = player.transform;
            }
        }

        private void FixedUpdate()
        {
            if (_playerTarget == null) return;

            // 1. Aim at player with dynamic banking (roll)
            Vector3 directionToPlayer = (_playerTarget.position - transform.position).normalized;
            
            // Calculate how hard we are turning to determine how much to bank (roll)
            float turnAmount = Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up);
            float targetRoll = Mathf.Clamp(-turnAmount * 1.2f, -50f, 50f); 
            
            Quaternion lookRot = Quaternion.LookRotation(directionToPlayer);
            Quaternion rollRot = Quaternion.Euler(0, 0, targetRoll);
            Quaternion finalRotation = lookRot * rollRot;
            
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, finalRotation, turnSpeed * Time.fixedDeltaTime));

            // 2. Thrust Forward (Arcade style: perfectly follow the nose, zero drifting!)
            rb.linearVelocity = transform.forward * flightSpeed;

            // 3. Combat
            HandleShooting();
        }

        private void HandleShooting()
        {
            if (Time.time < _nextFireTime || laserPrefab == null || firePoints == null || firePoints.Length == 0) return;

            float distance = Vector3.Distance(transform.position, _playerTarget.position);
            if (distance <= attackRange)
            {
                // Widened the firing cone so they actually pull the trigger
                float angle = Vector3.Angle(transform.forward, (_playerTarget.position - transform.position));
                if (angle < firingConeAngle)
                {
                    _nextFireTime = Time.time + fireRate;
                    
                    foreach (var fp in firePoints)
                    {
                        if (fp != null)
                        {
                            // Gimballed aim: The laser shoots exactly at the player, ensuring it hits!
                            Vector3 perfectAim = (_playerTarget.position - fp.position).normalized;
                            var laser = Instantiate(laserPrefab, fp.position, fp.rotation);
                            laser.Fire(perfectAim, laserDamage);
                        }
                    }
                }
            }
        }
    }
}
