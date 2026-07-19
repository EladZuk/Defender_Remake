using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DefenderRemake.Data;
using DefenderRemake.Player;
using DefenderRemake.Gameplay;

namespace DefenderRemake.UI
{
    public class HUDManager3D : MonoBehaviour
    {
        [Header("Persistent Data")]
        [SerializeField] private GameSessionData sessionData;
        
        [Header("Player References")]
        [SerializeField] private PlayerController3D player;
        [SerializeField] private WeaponSystem3D weapon;
        [SerializeField] private BoostSystem boost;
        [SerializeField] private EnergyShield3D shield;

        [Header("UI Sliders")]
        [SerializeField] private Slider hullHealthSlider;
        [SerializeField] private Slider shieldSlider;
        [SerializeField] private Slider heatSlider;
        [SerializeField] private Slider boostSlider;

        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField, Tooltip("Displays survivors converted to missiles under the reticle")]
        private TextMeshProUGUI missilesText;

        private void OnEnable()
        {
            if (sessionData != null)
            {
                sessionData.OnScoreChanged += UpdateScore;
                sessionData.OnLivesChanged += UpdateLives;
                sessionData.OnHeatLevelChanged += UpdateHeat;
                sessionData.OnSurvivorCountChanged += UpdateMissiles;
            }

            if (player != null)
            {
                player.OnHullHealthChanged += UpdateHullHealth;
            }

            if (weapon != null)
            {
                weapon.OnHeatChanged += UpdateHeatWeapon;
            }

            if (boost != null)
            {
                boost.OnBoostMeterChanged += UpdateBoost;
            }

            if (shield != null)
            {
                shield.OnShieldHealthChanged += UpdateShield;
            }
        }

        private void OnDisable()
        {
            if (sessionData != null)
            {
                sessionData.OnScoreChanged -= UpdateScore;
                sessionData.OnLivesChanged -= UpdateLives;
                sessionData.OnHeatLevelChanged -= UpdateHeat;
                sessionData.OnSurvivorCountChanged -= UpdateMissiles;
            }

            if (player != null)
            {
                player.OnHullHealthChanged -= UpdateHullHealth;
            }

            if (weapon != null)
            {
                weapon.OnHeatChanged -= UpdateHeatWeapon;
            }

            if (boost != null)
            {
                boost.OnBoostMeterChanged -= UpdateBoost;
            }

            if (shield != null)
            {
                shield.OnShieldHealthChanged -= UpdateShield;
            }
        }

        private void Start()
        {
            // Initialize UI with starting values
            if (sessionData != null)
            {
                UpdateScore(sessionData.Score);
                UpdateLives(sessionData.Lives);
                UpdateHeat(sessionData.HeatLevel);
                UpdateMissiles(sessionData.SurvivorCount);
            }

            if (player != null)
            {
                UpdateHullHealth(player.CurrentHullHealth, player.MaxHullHealth);
            }
            
            if (boost != null)
            {
                UpdateBoost(boost.CurrentMeter);
            }

            if (shield != null)
            {
                UpdateShield(shield.CurrentShield, shield.MaxShield);
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"SCORE: {score}";
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
                livesText.text = $"LIVES: {lives}";
        }

        private void UpdateHeat(float heat)
        {
            if (heatSlider != null)
                heatSlider.value = heat / 100f; // Assuming max heat is 100
        }

        private void UpdateHeatWeapon(float heat, bool isOverheated)
        {
            UpdateHeat(heat);
        }

        private void UpdateBoost(float currentMeter)
        {
            if (boostSlider != null)
                boostSlider.value = currentMeter / 100f; // Assuming max boost is 100
        }

        private void UpdateHullHealth(int currentHealth, int maxHealth)
        {
            if (hullHealthSlider != null)
                hullHealthSlider.value = (float)currentHealth / maxHealth;
        }

        private void UpdateShield(int currentShield, int maxShield)
        {
            if (shieldSlider != null)
                shieldSlider.value = (float)currentShield / maxShield;
        }

        private void UpdateMissiles(int survivorCount)
        {
            if (missilesText != null)
            {
                missilesText.text = $"MISSILES: {survivorCount}";
            }
        }
    }
}
