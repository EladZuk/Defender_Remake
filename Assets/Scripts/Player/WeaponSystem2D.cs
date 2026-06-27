using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using DefenderRemake.UI;

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

        [Header("UI")]
        [SerializeField, Tooltip("Reference to the HeatBarUI to update every frame")]
        private HeatBarUI heatBarUI;

        [Header("Overheat Mechanics")]
        [SerializeField, Tooltip("How much heat is generated per shot (max 100)")] 
        private float heatPerShot = 20f;
        [SerializeField, Tooltip("How fast heat dissipates per second")] 
        private float passiveCooldownRate = 35f;
        [SerializeField, Tooltip("Time locked out from firing if meter hits 100")] 
        private float overheatLockoutDuration = 1.5f;
        [SerializeField, Tooltip("Time between allowed shots")] 
        private float fireRate = 0.33f; // ~3 shots per sec

        // State — SerializeField makes CurrentHeat visible in Inspector during Play Mode
        [SerializeField]
        private float _currentHeatDebug;
        public float CurrentHeat
        {
            get => _currentHeatDebug;
            private set => _currentHeatDebug = value;
        }
        public bool IsOverheated { get; private set; }
        
        private float _nextFireTime = 0f;
        private Coroutine _overheatCoroutine;
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
            // Push heat state to the UI bar every frame
            if (heatBarUI != null)
                heatBarUI.SetHeat(CurrentHeat, IsOverheated);
        }

        private void HandleFiring()
        {
            if (IsOverheated || Time.time < _nextFireTime) return;

            // GetKeyDown = tap to fire. Fire rate is controlled by _nextFireTime
            if (Input.GetKeyDown(fireKey))
            {
                FireLaser();
            }
        }

        private void FireLaser()
        {
            _nextFireTime = Time.time + fireRate;
            CurrentHeat = Mathf.Clamp(CurrentHeat + heatPerShot, 0f, 100f);

            // Only start the overheat routine if one isn't already running
            if (CurrentHeat >= 100f && _overheatCoroutine == null)
            {
                _overheatCoroutine = StartCoroutine(OverheatRoutine());
            }

            // Get a projectile from the pool
            var laser = _laserPool.Get();
            laser.transform.position = firePoint.position;
            
            // Firing direction uses transform.localScale.x
            // PlayerController2D flips the whole transform (scale -1 = facing left)
            Vector2 fireDir = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;

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
            yield return new WaitForSeconds(overheatLockoutDuration);
            CurrentHeat = 0f;
            IsOverheated = false;
            _overheatCoroutine = null; // Clear so the next overheat can start a fresh coroutine
        }
    }
}
