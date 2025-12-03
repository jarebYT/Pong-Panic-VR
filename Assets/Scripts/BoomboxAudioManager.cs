using UnityEngine;

public class BoomboxAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] playlist;
    private int currentIndex = 0;

    private void Start()
    {
        if (playlist.Length > 0)
        {
            audioSource.clip = playlist[currentIndex];
            audioSource.Play();
        }
    }

    public void PlayPause()
    {
        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Play();
    }

    public void Next()
    {
        if (playlist.Length == 0) return;
        currentIndex = (currentIndex + 1) % playlist.Length;
        audioSource.clip = playlist[currentIndex];
        audioSource.Play();
    }

    public void Prev()
    {
        if (playlist.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = playlist.Length - 1;
        audioSource.clip = playlist[currentIndex];
        audioSource.Play();
    }

    public void VolumeUp()
    {
        audioSource.volume = Mathf.Clamp(audioSource.volume + 0.1f, 0f, 1f);
    }

    public void VolumeDown()
    {
        audioSource.volume = Mathf.Clamp(audioSource.volume - 0.1f, 0f, 1f);
    }
}
