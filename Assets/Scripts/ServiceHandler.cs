using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Lets the player grab the levitating ball and toss it to serve.
///
/// The throw velocity is measured by sampling the ball position over the last
/// ~90 ms of the hold (smoothed), which is far more reliable in VR than a
/// single-frame delta, then handed to Ball.NotifyServed so it wins over
/// whatever XRGrabInteractable applies on detach.
/// </summary>
public class ServiceHandler : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Ball ball;

    [Header("Throw")]
    [Tooltip("Time window (s) used to average the hand velocity at release.")]
    [SerializeField] private float velocitySampleWindow = 0.09f;
    [SerializeField] private float maxThrowSpeed = 7f;

    private const int SampleCount = 16;
    private readonly Vector3[] positionSamples = new Vector3[SampleCount];
    private readonly float[] timeSamples = new float[SampleCount];
    private int sampleIndex;
    private bool isHeld;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (ball == null) ball = GetComponent<Ball>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
        else
        {
            Debug.LogWarning("[ServiceHandler] No XRGrabInteractable on the ball - the player won't be able to grab it.");
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void Update()
    {
        if (!isHeld) return;
        sampleIndex = (sampleIndex + 1) % SampleCount;
        positionSamples[sampleIndex] = transform.position;
        timeSamples[sampleIndex] = Time.time;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
        for (int i = 0; i < SampleCount; i++)
        {
            positionSamples[i] = transform.position;
            timeSamples[i] = Time.time;
        }
        if (ball != null) ball.NotifyHeld();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
        Vector3 throwVelocity = ComputeSmoothedVelocity();
        if (ball != null) ball.NotifyServed(throwVelocity);
        else if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = throwVelocity;
        }
    }

    /// <summary>Average velocity over the sample window ending now.</summary>
    private Vector3 ComputeSmoothedVelocity()
    {
        Vector3 newest = positionSamples[sampleIndex];
        float newestTime = timeSamples[sampleIndex];

        // Find the oldest sample still inside the window.
        int oldest = sampleIndex;
        for (int i = 1; i < SampleCount; i++)
        {
            int index = (sampleIndex + i) % SampleCount; // walks from the oldest entry toward the newest
            if (newestTime - timeSamples[index] <= velocitySampleWindow)
            {
                oldest = index;
                break;
            }
        }

        float dt = newestTime - timeSamples[oldest];
        if (dt < 0.005f) return Vector3.zero;

        Vector3 velocity = (newest - positionSamples[oldest]) / dt;
        return Vector3.ClampMagnitude(velocity, maxThrowSpeed);
    }

    public bool IsHeld() => isHeld;
}
