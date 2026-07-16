using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a paddle to the player's chosen hand (left or right controller).
///
/// The paddle is NOT parented to the controller. Instead, its kinematic
/// rigidbody follows the controller pose with MovePosition/MoveRotation every
/// physics step. This way PhysX knows the paddle's velocity, contacts against
/// the ball are precise even on fast swings (no tunneling through a teleporting
/// transform), and speculative CCD works as intended.
/// </summary>
public class HandPaddleBinding : MonoBehaviour
{
    [Header("Hand Selection")]
    [SerializeField] private bool useLeftHand = true; // true = LEFT hand, false = RIGHT hand
    [Tooltip("Choose which hand this player wants to use for their paddle")]

    [Header("Visual References")]
    [SerializeField] private GameObject paddle; // The paddle mesh this player controls
    [SerializeField] private bool hideControllerModel = true; // Hide the default controller visual

    [Header("Grip Adjustment")]
    [Tooltip("Position offset in the controller's local space (adjust so the handle sits in the fist).")]
    [SerializeField] private Vector3 gripPositionOffset = Vector3.zero;
    [Tooltip("Rotation offset in degrees (adjust so the paddle face points forward naturally).")]
    [SerializeField] private Vector3 gripRotationOffset = Vector3.zero;

    [Header("AI Setting")]
    [SerializeField] private bool isAI = false; // If true, do not bind to controllers (AI moves paddle)

    // Transforms whose name contains one of these are helpers (teleport stabilizers,
    // attach points, visuals...) and NOT the actual tracked controller pose.
    private static readonly string[] ExcludedNameParts =
    {
        "Visual", "Stabilized", "Teleport", "Attach", "Origin", "Base", "Model", "Universal", "Thumbstick"
    };

    private Transform controllerTransform;
    private Rigidbody paddleRb;
    private bool isSetup = false;
    private float nextRetryTime = 0f;
    private const float RETRY_INTERVAL = 1.0f;

    private void Start()
    {
        SetupHandPaddleBinding();
    }

    private void Update()
    {
        // Retry until the XR rig exists; also rebind if the controller vanished.
        if (isAI) return;
        if ((!isSetup || controllerTransform == null) && Time.time >= nextRetryTime)
        {
            nextRetryTime = Time.time + RETRY_INTERVAL;
            isSetup = false;
            SetupHandPaddleBinding();
        }
    }

    private void FixedUpdate()
    {
        if (!isSetup || isAI || controllerTransform == null || paddleRb == null) return;

        // Follow the controller pose through physics so contacts stay precise.
        Vector3 targetPosition = controllerTransform.TransformPoint(gripPositionOffset);
        Quaternion targetRotation = controllerTransform.rotation * Quaternion.Euler(gripRotationOffset);
        paddleRb.MovePosition(targetPosition);
        paddleRb.MoveRotation(targetRotation);
    }

    private void SetupHandPaddleBinding()
    {
        if (paddle == null)
        {
            Debug.LogError($"[HandPaddleBinding] No paddle assigned to {gameObject.name}");
            return;
        }

        if (isAI)
        {
            isSetup = true;
            return;
        }

        // The paddle must be free (not parented to a hand) and physical.
        if (paddle.transform.parent != null)
        {
            paddle.transform.SetParent(null, true);
        }
        paddleRb = paddle.GetComponent<Rigidbody>();
        if (paddleRb == null) paddleRb = paddle.AddComponent<Rigidbody>();
        paddleRb.isKinematic = true;
        paddleRb.useGravity = false;
        paddleRb.interpolation = RigidbodyInterpolation.Interpolate;
        paddleRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (FindController())
        {
            isSetup = true;
            // Snap immediately so the paddle doesn't fly across the room on bind.
            paddle.transform.SetPositionAndRotation(
                controllerTransform.TransformPoint(gripPositionOffset),
                controllerTransform.rotation * Quaternion.Euler(gripRotationOffset));

            if (hideControllerModel) HideControllerVisuals();

            Debug.Log($"[HandPaddleBinding] {gameObject.name} paddle follows '{controllerTransform.name}' ({(useLeftHand ? "LEFT" : "RIGHT")} hand)");
        }
    }

    /// <summary>
    /// Find the real tracked XR controller. The hierarchy contains many transforms
    /// named "* Controller *" (visuals, stabilized attach points, teleport
    /// origins...); candidates are scored so we always pick the tracked hand pose.
    /// </summary>
    private bool FindController()
    {
        string side = useLeftHand ? "Left" : "Right";
        string exactName = $"{side} Controller";

        Transform best = null;
        int bestScore = int.MinValue;

        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            string n = t.name;
            if (!n.Contains(side)) continue;
            if (!n.Contains("Controller") && !n.Contains("Hand")) continue;

            bool excluded = false;
            foreach (string part in ExcludedNameParts)
            {
                if (n.Contains(part)) { excluded = true; break; }
            }
            if (excluded) continue;

            int score = 0;
            if (n == exactName) score += 10;                                                        // "Left Controller" / "Right Controller"
            if (t.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>() != null) score += 5; // actual tracked pose
            if (n.Contains("Controller")) score += 1;

            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        controllerTransform = best;
        return best != null;
    }

    /// <summary>
    /// Hide the default controller 3D model so only the paddle is visible,
    /// making the paddle feel like it replaces the hand.
    /// </summary>
    private void HideControllerVisuals()
    {
        foreach (Transform child in controllerTransform)
        {
            if (child.gameObject == paddle) continue;
            if (paddle.transform.IsChildOf(child)) continue;

            // Only hide visual children (renderers); keep interactors and logic alive.
            if (child.GetComponentInChildren<Renderer>() != null &&
                child.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>() == null)
            {
                child.gameObject.SetActive(false);
            }
        }

        foreach (Renderer renderer in controllerTransform.GetComponents<Renderer>())
        {
            renderer.enabled = false;
        }

        foreach (Image image in controllerTransform.GetComponentsInChildren<Image>())
        {
            if (!image.transform.IsChildOf(paddle.transform))
            {
                image.enabled = false;
            }
        }
    }

    public bool IsLeftHand() => useLeftHand;
    public Transform GetControllerTransform() => controllerTransform;
    public bool IsSetup() => isSetup;

    /// <summary>Switch between left and right hand at runtime.</summary>
    public void SwitchHand()
    {
        useLeftHand = !useLeftHand;
        isSetup = false;
        SetupHandPaddleBinding();
    }
}
