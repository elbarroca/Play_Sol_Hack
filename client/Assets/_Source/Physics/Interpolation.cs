using UnityEngine;

namespace PlaceholderHack.Physics
{
    public class Interpolation : MonoBehaviour
    {
        [Header("Interpolation Settings")]
        [SerializeField] private float interpolationSpeed = 10f;
        [SerializeField] private bool useFixedUpdate = true;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool hasTargetPosition;
        private bool hasTargetRotation;

        private void Start()
        {
            // Initialize with current transform
            targetPosition = transform.position;
            targetRotation = transform.rotation;
            hasTargetPosition = true;
            hasTargetRotation = true;
        }

        private void Update()
        {
            if (!useFixedUpdate)
            {
                Interpolate();
            }
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
            {
                Interpolate();
            }
        }

        private void Interpolate()
        {
            if (hasTargetPosition)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition,
                    interpolationSpeed * (useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime));
            }

            if (hasTargetRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    interpolationSpeed * (useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime));
            }
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            hasTargetPosition = true;
        }

        public void SetTargetRotation(Quaternion rotation)
        {
            targetRotation = rotation;
            hasTargetRotation = true;
        }

        public void SetTargetTransform(Vector3 position, Quaternion rotation)
        {
            SetTargetPosition(position);
            SetTargetRotation(rotation);
        }

        public void SnapToTarget()
        {
            if (hasTargetPosition)
            {
                transform.position = targetPosition;
            }

            if (hasTargetRotation)
            {
                transform.rotation = targetRotation;
            }
        }

        public bool IsAtTarget(float positionThreshold = 0.01f, float rotationThreshold = 1f)
        {
            bool positionReached = !hasTargetPosition ||
                Vector3.Distance(transform.position, targetPosition) < positionThreshold;

            bool rotationReached = !hasTargetRotation ||
                Quaternion.Angle(transform.rotation, targetRotation) < rotationThreshold;

            return positionReached && rotationReached;
        }
    }
}