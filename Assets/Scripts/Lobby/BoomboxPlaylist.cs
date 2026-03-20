using UnityEngine;

public class BoomboxPlaylist : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] playlist;

    private int currentIndex = -1;

    void Start()
    {
        if (audioSource == null || playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("BoomBoxPlaylist mal configurée.");
            return;
        }

        PlayRandom();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandom();
        }
    }

    void PlayRandom()
    {
        int newIndex;

        // évite de rejouer le même clip
        do
        {
            newIndex = Random.Range(0, playlist.Length);
        }
        while (playlist.Length > 1 && newIndex == currentIndex);

        currentIndex = newIndex;

        audioSource.clip = playlist[currentIndex];
        audioSource.Play();
    }
}
