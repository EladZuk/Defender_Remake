using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace DefenderRemake.Player
{
    public class WeaponSystem2D : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField, Tooltip("Prefab for the laser shot")] 
        private LaserProjectile2D laserPrefab;
        [SerializeField, Tooltip("Where the laser fires from")] 
        private Transform firePoint;
        [SerializeField, Tooltip("Input key to shoot")] 
        private KeyCode fireKey = KeyCode.Space;

        [Header("Overheat Mechanics")]
        [SerializeField, Tooltip("How much heat is generated per shot (max 100)")] 
        private float heatPerShot = 20f;
        [SerializeField, Tooltip("How fast heat dissipates per second")] 
        private float passiveCooldownRate = 35f;
        [SerializeField, Tooltip("Time locked out from firing if meter hits 100")] 
        private float overheatLockoutDuration = 1.5f;
        [SerializeField, Tooltip("Time between allowed shots")] 
        private float fireRate = 0.33f; // ~3 shots per sec

        // State
        public float CurrentHeat { get; private set; } = 0f;
        public bool IsOverheated { get; private set; }
        
        private float _nextFireTime = 0f;
        private IObjectPool<LaserProjectile2D> _laserPool;

        private void Awake()
        {
            // Initialize Unity's built-in Object Pool to prevent garbage collection spikes during high-freq firing
            _laserPool = new ObjectPool<LaserProjectile2D>(
                createFunc: () => 
                {
                    var laser = Instantiate(laserPrefab);
                    laser.SetPool(_laserPool);
                    return laser;
                },
                actionOnGet: (laser) => laser.gameObject.SetActive(true),
                actionOnRelease: (laser) => laser.gameObject.SetActive(false),
                actionOnDestroy: (laser) => Destroy(laser.gameObject),
                collectionCheck: false,
                defaultCapacity: 15,
                maxSize: 50
            );
        }

        private void Update()
        {
            HandleFiring();
            ProcessHeat();
        }

        private void HandleFiring()
        {
            if (IsOverheated || Time.time < _nextFireTime) return;

            if (Input.GetKeyDown(fireKey))
            {
                FireLaser();
            }
        }

        private void FireLaser()
        {
            _nextFireTime = Time.time + fireRate;
            CurrentHeat += heatPerShot;

            if (CurrentHeat >= 100f)
            {
                CurrentHeat = 100f;
                StartCoroutine(OverheatRoutine());
            }

            // Get a projectile from the pool
            var laser = _laserPool.Get();
            laser.transform.position = firePoint.position;
            
            // Determine firing direction based on player facing
            // We can read the local scale or the SpriteRenderer flipX if available,
            // but for a true retro feel, we can just fire horizontally based on the transform's right
            // If the parent is flipping, right is flipped.
            
            // Wait, PlayerController2D flips the SpriteRenderer, NOT the transform.
            // Let's grab the sprite renderer from the parent to check facing direction
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            Vector2 fireDir = (sr != null && sr.flipX) ? Vector2.left : Vector2.right;

            laser.Fire(fireDir);
            
            // TODO: Play laser audio via AudioSystem
        }

        private void ProcessHeat()
        {
            if (!IsOverheated && CurrentHeat > 0f)
            {
                CurrentHeat -= passiveCooldownRate * Time.deltaTime;
                CurrentHeat = Mathf.Max(CurrentHeat, 0f);
            }
        }

        private IEnumerator OverheatRoutine()
        {
            IsOverheated = true;
            
            // Wait for the lockout penalty
            yield return new WaitForSeconds(overheatLockoutDuration);
            
            // Clear heat and unlock
            CurrentHeat = 0f;
            IsOverheated = false;
        }
    }
}
