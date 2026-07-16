using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Starts (or restarts) the match when the player presses a controller trigger.
/// Space bar also works for in-editor testing.
/// </summary>
public class PlayerReadyInput : MonoBehaviour
{
    [SerializeField] private PingPongManager pingPongManager;

    private InputAction startAction;

    private void Awake()
    {
        if (pingPongManager == null)
        {
            pingPongManager = FindFirstObjectByType<PingPongManager>();
        }

        startAction = new InputAction("StartMatch", InputActionType.Button);
        startAction.AddBinding("<XRController>{LeftHand}/triggerButton");
        startAction.AddBinding("<XRController>{RightHand}/triggerButton");
        startAction.AddBinding("<XRController>/triggerButton");
        startAction.AddBinding("<Keyboard>/space");
    }

    private void OnEnable()
    {
        startAction.performed += OnStartPressed;
        startAction.Enable();
    }

    private void OnDisable()
    {
        startAction.performed -= OnStartPressed;
        startAction.Disable();
    }

    private void OnDestroy()
    {
        startAction?.Dispose();
    }

    private void OnStartPressed(InputAction.CallbackContext ctx)
    {
        if (pingPongManager != null)
        {
            pingPongManager.RequestStartMatch();
        }
    }
}
