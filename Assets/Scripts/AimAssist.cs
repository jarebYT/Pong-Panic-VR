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
    [SerializeField] private float assistForceMultiplier = 0.12f;  // Subtle correction
    [SerializeField] private float tableWidth = 1.525f;            // Standard ping pong table width
    [SerializeField] private float detectionDistance = 3f;         // How far to look ahead
    [SerializeField] private Vector3 tableCenter = Vector3.zero;   // Center of table in world space

    [Header("Gravity Assist")]
    [SerializeField] private bool enableGravityAssist = true;

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
            return;
        }

        // Apply gravity assist by modifying physics
        if (enableGravityAssist)
        {
            ApplyGravityAssist();
        }

        Debug.Log($"[AimAssist] Initialized - Aim Assist: {enableAimAssist}, Gravity Assist: {enableGravityAssist}");
    }

    private void FixedUpdate()
    {
        if (!enableAimAssist || rb == null)
            return;

        // Check if ball is moving and in a meaningful trajectory
        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.5f)
            return;

        // Apply aim assist
        if (ShouldApplyCorrection())
        {
            ApplyTrajectoryCorrection();
        }
    }

    /// <summary>
    /// Apply stronger gravity for easier gameplay
    /// </summary>
    private void ApplyGravityAssist()
    {
        if (rb != null)
        {
            // Increase downward gravity
            rb.mass = Mathf.Max(rb.mass * 0.5f, 0.001f); // Lighter ball falls faster
            Debug.Log($"[AimAssist] Gravity assist applied - mass reduced to {rb.mass}");
        }
    }

    /// <summary>
    /// Check if ball needs trajectory correction
    /// </summary>
    private bool ShouldApplyCorrection()
    {
        Vector3 velocity = rb.linearVelocity;

        // Only assist if ball is traveling relatively horizontally (not straight down)
        if (Mathf.Abs(velocity.y) > Mathf.Abs(velocity.x) * 2f)
            return false;

        // Check if ball is nearing the table (predict 0.15 seconds ahead)
        Vector3 predictedPosition = transform.position + (velocity * 0.15f);
        
        // Ball should be approaching table area (Y close to 0, X and Z in bounds)
        return Mathf.Abs(predictedPosition.y) < 1f && 
               Mathf.Abs(predictedPosition.x - tableCenter.x) < detectionDistance;
    }

    /// <summary>
    /// Apply subtle correction to keep ball on table
    /// </summary>
    private void ApplyTrajectoryCorrection()
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 correctionForce = Vector3.zero;

        // Calculate distance from table center
        float distanceFromCenter = transform.position.x - tableCenter.x;

        // If ball is veering too far to the side, gently push it back toward center
        if (Mathf.Abs(distanceFromCenter) > tableWidth / 4f)
        {
            float correctionDirection = distanceFromCenter > 0 ? -1f : 1f;
            float correctionStrength = Mathf.Min(Mathf.Abs(distanceFromCenter) / tableWidth, 1f);
            
            correctionForce.x = correctionDirection * velocity.magnitude * assistForceMultiplier * correctionStrength;
            
            rb.AddForce(correctionForce, ForceMode.Acceleration);
            
            Debug.DrawRay(transform.position, correctionForce.normalized * 0.5f, Color.yellow);
        }
    }

    /// <summary>
    /// Enable or disable aim assist dynamically
    /// </summary>
    public void SetAimAssistEnabled(bool enabled)
    {
        enableAimAssist = enabled;
        Debug.Log($"[AimAssist] Aim assist: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    /// <summary>
    /// Enable or disable gravity assist dynamically
    /// </summary>
    public void SetGravityAssistEnabled(bool enabled)
    {
        enableGravityAssist = enabled;
        Debug.Log($"[AimAssist] Gravity assist: {(enabled ? "ENABLED" : "DISABLED")}");
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
