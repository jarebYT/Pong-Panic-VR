using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LobbyBounds : MonoBehaviour
{
    public Transform head; // XR Camera
    public Volume volume;
    public float maxDistance = 0.6f;

    private Vignette vignette;
    private Collider bounds;

    void Start()
    {
        bounds = GetComponent<Collider>();
        volume.profile.TryGet(out vignette);
        vignette.intensity.value = 0f;
    }

    void Update()
    {
        Vector3 closest = bounds.ClosestPoint(head.position);
        float distance = Vector3.Distance(head.position, closest);

        float t = Mathf.Clamp01(distance / maxDistance);
        vignette.intensity.value = Mathf.Lerp(0f, 0.6f, t);
    }
}
