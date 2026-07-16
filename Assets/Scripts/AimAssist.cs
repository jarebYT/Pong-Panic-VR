using UnityEngine;

/// <summary>
/// VR aim assist for the ball.
/// When the player hits the ball, the raw physics velocity is blended toward an
/// ideal ballistic arc that lands on the opponent's side of the table. The arc is
/// computed with the ball's own slowed gravity (NOT Physics.gravity), so the
/// assisted trajectory matches what actually happens in flight.
///
/// The assist scales with how well the player's hit is already aimed: a hit
/// roughly toward the opponent gets fully assisted, a hit sideways/backwards is
/// barely corrected, so the game stays fair and readable.
/// </summary>
public class AimAssist : MonoBehaviour
{
    [SerializeField] private bool enableAimAssist = true;

    [Header("Assisted Arc")]
    [Tooltip("Horizontal speed range for the ideal arc (clamped from the player's real hit speed).")]
    [SerializeField] private float minHorizontalSpeed = 2.6f;
    [SerializeField] private float maxHorizontalSpeed = 6.5f;
    [Tooltip("Random spread of the landing point, as a fraction of the target side's size (0 = always dead center).")]
    [Range(0f, 0.45f)]
    [SerializeField] private float targetSpread = 0.28f;

    /// <summary>
    /// Blend the raw hit velocity toward an ideal arc landing on the target table side.
    /// </summary>
    /// <param name="startPosition">Ball position at the moment of the hit.</param>
    /// <param name="rawVelocity">Velocity produced by the physical paddle contact.</param>
    /// <param name="targetSide">World bounds of the opponent's table side collider.</param>
    /// <param name="gravity">Positive magnitude of the ball's custom gravity (m/s²).</param>
    /// <param name="strength">0 = no assist, 1 = fully guided.</param>
    public Vector3 ComputeAssistedVelocity(Vector3 startPosition, Vector3 rawVelocity, Bounds targetSide, float gravity, float strength)
    {
        return ComputeAssistedVelocity(startPosition, rawVelocity, targetSide, gravity, strength, out _, out _);
    }

    /// <summary>
    /// Same as above, but also reports the chosen landing point and whether the
    /// assist was actually applied (used for the in-flight steering assist).
    /// </summary>
    public Vector3 ComputeAssistedVelocity(Vector3 startPosition, Vector3 rawVelocity, Bounds targetSide, float gravity, float strength,
                                           out Vector3 landingTarget, out bool assisted)
    {
        landingTarget = targetSide.center;
        landingTarget.y = targetSide.max.y;
        assisted = false;

        if (!enableAimAssist || strength <= 0.01f || gravity <= 0.01f)
            return rawVelocity;

        // Landing target: center of the opponent side + a little spread so returns vary.
        Vector3 target = targetSide.center;
        target.x += Random.Range(-targetSide.extents.x, targetSide.extents.x) * targetSpread * 2f;
        target.z += Random.Range(-targetSide.extents.z, targetSide.extents.z) * targetSpread * 2f;
        target.y = targetSide.max.y + 0.02f;
        landingTarget = target;

        Vector3 toTarget = target - startPosition;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float distanceXZ = toTargetXZ.magnitude;
        if (distanceXZ < 0.15f) return rawVelocity;

        // Scale the assist with hit alignment: don't hijack a hit aimed the wrong way.
        Vector3 rawXZ = new Vector3(rawVelocity.x, 0f, rawVelocity.z);
        float alignment = rawXZ.sqrMagnitude > 0.001f
            ? Vector3.Dot(rawXZ.normalized, toTargetXZ.normalized)
            : 1f; // straight vertical hit: let the assist carry it over
        float effectiveStrength = strength * Mathf.InverseLerp(-0.4f, 0.4f, alignment);
        if (effectiveStrength <= 0.01f) return rawVelocity;

        // Ideal ballistic arc: keep the player's horizontal energy, solve the
        // vertical speed so the ball lands exactly on the target under 'gravity'.
        float horizontalSpeed = Mathf.Clamp(rawXZ.magnitude, minHorizontalSpeed, maxHorizontalSpeed);
        float flightTime = distanceXZ / horizontalSpeed;
        float verticalSpeed = (toTarget.y + 0.5f * gravity * flightTime * flightTime) / flightTime;

        Vector3 idealVelocity = toTargetXZ.normalized * horizontalSpeed;
        idealVelocity.y = verticalSpeed;

        assisted = true;
        return Vector3.Lerp(rawVelocity, idealVelocity, effectiveStrength);
    }

    public void SetAimAssistEnabled(bool value)
    {
        enableAimAssist = value;
    }
}
