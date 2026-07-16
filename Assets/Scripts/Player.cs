using UnityEngine;

/// <summary>
/// Represents a player (human or AI) with their score, paddle, table side and service point.
/// Also enforces the physics setup the paddle needs for precise ball collisions:
/// tag, kinematic rigidbody with speculative CCD, and a velocity tracker.
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private GameObject paddle;
    [SerializeField] private BoxCollider sideCollider;
    [SerializeField] private Transform servicePoint;
    [SerializeField] private HandPaddleBinding handPaddleBinding;

    public int Score => score;
    public GameObject Paddle => paddle;
    public BoxCollider SideCollider => sideCollider;
    public Transform ServicePoint => servicePoint;
    public HandPaddleBinding HandPaddleBinding => handPaddleBinding;
    public string PlayerName => gameObject.name;

    /// <summary>Reset the score and make sure the paddle is physically ready. Called at match start.</summary>
    public void Initialize()
    {
        score = 0;

        if (handPaddleBinding == null)
        {
            handPaddleBinding = GetComponent<HandPaddleBinding>();
        }

        EnsurePaddlePhysics();
    }

    /// <summary>
    /// Precise VR collisions need the paddle to be a proper moving collider:
    ///   - tagged "Paddle" so the ball recognizes it,
    ///   - kinematic rigidbody (mandatory for a collider moved by hand/AI),
    ///   - speculative CCD so fast swings don't tunnel through the ball,
    ///   - a PaddleVelocityTracker so the swing speed transfers to the ball.
    /// </summary>
    private void EnsurePaddlePhysics()
    {
        if (paddle == null)
        {
            Debug.LogError($"[Player] {PlayerName} has no paddle assigned!");
            return;
        }

        if (!paddle.CompareTag("Paddle")) paddle.tag = "Paddle";

        Rigidbody paddleRb = paddle.GetComponent<Rigidbody>();
        if (paddleRb == null) paddleRb = paddle.AddComponent<Rigidbody>();
        paddleRb.isKinematic = true;
        paddleRb.useGravity = false;
        paddleRb.interpolation = RigidbodyInterpolation.Interpolate;
        paddleRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (paddle.GetComponent<PaddleVelocityTracker>() == null)
        {
            paddle.AddComponent<PaddleVelocityTracker>();
        }

        if (paddle.GetComponentInChildren<Collider>() == null)
        {
            Debug.LogWarning($"[Player] {PlayerName}'s paddle has no collider - the ball will fly through it!");
        }
    }

    public void AddScore()
    {
        score++;
    }

    public void SwitchHand()
    {
        if (handPaddleBinding != null)
        {
            handPaddleBinding.SwitchHand();
        }
    }

    public string GetScoreDisplay()
    {
        return $"{PlayerName}: {score}";
    }
}
