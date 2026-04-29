using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Controls paddle movement in VR by tracking the player's hand/controller position.
/// Works with both XR Controllers and Hand Tracking on Quest 2.
/// </summary>
public class PaddleController : MonoBehaviour
{
    [Header("Controller Reference")]
    [SerializeField] private Transform controllerTransform;
    [SerializeField] private XRDirectInteractor interactor; // Optional: for pinch/grab detection

    [Header("Paddle Constraints")]
    [SerializeField] private bool constrainToTable = true;
    [SerializeField] private float minY = -0.5f;
    [SerializeField] private float maxY = 0.5f;
    [SerializeField] private float minZ = -1.0f;
    [SerializeField] private float maxZ = 1.0f;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothFollowing = true;
    [SerializeField] private float smoothSpeed = 15f;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool isTracked = false;

    private void Start()
    {
        // Get Rigidbody if exists (for smooth movement)
        rb = GetComponent<Rigidbody>();

        // If no controller assigned, try to find one
        if (controllerTransform == null)
        {
            Debug.LogWarning($"[PaddleController] No controller assigned to {gameObject.name}. Please assign in Inspector!");
        }

        targetPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (controllerTransform == null)
            return;

        // Get controller position
        Vector3 newPosition = controllerTransform.position;

        // Apply constraints if enabled
        if (constrainToTable)
        {
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
            // X position is usually fixed for each player (left/right side)
        }

        targetPosition = newPosition;

        // Move paddle
        if (useSmoothFollowing && rb != null)
        {
            // Use Rigidbody for smooth physics-aware movement
            rb.linearVelocity = (targetPosition - transform.position) * smoothSpeed;
        }
        else
        {
            // Direct movement (teleport-like, but still smooth)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * smoothSpeed);
        }

        // Also match rotation (optional, but realistic)
        transform.rotation = controllerTransform.rotation;

        isTracked = true;
    }

    /// <summary>
    /// Manually set the controller this paddle should follow
    /// Call this at runtime if controller wasn't assigned in Inspector
    /// </summary>
    public void SetController(Transform controller)
    {
        controllerTransform = controller;
        if (controllerTransform != null)
        {
            Debug.Log($"[PaddleController] {gameObject.name} now tracking {controller.name}");
        }
    }

    /// <summary>
    /// Get the controller this paddle is tracking
    /// </summary>
    public Transform GetController()
    {
        return controllerTransform;
    }

    /// <summary>
    /// Check if controller is being tracked
    /// </summary>
    public bool IsTracked()
    {
        return isTracked && controllerTransform != null;
    }

    /// <summary>
    /// Update constraint bounds dynamically
    /// </summary>
    public void SetConstraints(float minY, float maxY, float minZ, float maxZ)
    {
        this.minY = minY;
        this.maxY = maxY;
        this.minZ = minZ;
        this.maxZ = maxZ;
    }

    /// <summary>
    /// Enable/disable constraints
    /// </summary>
    public void SetConstraintsEnabled(bool enabled)
    {
        constrainToTable = enabled;
    }
}
