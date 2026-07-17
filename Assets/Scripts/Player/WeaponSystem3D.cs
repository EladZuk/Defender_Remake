using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using DefenderRemake.UI;
using DefenderRemake.Gameplay;
using DefenderRemake.Systems;

namespace DefenderRemake.Player
{
    public class WeaponSystem3D : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField, Tooltip("Prefab for the laser shot")] 
        private LaserProjectile3D laserPrefab;
        [SerializeField, Tooltip("Where the laser fires from (usually the nose of the ship)")] 
        private Transform firePoint;
        [SerializeField, Tooltip("Input key to shoot")] 
        private KeyCode fireKey = KeyCode.Space; // Or Mouse0!
        
        [Header("Targeting")]
        [SerializeField, Tooltip("Main Camera to raycast from center screen (Crosshair)")]
        private Camera mainCamera;
        [SerializeField, Tooltip("Max distance the laser targets if nothing is hit")]
        private float maxTargetDistance = 1000f;

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
        private float fireRate = 0.33f;

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
        private IObjectPool<LaserProjectile3D> _laserPool;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            _laserPool = new ObjectPool<LaserProjectile3D>(
                createFunc: () => 
                {
                    var laser = Instantiate(laserPrefab);
                    // Standard instantiate, pool release must be handled inside the laser or via destruction for now.
                    // To keep it simple, we let the laser Destroy() itself and we don't strictly return to pool unless we write a wrapper.
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

        private void Start()
        {
            // Read carried over heat from the 2D phase!
            if (PersistentGameManager.Instance != null)
            {
                CurrentHeat = PersistentGameManager.Instance.carryoverHeatLevel;
                if (CurrentHeat >= 100f)
                {
                    _overheatCoroutine = StartCoroutine(OverheatRoutine());
                }
            }
        }

        private void Update()
        {
            HandleFiring();
            ProcessHeat();
            
            if (heatBarUI != null)
                heatBarUI.SetHeat(CurrentHeat, IsOverheated);
        }

        private void HandleFiring()
        {
            if (Time.timeScale == 0f) return; 
            if (IsOverheated || Time.time < _nextFireTime) return;

            // Allow firing with Mouse Left Click or Space
            if (Input.GetKey(fireKey) || Input.GetMouseButton(0))
            {
                FireLaser();
            }
        }

        private void FireLaser()
        {
            _nextFireTime = Time.time + fireRate;
            CurrentHeat = Mathf.Clamp(CurrentHeat + heatPerShot, 0f, 100f);

            if (CurrentHeat >= 100f && _overheatCoroutine == null)
            {
                _overheatCoroutine = StartCoroutine(OverheatRoutine());
            }

            // CROSSHAIR TARGETING (Raycast from center of screen)
            Vector3 targetPoint;
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Center of screen
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxTargetDistance))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * maxTargetDistance;
            }

            // Calculate firing direction from the ship's nose to the crosshair target
            Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

            var laser = _laserPool.Get();
            laser.transform.position = firePoint.position;
            laser.transform.rotation = Quaternion.LookRotation(fireDirection); // Align laser visual
            
            laser.Fire(fireDirection);

            // Save heat to Persistent Data in case they warp during combat (unlikely but safe)
            if (PersistentGameManager.Instance != null)
            {
                PersistentGameManager.Instance.carryoverHeatLevel = CurrentHeat;
            }
        }

        private void ProcessHeat()
        {
            if (CurrentHeat > 0f)
            {
                CurrentHeat = Mathf.Max(CurrentHeat - passiveCooldownRate * Time.deltaTime, 0f);
            }
        }

        private IEnumerator OverheatRoutine()
        {
            IsOverheated = true;
            yield return new WaitForSeconds(overheatLockoutDuration);
            IsOverheated = false;
            _overheatCoroutine = null;
        }
    }
}
