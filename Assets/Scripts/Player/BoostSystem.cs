using UnityEngine;
using System;
using System.Collections;

namespace DefenderRemake.Player
{
    public class BoostSystem : MonoBehaviour
    {
        public event Action<float> OnBoostMeterChanged;

        [Header("Boost Dynamics")]
        [SerializeField, Tooltip("Speed multiplier when boost is active")] 
        private float boostSpeedMultiplier = 2.2f;
        
        [Header("Meter Settings")]
        [SerializeField, Tooltip("How much meter drains per second (max 100)")] 
        private float boostDrainRate = 25f;
        
        [SerializeField, Tooltip("Seconds to wait before recharging after fully depleted")] 
        private float cooldownDelay = 1.5f;
        
        [SerializeField, Tooltip("How much meter recovers per second")] 
        private float refillRate = 20f;
        
        [SerializeField, Tooltip("Input key to trigger boost")] 
        private KeyCode boostKey = KeyCode.LeftShift;

        [Header("Visuals")]
        [SerializeField, Tooltip("Child GameObject with SpriteAnimator to show while boosting")]
        private GameObject boostVisualEffect;

        // State properties
        public float CurrentMeter { get; private set; } = 100f;
        public bool IsLockedOut { get; private set; }

        private bool _isBoosting;
        public bool IsBoosting
        {
            get => _isBoosting;
            private set
            {
                if (_isBoosting == value) return;
                _isBoosting = value;
                
                if (boostVisualEffect != null)
                {
                    ParticleSystem ps = boostVisualEffect.GetComponent<ParticleSystem>();
                    TrailRenderer tr = boostVisualEffect.GetComponent<TrailRenderer>();

                    // If it's a particle or trail, the GameObject MUST be active for them to work.
                    // We turn the object on, but we don't turn it off, so it can fade out smoothly.
                    if ((ps != null || tr != null) && !boostVisualEffect.activeSelf)
                    {
                        boostVisualEffect.SetActive(true);
                    }

                    if (ps != null)
                    {
                        if (_isBoosting) ps.Play();
                        else ps.Stop();
                    }
                    else if (tr != null)
                    {
                        tr.emitting = _isBoosting;
                    }
                    else
                    {
                        // Fallback to instantly turning it off (good for Sprites or Spheres)
                        boostVisualEffect.SetActive(_isBoosting);
                    }
                }
            }
        }

        private Coroutine _cooldownCoroutine;

        private void Update()
        {
            HandleBoostInput();
            ProcessMeter();
        }

        private void HandleBoostInput()
        {
            if (IsLockedOut)
            {
                IsBoosting = false;
                return;
            }

            IsBoosting = Input.GetKey(boostKey) && CurrentMeter > 0f;
        }

        private void ProcessMeter()
        {
            float previousMeter = CurrentMeter;

            if (IsBoosting)
            {
                // Stop any recovery if we somehow started boosting again
                if (_cooldownCoroutine != null)
                {
                    StopCoroutine(_cooldownCoroutine);
                    _cooldownCoroutine = null;
                }

                CurrentMeter -= boostDrainRate * Time.deltaTime;

                if (CurrentMeter <= 0f)
                {
                    CurrentMeter = 0f;
                    IsBoosting = false;
                    _cooldownCoroutine = StartCoroutine(CooldownRoutine());
                }
            }
            else if (!IsLockedOut && CurrentMeter < 100f)
            {
                // Passive recovery when not boosting and not locked out
                CurrentMeter += refillRate * Time.deltaTime;
                CurrentMeter = Mathf.Clamp(CurrentMeter, 0f, 100f);
            }

            if (CurrentMeter != previousMeter)
            {
                OnBoostMeterChanged?.Invoke(CurrentMeter);
            }
        }

        private IEnumerator CooldownRoutine()
        {
            IsLockedOut = true;
            yield return new WaitForSeconds(cooldownDelay);
            IsLockedOut = false;
            _cooldownCoroutine = null;
        }

        /// <summary>
        /// Returns the current speed multiplier. Multiply base movement by this.
        /// </summary>
        public float GetSpeedMultiplier()
        {
            return IsBoosting ? boostSpeedMultiplier : 1f;
        }

        public void ResetMeter()
        {
            CurrentMeter = 100f;
            IsLockedOut = false;
            if (_cooldownCoroutine != null)
            {
                StopCoroutine(_cooldownCoroutine);
                _cooldownCoroutine = null;
            }
        }
    }
}
