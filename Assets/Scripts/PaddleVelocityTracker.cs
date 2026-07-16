using UnityEngine;

/// <summary>
/// Tracks the velocity of a paddle moved by a VR controller (or by the AI).
/// The velocity is smoothed over a short time window instead of a raw
/// frame-to-frame derivative: VR tracking is noisy, and a single-frame sample
/// at the moment of impact made hits feel random. The smoothed value
/// represents the actual swing the player performed.
/// </summary>
public class PaddleVelocityTracker : MonoBehaviour
{
    [Tooltip("Time window (s) over which the swing velocity is averaged.")]
    [SerializeField] private float sampleWindow = 0.08f;

    private const int SampleCount = 24;
    private readonly Vector3[] positions = new Vector3[SampleCount];
    private readonly float[] times = new float[SampleCount];
    private int head;

    private Quaternion lastRotation;
    private float lastRotationTime;

    /// <summary>Smoothed swing velocity (m/s).</summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>Approximate angular velocity (rad/s), cosmetic use only.</summary>
    public Vector3 AngularVelocity { get; private set; }

    private void OnEnable()
    {
        for (int i = 0; i < SampleCount; i++)
        {
            positions[i] = transform.position;
            times[i] = Time.time;
        }
        lastRotation = transform.rotation;
        lastRotationTime = Time.time;
        Velocity = Vector3.zero;
        AngularVelocity = Vector3.zero;
    }

    private void Update()
    {
        // Record position history.
        head = (head + 1) % SampleCount;
        positions[head] = transform.position;
        times[head] = Time.time;

        // Smoothed linear velocity over the window.
        float newestTime = times[head];
        int oldest = head;
        for (int i = 1; i < SampleCount; i++)
        {
            int index = (head + i) % SampleCount; // from the oldest entry toward the newest
            if (newestTime - times[index] <= sampleWindow)
            {
                oldest = index;
                break;
            }
        }

        float dt = newestTime - times[oldest];
        Velocity = dt > 0.004f ? (positions[head] - positions[oldest]) / dt : Vector3.zero;

        // Simple angular velocity (kept for cosmetic spin).
        float rotDt = Time.time - lastRotationTime;
        if (rotDt > 0.004f)
        {
            Quaternion delta = transform.rotation * Quaternion.Inverse(lastRotation);
            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            AngularVelocity = axis * (angle * Mathf.Deg2Rad / rotDt);
            lastRotation = transform.rotation;
            lastRotationTime = Time.time;
        }
    }
}
