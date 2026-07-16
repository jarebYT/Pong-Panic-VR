using UnityEngine;

/// <summary>
/// A high-fidelity, procedurally synthesized audio generator for table tennis.
/// Generates clean, authentic organic "clicks" (paddle), "knocks" (table), and "thuds" (ground)
/// dynamically at runtime. Scaled dynamically based on collision relative velocity.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BallSoundController : MonoBehaviour
{
    private double sampleRate;
    private double phase;
    
    // Play state
    private float soundTimer = 0f;
    private float soundDuration = 0f;
    private float startFreq = 0f;
    private float endFreq = 0f;
    private float volume = 0f;
    private float noiseMix = 0f;
    private bool playing = false;
    private object stateLock = new object();

    private void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 48000;

        // Ensure the AudioSource is present and configured
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Full 3D spatial sound
        audioSource.minDistance = 0.5f;
        audioSource.maxDistance = 15f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        
        // Playing a silent clip or playing on awake triggers OnAudioFilterRead to stream
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// Play a procedurally generated ping-pong collision sound based on material and impact speed.
    /// </summary>
    public void PlayCollisionSound(string materialType, float impactSpeed)
    {
        lock (stateLock)
        {
            // Scale volume logarithmically so soft hits are audible and hard hits are crisp
            volume = Mathf.Clamp01(impactSpeed / 7.5f) * 0.9f;
            if (volume < 0.08f) volume = 0.08f; // Minimal audible pop

            if (materialType == "Paddle")
            {
                // Rubber/Wood crisp click
                startFreq = 1050f;
                endFreq = 800f;
                soundDuration = 0.03f;
                noiseMix = 0.04f;
            }
            else if (materialType == "Table")
            {
                // Solid hollow bounce knock
                startFreq = 480f;
                endFreq = 340f;
                soundDuration = 0.065f;
                noiseMix = 0.07f;
            }
            else
            {
                // Soft thud for ground / other
                startFreq = 150f;
                endFreq = 90f;
                soundDuration = 0.11f;
                noiseMix = 0.22f;
            }

            soundTimer = 0f;
            phase = 0f;
            playing = true;
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!playing) return;

        double dt = 1.0 / sampleRate;

        lock (stateLock)
        {
            for (int i = 0; i < data.Length; i += channels)
            {
                if (soundTimer >= soundDuration)
                {
                    playing = false;
                    // Zero out the rest of the buffer to avoid static/clicks
                    for (int j = i; j < data.Length; j++)
                    {
                        data[j] = 0f;
                    }
                    break;
                }

                float progress = soundTimer / soundDuration;
                // Clean exponential decay envelope
                float envelope = Mathf.Exp(-6f * progress); 
                float currentFreq = Mathf.Lerp(startFreq, endFreq, progress);

                // Wave synthesis
                phase += 2.0 * Mathf.PI * currentFreq * dt;
                if (phase > 2.0 * Mathf.PI) phase -= 2.0 * Mathf.PI;
                float osc = Mathf.Sin((float)phase);

                // Noise generator for textured acoustics
                float noise = Random.Range(-1.0f, 1.0f);

                // Combined signal
                float sample = (osc * (1f - noiseMix) + noise * noiseMix) * envelope * volume;

                // Output sample to all channels (mono-to-stereo/spatial mapping)
                for (int c = 0; c < channels; c++)
                {
                    if (i + c < data.Length)
                    {
                        data[i + c] = sample;
                    }
                }

                soundTimer += (float)dt;
            }
        }
    }
}
