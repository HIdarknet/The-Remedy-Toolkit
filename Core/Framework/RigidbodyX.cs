using Remedy.Framework;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyX : MonoBehaviour
{
    // Reference to the Rigidbody component.
    public Rigidbody Rigidbody => this.GetCachedComponent<Rigidbody>();

    // Customizable fields.
    [Tooltip("Time in seconds to complete the rotation.")]
    public float TorqueTime = 1f;
    [Tooltip("Maximum torque magnitude to use.")]
    public float MaxTorque = 10f;
    [Tooltip("Damping factor to counteract overshooting.")]
    public float TorqueDampFactor = 1f;
    [Tooltip("Threshold angle (in degrees) below which torque won't be applied.")]
    public float AngleThreshold = 0.1f;

    private Vector3 _torqueDirection;

    /// <summary>
    /// Gradually applies torque to the Rigidbody so that its forward direction rotates
    /// towards the given target direction over time.
    /// </summary>
    /// <param name="targetDirection">The desired direction to face (in world space).</param>
    public void TorqueTo(Vector3 targetDirection)
    {
        Vector3 currentDirection = transform.forward.normalized;
        Vector3 desiredDirection = targetDirection.normalized;

        float angleDifference = Vector3.Angle(currentDirection, desiredDirection);
        if (angleDifference < AngleThreshold)
            return; // The object is close enough to the target direction.

        // Determine the axis of rotation using the cross product.
        Vector3 rotationAxis = Vector3.Cross(currentDirection, desiredDirection);
        if (rotationAxis == Vector3.zero)
            return; // The vectors are parallel (or one is zero).

        // Calculate the torque magnitude.
        // We use the angle difference scaled by maxTorque and further divided by torqueTime.
        // (Shorter torqueTime means more aggressive torque.)
        float torqueMagnitude = (MaxTorque * (angleDifference / 180f)) / TorqueTime;

        // Get the component of the current angular velocity along the torque axis.
        Vector3 angularVelocityOnAxis = Vector3.Project(Rigidbody.angularVelocity, rotationAxis);

        // Apply damping to help prevent overshoot.
        Vector3 dampingTorque = TorqueDampFactor * angularVelocityOnAxis;

        // Compute the final torque vector.
        Vector3 finalTorque = rotationAxis.normalized * torqueMagnitude - dampingTorque;

        // Apply the torque in every FixedUpdate call.
        Rigidbody.AddTorque(finalTorque, ForceMode.Force);
    }

    // For demonstration purposes, this could be called each FixedUpdate.
    // In your real use-case, you might have a target direction stored or calculated elsewhere.
    private void FixedUpdate()
    {
        // Example: rotate towards the world forward.
        TorqueTo(Vector3.forward);
    }
}
