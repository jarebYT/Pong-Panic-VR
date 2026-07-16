using System.Collections;
using UnityEngine;

/// <summary>
/// AI opponent (Player 2).
///
/// Design goals:
///   - Visible, believable paddle movement (hover + chase with a capped speed).
///   - RELIABLE returns: the hit itself is deterministic (the ball velocity is
///     set directly toward the player's side), physics collisions are only a fallback.
///   - BEATABLE: limited paddle speed, human-like reaction delay, a per-return
///     miss chance and an aim error that sometimes sends the ball out.
///
/// All positions are derived from the table side colliders, so the AI works on
/// any map without hardcoded coordinates.
/// </summary>
public class OpponentAI : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool aiEnabled = true;

    [Header("Difficulty")]
    [Tooltip("Max paddle chase speed in m/s. Lower = easier to beat with fast/angled shots.")]
    [SerializeField] private float paddleSpeed = 3.4f;
    [Tooltip("Delay before the AI starts reacting to an incoming ball.")]
    [SerializeField] private float reactionTime = 0.14f;
    [Tooltip("Chance (0-1) that the AI completely whiffs a return.")]
    [Range(0f, 1f)]
    [SerializeField] private float missChance = 0.15f;
    [Tooltip("Random error radius (m) added to the AI's aim. Larger = more AI shots land out.")]
    [SerializeField] private float aimErrorRadius = 0.4f;
    [Tooltip("Arc apex height range above the table for AI returns (low = flat/fast, high = slow lob).")]
    [SerializeField] private float arcApexMin = 0.3f;
    [SerializeField] private float arcApexMax = 0.5f;

    [Header("Behaviour")]
    [SerializeField] private float strikeRadius = 0.3f;
    [SerializeField] private float serveDelay = 1.6f;
    [SerializeField] private float hoverHeight = 0.22f;
    [SerializeField] private float swayAmplitude = 0.12f;
    [SerializeField] private float swaySpeed = 0.8f;

    private PingPongManager manager;
    private BallManager ballManager;
    private Player me;
    private Player humanPlayer;
    private GameObject paddle;
    private Rigidbody paddleRb;
    private Quaternion restRotation;

    // Table geometry (computed from side colliders at startup)
    private Vector3 homePosition;
    private Vector3 towardOpponent;   // horizontal unit vector from my side to the player's side
    private float tableHeight;
    private float tableHalfDepth;
    private Bounds mySideBounds;

    // Per-approach state
    private float approachTimer;
    private bool missRolled;
    private bool willMiss;
    private bool struckThisApproach;
    private Vector3 missOffset;

    private bool serveRoutineRunning;
    private Coroutine swingCoroutine;

    private void Start()
    {
        manager = FindFirstObjectByType<PingPongManager>();
        ballManager = FindFirstObjectByType<BallManager>();
        me = GetComponent<Player>();

        if (manager == null || ballManager == null || me == null || me.Paddle == null)
        {
            Debug.LogError("[OpponentAI] Missing references (manager, ball manager or paddle). AI disabled.");
            enabled = false;
            return;
        }

        humanPlayer = manager.GetOpponent(me);
        paddle = me.Paddle;
        restRotation = paddle.transform.rotation;

        // Kinematic rigidbody so the moving paddle generates proper collision events.
        paddleRb = paddle.GetComponent<Rigidbody>();
        if (paddleRb == null) paddleRb = paddle.AddComponent<Rigidbody>();
        paddleRb.isKinematic = true;
        paddleRb.useGravity = false;
        paddleRb.interpolation = RigidbodyInterpolation.Interpolate;

        ComputeTableGeometry();
        paddle.transform.position = homePosition;
    }

    /// <summary>Derive home position and orientation axes from the table side colliders.</summary>
    private void ComputeTableGeometry()
    {
        mySideBounds = me.SideCollider.bounds;
        Bounds theirSide = humanPlayer.SideCollider.bounds;

        Vector3 toOpp = theirSide.center - mySideBounds.center;
        toOpp.y = 0f;
        towardOpponent = toOpp.normalized;
        tableHeight = mySideBounds.max.y;

        // Depth of my table half along the table axis (works whatever the map orientation).
        tableHalfDepth = Mathf.Abs(mySideBounds.extents.x * towardOpponent.x)
                       + Mathf.Abs(mySideBounds.extents.z * towardOpponent.z);

        homePosition = mySideBounds.center - towardOpponent * (tableHalfDepth + 0.25f);
        homePosition.y = tableHeight + hoverHeight;
    }

    private void FixedUpdate()
    {
        if (!aiEnabled || manager == null) return;

        Ball ball = ballManager.GetCurrentBall();

        // My serve: run the serve routine once the ball is levitating.
        if (manager.currentState == PingPongManager.GameState.Service &&
            manager.Server == me && !serveRoutineRunning &&
            ball != null && ball.Phase == Ball.BallPhase.Idle)
        {
            StartCoroutine(ServeRoutine(ball));
            return;
        }

        if (serveRoutineRunning) return; // the coroutine drives the paddle

        if (manager.currentState == PingPongManager.GameState.Rally &&
            ball != null && ball.Phase == Ball.BallPhase.Live)
        {
            UpdateRally(ball);
        }
        else
        {
            ResetApproach();
            HoverAtHome();
        }
    }

    // ===== RALLY =====

    private void UpdateRally(Ball ball)
    {
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        Vector3 ballVelocity = ballRb != null ? ballRb.linearVelocity : Vector3.zero;
        Vector3 ballPosition = ball.transform.position;

        bool incoming = Vector3.Dot(ballVelocity, -towardOpponent) > 0.2f;
        // NO VOLLEYS: the AI may only strike after the ball has bounced on its side.
        // This also keeps it away from the net (real table tennis behaviour).
        bool bouncedOnMySide = manager.HasBallBouncedOnSide(me);

        if (!incoming && !bouncedOnMySide)
        {
            // Ball moving away and nothing to play: drift home, mirroring laterally.
            ResetApproach();
            Vector3 lateral = Vector3.Cross(Vector3.up, towardOpponent);
            float lateralOffset = Vector3.Dot(ballPosition - homePosition, lateral) * 0.35f;
            Vector3 target = homePosition + lateral * Mathf.Clamp(lateralOffset, -0.6f, 0.6f);
            MovePaddleTowards(target, paddleSpeed * 0.6f);
            return;
        }

        // Human-like reaction delay before starting to move.
        approachTimer += Time.fixedDeltaTime;
        if (approachTimer < reactionTime)
        {
            HoverAtHome();
            return;
        }

        // Decide once per approach whether this return will be missed.
        if (!missRolled)
        {
            missRolled = true;
            willMiss = Random.value < missChance;
            Vector3 lateral = Vector3.Cross(Vector3.up, towardOpponent);
            missOffset = willMiss ? lateral * (Random.value < 0.5f ? -0.45f : 0.45f) : Vector3.zero;
        }

        if (!bouncedOnMySide)
        {
            // WAIT FOR THE BOUNCE: stay at the back line, laterally aligned with
            // where the ball is going to land on my side.
            Vector3 landing = PredictLandingPoint(ballPosition, ballVelocity, ball.GravityMagnitude);
            Vector3 lateralAxis = Vector3.Cross(Vector3.up, towardOpponent);
            float lateralOffset = Mathf.Clamp(Vector3.Dot(landing - homePosition, lateralAxis), -1f, 1f);
            Vector3 waitTarget = homePosition + lateralAxis * lateralOffset + missOffset;
            MovePaddleTowards(ClampToReach(waitTarget), paddleSpeed);
            return;
        }

        // The ball HAS bounced on my side: step in and strike when in reach.
        Vector3 chaseTarget = ClampToReach(ballPosition + ballVelocity * 0.1f + missOffset);
        MovePaddleTowards(chaseTarget, paddleSpeed);

        if (!struckThisApproach && !willMiss &&
            Vector3.Distance(paddle.transform.position, ballPosition) < strikeRadius)
        {
            StrikeBall(ball, ballPosition);
        }
    }

    /// <summary>
    /// Where will the ball first reach table height, under the slowed gravity?
    /// Used to pre-position the paddle laterally while waiting for the bounce.
    /// </summary>
    private Vector3 PredictLandingPoint(Vector3 position, Vector3 velocity, float gravity)
    {
        gravity = Mathf.Max(gravity, 0.1f);
        float heightAboveTable = Mathf.Max(0f, position.y - tableHeight);
        float verticalSpeed = velocity.y; // positive = rising
        // Time until y(t) = tableHeight: solve -g/2 t² + vy t + h = 0.
        float time = (verticalSpeed + Mathf.Sqrt(verticalSpeed * verticalSpeed + 2f * gravity * heightAboveTable)) / gravity;

        Vector3 landing = position + new Vector3(velocity.x, 0f, velocity.z) * time;
        landing.y = tableHeight;
        return landing;
    }

    private void StrikeBall(Ball ball, Vector3 fromPosition)
    {
        struckThisApproach = true;

        Vector3 target = PickTargetOnPlayerSide();
        float apex = Random.Range(arcApexMin, arcApexMax);
        Vector3 velocity = ComputeArcVelocity(fromPosition, target, apex, ball.GravityMagnitude);

        ball.ExternalStrike(velocity, me);
        PlaySwingAnimation(velocity.normalized);
    }

    /// <summary>Aim point on the player's side, with an error that can land the shot out.</summary>
    private Vector3 PickTargetOnPlayerSide()
    {
        Bounds side = humanPlayer.SideCollider.bounds;
        Vector3 target = side.center;
        target.x += Random.Range(-side.extents.x, side.extents.x) * 0.55f;
        target.z += Random.Range(-side.extents.z, side.extents.z) * 0.55f;

        Vector2 error = Random.insideUnitCircle * aimErrorRadius;
        target += new Vector3(error.x, 0f, error.y);
        target.y = side.max.y + 0.02f;
        return target;
    }

    // ===== SERVE =====

    private IEnumerator ServeRoutine(Ball ball)
    {
        serveRoutineRunning = true;

        // Settle near the levitating ball, far enough to never touch it.
        Vector3 prep = ball.transform.position - towardOpponent * 0.45f;
        float settle = 0f;
        while (settle < 1.2f && ball != null && ball.Phase == Ball.BallPhase.Idle)
        {
            MovePaddleTowards(ClampToReach(prep), paddleSpeed);
            settle += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(serveDelay);

        if (ball == null || ball.Phase == Ball.BallPhase.Dead)
        {
            serveRoutineRunning = false;
            yield break;
        }

        // Small toss, wait for the apex under slowed gravity, then strike.
        float tossSpeed = 1.5f;
        ball.NotifyServed(Vector3.up * tossSpeed);
        yield return new WaitForSeconds(tossSpeed / Mathf.Max(ball.GravityMagnitude, 0.1f));

        if (ball != null && ball.Phase == Ball.BallPhase.Live)
        {
            StrikeBall(ball, ball.transform.position);
        }

        serveRoutineRunning = false;
    }

    // ===== MOVEMENT / VISUALS =====

    private void HoverAtHome()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed * 2f * Mathf.PI) * swayAmplitude;
        Vector3 lateral = Vector3.Cross(Vector3.up, towardOpponent);
        Vector3 target = homePosition + lateral * sway + Vector3.up * (Mathf.Sin(Time.time * 1.3f) * 0.03f);
        MovePaddleTowards(target, paddleSpeed * 0.5f);
    }

    private void MovePaddleTowards(Vector3 target, float speed)
    {
        Vector3 next = Vector3.MoveTowards(paddle.transform.position, target, speed * Time.fixedDeltaTime);
        if (paddleRb != null) paddleRb.MovePosition(next);
        else paddle.transform.position = next;
    }

    /// <summary>Keep the paddle within a plausible reach box over my half of the table.</summary>
    private Vector3 ClampToReach(Vector3 position)
    {
        Bounds reach = mySideBounds;
        reach.Expand(new Vector3(0.5f, 0f, 0.5f));

        position.x = Mathf.Clamp(position.x, reach.min.x, reach.max.x);
        position.z = Mathf.Clamp(position.z, reach.min.z, reach.max.z);
        // Stay a hand's height above the table (never sitting flat on the surface).
        position.y = Mathf.Clamp(position.y, tableHeight + 0.12f, tableHeight + 0.9f);

        // Defend from the back: never approach the net closer than ~half of my
        // half-table depth (prevents net-camping and strikes right at the net).
        float alongAxis = Vector3.Dot(position - mySideBounds.center, towardOpponent);
        float maxForward = tableHalfDepth * 0.45f;
        if (alongAxis > maxForward)
        {
            position -= towardOpponent * (alongAxis - maxForward);
        }
        return position;
    }

    private void PlaySwingAnimation(Vector3 direction)
    {
        if (swingCoroutine != null) StopCoroutine(swingCoroutine);
        swingCoroutine = StartCoroutine(SwingRoutine(direction));
    }

    private IEnumerator SwingRoutine(Vector3 direction)
    {
        // Quick lunge forward and back: purely cosmetic, the hit already happened.
        Vector3 start = paddle.transform.position;
        float duration = 0.18f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float lunge = Mathf.Sin(t * Mathf.PI) * 0.15f;
            if (paddleRb != null) paddleRb.MovePosition(start + direction * lunge);
            paddle.transform.rotation = restRotation * Quaternion.Euler(-20f * Mathf.Sin(t * Mathf.PI), 0f, 0f);
            yield return null;
        }
        paddle.transform.rotation = restRotation;
        swingCoroutine = null;
    }

    private void ResetApproach()
    {
        approachTimer = 0f;
        missRolled = false;
        willMiss = false;
        struckThisApproach = false;
        missOffset = Vector3.zero;
    }

    /// <summary>Called by the manager at the start of every round.</summary>
    public void ResetForNewRound()
    {
        StopAllCoroutines();
        serveRoutineRunning = false;
        swingCoroutine = null;
        ResetApproach();
        if (paddle != null) paddle.transform.rotation = restRotation;
    }

    /// <summary>Legacy alias.</summary>
    public void ResetAI() => ResetForNewRound();

    /// <summary>
    /// Launch velocity of a ballistic arc from 'from' to 'target' passing through
    /// an apex 'apexHeight' above the higher of the two points. Same model as the
    /// player's arcade hits: guaranteed to clear the net and land on the target.
    /// </summary>
    private static Vector3 ComputeArcVelocity(Vector3 from, Vector3 target, float apexHeight, float g)
    {
        g = Mathf.Max(g, 0.1f);
        float apexY = Mathf.Max(from.y, target.y) + Mathf.Max(apexHeight, 0.1f);

        float verticalSpeed = Mathf.Sqrt(2f * g * Mathf.Max(0.05f, apexY - from.y));
        float timeUp = verticalSpeed / g;
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.05f, apexY - target.y) / g);
        float flightTime = timeUp + timeDown;

        Vector3 horizontal = target - from;
        horizontal.y = 0f;
        Vector3 velocity = horizontal / flightTime;
        velocity.y = verticalSpeed;
        return velocity;
    }
}
