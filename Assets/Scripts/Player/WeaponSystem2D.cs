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
        private Coroutine _fireRateBoostCoroutine;
        private IObjectPool<LaserProjectile2D> _laserPool;
        private float _originalFireRate;

        private void Awake()
        {
            _originalFireRate = fireRate;
            
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
            if (Time.timeScale == 0f) return; // Prevent firing while paused
            if (IsOverheated || Time.time < _nextFireTime) return;

            // GetKeyDown = tap to fire. Fire rate is controlled by _nextFireTime
            // Use GetKey instead of GetKeyDown if we want holding space to auto-fire, 
            // especially since we have a fire rate boost!
            if (Input.GetKey(fireKey))
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
            // Cool down whether or not we are overheated
            // The lockout (IsOverheated) only blocks firing — not the visual cooldown
            if (CurrentHeat > 0f)
            {
                CurrentHeat = Mathf.Max(CurrentHeat - passiveCooldownRate * Time.deltaTime, 0f);
            }
        }

        private IEnumerator OverheatRoutine()
        {
            IsOverheated = true;
            yield return new WaitForSeconds(overheatLockoutDuration);
            // Lockout ends — heat continues to drain via ProcessHeat naturally
            IsOverheated = false;
            _overheatCoroutine = null;
        }

        public void ResetHeatAndBoost(float duration, float fireRateMultiplier)
        {
            CurrentHeat = 0f;
            IsOverheated = false;
            if (_overheatCoroutine != null)
            {
                StopCoroutine(_overheatCoroutine);
                _overheatCoroutine = null;
            }

            if (_fireRateBoostCoroutine != null)
            {
                StopCoroutine(_fireRateBoostCoroutine);
            }
            _fireRateBoostCoroutine = StartCoroutine(FireRateBoostRoutine(duration, fireRateMultiplier));
        }

        private IEnumerator FireRateBoostRoutine(float duration, float multiplier)
        {
            fireRate = _originalFireRate * multiplier;
            yield return new WaitForSeconds(duration);
            fireRate = _originalFireRate;
            _fireRateBoostCoroutine = null;
        }

        // Called by PlayerController2D when the player respawns
        public void FullReset()
        {
            CurrentHeat = 0f;
            IsOverheated = false;
            fireRate = _originalFireRate;
            _overheatCoroutine = null;
            _fireRateBoostCoroutine = null;
        }
    }
}
