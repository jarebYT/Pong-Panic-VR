using UnityEngine;
using System.Collections;

/// <summary>
/// Handles visual feedback when ball is destroyed.
/// Creates a "poof" effect when ball hits ground or disappears.
/// </summary>
public class BallFeedback : MonoBehaviour
{
    [Header("Poof Effect Settings")]
    [SerializeField] private bool enablePoofEffect = true;
    [SerializeField] private float poofDuration = 0.3f;
    [SerializeField] private float poofScale = 1.2f;
    [SerializeField] private Material poofMaterial; // Optional: material for effect

    [Header("Particle Settings")]
    [SerializeField] private bool spawnParticles = true;
    [SerializeField] private int particleCount = 20;
    [SerializeField] private float particleSpeed = 5f;
    [SerializeField] private float particleLifetime = 1f;

    [Header("Sound Settings")]
    [SerializeField] private bool playSound = true;
    [SerializeField] private AudioClip poofSoundClip;
    [SerializeField] private float poofVolume = 0.7f;

    private Renderer ballRenderer;
    private Rigidbody rb;
    private bool isDisappearing = false;

    private void Start()
    {
        ballRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Trigger the poof effect when ball is destroyed
    /// </summary>
    public void PlayPoofEffect()
    {
        if (isDisappearing) return;
        
        isDisappearing = true;

        if (enablePoofEffect)
        {
            StartCoroutine(PoofCoroutine());
        }

        if (spawnParticles)
        {
            SpawnParticles();
        }

        if (playSound && poofSoundClip != null)
        {
            PlayPoofSound();
        }

        Debug.Log("[BallFeedback] Poof effect triggered");
    }

    /// <summary>
    /// Coroutine for poof animation
    /// </summary>
    private IEnumerator PoofCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < poofDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / poofDuration;

            // Scale up then fade out
            float scale = Mathf.Lerp(1f, poofScale * 0.1f, progress);
            transform.localScale = originalScale * scale;

            // Fade out renderer
            if (ballRenderer != null)
            {
                Color color = ballRenderer.material.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                ballRenderer.material.color = color;
            }

            yield return null;
        }

        // Ensure ball is invisible
        if (ballRenderer != null)
        {
            ballRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Spawn particle effects at ball position
    /// </summary>
    private void SpawnParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            // Create a small sphere for particle
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = transform.position;
            particle.transform.localScale = Vector3.one * 0.05f;

            // Remove collider from particle
            Collider collider = particle.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            // Add rigidbody for physics
            Rigidbody particleRb = particle.GetComponent<Rigidbody>();
            if (particleRb == null)
            {
                particleRb = particle.AddComponent<Rigidbody>();
            }

            // Random direction
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            particleRb.linearVelocity = randomDirection * particleSpeed;

            // Color (match ball color if possible)
            Renderer particleRenderer = particle.GetComponent<Renderer>();
            if (particleRenderer != null && ballRenderer != null)
            {
                particleRenderer.material.color = ballRenderer.material.color;
            }

            // Destroy particle after lifetime
            Destroy(particle, particleLifetime);
        }
    }

    /// <summary>
    /// Play poof sound effect
    /// </summary>
    private void PlayPoofSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = poofSoundClip;
        audioSource.volume = poofVolume;
        audioSource.Play();
    }
}
