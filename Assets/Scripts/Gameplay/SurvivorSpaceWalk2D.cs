using UnityEngine;

namespace DefenderRemake.Gameplay
{
    /// <summary>
    /// Attached to a Survivor prefab alongside SurvivorPickup2D.
    /// Makes the survivor drift and tumble like a spacewalking astronaut.
    /// </summary>
    public class SurvivorSpaceWalk2D : MonoBehaviour
    {
        [Header("Float Motion")]
        [SerializeField, Tooltip("Amplitude of the vertical sine wave drift (world units)")]
        private float floatAmplitude = 0.4f;
        [SerializeField, Tooltip("Speed of the vertical float cycle")]
        private float floatFrequency = 0.8f;
        [SerializeField, Tooltip("Slow horizontal drift speed")]
        private float driftSpeed = 0.3f;

        [Header("Rotation")]
        [SerializeField, Tooltip("Degrees per second of slow spin (randomized direction on spawn)")]
        private float rotationSpeed = 25f;

        // Internal state
        private float _originY;
        private float _timeOffset;
        private float _driftDirection;   // +1 or -1
        private float _rotationDir;      // +1 or -1
        private Vector3 _originalScale;  // Preserve the prefab's authored scale

        private void Awake()
        {
            _originY = transform.position.y;
            _originalScale = transform.localScale;

            // Randomize phase so a group of them don't all bob in sync
            _timeOffset = Random.Range(0f, Mathf.PI * 2f);

            // Random facing: only flip the X sign, keep the original scale magnitude
            float facingX = Random.value > 0.5f ? 1f : -1f;
            transform.localScale = new Vector3(
                Mathf.Abs(_originalScale.x) * facingX,
                _originalScale.y,
                _originalScale.z
            );

            // Random slow horizontal drift direction
            _driftDirection = Random.value > 0.5f ? 1f : -1f;

            // Random tumble direction (some float clockwise, some counter)
            _rotationDir = Random.value > 0.5f ? 1f : -1f;
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            // --- Vertical float (sine wave around spawn Y) ---
            float newY = _originY + Mathf.Sin((Time.time + _timeOffset) * floatFrequency) * floatAmplitude;

            // --- Slow horizontal drift ---
            float newX = transform.position.x + _driftDirection * driftSpeed * Time.deltaTime;

            transform.position = new Vector3(newX, newY, transform.position.z);

            // --- Slow tumble rotation ---
            transform.Rotate(0f, 0f, _rotationDir * rotationSpeed * Time.deltaTime);
        }
    }
}
