using UnityEngine;

/// <summary>
/// Represents a player in the ping pong game with their score, paddle, and game state.
/// Assign via Inspector, don't use constructor.
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private GameObject paddle;
    [SerializeField] private BoxCollider sideCollider;
    [SerializeField] private Transform servicePoint;
    
    // Runtime counters
    public int countBallTouch { get; set; }
    public int countServiceSideTouch { get; set; }

    // Properties for access
    public int Score => score;
    public GameObject Paddle => paddle;
    public BoxCollider SideCollider => sideCollider;
    public Transform ServicePoint => servicePoint;
    public string PlayerName => gameObject.name;

    /// <summary>
    /// Initialize player with score and reset counters.
    /// Call this when starting a new game.
    /// </summary>
    public void Initialize()
    {
        score = 0;
        countBallTouch = 0;
        countServiceSideTouch = 0;
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