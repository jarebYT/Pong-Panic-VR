using System.Collections;
using UnityEngine;

/// <summary>
/// Visual feedback for the ball's life cycle:
///   - "Pop"  when the ball appears at the service point (scale-in with overshoot + sparks)
///   - "Poof" when the ball leaves play (shrink + smoke burst + sound)
///
/// Particles use a single one-shot ParticleSystem created at runtime
/// (no rigidbody debris — Quest 2 friendly).
/// </summary>
public class BallFeedback : MonoBehaviour
{
    [Header("Poof (disappear)")]
    [SerializeField] private bool enablePoofEffect = true;
    [SerializeField] private float poofDuration = 0.3f;
    [SerializeField] private Material poofMaterial;

    [Header("Pop (appear)")]
    [SerializeField] private float popDuration = 0.35f;

    [Header("Particles")]
    [SerializeField] private bool spawnParticles = true;
    [SerializeField] private int particleCount = 18;
    [SerializeField] private float particleSpeed = 1.6f;
    [SerializeField] private float particleLifetime = 0.5f;

    [Header("Sound")]
    [SerializeField] private bool playSound = true;
    [SerializeField] private AudioClip poofSoundClip;
    [SerializeField] private float poofVolume = 0.7f;

    private Renderer ballRenderer;
    private Vector3 originalScale;
    private bool poofPlayed;

    private void Awake()
    {
        ballRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
    }

    /// <summary>Appearance effect: scale-in with a bouncy overshoot + sparks.</summary>
    public void PlayPopEffect()
    {
        StopAllCoroutines();
        poofPlayed = false;
        StartCoroutine(PopCoroutine());

        if (spawnParticles)
        {
            SpawnBurst(transform.position, Color.white, particleCount / 2, particleSpeed * 0.8f, 0.35f);
        }
    }

    /// <summary>Disappearance effect: shrink + smoke burst + optional sound.</summary>
    public void PlayPoofEffect()
    {
        if (poofPlayed) return;
        poofPlayed = true;

        StopAllCoroutines();
        if (enablePoofEffect) StartCoroutine(PoofCoroutine());

        if (spawnParticles)
        {
            Color color = ballRenderer != null ? ballRenderer.material.color : Color.white;
            SpawnBurst(transform.position, color, particleCount, particleSpeed, particleLifetime);
        }

        if (playSound && poofSoundClip != null)
        {
            AudioSource.PlayClipAtPoint(poofSoundClip, transform.position, poofVolume);
        }
    }

    private IEnumerator PopCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            // Overshoot curve: 0 → 1.25 → 1
            float scale = t < 0.6f
                ? Mathf.SmoothStep(0f, 1.25f, t / 0.6f)
                : Mathf.SmoothStep(1.25f, 1f, (t - 0.6f) / 0.4f);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private IEnumerator PoofCoroutine()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < poofDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / poofDuration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        if (ballRenderer != null) ballRenderer.enabled = false;
    }

    /// <summary>
    /// One-shot particle burst, detached from the ball so it survives its destruction.
    /// </summary>
    private void SpawnBurst(Vector3 position, Color color, int count, float speed, float lifetime)
    {
        GameObject burstObject = new GameObject("BallBurstFX");
        burstObject.transform.position = position;

        ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.duration = 0.1f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = 0.03f;
        main.startColor = color;
        main.maxParticles = count;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.02f;

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = ResolveParticleMaterial();

        particles.Play();
        Destroy(burstObject, lifetime + 0.5f);
    }

    private Material ResolveParticleMaterial()
    {
        if (poofMaterial != null) return poofMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        return new Material(shader);
    }
}
