using UnityEngine;

public class Turret : MonoBehaviour
{
    private void RotateTurret(Transform rotPoint, Transform target, float maxRotationAngle, float rotationSpeed)
    {
        // Get direction vector from turret to target
        Vector3 directionToTarget = target.position - rotPoint.position;
        directionToTarget.z = 0; // Ensure it's 2D

        // Get current turret angle (local space)
        float currentAngle = rotPoint.localEulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f; // Convert to signed angle (-180 to 180)

        // Get desired angle towards target
        float desiredAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        if (desiredAngle > 180f) desiredAngle -= 360f; // Convert to signed angle (-180 to 180)

        // Clamp the desired angle within the max rotation limits
        float clampedAngle = Mathf.Clamp(desiredAngle, -maxRotationAngle, maxRotationAngle);

        // Determine the shortest rotation direction
        float angleDifference = Mathf.DeltaAngle(currentAngle, clampedAngle);

        // If the shortest path violates the clamp, take the long route
        if (Mathf.Abs(desiredAngle) > maxRotationAngle)
        {
            if (angleDifference > 0)
                angleDifference = -(360f - Mathf.Abs(angleDifference)); // Force long route
            else
                angleDifference = (360f - Mathf.Abs(angleDifference)); // Force long route
        }

        // Rotate linearly based on rotationSpeed
        float step = rotationSpeed * Time.deltaTime;
        float finalAngle = Mathf.MoveTowardsAngle(currentAngle, currentAngle + angleDifference, step);

        // Apply the new rotation
        rotPoint.localRotation = Quaternion.Euler(0, 0, finalAngle);
    }
}
