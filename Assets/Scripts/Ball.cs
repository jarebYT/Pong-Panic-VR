using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles ball physics, collisions, and game state updates.
/// Communicates with PingPongManager through clearly defined events.
/// </summary>
public class Ball : MonoBehaviour
{
    private GameObject lastPaddleHitted;
    private GameObject lastTableSideTouched;
    public PingPongManager pingPongManager;
    private Rigidbody rb;

    // Events
    public static UnityEvent<Ball> OnBallDestroyed = new UnityEvent<Ball>();
    public UnityEvent OnTableHit = new UnityEvent();
    public UnityEvent OnPaddleHit = new UnityEvent();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Ball must have a Rigidbody component!");
        }
    }

    /// <summary>
    /// Set the PingPongManager reference
    /// </summary>
    public void SetPingPongManager(PingPongManager manager)
    {
        pingPongManager = manager;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // TABLE COLLISION
        if (collision.gameObject.CompareTag("Table"))
        {
            HandleTableCollision(collision.gameObject);
        }
        // PADDLE COLLISION
        else if (collision.gameObject.CompareTag("Paddle"))
        {
            HandlePaddleCollision(collision.gameObject);
        }
        // GROUND COLLISION
        else if (collision.gameObject.CompareTag("Ground"))
        {
            HandleGroundCollision();
        }
    }

    /// <summary>
    /// Handle ball hitting the table
    /// Determines if ball changed sides or stayed on same side
    /// </summary>
    private void HandleTableCollision(GameObject tableCollider)
    {
        if (pingPongManager == null)
        {
            Debug.LogError("Ball has no PingPongManager reference!");
            return;
        }

        // Only process if ball hasn't been destroyed
        if (gameObject == null) return;

        // If this is the first table touch (or different side)
        if (lastTableSideTouched != tableCollider)
        {
            lastTableSideTouched = tableCollider;

            if (pingPongManager.currentState == PingPongManager.GameState.Game)
            {
                // Player switched successfully
                pingPongManager.OnTableHit(tableCollider);
            }
            else if (pingPongManager.currentState == PingPongManager.GameState.Service)
            {
                // Service: first touch of own side allowed
                pingPongManager.OnServiceTableHit(tableCollider);
            }
        }
        // Ball hit same side of table again
        else if (lastTableSideTouched == tableCollider)
        {
            // Double touch on same side = fault
            pingPongManager.OnDoubleTouchSameSide(tableCollider);
        }

        OnTableHit.Invoke();
        Debug.Log($"Ball hit table: {tableCollider.name}");
    }

    /// <summary>
    /// Handle ball hitting a paddle
    /// Track which paddle was hit last
    /// </summary>
    private void HandlePaddleCollision(GameObject paddle)
    {
        if (pingPongManager == null) return;

        // Different paddle = switch player
        if (paddle != lastPaddleHitted)
        {
            lastPaddleHitted = paddle;
            pingPongManager.OnPaddleHit(paddle);
        }
        // Same paddle again = additional touch (counts as fault if > 1)
        else
        {
            pingPongManager.OnAdditionalPaddleTouch(paddle);
        }

        OnPaddleHit.Invoke();
        Debug.Log($"Ball hit paddle: {paddle.name}");
    }

    /// <summary>
    /// Handle ball hitting the ground (out of play)
    /// The player whose side it touched last loses the point
    /// </summary>
    private void HandleGroundCollision()
    {
        if (pingPongManager == null) return;

        // Play poof effect
        BallFeedback feedback = GetComponent<BallFeedback>();
        if (feedback != null)
        {
            feedback.PlayPoofEffect();
        }

        pingPongManager.OnBallOutOfPlay(lastTableSideTouched);
        OnBallDestroyed.Invoke(this);
        
        Debug.Log("Ball hit ground - point awarded");
        
        // Destroy after a brief delay to allow poof animation
        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// Reset ball state for new rally
    /// </summary>
    public void ResetBallState()
    {
        lastPaddleHitted = null;
        lastTableSideTouched = null;
        
        // Reset physics state
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Reset position to service point
        if (pingPongManager != null && pingPongManager.GetActivePlayer() != null)
        {
            transform.position = pingPongManager.GetActivePlayer().ServicePoint.position;
        }
        
        Debug.Log("Ball state reset - ready for service");
    }
}

