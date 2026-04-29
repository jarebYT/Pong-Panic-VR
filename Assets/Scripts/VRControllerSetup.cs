using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Manages XR controller setup and binding to players.
/// This script finds the left/right XR controllers and assigns them to the appropriate paddles.
/// Essential for VR gameplay on Quest 2.
/// </summary>
public class VRControllerSetup : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("Controller Search Settings")]
    [SerializeField] private bool autoFindControllers = true;
    [SerializeField] private bool useHandTracking = false; // Set to true if using hand tracking instead of controllers

    private Transform leftControllerTransform;
    private Transform rightControllerTransform;

    private void Start()
    {
        Debug.LogWarning("[VRControllerSetup] OBSOLETE - This system is no longer used. Use HandPaddleBinding instead!");
        // Old system - disabled
        // if (player1 == null || player2 == null)
        // {
        //     Debug.LogError("[VRControllerSetup] Player1 and Player2 must be assigned in Inspector!");
        //     return;
        // }
        // if (autoFindControllers)
        // {
        //     FindAndSetupControllers();
        // }
    }

    /// <summary>
    /// Automatically find XR controllers in the scene and assign them to paddles
    /// [DEPRECATED] - Use HandPaddleBinding instead
    /// </summary>
    public void FindAndSetupControllers()
    {
        Debug.LogWarning("[VRControllerSetup] DEPRECATED - Use HandPaddleBinding instead!");
    }

    /// <summary>
    /// Alternative method to find controllers via XRBaseController component
    /// [DEPRECATED] - Use HandPaddleBinding instead
    /// </summary>
    private void FindControllersByXRController()
    {
        Debug.LogWarning("[VRControllerSetup] DEPRECATED - Use HandPaddleBinding instead!");
    }

    /// <summary>
    /// Manually set controllers (call if auto-find fails)
    /// [DEPRECATED] - Use HandPaddleBinding instead
    /// </summary>
    public void SetControllers(Transform left, Transform right)
    {
        Debug.LogWarning("[VRControllerSetup] DEPRECATED - Use HandPaddleBinding instead!");
    }

    /// <summary>
    /// Get the left controller transform
    /// </summary>
    public Transform GetLeftController() => leftControllerTransform;

    /// <summary>
    /// Get the right controller transform
    /// </summary>
    public Transform GetRightController() => rightControllerTransform;
}
