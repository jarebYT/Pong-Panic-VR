using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a paddle to a player's hand (controller).
/// Each player chooses which hand to use (left or right).
/// The paddle becomes parented to the chosen controller and follows it exactly.
/// </summary>
public class HandPaddleBinding : MonoBehaviour
{
    [Header("Hand Selection")]
    [SerializeField] private bool useLeftHand = true; // true = use LEFT hand, false = use RIGHT hand
    [Tooltip("Choose which hand this player wants to use for their paddle")]
    
    [Header("Visual References")]
    [SerializeField] private GameObject paddle; // The paddle mesh this player controls
    [SerializeField] private bool hideControllerModel = true; // Hide the default controller visual

    private Transform controllerTransform;
    private GameObject controllerModel;
    private bool isSetup = false;

    private void Start()
    {
        SetupHandPaddleBinding();
    }

    /// <summary>
    /// Setup: Find the chosen controller and parent paddle to it.
    /// Each player can choose left or right hand independently via UseLeftHand.
    /// </summary>
    private void SetupHandPaddleBinding()
    {
        if (paddle == null)
        {
            Debug.LogError($"[HandPaddleBinding] No paddle assigned to {gameObject.name}");
            return;
        }

        // Find the controller (left or right) based on player's choice
        if (FindAndBindController())
        {
            isSetup = true;
            Debug.Log($"[HandPaddleBinding] {gameObject.name} paddle bound to {(useLeftHand ? "LEFT" : "RIGHT")} hand (player's choice)");
        }
        else
        {
            Debug.LogError($"[HandPaddleBinding] Could not find {(useLeftHand ? "LEFT" : "RIGHT")} controller");
        }
    }

    /// <summary>
    /// Find the XR controller (left or right, based on player's UseLeftHand choice) 
    /// and parent paddle to it
    /// </summary>
    private bool FindAndBindController()
    {
        // Search for left or right controller based on player's choice
        string searchName = useLeftHand ? "Left" : "Right";
        
        // Try to find by name containing "Left" or "Right"
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            if (t.name.Contains(searchName) && t.name.Contains("Controller"))
            {
                controllerTransform = t;
                BindPaddleToController();
                return true;
            }
        }

        // If not found by Controller name, try other XR patterns
        foreach (Transform t in allTransforms)
        {
            if (t.name.Contains(searchName) && (t.name.Contains("Hand") || t.name.Contains("Grip")))
            {
                controllerTransform = t;
                BindPaddleToController();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Parent paddle to controller and optionally hide controller visual
    /// </summary>
    private void BindPaddleToController()
    {
        // Parent the paddle to the controller
        paddle.transform.SetParent(controllerTransform);
        
        // Reset local position/rotation so paddle aligns with hand
        paddle.transform.localPosition = Vector3.zero;
        paddle.transform.localRotation = Quaternion.identity;

        Debug.Log($"[HandPaddleBinding] Paddle parented to {controllerTransform.name}");

        // Optionally hide the controller model
        if (hideControllerModel)
        {
            HideControllerVisuals();
        }
    }

    /// <summary>
    /// Hide the default controller 3D model so only the paddle is visible.
    /// This makes the paddle feel like YOUR hand/paddle instead of seeing both.
    /// </summary>
    private void HideControllerVisuals()
    {
        // Hide all children of controller EXCEPT the paddle
        foreach (Transform child in controllerTransform)
        {
            // Skip if this is the paddle
            if (child.gameObject == paddle)
                continue;

            // Skip if paddle is a child of this transform
            if (paddle.transform.IsChildOf(child))
                continue;

            // Disable this child and all its renderers
            child.gameObject.SetActive(false);
        }

        // Also disable renderers on the controller itself
        Renderer[] renderers = controllerTransform.GetComponents<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Hide UI elements if any
        Image[] images = controllerTransform.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (!image.transform.IsChildOf(paddle.transform))
            {
                image.enabled = false;
            }
        }

        Debug.Log($"[HandPaddleBinding] Controller model hidden - only paddle visible");
    }

    /// <summary>
    /// Get which hand this paddle is bound to
    /// </summary>
    public bool IsLeftHand() => useLeftHand;

    /// <summary>
    /// Get the controller this paddle is bound to
    /// </summary>
    public Transform GetControllerTransform() => controllerTransform;

    /// <summary>
    /// Check if binding was successful
    /// </summary>
    public bool IsSetup() => isSetup;

    /// <summary>
    /// Switch between left and right hand at runtime.
    /// Useful if a player wants to change hands after starting.
    /// </summary>
    public void SwitchHand()
    {
        if (paddle.transform.parent != null)
        {
            paddle.transform.SetParent(null);
        }
        
        useLeftHand = !useLeftHand;
        Debug.Log($"[HandPaddleBinding] {gameObject.name} switching hands...");
        SetupHandPaddleBinding();
    }
}
