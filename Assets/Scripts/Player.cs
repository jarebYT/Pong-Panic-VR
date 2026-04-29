using UnityEngine;

/// <summary>
/// Represents a player in the ping pong game with their score, paddle, and game state.
/// The paddle is bound to the player's chosen hand (left or right) via HandPaddleBinding.
/// Each player can independently choose which hand to use for their paddle.
/// Assign via Inspector, don't use constructor.
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private GameObject paddle;
    [SerializeField] private BoxCollider sideCollider;
    [SerializeField] private Transform servicePoint;
    [SerializeField] private HandPaddleBinding handPaddleBinding;
    
    // Runtime counters
    public int countBallTouch { get; set; }
    public int countServiceSideTouch { get; set; }

    // Properties for access
    public int Score => score;
    public GameObject Paddle => paddle;
    public BoxCollider SideCollider => sideCollider;
    public Transform ServicePoint => servicePoint;
    public HandPaddleBinding HandPaddleBinding => handPaddleBinding;
    public string PlayerName => gameObject.name;

    /// <summary>
    /// Initialize player with score and reset counters.
    /// Auto-finds HandPaddleBinding if not assigned.
    /// Call this when starting a new game.
    /// </summary>
    public void Initialize()
    {
        score = 0;
        countBallTouch = 0;
        countServiceSideTouch = 0;

        // Auto-find HandPaddleBinding if not assigned
        if (handPaddleBinding == null && paddle != null)
        {
            handPaddleBinding = GetComponent<HandPaddleBinding>();
        }

        // Verify binding was successful
        if (handPaddleBinding != null && !handPaddleBinding.IsSetup())
        {
            Debug.LogWarning($"[Player] {gameObject.name} HandPaddleBinding not properly setup!");
        }
    }

    /// <summary>
    /// Switch paddle between left and right hand at runtime.
    /// Useful if a player wants to change their hand choice mid-game or after setup.
    /// </summary>
    public void SwitchHand()
    {
        if (handPaddleBinding != null)
        {
            handPaddleBinding.SwitchHand();
            Debug.Log($"[Player] {gameObject.name} switched hand - now using {(handPaddleBinding.IsLeftHand() ? "LEFT" : "RIGHT")} hand");
        }
    }

    /// <summary>
    /// Add a point to this player's score
    /// </summary>
    public void AddScore()
    {
        score++;
    }

    /// <summary>
    /// Reset counters for new rally
    /// </summary>
    public void ResetCounters()
    {
        countBallTouch = 0;
        countServiceSideTouch = 0;
    }

    /// <summary>
    /// Get score string for display
    /// </summary>
    public string GetScoreDisplay()
    {
        return $"{PlayerName}: {score}";
    }
}