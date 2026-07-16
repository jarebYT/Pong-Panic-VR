using UnityEngine;

/// <summary>
/// Keeps a world-space UI element rotated to face the local player's camera,
/// so the score/notification text is always readable and never appears mirrored
/// or upside-down. In a real networked build, each client billboards toward its
/// own local camera, so both players always read it correctly.
/// </summary>
[DefaultExecutionOrder(1000)]
public class UIBillboard : MonoBehaviour
{
    [Tooltip("If true, the UI only rotates around the world Y axis (stays upright). Recommended for a table-side scoreboard.")]
    [SerializeField] private bool yAxisOnly = true;

    [Tooltip("Optional explicit camera. If left empty, the active Camera.main is used.")]
    [SerializeField] private Transform targetCamera;

    private void LateUpdate()
    {
        Transform cam = targetCamera;
        if (cam == null)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
            }
        }
        if (cam == null) return;

        // Direction from the UI to the camera (canvas front face = +Z must point at camera)
        Vector3 toCam = cam.position - transform.position;

        if (yAxisOnly)
        {
            toCam.y = 0f;
        }

        if (toCam.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }
}
