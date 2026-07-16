using UnityEngine;
using System.Collections;

/// <summary>
/// Manages service phase UI and animations.
/// Displays service instructions and visual feedback during service phase.
/// </summary>
public class ServiceUIManager : MonoBehaviour
{
    [SerializeField] private GameplayUIManager gameplayUIManager;
    
    [Header("Service Instructions")]
    [SerializeField] private string grabBallInstruction = "Saisis la balle et lance!";

    private Coroutine currentServiceAnimation;

    private void Start()
    {
        if (gameplayUIManager == null)
        {
            gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();
        }
    }

    /// <summary>
    /// Show service start instruction
    /// </summary>
    public void ShowServiceStart(Player servingPlayer)
    {
        if (gameplayUIManager == null) return;

        StopServiceAnimation();

        string instruction = $"🎾 {servingPlayer.PlayerName}\n{grabBallInstruction}";
        gameplayUIManager.ShowServiceInstruction(servingPlayer.PlayerName, grabBallInstruction);

        // Start pulsing animation during service
        currentServiceAnimation = StartCoroutine(ServicePulseAnimation());

        Debug.Log($"[ServiceUIManager] Service started: {servingPlayer.PlayerName}");
    }

    /// <summary>
    /// Show service phase feedback
    /// </summary>
    public void ShowServiceFeedback(string message, bool isValid)
    {
        if (gameplayUIManager == null) return;

        Color feedbackColor = isValid ? Color.green : Color.red;
        gameplayUIManager.ShowAlert(message, feedbackColor, 1.5f);

        Debug.Log($"[ServiceUIManager] Service feedback: {message}");
    }

    /// <summary>
    /// Coroutine: Pulsing animation during service
    /// </summary>
    private IEnumerator ServicePulseAnimation()
    {
        // Pulsing is now handled beautifully inside GameplayUIManager
        yield break;
    }

    /// <summary>
    /// Stop service animation
    /// </summary>
    private void StopServiceAnimation()
    {
        if (currentServiceAnimation != null)
        {
            StopCoroutine(currentServiceAnimation);
            currentServiceAnimation = null;
        }
    }
}
