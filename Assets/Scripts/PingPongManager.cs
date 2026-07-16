using System.Collections;
using UnityEngine;

/// <summary>
/// Central game manager for Pong Panic VR (single player vs AI).
///
/// Match flow:
///   WaitingToStart  → player presses the trigger
///   Countdown       → 3, 2, 1 with UI
///   Service         → ball pops in levitating at the server's service point
///   Rally           → ball goes back and forth under slowed gravity
///   PointPause      → poof, score UI update, short pause
///   ... back to Service, until someone reaches winScore (GameOver).
///
/// Simplified, VR-friendly rules:
///   - The ball falling on the floor ends the point. If it had bounced on the
///     receiver's side, the last hitter wins the point (missed return);
///     otherwise the last hitter loses it (shot out).
///   - Double bounce on one side = point for the other player.
///   - Dropping the ball during your own serve just respawns it (no fault).
/// </summary>
public class PingPongManager : MonoBehaviour
{
    [SerializeField] private Player player1;               // Human (VR)
    [SerializeField] private Player player2;               // AI
    [SerializeField] private BallManager ballManager;
    [SerializeField] private GameplayUIManager gameplayUIManager;

    [Header("Match Rules")]
    [SerializeField] private int winScore = 11;
    [SerializeField] private int winMargin = 2;
    [SerializeField] private int countdownSeconds = 3;
    [SerializeField] private float pointPauseDuration = 1.8f;

    [Header("VR Comfort")]
    [Range(0f, 1f)]
    [Tooltip("How strongly player hits are guided onto the opponent's side (0 = raw physics).")]
    [SerializeField] private float aimAssistStrength = 0.55f;
    [Tooltip("Disable all XR locomotion so the player stays on their side of the table.")]
    [SerializeField] private bool lockPlayerMovement = true;

    [Header("Opening Announcement")]
    [Tooltip("Play an announcer voice when the match starts. The countdown then lasts as long as the audio clip (the classic 3-2-1 plays over its final seconds).")]
    [SerializeField] private bool playOpeningVoice = false;
    [Tooltip("The announcer clip for THIS map (assign a different one per map on the prefab instance).")]
    [SerializeField] private AudioClip openingVoiceClip;
    [Range(0f, 1f)]
    [SerializeField] private float openingVoiceVolume = 1f;

    private AudioSource voiceSource;
    private bool openingVoicePlayed = false;

    public enum GameState
    {
        WaitingToStart, // Waiting for the trigger press
        Countdown,      // 3, 2, 1...
        Service,        // Ball levitating, waiting for the serve
        Rally,          // Ball in play
        PointPause,     // Point scored, short break before next round
        GameOver        // Match finished, trigger to replay
    }

    public GameState currentState { get; private set; } = GameState.WaitingToStart;

    public Player Player1 => player1;
    public Player Player2 => player2;
    public Player Server => server;
    public float AimAssistStrength => aimAssistStrength;

    // Kept for backward compatibility with scene serialization / older scripts.
    public Player activePlayer;
    public Player inactivePlayer;

    // Serving / rally tracking
    private Player server;
    private Player lastHitter;
    private bool serveOwnBounceAllowed;      // one bounce on the server's own side is legal on the serve stroke
    private bool opponentSideBouncedSinceHit;
    private Player lastBounceOwner;
    private int consecutiveSameSideBounces;

    private Coroutine flowCoroutine;
    private float lastBallSeenTime;

    private void Start()
    {
        if (gameplayUIManager == null) gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();
        if (ballManager == null) ballManager = FindFirstObjectByType<BallManager>();

        if (player1 == null || player2 == null || ballManager == null)
        {
            Debug.LogError("[PingPongManager] Player1, Player2 and BallManager must be assigned!");
            enabled = false;
            return;
        }

        player1.Initialize();
        player2.Initialize();

        if (lockPlayerMovement) DisablePlayerLocomotion();

        server = player1;
        UpdateActivePlayerRefs();
        EnterWaitingToStart();
    }

    private void Update()
    {
        // Watchdog: if the ball vanished without reporting (edge case), restart the round
        // so a rally can never leave the game stuck without a ball.
        if (currentState == GameState.Service || currentState == GameState.Rally)
        {
            if (ballManager.GetCurrentBall() != null)
            {
                lastBallSeenTime = Time.time;
            }
            else if (Time.time - lastBallSeenTime > 2.5f)
            {
                Debug.LogWarning("[PingPongManager] Ball lost without notification - restarting round.");
                StartRound();
            }
        }
    }

    // ===== MATCH FLOW =====

    private void EnterWaitingToStart()
    {
        currentState = GameState.WaitingToStart;
        if (gameplayUIManager != null)
        {
            gameplayUIManager.UpdateScoreDisplay(player1.PlayerName, 0, player2.PlayerName, 0, server.PlayerName, "");
            gameplayUIManager.ShowInstruction("Appuie sur la <b>gâchette</b> pour commencer !");
        }
    }

    /// <summary>
    /// Called by PlayerReadyInput when the player presses the trigger.
    /// Starts (or restarts, after game over) the match countdown.
    /// </summary>
    public void RequestStartMatch()
    {
        if (currentState != GameState.WaitingToStart && currentState != GameState.GameOver) return;

        player1.Initialize();
        player2.Initialize();
        server = player1; // The human always serves first
        UpdateActivePlayerRefs();

        if (flowCoroutine != null) StopCoroutine(flowCoroutine);
        flowCoroutine = StartCoroutine(CountdownRoutine());
    }

    /// <summary>Backward-compatible alias (older input scripts call this).</summary>
    public void SetLocalPlayerReady() => RequestStartMatch();

    private IEnumerator CountdownRoutine()
    {
        currentState = GameState.Countdown;
        if (gameplayUIManager != null) gameplayUIManager.ClearInstruction();

        // Opening announcement: ONLY on the very first match, not on replays.
        // The whole countdown stretches to the clip length, with the classic
        // 3-2-1 playing over the final seconds of the audio.
        if (playOpeningVoice && openingVoiceClip != null && !openingVoicePlayed)
        {
            openingVoicePlayed = true;
            EnsureVoiceSource();
            voiceSource.clip = openingVoiceClip;
            voiceSource.volume = openingVoiceVolume;
            voiceSource.Play();

            float preCountdownWait = Mathf.Max(0f, openingVoiceClip.length - countdownSeconds);
            if (preCountdownWait > 0.5f && gameplayUIManager != null)
            {
                gameplayUIManager.ShowInstruction("🎙️ Bienvenue dans l'arène !");
            }
            yield return new WaitForSeconds(preCountdownWait);
            if (gameplayUIManager != null) gameplayUIManager.ClearInstruction();
        }

        for (int i = countdownSeconds; i > 0; i--)
        {
            if (gameplayUIManager != null) gameplayUIManager.ShowCountdown(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        if (gameplayUIManager != null) gameplayUIManager.ShowCountdown("C'EST PARTI !");
        yield return new WaitForSeconds(0.7f);

        StartRound();
    }

    /// <summary>2D announcer audio source, created on demand.</summary>
    private void EnsureVoiceSource()
    {
        if (voiceSource != null) return;
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f; // announcer voice: heard everywhere, no 3D falloff
    }

    /// <summary>Spawn a fresh levitating ball at the current server's service point.</summary>
    private void StartRound()
    {
        currentState = GameState.Service;
        ResetRallyTracking();
        lastBallSeenTime = Time.time;

        Ball ball = ballManager.SpawnBall(server.ServicePoint);
        if (ball == null)
        {
            Debug.LogError("[PingPongManager] Failed to spawn ball!");
            return;
        }
        // Human serve: physical idle (the paddle can strike the levitating ball).
        // AI serve: kinematic idle so the AI paddle can't accidentally knock it away.
        bool humanServes = server.GetComponent<OpponentAI>() == null;
        ball.ResetBallState(humanServes);

        OpponentAI ai = player2.GetComponent<OpponentAI>();
        if (ai != null) ai.ResetForNewRound();

        if (gameplayUIManager != null)
        {
            UpdateScoreUI();
            gameplayUIManager.ShowInstruction(server == player1
                ? "🏓 À toi de servir !\nAttrape la balle, lance-la et frappe !"
                : "Service de l'adversaire…");
        }

        Debug.Log($"[PingPongManager] New round - {server.PlayerName} serves");
    }

    private void ResetRallyTracking()
    {
        lastHitter = null;
        serveOwnBounceAllowed = false;
        opponentSideBouncedSinceHit = false;
        lastBounceOwner = null;
        consecutiveSameSideBounces = 0;
    }

    // ===== BALL EVENTS =====

    /// <summary>
    /// Whether this player's paddle is currently allowed to strike the ball.
    /// During service only the server may hit; during a rally anyone can.
    /// </summary>
    public bool CanStrike(Player hitter)
    {
        if (currentState == GameState.Service)
        {
            // The AI serves via ExternalStrike (scripted), never by physical contact:
            // this prevents its paddle from accidentally knocking the levitating ball.
            if (hitter.GetComponent<OpponentAI>() != null) return false;
            return hitter == server;
        }
        return currentState == GameState.Rally;
    }

    /// <summary>Called by the ball when a paddle strikes it.</summary>
    public void OnPaddleHit(Player hitter)
    {
        if (currentState == GameState.Service && hitter == server)
        {
            currentState = GameState.Rally;
            serveOwnBounceAllowed = true; // the serve may legally bounce once on the server's side
            if (gameplayUIManager != null) gameplayUIManager.ClearInstruction();
        }
        else if (currentState != GameState.Rally)
        {
            return;
        }
        else
        {
            serveOwnBounceAllowed = false;
        }

        lastHitter = hitter;
        opponentSideBouncedSinceHit = false;
        lastBounceOwner = null;
        consecutiveSameSideBounces = 0;
    }

    /// <summary>
    /// Which player's side of the table is the given world position over?
    /// Uses the SideCollider XZ bounds (the play zones). Returns null if the
    /// point is over neither side (net gap or off the table).
    /// </summary>
    public Player GetSideOwnerAt(Vector3 worldPosition, float margin = 0f)
    {
        if (IsOverSide(player1, worldPosition, margin)) return player1;
        if (IsOverSide(player2, worldPosition, margin)) return player2;
        return null;
    }

    private static bool IsOverSide(Player player, Vector3 pos, float margin)
    {
        if (player == null || player.SideCollider == null) return false;
        Bounds b = player.SideCollider.bounds;
        return pos.x >= b.min.x - margin && pos.x <= b.max.x + margin
            && pos.z >= b.min.z - margin && pos.z <= b.max.z + margin;
    }

    /// <summary>
    /// Has the ball bounced on this player's side since the LAST paddle hit?
    /// (OnPaddleHit resets the tracking, so a hit in between always clears this.)
    /// Used by the AI to wait for the bounce before striking (no volleys).
    /// </summary>
    public bool HasBallBouncedOnSide(Player player)
    {
        return lastBounceOwner == player && consecutiveSameSideBounces >= 1;
    }

    /// <summary>
    /// Called by the ball (scripted detection) when it bounces on a player's side.
    ///
    /// Case handling:
    ///  - 1st bounce on the opponent's side after a hit  → normal exchange, play on.
    ///  - 2 bounces IN A ROW on the same side             → that side's owner failed
    ///    to return: point to the other player, ball poofs.
    ///  - Same side touched twice but with a paddle hit in between → NOT a double
    ///    bounce (OnPaddleHit resets lastBounceOwner), play continues normally.
    ///  - Bounce back on the hitter's own side: legal once on the serve stroke;
    ///    if the ball already bounced on the opponent side, the opponent missed it
    ///    (point to the hitter); otherwise the shot never crossed (point against).
    /// </summary>
    public void OnTableBounce(Player sideOwner)
    {
        if (currentState != GameState.Rally || lastHitter == null || sideOwner == null) return;

        if (sideOwner == lastBounceOwner) consecutiveSameSideBounces++;
        else { lastBounceOwner = sideOwner; consecutiveSameSideBounces = 1; }

        if (consecutiveSameSideBounces >= 2)
        {
            // Double bounce: the owner of that side let it drop twice, they lose the point.
            AwardPointAndRemoveBall(GetOpponent(sideOwner), $"Double rebond chez {sideOwner.PlayerName}");
            return;
        }

        Player opponent = GetOpponent(lastHitter);

        if (sideOwner == opponent)
        {
            // Normal exchange: the shot reached the other side.
            opponentSideBouncedSinceHit = true;
            if (opponent == player1 && gameplayUIManager != null)
            {
                gameplayUIManager.ShowMessage("👉 À toi !", Color.cyan, 0.7f);
            }
        }
        else // first bounce back on the hitter's own side
        {
            if (opponentSideBouncedSinceHit)
            {
                // Ball came back over without being returned: receiver missed it.
                AwardPointAndRemoveBall(lastHitter, $"{opponent.PlayerName} n'a pas renvoyé la balle");
            }
            else if (serveOwnBounceAllowed)
            {
                serveOwnBounceAllowed = false; // legal first serve bounce, consume it
            }
            else
            {
                AwardPointAndRemoveBall(opponent, $"La balle est retombée du côté de {lastHitter.PlayerName}");
            }
        }
    }

    /// <summary>
    /// Called by the ball when it flies INTO the net (crosses the net plane below
    /// the net top). The hitter loses the point — unless the ball had already
    /// bounced on the opponent's side, in which case the opponent failed to return.
    /// </summary>
    public void OnNetFault()
    {
        if (currentState != GameState.Rally || lastHitter == null) return;

        Player opponent = GetOpponent(lastHitter);
        if (opponentSideBouncedSinceHit)
        {
            // The ball had bounced on the opponent's side and came back into the net
            // untouched: the opponent failed to return it.
            AwardPointAndRemoveBall(lastHitter, $"{opponent.PlayerName} n'a pas renvoyé la balle");
        }
        else
        {
            AwardPointAndRemoveBall(opponent, $"🥅 Filet ! La balle de {lastHitter.PlayerName} n'est pas passée");
        }
    }

    /// <summary>Called by the ball when it dies (floor, fell out of the area, stalled).</summary>
    public void OnBallLost()
    {
        if (currentState == GameState.Service)
        {
            // Ball dropped before the serve: no fault, just pop a new one.
            if (gameplayUIManager != null)
            {
                gameplayUIManager.ShowMessage("Nouvelle balle !", Color.white, 1f);
            }
            StartCoroutine(RespawnServeBallRoutine());
            return;
        }

        if (currentState != GameState.Rally || lastHitter == null) return;

        Player opponent = GetOpponent(lastHitter);
        if (opponentSideBouncedSinceHit)
        {
            AwardPoint(lastHitter, $"{opponent.PlayerName} a raté la balle");
        }
        else
        {
            AwardPoint(opponent, $"{lastHitter.PlayerName} a envoyé la balle dehors");
        }
    }

    private IEnumerator RespawnServeBallRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (currentState == GameState.Service) StartRound();
    }

    // ===== SCORING =====

    /// <summary>Score a point while the ball is still flying (e.g. double bounce): poof it first.</summary>
    private void AwardPointAndRemoveBall(Player winner, string reason)
    {
        Ball ball = ballManager.GetCurrentBall();
        if (ball != null) ball.Despawn();
        AwardPoint(winner, reason);
    }

    private void AwardPoint(Player winner, string reason)
    {
        if (currentState != GameState.Rally && currentState != GameState.Service) return;

        currentState = GameState.PointPause;
        winner.AddScore();

        Debug.Log($"[PingPongManager] Point {winner.PlayerName} ({reason}) - {player1.Score}:{player2.Score}");

        // Table tennis serve rotation: every 2 points, every single point at deuce.
        int totalPoints = player1.Score + player2.Score;
        bool deuce = player1.Score >= winScore - 1 && player2.Score >= winScore - 1;
        if (deuce || totalPoints % 2 == 0)
        {
            server = GetOpponent(server);
            UpdateActivePlayerRefs();
        }

        if (gameplayUIManager != null)
        {
            UpdateScoreUI();
            bool playerScored = winner == player1;
            gameplayUIManager.ShowMessage(
                playerScored ? $"🎉 Point pour toi !\n{reason}" : $"❌ Point adversaire\n{reason}",
                playerScored ? Color.green : new Color(1f, 0.35f, 0.3f),
                pointPauseDuration);
        }

        if (CheckWin(winner)) return;

        if (flowCoroutine != null) StopCoroutine(flowCoroutine);
        flowCoroutine = StartCoroutine(NextRoundRoutine());
    }

    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(pointPauseDuration);
        StartRound();
    }

    private bool CheckWin(Player candidate)
    {
        Player opponent = GetOpponent(candidate);
        if (candidate.Score >= winScore && candidate.Score >= opponent.Score + winMargin)
        {
            currentState = GameState.GameOver;
            if (gameplayUIManager != null)
            {
                bool playerWon = candidate == player1;
                gameplayUIManager.ShowInstruction(
                    (playerWon ? "🏆 TU AS GAGNÉ !" : "💀 L'adversaire a gagné…") +
                    $"\n{player1.Score} - {player2.Score}\nAppuie sur la <b>gâchette</b> pour rejouer");
            }
            Debug.Log($"[PingPongManager] GAME OVER - {candidate.PlayerName} wins {player1.Score}:{player2.Score}");
            return true;
        }
        return false;
    }

    // ===== HELPERS =====

    private void UpdateScoreUI()
    {
        gameplayUIManager.UpdateScoreDisplay(
            player1.PlayerName, player1.Score,
            player2.PlayerName, player2.Score,
            server.PlayerName, "");
    }

    private void UpdateActivePlayerRefs()
    {
        activePlayer = server;
        inactivePlayer = GetOpponent(server);
    }

    public Player GetOpponent(Player player) => player == player1 ? player2 : player1;

    /// <summary>Resolve which player owns a paddle GameObject (the collider hit may be a child).</summary>
    public Player GetPlayerFromPaddle(GameObject paddleObject)
    {
        if (IsPartOfPaddle(paddleObject, player1)) return player1;
        if (IsPartOfPaddle(paddleObject, player2)) return player2;
        return null;
    }

    private static bool IsPartOfPaddle(GameObject candidate, Player player)
    {
        if (player == null || player.Paddle == null) return false;
        return candidate == player.Paddle || candidate.transform.IsChildOf(player.Paddle.transform);
    }

    /// <summary>
    /// The player must stay on their side of the table: disable every XR
    /// locomotion provider (teleport, continuous move, snap turn...).
    /// </summary>
    private void DisablePlayerLocomotion()
    {
        var providers = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var provider in providers)
        {
            provider.enabled = false;
        }
        if (providers.Length > 0)
        {
            Debug.Log($"[PingPongManager] Locked player movement ({providers.Length} locomotion providers disabled)");
        }
    }

    // Legacy getters kept so older utility scripts keep compiling.
    public Player GetActivePlayer() => server;
    public Player GetInactivePlayer() => GetOpponent(server);
    public string GetScoreDisplay() => $"{player1.PlayerName}: {player1.Score} | {player2.PlayerName}: {player2.Score}";
}
