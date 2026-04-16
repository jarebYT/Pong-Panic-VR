using UnityEngine;

/// <summary>
/// Ball trajectory correction system for VR precision.
/// Applies subtle force corrections to keep ball on table when trajectory is poor.
/// This compensates for VR hand imprecision without making the game trivial.
/// </summary>
public class AimAssist : MonoBehaviour
{
    [Header("Aim Assist Settings")]
    [SerializeField] private bool enableAimAssist = true;
    [SerializeField] private float assistForceMultiplier = 0.15f;  // Subtle correction
    [SerializeField] private float tableWidth = 1.525f;            // Standard ping pong table width
    [SerializeField] private float detectionDistance = 2f;         // How far to look ahead
    [SerializeField] private Vector3 tableCenter = Vector3.zero;   // Center of table in world space

    private Rigidbody rb;
    private Ball ball;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ball = GetComponent<Ball>();

        if (rb == null)
        {
            Debug.LogWarning("AimAssist: Rigidbody not found on ball");
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!enableAimAssist || rb == null || rb.linearVelocity.magnitude < 0.1f)
            return;

        // Only assist if ball is traveling horizontally toward table
        if (IsNearingTable())
        {
            ApplyTrajectoryCorrection();
        }
    }

    /// <summary>
    /// Check if ball is nearing the table
    /// </summary>
    private bool IsNearingTable()
    {
        // Predict where ball will be in 0.1 seconds
        Vector3 predictedPosition = transform.position + (rb.linearVelocity * 0.1f);

        // Check if ball is approaching table horizontally
        float distanceToTableCenter = Mathf.Abs(predictedPosition.x - tableCenter.x);

        return distanceToTableCenter < detectionDistance && 
               rb.linearVelocity.y < 2f; // Not going straight up
    }

    /// <summary>
    /// Apply subtle correction to trajectory
    /// </summary>
    private void ApplyTrajectoryCorrection()
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 correctionForce = Vector3.zero;

        // Calculate distance from table center
        float distanceFromCenter = Mathf.Abs(transform.position.x - tableCenter.x);
        float normalizedDistance = distanceFromCenter / (tableWidth / 2f);

        // If ball is veering too far to the side, gently push it back
        if (distanceFromCenter > tableWidth / 3f)
        {
            float correctionDirection = transform.position.x > tableCenter.x ? -1f : 1f;
            correctionForce.x = correctionDirection * velocity.magnitude * assistForceMultiplier;

            // Slightly dampen vertical velocity to keep ball from bouncing high
            correctionForce.y = -0.1f * velocity.y * assistForceMultiplier;

            rb.AddForce(correctionForce, ForceMode.Acceleration);

            Debug.DrawRay(transform.position, correctionForce * 0.5f, Color.yellow);
        }

        // Also apply slight z-correction if going too far back (safety net)
        if (velocity.z > 5f)
        {
            correctionForce.z = -velocity.z * assistForceMultiplier * 0.5f;
            rb.AddForce(correctionForce, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Enable or disable aim assist dynamically
    /// </summary>
    public void SetAimAssistEnabled(bool enabled)
    {
        enableAimAssist = enabled;
    }

    /// <summary>
    /// Adjust assist strength (0-1, where 1 is maximum assistance)
    /// </summary>
    public void SetAssistStrength(float strength)
    {
        assistForceMultiplier = Mathf.Clamp01(strength) * 0.25f;
    }

    /// <summary>
    /// Set table position for assist calculations
    /// </summary>
    public void SetTableCenter(Vector3 center)
    {
        tableCenter = center;
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enableAimAssist) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(tableCenter, new Vector3(tableWidth, 0.1f, 2f));
    }
}
