using UnityEngine;
using System;
using System.Collections;

namespace DefenderRemake.Gameplay
{
    public class EnergyShield3D : MonoBehaviour
    {
        public event Action<int, int> OnShieldHealthChanged;

        [Header("Shield Stats")]
        public int maxShield = 100;
        public float rechargeDelay = 3f; // Time before shield starts recharging after taking damage
        public int rechargeRate = 10;    // How much shield recharges per second

        [Header("Visuals")]
        [SerializeField, Tooltip("The visual bubble or mesh for the shield")]
        private MeshRenderer shieldRenderer;
        [SerializeField, Tooltip("How long the shield flashes when hit")]
        private float flashDuration = 0.2f;
        [SerializeField, Tooltip("The intensity multiplier of the flash")]
        private float flashIntensity = 3f;

        private int _currentShield;
        private float _lastDamageTime;
        private Coroutine _flashCoroutine;
        private MaterialPropertyBlock _propBlock;

        public int MaxShield => maxShield;
        public int CurrentShield => _currentShield;

        private void Awake()
        {
            _currentShield = maxShield;
            _propBlock = new MaterialPropertyBlock();

            if (shieldRenderer != null)
            {
                shieldRenderer.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_currentShield < maxShield && Time.time > _lastDamageTime + rechargeDelay)
            {
                int oldShield = _currentShield;
                _currentShield += Mathf.RoundToInt(rechargeRate * Time.deltaTime);
                if (_currentShield > maxShield) _currentShield = maxShield;
                
                if (_currentShield != oldShield)
                {
                    OnShieldHealthChanged?.Invoke(_currentShield, maxShield);
                }
            }
        }

        public int AbsorbDamage(int damageAmount)
        {
            if (_currentShield <= 0) 
            {
                return damageAmount;
            }

            _lastDamageTime = Time.time;
            TriggerHitFlash();

            int remainingDamage = damageAmount - _currentShield;
            _currentShield -= damageAmount;
            
            if (_currentShield < 0) 
                _currentShield = 0;
                
            OnShieldHealthChanged?.Invoke(_currentShield, maxShield);

            return Mathf.Max(0, remainingDamage);
        }

        private void TriggerHitFlash()
        {
            if (shieldRenderer == null)
            {
                Debug.LogError("[SHIELD ERROR] The Shield Renderer is NOT assigned in the Inspector! The shield absorbed the hit but cannot flash!");
                return;
            }

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (shieldRenderer != null)
            {
                shieldRenderer.gameObject.SetActive(true);
            }
            
            yield return new WaitForSeconds(flashDuration);
            
            if (shieldRenderer != null)
            {
                shieldRenderer.gameObject.SetActive(false);
            }
        }

        private void SetShieldVisualIntensity(float alpha)
        {
            if (shieldRenderer == null) return;
            shieldRenderer.GetPropertyBlock(_propBlock);
            
            // Try standard Additive/Transparent shader properties
            if (shieldRenderer.sharedMaterial.HasProperty("_TintColor"))
            {
                Color c = shieldRenderer.sharedMaterial.GetColor("_TintColor");
                c.a = alpha;
                _propBlock.SetColor("_TintColor", c);
            }
            else if (shieldRenderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                Color c = shieldRenderer.sharedMaterial.GetColor("_BaseColor");
                c.a = alpha;
                _propBlock.SetColor("_BaseColor", c);
            }
            else if (shieldRenderer.sharedMaterial.HasProperty("_Color"))
            {
                Color c = shieldRenderer.sharedMaterial.GetColor("_Color");
                c.a = alpha;
                _propBlock.SetColor("_Color", c);
            }
            
            shieldRenderer.SetPropertyBlock(_propBlock);
        }
    }
}
