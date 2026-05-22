using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager for Pong Panic VR.
/// Handles game logic, scoring, and state management.
/// </summary>
public class PingPongManager : MonoBehaviour
{
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;
    [SerializeField] private BallManager ballManager;
    [SerializeField] private GameplayUIManager gameplayUIManager;
    [SerializeField] private ServiceUIManager serviceUIManager;
    [SerializeField] private int winScore = 11;
    [SerializeField] private int winMargin = 2;

    public Player activePlayer;
    public Player inactivePlayer;

    public enum GameState
    {
        Service,      // Player serving
        Game,         // Rally in progress
        Inactive      // Game over
    }

    public GameState currentState { get; private set; }

    // Game stats
    private int gameRallies = 0;
    private float lastBallSpawnTime = 0f;
    private const float BALL_SPAWN_DELAY = 0.5f; // Delay before respawning ball
    
    private void Start()
    {
        // Auto-find UI managers if not assigned
        if (gameplayUIManager == null)
        {
            gameplayUIManager = FindFirstObjectByType<GameplayUIManager>();
        }
        if (serviceUIManager == null)
        {
            serviceUIManager = FindFirstObjectByType<ServiceUIManager>();
        }

        InitializeGame();
    }
    
    private void Update()
    {
        // Prevent rapid re-spawns
        if (currentState == GameState.Service && ballManager.GetCurrentBall() == null)
        {
            if (Time.time - lastBallSpawnTime > BALL_SPAWN_DELAY)
            {
                SpawnNewBall();
            }
        }
    }

    /// <summary>
    /// Initialize the game at startup
    /// </summary>
    private void InitializeGame()
    {
        // Setup players
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player 1 and Player 2 must be assigned in Inspector!");
            return;
        }

        player1.Initialize();
        player2.Initialize();

        activePlayer = player1;
        inactivePlayer = player2;
        currentState = GameState.Service;
        gameRallies = 0;

        // Update UI
        if (gameplayUIManager != null)
        {
            gameplayUIManager.UpdateScoreDisplay(
                player1.PlayerName, player1.Score,
                player2.PlayerName, player2.Score
            );
        }

        // Show service start
        if (serviceUIManager != null)
        {
            serviceUIManager.ShowServiceStart(activePlayer);
        }

        // Spawn initial ball
        SpawnNewBall();

        Debug.Log($"Game Initialized - {player1.PlayerName} serves first");
    }

    /// <summary>
    /// Spawn a new ball at the active player's service point
    /// </summary>
    public void SpawnNewBall()
    {
        if (ballManager == null)
        {
            Debug.LogError("BallManager not assigned!");
            return;
        }

        Ball ball = ballManager.SpawnBall(activePlayer.ServicePoint);
        if (ball != null)
        {
            ball.ResetBallState();
            lastBallSpawnTime = Time.time;
            Debug.Log($"Ball spawned for {activePlayer.PlayerName} to serve");
        }
        else
        {
            Debug.LogError("Failed to spawn ball!");
        }
    }

    // ===== BALL EVENT HANDLERS =====

    /// <summary>
    /// Called when ball hits the table (changes sides)
    /// </summary>
    public void OnTableHit(GameObject tableSide)
    {
        if (currentState == GameState.Game)
        {
            // Ball went to opponent's side - switch player
            SwitchActivePlayer();
            Debug.Log($"{activePlayer.PlayerName} can now hit");
        }
        else if (currentState == GameState.Service)
        {
            // This shouldn't normally happen in service state
            Debug.LogWarning("Table hit during service state");
        }
    }

    /// <summary>
    /// Called when ball hits table during service
    /// </summary>
    public void OnServiceTableHit(GameObject tableSide)
    {
        if (activePlayer.countServiceSideTouch == 0)
        {
            // First touch of service box = ok
            activePlayer.countServiceSideTouch++;
            Debug.Log($"{activePlayer.PlayerName} touched service side");
        }
        else if (activePlayer.countServiceSideTouch == 1)
        {
            // Second touch of service box = legal serve complete, enter rally
            activePlayer.countServiceSideTouch = 0;
            currentState = GameState.Game;
            activePlayer.countBallTouch = 1;
            Debug.Log($"{activePlayer.PlayerName} served - Game started!");
        }
        else
        {
            // Too many touches on service side = fault
            ServiceFault();
        }
    }

    /// <summary>
    /// Called when ball hits same side of table twice
    /// </summary>
    public void OnDoubleTouchSameSide(GameObject tableSide)
    {
        if (currentState == GameState.Game)
        {
            // Same player hit twice = fault
            AwardPoint(inactivePlayer, "Double touch on same side");
            ResetRally();
        }
    }

    /// <summary>
    /// Called when ball hits a paddle
    /// </summary>
    public void OnPaddleHit(GameObject paddle)
    {
        if (currentState == GameState.Service)
        {
            // During service, first paddle touch prepares for service
            activePlayer.countBallTouch = 1;
            Debug.Log($"{activePlayer.PlayerName} preparing serve");
        }
        else if (currentState == GameState.Game)
        {
            // Normal rally
            if (activePlayer.countBallTouch == 0)
            {
                activePlayer.countBallTouch = 1;
                Debug.Log($"{activePlayer.PlayerName} hit the ball");
            }
            else
            {
                // Second touch by same player on paddle = fault
                AwardPoint(inactivePlayer, "Double hit by same player");
                ResetRally();
            }
        }
    }

    /// <summary>
    /// Called when same paddle is hit twice
    /// </summary>
    public void OnAdditionalPaddleTouch(GameObject paddle)
    {
        activePlayer.countBallTouch++;

        if (currentState == GameState.Game && activePlayer.countBallTouch > 1)
        {
            // Double touch on paddle = fault
            AwardPoint(inactivePlayer, "Double touch on paddle");
            ResetRally();
        }
    }

    /// <summary>
    /// Called when ball hits the ground (out of play)
    /// The last player to touch their side loses the point
    /// </summary>
    public void OnBallOutOfPlay(GameObject lastTableSideTouched)
    {
        if (lastTableSideTouched == null)
        {
            // Ball went out of bounds before touching table
            AwardPoint(inactivePlayer, "Ball out of bounds on serve");
        }
        else if (lastTableSideTouched == activePlayer.SideCollider.gameObject)
        {
            // Ball went to ground on active player's side
            AwardPoint(inactivePlayer, "Ball fell on opponent's side");
        }
        else
        {
            // Ball went to ground on inactive player's side
            AwardPoint(activePlayer, "Ball fell on own side");
        }

        ResetRally();
    }

    // ===== GAME LOGIC =====

    /// <summary>
    /// Award point to a player and check for win
    /// </summary>
    private void AwardPoint(Player player, string reason)
    {
        player.AddScore();
        gameRallies++;

        Debug.Log($"Point to {player.PlayerName}! Reason: {reason}");
        Debug.Log($"Score - {player1.PlayerName}: {player1.Score} | {player2.PlayerName}: {player2.Score}");

        // Show point notification
        if (gameplayUIManager != null)
        {
            gameplayUIManager.ShowPointScored(player.PlayerName, player.Score);
            gameplayUIManager.UpdateScoreDisplay(
                player1.PlayerName, player1.Score,
                player2.PlayerName, player2.Score
            );
        }

        // In ping pong, serve changes every 2 points total
        int totalPoints = player1.Score + player2.Score;
        if (totalPoints % 4 == 2) // After every 2 points, switch server
        {
            SwitchActivePlayer();
            Debug.Log($"Serve switch: {activePlayer.PlayerName} now serves");
        }

        CheckWinCondition(player);
    }

    /// <summary>
    /// Check if a player has won the game
    /// </summary>
    private void CheckWinCondition(Player player)
    {
        if (player.Score >= winScore && player.Score >= inactivePlayer.Score + winMargin)
        {
            EndGame(player);
        }
    }

    /// <summary>
    /// Handle service fault (lose point, switch server)
    /// </summary>
    private void ServiceFault()
    {
        Debug.Log($"{activePlayer.PlayerName} service fault!");
        AwardPoint(inactivePlayer, "Service fault");
        // Service fault ends the rally, so ResetRally will be called after AwardPoint
        ResetRally();
    }

    /// <summary>
    /// Reset the rally (but keep same server if no point was scored)
    /// </summary>
    private void ResetRally()
    {
        // Reset both players' counters
        activePlayer.ResetCounters();
        inactivePlayer.ResetCounters();
        
        // Return to service state
        currentState = GameState.Service;
        
        // Spawn new ball for next service (with delay to prevent rapid respawns)
        lastBallSpawnTime = Time.time;

        // Show service instruction
        if (serviceUIManager != null)
        {
            serviceUIManager.ShowServiceStart(activePlayer);
        }

        Debug.Log($"===== NEW RALLY - {activePlayer.PlayerName} to serve =====");
    }

    /// <summary>
    /// Switch active and inactive players
    /// </summary>
    private void SwitchActivePlayer()
    {
        Player temp = activePlayer;
        activePlayer = inactivePlayer;
        inactivePlayer = temp;

        activePlayer.ResetCounters();
        inactivePlayer.ResetCounters();
    }

    /// <summary>
    /// End the game when someone wins
    /// </summary>
    private void EndGame(Player winner)
    {
        currentState = GameState.Inactive;

        Debug.Log("========================================");
        Debug.Log($"GAME OVER! Winner: {winner.PlayerName}");
        Debug.Log($"Final Score: {player1.GetScoreDisplay()} vs {player2.GetScoreDisplay()}");
        Debug.Log($"Total rallies: {gameRallies}");
        Debug.Log("========================================");

        // TODO: Show win screen UI
        // TODO: Allow restart or back to lobby
    }

    // ===== PUBLIC GETTERS =====

    public Player GetActivePlayer() => activePlayer;
    public Player GetInactivePlayer() => inactivePlayer;
    public int GetGameState() => (int)currentState;
    public string GetScoreDisplay() => $"{player1.GetScoreDisplay()} | {player2.GetScoreDisplay()}";
}
