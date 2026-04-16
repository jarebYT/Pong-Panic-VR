using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Handles ball serving in VR.
/// Allows player to grab and hold the ball, then release to serve.
/// </summary>
public class ServiceHandler : MonoBehaviour
{
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    [SerializeField] private PingPongManager pingPongManager;
    [SerializeField] private Rigidbody rb;
    
    private bool isHeld = false;
    private Vector3 lastFramePosition;
    private bool hasBeenServed = false;

    private void Start()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnGrabbed);
            grabInteractable.deactivated.AddListener(OnReleased);
        }

        lastFramePosition = transform.position;
        hasBeenServed = false;
    }

    /// <summary>
    /// Called when ball is grabbed
    /// </summary>
    private void OnGrabbed(ActivateEventArgs args)
    {
        isHeld = true;
        hasBeenServed = false;

        // Disable physics while held
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Debug.Log("Ball grabbed for serve");
    }

    /// <summary>
    /// Called when ball is released
    /// </summary>
    private void OnReleased(DeactivateEventArgs args)
    {
        isHeld = false;

        if (rb != null)
        {
            rb.isKinematic = false;

            // Calculate throw velocity from hand movement
            Vector3 throwVelocity = (transform.position - lastFramePosition) / Time.deltaTime;
            rb.linearVelocity = throwVelocity;
        }

        hasBeenServed = true;
        Debug.Log("Ball served!");
    }

    private void FixedUpdate()
    {
        if (isHeld)
        {
            lastFramePosition = transform.position;
        }
    }

    /// <summary>
    /// Check if ball is currently held
    /// </summary>
    public bool IsHeld() => isHeld;

    /// <summary>
    /// Check if ball has been served
    /// </summary>
    public bool HasBeenServed() => hasBeenServed;

    /// <summary>
    /// Reset service state for new rally
    /// </summary>
    public void Reset()
    {
        isHeld = false;
        hasBeenServed = false;
        lastFramePosition = transform.position;
    }
}
