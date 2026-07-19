using UnityEngine;

namespace DefenderRemake.Gameplay
{
    public class FloatingSpaceProp3D : MonoBehaviour
    {
        [Header("Tumble Settings")]
        public float maxRotationSpeed = 15f;
        
        [Header("Drift Settings")]
        public float maxDriftDistance = 2f;
        public float maxDriftSpeed = 0.5f;

        private Vector3 _startPos;
        private Vector3 _rotationSpeeds;
        
        // Sine wave offsets to prevent all objects moving in unison
        private Vector3 _timeOffsets;
        private Vector3 _driftSpeeds;
        private Vector3 _driftDistances;

        private Rigidbody _rb;
        private bool _usesPhysics = false;

        private void Start()
        {
            _startPos = transform.position;
            _rb = GetComponent<Rigidbody>();
            _usesPhysics = _rb != null;

            // Randomize tumbling
            _rotationSpeeds = new Vector3(
                Random.Range(-maxRotationSpeed, maxRotationSpeed),
                Random.Range(-maxRotationSpeed, maxRotationSpeed),
                Random.Range(-maxRotationSpeed, maxRotationSpeed)
            );

            if (_usesPhysics)
            {
                // Apply a single burst of physical velocity so the engine takes over entirely!
                _rb.linearVelocity = new Vector3(
                    Random.Range(-maxDriftSpeed, maxDriftSpeed),
                    Random.Range(-maxDriftSpeed, maxDriftSpeed),
                    Random.Range(-maxDriftSpeed, maxDriftSpeed)
                );
                
                // Convert rotation speed to angular velocity
                _rb.angularVelocity = _rotationSpeeds * Mathf.Deg2Rad;
            }
            else
            {
                // Sine wave drift setup for non-physics objects
                _driftDistances = new Vector3(
                    Random.Range(0, maxDriftDistance),
                    Random.Range(0, maxDriftDistance),
                    Random.Range(0, maxDriftDistance)
                );

                _driftSpeeds = new Vector3(
                    Random.Range(maxDriftSpeed * 0.5f, maxDriftSpeed),
                    Random.Range(maxDriftSpeed * 0.5f, maxDriftSpeed),
                    Random.Range(maxDriftSpeed * 0.5f, maxDriftSpeed)
                );

                _timeOffsets = new Vector3(
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f)
                );
            }
        }

        private void Update()
        {
            if (_usesPhysics) return; // Physics engine handles all movement and rotation now!

            // Tumble
            transform.Rotate(_rotationSpeeds * Time.deltaTime, Space.Self);

            // Drift
            float newX = _startPos.x + Mathf.Sin(Time.time * _driftSpeeds.x + _timeOffsets.x) * _driftDistances.x;
            float newY = _startPos.y + Mathf.Sin(Time.time * _driftSpeeds.y + _timeOffsets.y) * _driftDistances.y;
            float newZ = _startPos.z + Mathf.Sin(Time.time * _driftSpeeds.z + _timeOffsets.z) * _driftDistances.z;

            transform.position = new Vector3(newX, newY, newZ);
        }
    }
}
