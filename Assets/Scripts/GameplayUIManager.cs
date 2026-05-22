using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages gameplay UI feedback: score announcements, point notifications, service instructions.
/// Displays world-space UI messages above the table for clear gameplay communication.
/// </summary>
public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas; // World-space canvas above table
    [SerializeField] private TextMeshProUGUI notificationText; // Main notification text
    [SerializeField] private TextMeshProUGUI scoreText; // Live score display
    [SerializeField] private TextMeshProUGUI serviceText; // Service instructions

    [Header("Notification Settings")]
    [SerializeField] private float notificationDuration = 2.5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Color pointNotificationColor = Color.yellow;
    [SerializeField] private Color serviceNotificationColor = Color.cyan;
    [SerializeField] private Color warningColor = Color.red;

    private CanvasGroup notificationCanvasGroup;
    private CanvasGroup scoreCanvasGroup;
    private CanvasGroup serviceCanvasGroup;

    private Coroutine currentNotificationCoroutine;
    private Coroutine currentServiceCoroutine;

    private void Start()
    {
        InitializeUIElements();
    }

    /// <summary>
    /// Initialize UI elements and canvas groups
    /// </summary>
    private void InitializeUIElements()
    {
        if (notificationText == null)
        {
            Debug.LogWarning("[GameplayUIManager] Notification text not assigned");
            return;
        }

        // Get or add CanvasGroup for smooth fading
        notificationCanvasGroup = notificationText.GetComponent<CanvasGroup>();
        if (notificationCanvasGroup == null)
        {
            notificationCanvasGroup = notificationText.gameObject.AddComponent<CanvasGroup>();
        }

        if (scoreText != null)
        {
            scoreCanvasGroup = scoreText.GetComponent<CanvasGroup>();
            if (scoreCanvasGroup == null)
            {
                scoreCanvasGroup = scoreText.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (serviceText != null)
        {
            serviceCanvasGroup = serviceText.GetComponent<CanvasGroup>();
            if (serviceCanvasGroup == null)
            {
                serviceCanvasGroup = serviceText.gameObject.AddComponent<CanvasGroup>();
            }
            serviceText.text = "";
        }

        // Start with invisible notification
        if (notificationCanvasGroup != null)
        {
            notificationCanvasGroup.alpha = 0f;
        }

        Debug.Log("[GameplayUIManager] Initialized");
    }

    /// <summary>
    /// Display a point scored notification
    /// </summary>
    public void ShowPointScored(string playerName, int newScore)
    {
        StopCurrentNotification();

        if (notificationText == null) return;

        notificationText.text = $"{playerName} a marqué un point!\nScore: {newScore}";
        notificationText.color = pointNotificationColor;

        currentNotificationCoroutine = StartCoroutine(
            ShowNotificationCoroutine(notificationDuration)
        );

        Debug.Log($"[GameplayUIManager] Point scored: {playerName} - {newScore}");
    }

    /// <summary>
    /// Display a fault/invalid play notification
    /// </summary>
    public void ShowFault(string playerName, string reason)
    {
        StopCurrentNotification();

        if (notificationText == null) return;

        notificationText.text = $"❌ Faute {playerName}\n{reason}";
        notificationText.color = warningColor;

        currentNotificationCoroutine = StartCoroutine(
            ShowNotificationCoroutine(notificationDuration + 0.5f)
        );

        Debug.Log($"[GameplayUIManager] Fault: {playerName} - {reason}");
    }

    /// <summary>
    /// Display service instructions
    /// </summary>
    public void ShowServiceInstruction(string playerName, string instruction)
    {
        if (serviceText == null) return;

        StopCurrentServiceInstruction();

        serviceText.text = $"🎾 Service {playerName}\n{instruction}";
        serviceText.color = serviceNotificationColor;

        if (serviceCanvasGroup != null)
        {
            serviceCanvasGroup.alpha = 1f;
        }

        currentServiceCoroutine = StartCoroutine(
            FadeOutCoroutine(serviceCanvasGroup, 5f)
        );

        Debug.Log($"[GameplayUIManager] Service: {playerName} - {instruction}");
    }

    /// <summary>
    /// Update live score display
    /// </summary>
    public void UpdateScoreDisplay(string player1Name, int score1, string player2Name, int score2)
    {
        if (scoreText == null) return;

        scoreText.text = $"{player1Name}: {score1} - {score2} :{player2Name}";
    }

    /// <summary>
    /// Show a quick alert message
    /// </summary>
    public void ShowAlert(string message, Color color, float duration = 1.5f)
    {
        StopCurrentNotification();

        if (notificationText == null) return;

        notificationText.text = message;
        notificationText.color = color;

        currentNotificationCoroutine = StartCoroutine(
            ShowNotificationCoroutine(duration)
        );
    }

    /// <summary>
    /// Coroutine: Fade in, hold, fade out notification
    /// </summary>
    private IEnumerator ShowNotificationCoroutine(float displayDuration)
    {
        if (notificationCanvasGroup == null) yield break;

        // Fade in
        yield return StartCoroutine(FadeInCoroutine(notificationCanvasGroup, fadeInDuration));

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeOutCoroutine(notificationCanvasGroup, fadeOutDuration));
    }

    /// <summary>
    /// Coroutine: Fade in
    /// </summary>
    private IEnumerator FadeInCoroutine(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Coroutine: Fade out
    /// </summary>
    private IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Stop current notification
    /// </summary>
    private void StopCurrentNotification()
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
            currentNotificationCoroutine = null;
        }
    }

    /// <summary>
    /// Stop current service instruction
    /// </summary>
    private void StopCurrentServiceInstruction()
    {
        if (currentServiceCoroutine != null)
        {
            StopCoroutine(currentServiceCoroutine);
            currentServiceCoroutine = null;
        }
    }
}
