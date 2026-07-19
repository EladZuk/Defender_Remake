using UnityEngine;

namespace DefenderRemake.Enemies
{
    public class EnemyAI3D_Kamikaze : EnemyBase3D
    {
        [Header("Ramming Settings")]
        [SerializeField] private float rammingSpeed = 45f;
        [SerializeField] private float lockOnTurnSpeed = 5f;
        [SerializeField] private int rammingDamage = 50;

        private Transform _playerTarget;

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

            // Kamikaze logic: Aggressively turn towards player and bank hard
            Vector3 directionToPlayer = (_playerTarget.position - transform.position).normalized;
            
            float turnAmount = Vector3.SignedAngle(transform.forward, directionToPlayer, Vector3.up);
            float targetRoll = Mathf.Clamp(-turnAmount * 2.0f, -75f, 75f); // Kamikazes bank extremely hard
            
            Quaternion lookRot = Quaternion.LookRotation(directionToPlayer);
            Quaternion rollRot = Quaternion.Euler(0, 0, targetRoll);
            Quaternion finalRotation = lookRot * rollRot;

            rb.MoveRotation(Quaternion.Slerp(rb.rotation, finalRotation, lockOnTurnSpeed * Time.fixedDeltaTime));

            // Arcade Thrust
            rb.linearVelocity = transform.forward * rammingSpeed;
        }

        protected override void HandleRamming(GameObject hitObject)
        {
            // Override base collision to ensure we blow up!
            if (hitObject.CompareTag("Player"))
            {
                var playerDamageable = hitObject.GetComponentInParent<Systems.IDamageable>();
                if (playerDamageable != null)
                {
                    playerDamageable.TakeDamage(rammingDamage);
                }
                
                // Kamikazes successfully detonated, so they die immediately
                Die();
            }
            else
            {
                base.HandleRamming(hitObject);
            }
        }
    }
}
