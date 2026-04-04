using UnityEngine;

/// <summary>
/// Manages the ball lifecycle: instantiation, destruction, and reference updates.
/// This ensures the ball reference is always correct in the game manager.
/// </summary>
public class BallManager : MonoBehaviour
{
    [SerializeField] private Ball ballPrefab;
    [SerializeField] private PingPongManager pingPongManager;
    
    private Ball currentBall;

    private void OnEnable()
    {
        // Subscribe to ball destruction event
        Ball.OnBallDestroyed += HandleBallDestroyed;
    }

    private void OnDisable()
    {
        Ball.OnBallDestroyed -= HandleBallDestroyed;
    }

    /// <summary>
    /// Spawn a new ball at the specified position and update manager reference
    /// </summary>
    public Ball SpawnBall(Transform spawnPoint)
    {
        // Destroy old ball if it exists
        if (currentBall != null)
        {
            Destroy(currentBall.gameObject);
        }

        // Instantiate new ball
        currentBall = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        currentBall.gameObject.name = "PingPongBall";
        currentBall.SetPingPongManager(pingPongManager);

        return currentBall;
    }

    /// <summary>
    /// Get current ball instance
    /// </summary>
    public Ball GetCurrentBall()
    {
        return currentBall;
    }

    /// <summary>
    /// Called when ball is destroyed (ground hit)
    /// </summary>
    private void HandleBallDestroyed(Ball ball)
    {
        if (ball == currentBall)
        {
            currentBall = null;
        }
    }
}
