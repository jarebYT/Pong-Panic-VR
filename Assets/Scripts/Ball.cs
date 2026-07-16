using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The ping pong ball.
/// Life cycle: spawned levitating at a service point (Idle, with a "pop"),
/// optionally grabbed by the player (Held), then in play under slowed
/// custom gravity (Live), and finally removed with a "poof" (Dead).
///
/// The ball never decides game rules by itself: it reports table bounces,
/// paddle hits and its own death to the PingPongManager, which owns scoring.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public enum BallPhase
    {
        Idle,   // Levitating at the service point, waiting to be grabbed or struck
        Held,   // Grabbed by the player's hand (XR moves it)
        Live,   // In play, slowed custom gravity applies
        Dead    // Out of play, poof effect running, about to be destroyed
    }

    [Header("Slowed Gravity (VR comfort)")]
    [Tooltip("Downward acceleration in m/s². Real gravity is 9.81 — lower = floatier, easier rallies.")]
    [SerializeField] private float gravityMagnitude = 3.4f;

    [Header("Arcade Hit")]
    [Tooltip("How strongly a hit snaps to the ideal arcade arc (1 = fully guided, 0 = raw physics). Keep high: this is what makes the game feel arcade.")]
    [Range(0f, 1f)]
    [SerializeField] private float arcadeBlend = 0.9f;
    [Tooltip("Apex height of the hit arc above the table (m). Higher = floatier lobs.")]
    [SerializeField] private float arcApexHeight = 0.35f;
    [Tooltip("How much a sideways swing moves the landing point left/right.")]
    [SerializeField] private float lateralControl = 0.45f;
    [Tooltip("Swing speed (m/s) mapped to the SHORTEST landing point (just past the net).")]
    [SerializeField] private float softSwingSpeed = 0.6f;
    [Tooltip("Swing speed (m/s) mapped to the DEEPEST landing point (back of the table).")]
    [SerializeField] private float hardSwingSpeed = 5f;
    [Tooltip("Below this swing speed a touch on a live ball is passive: natural bounce, no game hit.")]
    [SerializeField] private float minActiveSwingSpeed = 0.35f;

    [Header("Player Power Limiter")]
    [Tooltip("Power coefficient: the measured swing speed is MULTIPLIED by this before use (0.5 = half power). Lower = calmer shots.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float swingPowerScale = 0.5f;
    [Tooltip("The scaled swing speed is capped here: no matter how hard the player swings, the hit never exceeds this input speed.")]
    [SerializeField] private float maxSwingSpeed = 5.5f;
    [Tooltip("Hard cap on ball speed after any hit, keeps the game readable in VR.")]
    [SerializeField] private float maxSpeed = 9f;
    [Tooltip("Minimum time between two registered paddle hits (avoids multi-contact spam in one swing).")]
    [SerializeField] private float paddleHitCooldown = 0.25f;

    [Header("Idle Levitation")]
    [SerializeField] private float bobAmplitude = 0.035f;
    [SerializeField] private float bobFrequency = 1.4f;

    [Header("Table & Net (scripted — reliable, no tunneling)")]
    [Tooltip("How bouncy the table is (0.72 = keeps 72% of vertical speed each bounce).")]
    [Range(0f, 1f)]
    [SerializeField] private float tableBounceFactor = 0.72f;
    [Tooltip("Minimum upward speed after a table bounce: the ball always pops up visibly (arcade), never rolls flat.")]
    [SerializeField] private float minBounceUpSpeed = 1.0f;
    [Tooltip("Maximum upward speed after a table bounce.")]
    [SerializeField] private float maxBounceUpSpeed = 3.2f;
    [Tooltip("Minimum time between two table bounces: guarantees exactly ONE bounce event per contact.")]
    [SerializeField] private float tableBounceCooldown = 0.15f;
    [Tooltip("Net height above the table surface (m). A ball crossing the net BELOW this = net fault; above this passes freely.")]
    [SerializeField] private float netHeightAboveTable = 0.14f;
    [Tooltip("No net fault during this time after a paddle hit: a fresh arcade arc always clears the net, so a shot struck close to the net is never wrongly killed.")]
    [SerializeField] private float netGraceAfterHit = 0.15f;

    [Header("In-Flight Aim Assist (player hits only)")]
    [Tooltip("Gentle horizontal pull toward the assisted landing point while the ball flies.")]
    [SerializeField] private float steerAcceleration = 1.6f;
    [SerializeField] private float steerDuration = 1.2f;

    [Header("Safety Fallbacks")]
    [Tooltip("The ball normally dies ONLY on Ground/Net contact or double bounce. These are anti-softlock fallbacks.")]
    [SerializeField] private float fallDepthBelowSpawn = 3f;
    [SerializeField] private float maxDistanceFromSpawn = 15f;
    [SerializeField] private float stallSpeedThreshold = 0.25f;
    [SerializeField] private float stallTimeLimit = 4f;

    // Kept public / with these exact names because the prefab and other scripts reference them.
    public PingPongManager pingPongManager;
    public UnityEvent OnTableHit = new UnityEvent();
    public UnityEvent OnPaddleHit = new UnityEvent();
    public static UnityEvent<Ball> OnBallDestroyed = new UnityEvent<Ball>();

    public BallPhase Phase { get; private set; } = BallPhase.Idle;
    public float GravityMagnitude => gravityMagnitude;

    private Rigidbody rb;
    private Collider ballCollider;
    private BallSoundController soundController;
    private BallFeedback feedback;

    private Vector3 idleAnchor;
    private float idleTime;
    private bool physicalIdle = true; // dynamic idle (player can strike it) vs kinematic idle (AI serve, untouchable)
    private float stallTimer;
    private float lastPaddleHitTime = -10f;
    private bool hasPendingServeVelocity;
    private Vector3 pendingServeVelocity;

    // In-flight steering (mini aim assist after a player hit)
    private bool steerActive;
    private Vector3 steerTarget;
    private float steerTimer;

    // Scripted table/net detection
    private float ballRadius = 0.02f;
    private Vector3 lastPos;
    private bool hasLastPos;
    private float lastTableBounceTime = -10f;
    private float tableSurfaceY = float.NegativeInfinity; // cached at spawn, gates Ground kills

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
        soundController = GetComponent<BallSoundController>();
        if (soundController == null) soundController = gameObject.AddComponent<BallSoundController>();
        feedback = GetComponent<BallFeedback>();

        // Enforce reliable physics regardless of prefab values.
        rb.useGravity = false;                                           // We apply our own slowed gravity.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // No tunneling through paddles/table.
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.maxAngularVelocity = 40f;

        idleAnchor = transform.position;
    }

    public void SetPingPongManager(PingPongManager manager)
    {
        pingPongManager = manager;
    }

    /// <summary>
    /// Reset to the levitating Idle state at the current position (called right after spawn).
    /// Plays the "pop" appearance effect.
    /// </summary>
    /// <param name="physicalWhileIdle">
    /// True for the human serve: the ball stays dynamic so the paddle can strike it directly.
    /// False for the AI serve: the ball is kinematic, so the AI paddle physically cannot
    /// bump it away while getting into position (the AI serves via ExternalStrike anyway).
    /// </param>
    public void ResetBallState(bool physicalWhileIdle = true)
    {
        Phase = BallPhase.Idle;
        physicalIdle = physicalWhileIdle;
        idleAnchor = transform.position;
        idleTime = 0f;
        stallTimer = 0f;
        hasPendingServeVelocity = false;
        steerActive = false;
        hasLastPos = false;

        rb.isKinematic = !physicalIdle;
        if (physicalIdle)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (ballCollider != null) ballRadius = Mathf.Max(0.01f, ballCollider.bounds.extents.x);
        SetupScriptedSurfaces();
        CacheTableSurfaceHeight();

        if (feedback != null) feedback.PlayPopEffect();
    }

    /// <summary>
    /// Cache the table surface height so Ground death zones can be gated:
    /// whatever their shape, they must NEVER kill a ball above table level.
    /// </summary>
    private void CacheTableSurfaceHeight()
    {
        tableSurfaceY = float.NegativeInfinity;
        if (pingPongManager == null) return;

        Player p1 = pingPongManager.Player1;
        Player p2 = pingPongManager.Player2;
        if (p1 != null && p1.SideCollider != null)
            tableSurfaceY = Mathf.Max(tableSurfaceY, p1.SideCollider.bounds.max.y);
        if (p2 != null && p2.SideCollider != null)
            tableSurfaceY = Mathf.Max(tableSurfaceY, p2.SideCollider.bounds.max.y);

        if (float.IsNegativeInfinity(tableSurfaceY))
            tableSurfaceY = idleAnchor.y - 0.35f; // fallback: below the levitating spawn
    }

    /// <summary>
    /// A "Ground" contact only counts as a death when the ball is genuinely BELOW
    /// the table level. This makes the kill zones immune to any oversized or
    /// misplaced volume touching the play area.
    /// </summary>
    private bool IsBelowTableLevel()
    {
        return transform.position.y < tableSurfaceY - 0.1f;
    }

    /// <summary>
    /// Table bounces are handled by script (see CheckScriptedSurfaces) for perfect
    /// reliability in VR, so the ball must NOT also bounce off the table physically:
    /// ignore physics against every Table collider (the play zones). The net is a
    /// trigger (no physical bounce) handled by the scripted net detection; ground
    /// and paddle collisions stay physical (those work well and are needed).
    /// </summary>
    private void SetupScriptedSurfaces()
    {
        if (ballCollider == null) return;
        Collider[] all = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        foreach (Collider col in all)
        {
            if (col == ballCollider) continue;
            if (col.CompareTag("Table"))
            {
                Physics.IgnoreCollision(ballCollider, col, true);
            }
        }
    }

    /// <summary>Called by ServiceHandler when the player grabs the ball.</summary>
    public void NotifyHeld()
    {
        if (Phase == BallPhase.Dead) return;
        Phase = BallPhase.Held;
        stallTimer = 0f;
        steerActive = false;
    }

    /// <summary>
    /// Called by ServiceHandler when the player releases the ball (toss).
    /// The velocity is applied on the next FixedUpdate so it wins over
    /// whatever XRGrabInteractable sets on detach.
    /// </summary>
    public void NotifyServed(Vector3 throwVelocity)
    {
        if (Phase == BallPhase.Dead) return;
        Phase = BallPhase.Live;
        stallTimer = 0f;
        pendingServeVelocity = throwVelocity;
        hasPendingServeVelocity = true;
    }

    /// <summary>Puts the ball in play without setting a velocity (AI toss).</summary>
    public void SetLive()
    {
        if (Phase == BallPhase.Dead) return;
        Phase = BallPhase.Live;
        stallTimer = 0f;
        rb.isKinematic = false;
    }

    /// <summary>
    /// Deterministic hit used by the AI: sets the ball velocity directly and
    /// notifies the manager, bypassing physics reflection (reliable returns).
    /// </summary>
    public void ExternalStrike(Vector3 velocity, Player hitter)
    {
        if (Phase == BallPhase.Dead) return;

        Phase = BallPhase.Live;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        stallTimer = 0f;
        steerActive = false;
        lastPaddleHitTime = Time.time;

        if (soundController != null) soundController.PlayCollisionSound("Paddle", velocity.magnitude);
        OnPaddleHit.Invoke();
        if (pingPongManager != null) pingPongManager.OnPaddleHit(hitter);
    }

    /// <summary>Remove the ball with a poof WITHOUT reporting it (manager already scored the point).</summary>
    public void Despawn()
    {
        Kill(false);
    }

    private void FixedUpdate()
    {
        switch (Phase)
        {
            case BallPhase.Idle:
                UpdateIdleLevitation();
                break;

            case BallPhase.Live:
                if (hasPendingServeVelocity)
                {
                    hasPendingServeVelocity = false;
                    rb.isKinematic = false;
                    rb.linearVelocity = Vector3.ClampMagnitude(pendingServeVelocity, maxSpeed);
                }

                // Scripted table/net detection on the segment the ball just travelled.
                if (hasLastPos) CheckScriptedSurfaces();
                if (Phase != BallPhase.Live) break; // a net fault may have killed it
                lastPos = rb.position;
                hasLastPos = true;

                rb.AddForce(Vector3.down * gravityMagnitude, ForceMode.Acceleration);
                UpdateInFlightSteering();
                CheckSafetyNet();
                break;
        }
    }

    /// <summary>
    /// Gentle floating animation. In physical idle (player serve) it is driven
    /// through velocity (a servo toward the bobbing target) so paddle contacts
    /// stay valid; in kinematic idle (AI serve) the body is simply moved.
    /// </summary>
    private void UpdateIdleLevitation()
    {
        idleTime += Time.fixedDeltaTime;
        float bob = Mathf.Sin(idleTime * bobFrequency * 2f * Mathf.PI) * bobAmplitude;
        Vector3 target = idleAnchor + Vector3.up * bob;

        if (physicalIdle)
        {
            // Servo toward the bob target, clamped so a stray nudge can never
            // cause violent rubber-banding back to the anchor.
            Vector3 servoVelocity = (target - rb.position) / Time.fixedDeltaTime * 0.5f;
            rb.linearVelocity = Vector3.ClampMagnitude(servoVelocity, 1.5f);
            rb.angularVelocity = new Vector3(0f, 1.2f, 0f); // slow spin, looks alive
        }
        else
        {
            rb.MovePosition(target);
        }
    }

    /// <summary>
    /// Mini in-flight aim assist: corrects only the HEADING of the ball toward
    /// its landing point (never the speed). When the arcade arc is already
    /// exact the correction is zero, so trajectories stay natural; it only acts
    /// when something (physics blend, grazing contact) pushed the ball off course.
    /// </summary>
    private void UpdateInFlightSteering()
    {
        if (!steerActive) return;

        steerTimer -= Time.fixedDeltaTime;
        if (steerTimer <= 0f)
        {
            steerActive = false;
            return;
        }

        Vector3 toTarget = steerTarget - rb.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f) return;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        // Desired horizontal velocity: same speed, aimed at the landing point.
        Vector3 desired = toTarget.normalized * horizontalVelocity.magnitude;
        Vector3 correction = (desired - horizontalVelocity) * 4f;
        rb.AddForce(Vector3.ClampMagnitude(correction, steerAcceleration), ForceMode.Acceleration);
    }

    /// <summary>
    /// Scripted, tunneling-proof table & net detection.
    /// Runs on the segment lastPos → current position each physics step.
    /// </summary>
    private void CheckScriptedSurfaces()
    {
        if (pingPongManager == null) return;

        Player p1 = pingPongManager.Player1;
        Player p2 = pingPongManager.Player2;
        if (p1 == null || p2 == null || p1.SideCollider == null || p2.SideCollider == null) return;

        Bounds b1 = p1.SideCollider.bounds;
        Bounds b2 = p2.SideCollider.bounds;
        float surfaceY = Mathf.Max(b1.max.y, b2.max.y);
        Vector3 pos = rb.position;

        // --- NET FAULT ---
        // Only when the ball genuinely goes INTO the net: it crosses the net plane,
        // below the net top, within the table's width. The grace period after a hit
        // matters: a fresh arcade arc is guaranteed to land on the opponent side, so
        // a shot struck right next to the net must never be killed at launch.
        if (Time.time - lastPaddleHitTime > netGraceAfterHit)
        {
            Vector3 axis = b2.center - b1.center;
            axis.y = 0f;
            if (axis.sqrMagnitude > 0.0001f)
            {
                axis.Normalize();
                Vector3 netCenter = (b1.center + b2.center) * 0.5f;
                float dFrom = Vector3.Dot(lastPos - netCenter, axis);
                float dTo = Vector3.Dot(pos - netCenter, axis);

                if (dFrom != dTo && Mathf.Sign(dFrom) != Mathf.Sign(dTo))
                {
                    float t = dFrom / (dFrom - dTo);
                    Vector3 crossing = Vector3.Lerp(lastPos, pos, t);

                    // Must cross within the table width (a ball passing BESIDE the
                    // table at net height is not a net fault).
                    Vector3 lateralAxis = Vector3.Cross(Vector3.up, axis);
                    float halfWidth = Mathf.Abs(b1.extents.x * lateralAxis.x) + Mathf.Abs(b1.extents.z * lateralAxis.z);
                    float lateralDistance = Mathf.Abs(Vector3.Dot(crossing - netCenter, lateralAxis));

                    if (lateralDistance <= halfWidth + 0.05f &&
                        crossing.y > surfaceY - 0.05f &&
                        crossing.y <= surfaceY + netHeightAboveTable)
                    {
                        if (soundController != null) soundController.PlayCollisionSound("Table", rb.linearVelocity.magnitude);
                        pingPongManager.OnNetFault(); // the manager scores and despawns the ball
                        return;
                    }
                }
            }
        }
        if (Phase != BallPhase.Live) return;

        // --- TABLE BOUNCE ---
        if (rb.linearVelocity.y < 0f && Time.time - lastTableBounceTime > tableBounceCooldown)
        {
            Player side = pingPongManager.GetSideOwnerAt(pos, ballRadius);
            if (side != null && side.SideCollider != null)
            {
                float bounceY = side.SideCollider.bounds.max.y + ballRadius;

                // Reached the surface this step. The small tolerance below the exact
                // crossing kills float-equality edge cases (this is what made the
                // second bounce unreliable), while requiring lastPos near/above the
                // surface prevents a ball passing UNDER the table from being
                // teleported on top of it.
                if (pos.y <= bounceY && lastPos.y > bounceY - 0.05f)
                {
                    lastTableBounceTime = Time.time;

                    Vector3 corrected = pos;
                    corrected.y = bounceY;
                    rb.position = corrected;

                    Vector3 vel = rb.linearVelocity;
                    float impact = Mathf.Abs(vel.y);
                    float upSpeed = impact * tableBounceFactor;
                    // The guaranteed pop-up only applies during a rally (keeps
                    // exchanges readable). Outside of it (ball dropped or thrown
                    // during service) the ball settles naturally, so the stall
                    // fallback can recycle it instead of it bouncing forever.
                    if (pingPongManager.currentState == PingPongManager.GameState.Rally)
                        upSpeed = Mathf.Clamp(upSpeed, minBounceUpSpeed, maxBounceUpSpeed);
                    else
                        upSpeed = Mathf.Min(upSpeed, maxBounceUpSpeed);
                    vel.y = upSpeed;
                    rb.linearVelocity = vel;

                    steerActive = false; // the assist's job ends at the table
                    stallTimer = 0f;

                    if (soundController != null) soundController.PlayCollisionSound("Table", impact);
                    OnTableHit.Invoke();
                    pingPongManager.OnTableBounce(side);
                }
            }
        }
    }

    private void CheckSafetyNet()
    {
        // Anti-softlock fallbacks only: normal deaths come from Ground/Net contact.
        if (transform.position.y < idleAnchor.y - fallDepthBelowSpawn ||
            (transform.position - idleAnchor).sqrMagnitude > maxDistanceFromSpawn * maxDistanceFromSpawn)
        {
            Kill(true);
            return;
        }

        if (rb.linearVelocity.magnitude < stallSpeedThreshold)
        {
            stallTimer += Time.fixedDeltaTime;
            if (stallTimer >= stallTimeLimit) Kill(true);
        }
        else
        {
            stallTimer = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Phase == BallPhase.Dead) return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        // Paddle hit stays physical (it feels good). Table & net are handled by
        // the scripted detection. Ground is the physical death floor.
        if (collision.gameObject.CompareTag("Paddle"))
        {
            HandlePaddleCollision(collision, impactSpeed);
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            if (Phase != BallPhase.Live || !IsBelowTableLevel()) return;
            if (soundController != null) soundController.PlayCollisionSound("Ground", impactSpeed);
            Kill(true);
        }
    }

    /// <summary>The ground may be a trigger collider: handle that case too.</summary>
    private void OnTriggerEnter(Collider other)
    {
        HandleGroundTrigger(other);
    }

    /// <summary>
    /// Also checked every frame while overlapping: a ball that entered a Ground
    /// volume ABOVE table level (kill refused) must still die once it sinks
    /// below table level inside that same volume.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        HandleGroundTrigger(other);
    }

    private void HandleGroundTrigger(Collider other)
    {
        if (Phase != BallPhase.Live) return;

        if (other.CompareTag("Ground") && IsBelowTableLevel())
        {
            if (soundController != null) soundController.PlayCollisionSound("Ground", rb.linearVelocity.magnitude);
            Kill(true);
        }
    }

    /// <summary>
    /// ARCADE HIT MODEL.
    /// A paddle hit never uses the physics reflection (contact normals + raw
    /// velocities made shots feel random and could send the ball backwards).
    /// Instead, every hit launches the ball on an exact ballistic arc onto the
    /// opponent's side, computed with the slowed gravity:
    ///   - a sideways swing moves the landing point left/right,
    ///   - a harder swing lands deeper on the table,
    ///   - a backwards/soft swing still drops the ball just past the net.
    /// </summary>
    private void HandlePaddleCollision(Collision collision, float impactSpeed)
    {
        // A held ball can't be struck: release it first (classic toss + hit serve).
        if (Phase == BallPhase.Held) return;
        if (Time.time - lastPaddleHitTime < paddleHitCooldown) return;
        if (pingPongManager == null) return;

        Player hitter = pingPongManager.GetPlayerFromPaddle(collision.gameObject);
        if (hitter == null) return;
        if (!pingPongManager.CanStrike(hitter)) return; // e.g. AI's side blocked while player must serve

        // Swing velocity: smoothed by PaddleVelocityTracker (paddles are moved
        // by the controller transform, not by physics forces).
        PaddleVelocityTracker tracker = collision.gameObject.GetComponentInParent<PaddleVelocityTracker>();
        Vector3 swing = tracker != null ? tracker.Velocity : Vector3.zero;

        // A near-still paddle touching a live ball is a passive contact:
        // let physics bounce it naturally, no game hit is registered.
        // (Checked on the RAW swing, before the power limiter.)
        if (Phase == BallPhase.Live && swing.magnitude < minActiveSwingSpeed) return;

        // PLAYER POWER LIMITER: scale down the swing, then cap it. A violent VR
        // flick can no longer send the ball across the map.
        swing = Vector3.ClampMagnitude(swing * swingPowerScale, maxSwingSpeed);

        Player opponent = pingPongManager.GetOpponent(hitter);
        if (opponent == null || opponent.SideCollider == null) return;

        Vector3 arcVelocity = ComputeArcadeArc(opponent.SideCollider.bounds, swing, out Vector3 landingTarget);

        // Optional pinch of raw physics for organic variety (arcadeBlend = 1 → pure arc).
        Vector3 outVelocity = arcVelocity;
        if (arcadeBlend < 0.999f)
        {
            Vector3 normal = collision.GetContact(0).normal; // points from paddle toward ball
            Vector3 rawVelocity = Vector3.Reflect(rb.linearVelocity - swing, normal) * 0.5f + swing;
            outVelocity = Vector3.Lerp(rawVelocity, arcVelocity, arcadeBlend);
        }

        // The final shot can never carry much more energy than the intended arc:
        // whatever the physics blend added, the ball stays in play range.
        float speedCap = Mathf.Min(maxSpeed, arcVelocity.magnitude * 1.2f);

        Phase = BallPhase.Live;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.ClampMagnitude(outVelocity, speedCap);
        rb.angularVelocity = (tracker != null ? tracker.AngularVelocity : Vector3.zero) * 0.1f;
        stallTimer = 0f;
        lastPaddleHitTime = Time.time;

        // In-flight heading correction toward the landing point (human hits only):
        // cancels any residue of the physics blend so the shot stays honest.
        steerActive = hitter.GetComponent<OpponentAI>() == null;
        steerTarget = landingTarget;
        steerTimer = steerDuration;

        if (soundController != null) soundController.PlayCollisionSound("Paddle", Mathf.Max(impactSpeed, swing.magnitude));
        OnPaddleHit.Invoke();
        pingPongManager.OnPaddleHit(hitter);
    }

    /// <summary>
    /// Exact arcade arc: from the ball's position, through an apex above the net,
    /// down onto a landing point on the opponent's side chosen from the swing.
    /// Works for any table orientation (axes derived from the target side bounds).
    /// </summary>
    private Vector3 ComputeArcadeArc(Bounds targetSide, Vector3 swing, out Vector3 landingTarget)
    {
        float tableY = targetSide.max.y;
        Vector3 start = transform.position;

        Vector3 forward = targetSide.center - start;
        forward.y = 0f;
        forward = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        Vector3 lateral = Vector3.Cross(Vector3.up, forward);

        // Backwards swings count as zero forward power: the ball ALWAYS goes toward the opponent.
        float swingForward = Mathf.Max(0f, Vector3.Dot(swing, forward));
        float swingLateral = Vector3.Dot(swing, lateral);

        // Half-extents of the target side along our axes (orientation-independent).
        float halfWidth = Mathf.Abs(targetSide.extents.x * lateral.x) + Mathf.Abs(targetSide.extents.z * lateral.z);
        float halfDepth = Mathf.Abs(targetSide.extents.x * forward.x) + Mathf.Abs(targetSide.extents.z * forward.z);

        // Landing point: swing power = depth, swing direction = left/right.
        float power = Mathf.InverseLerp(softSwingSpeed, hardSwingSpeed, swingForward);
        float depthOffset = Mathf.Lerp(-halfDepth * 0.55f, halfDepth * 0.6f, power);
        float lateralOffset = Mathf.Clamp(swingLateral * lateralControl, -halfWidth * 0.7f, halfWidth * 0.7f);

        landingTarget = targetSide.center + forward * depthOffset + lateral * lateralOffset;
        landingTarget.y = tableY + 0.02f;

        // Ballistic solve under the slowed gravity, through an apex above both the
        // start point and the table: always clears the net, always lands on target.
        float g = Mathf.Max(gravityMagnitude, 0.1f);
        float apexY = Mathf.Max(start.y, tableY) + arcApexHeight;
        float verticalSpeed = Mathf.Sqrt(2f * g * Mathf.Max(0.05f, apexY - start.y));
        float timeUp = verticalSpeed / g;
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.05f, apexY - landingTarget.y) / g);
        float flightTime = timeUp + timeDown;

        Vector3 horizontal = landingTarget - start;
        horizontal.y = 0f;
        Vector3 velocity = horizontal / flightTime;
        velocity.y = verticalSpeed;
        return velocity;
    }

    private void Kill(bool reportToManager)
    {
        if (Phase == BallPhase.Dead) return;
        Phase = BallPhase.Dead;

        // Freeze in place while the poof plays.
        rb.isKinematic = true;
        if (ballCollider != null) ballCollider.enabled = false;

        if (feedback != null) feedback.PlayPoofEffect();

        if (reportToManager && pingPongManager != null)
        {
            pingPongManager.OnBallLost();
        }

        OnBallDestroyed.Invoke(this);
        Destroy(gameObject, 0.6f);
    }
}
