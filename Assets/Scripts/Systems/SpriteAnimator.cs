using UnityEngine;

namespace DefenderRemake.Systems
{
    /// <summary>
    /// Lightweight frame-by-frame sprite animator.
    /// Attach to any GameObject with a SpriteRenderer.
    /// Reusable for boost effects, enemy blinks, boss pulses, drone orbits, etc.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [Header("Animation Frames")]
        [SerializeField, Tooltip("Sprites to cycle through in order")]
        private Sprite[] frames;

        [SerializeField, Tooltip("How many frames per second to cycle (e.g. 10 = retro flicker, 24 = smooth)")]
        private float fps = 10f;

        private SpriteRenderer _renderer;
        private int _currentFrame;
        private float _frameTimer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            _frameTimer += Time.deltaTime;

            if (_frameTimer >= 1f / fps)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % frames.Length;
                _renderer.sprite = frames[_currentFrame];
            }
        }

        private void OnEnable()
        {
            // Reset to first frame whenever the effect is turned on
            _currentFrame = 0;
            _frameTimer = 0f;

            if (_renderer != null && frames != null && frames.Length > 0)
            {
                _renderer.sprite = frames[0];
            }
        }
    }
}
