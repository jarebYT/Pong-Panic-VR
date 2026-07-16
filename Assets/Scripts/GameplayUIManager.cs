using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// World-space gameplay UI above the table, mirrored on both faces (P1 / P2).
///
/// Three slots:
///   - scoreText:        persistent live score ("Toi  3 - 2  Adversaire  •  Service: Toi")
///   - notificationText: transient messages (points, faults) and the countdown
///   - serviceText:      persistent instructions ("Appuie sur la gâchette…", "À toi de servir !")
/// </summary>
public class GameplayUIManager : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI serviceText;

    [Header("Player 2 Panel (Back Face)")]
    [SerializeField] private TextMeshProUGUI notificationTextP2;
    [SerializeField] private TextMeshProUGUI scoreTextP2;
    [SerializeField] private TextMeshProUGUI serviceTextP2;

    [Header("Timings")]
    [SerializeField] private float notificationDuration = 2.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Colors")]
    [SerializeField] private Color scoreColor = Color.white;
    [SerializeField] private Color countdownColor = Color.yellow;
    [SerializeField] private Color instructionColor = Color.cyan;

    private Coroutine notificationCoroutine;
    private Coroutine instructionPulseCoroutine;

    private void Awake()
    {
        // Awake (not Start) so the PingPongManager can safely write texts from its own Start.
        SetSlot(scoreText, scoreTextP2, "", scoreColor, 1f);
        SetSlot(notificationText, notificationTextP2, "", Color.white, 0f);
        SetSlot(serviceText, serviceTextP2, "", instructionColor, 0f);
    }

    // ===== SCORE =====

    public void UpdateScoreDisplay(string player1Name, int score1, string player2Name, int score2,
                                   string serverName = "", string nextHitterName = "")
    {
        string serveTag = string.IsNullOrEmpty(serverName) ? "" : $"   <size=70%>Service : {serverName} 🏓</size>";
        string text = $"{player1Name}  <size=130%><b><color=#FFE24A>{score1} - {score2}</color></b></size>  {player2Name}{serveTag}";
        SetSlot(scoreText, scoreTextP2, text, scoreColor, 1f);
    }

    // ===== NOTIFICATIONS / COUNTDOWN =====

    /// <summary>Transient message that fades out after 'duration' seconds.</summary>
    public void ShowMessage(string message, Color color, float duration)
    {
        StopNotification();
        SetSlot(notificationText, notificationTextP2, message, color, 1f);
        notificationCoroutine = StartCoroutine(FadeOutNotificationAfter(duration));
    }

    /// <summary>Big countdown value with a pop animation. Stays until replaced.</summary>
    public void ShowCountdown(string value)
    {
        StopNotification();
        SetSlot(notificationText, notificationTextP2, $"<size=170%><b>{value}</b></size>", countdownColor, 1f);
        notificationCoroutine = StartCoroutine(PopThenFade());
    }

    public void ClearNotification()
    {
        StopNotification();
        SetSlot(notificationText, notificationTextP2, "", Color.white, 0f);
    }

    // ===== INSTRUCTIONS =====

    /// <summary>Persistent instruction with a gentle pulse (start prompt, serve prompt, game over).</summary>
    public void ShowInstruction(string message)
    {
        StopInstructionPulse();
        SetSlot(serviceText, serviceTextP2, message, instructionColor, 1f);
        instructionPulseCoroutine = StartCoroutine(PulseInstruction());
    }

    public void ClearInstruction()
    {
        StopInstructionPulse();
        SetSlot(serviceText, serviceTextP2, "", instructionColor, 0f);
    }

    // ===== LEGACY ALIASES (kept for ServiceUIManager and older callers) =====

    public void ShowAlert(string message, Color color, float duration = 1.5f) => ShowMessage(message, color, duration);
    public void ShowServiceInstruction(string playerName, string instruction) => ShowInstruction($"🏓 {playerName}\n{instruction}");

    // ===== INTERNALS =====

    private static void SetSlot(TextMeshProUGUI front, TextMeshProUGUI back, string text, Color color, float alpha)
    {
        Apply(front, text, color, alpha);
        Apply(back, text, color, alpha);
    }

    private static void Apply(TextMeshProUGUI target, string text, Color color, float alpha)
    {
        if (target == null) return;
        target.text = text;
        color.a = 1f;
        target.color = color;
        target.alpha = alpha;
        target.rectTransform.localScale = Vector3.one;
    }

    private IEnumerator FadeOutNotificationAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            if (notificationText != null) notificationText.alpha = alpha;
            if (notificationTextP2 != null) notificationTextP2.alpha = alpha;
            yield return null;
        }
        SetSlot(notificationText, notificationTextP2, "", Color.white, 0f);
        notificationCoroutine = null;
    }

    private IEnumerator PopThenFade()
    {
        // Quick scale pop from big to normal.
        float duration = 0.35f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1.5f, 1f, elapsed / duration);
            if (notificationText != null) notificationText.rectTransform.localScale = Vector3.one * scale;
            if (notificationTextP2 != null) notificationTextP2.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }
        yield return FadeOutNotificationAfter(notificationDuration);
    }

    private IEnumerator PulseInstruction()
    {
        while (true)
        {
            float scale = Mathf.Lerp(0.97f, 1.03f, (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f);
            if (serviceText != null) serviceText.rectTransform.localScale = Vector3.one * scale;
            if (serviceTextP2 != null) serviceTextP2.rectTransform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    private void StopNotification()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        if (notificationText != null) notificationText.rectTransform.localScale = Vector3.one;
        if (notificationTextP2 != null) notificationTextP2.rectTransform.localScale = Vector3.one;
    }

    private void StopInstructionPulse()
    {
        if (instructionPulseCoroutine != null)
        {
            StopCoroutine(instructionPulseCoroutine);
            instructionPulseCoroutine = null;
        }
        if (serviceText != null) serviceText.rectTransform.localScale = Vector3.one;
        if (serviceTextP2 != null) serviceTextP2.rectTransform.localScale = Vector3.one;
    }
}
