using UnityEngine;

public class BoomboxButton : MonoBehaviour
{
    public enum ButtonType { PlayPause, Next, Prev, VolUp, VolDown }
    public ButtonType type;

    private BoomboxAudioManager manager;

    private void Start()
    {
        manager = GetComponentInParent<BoomboxAudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie que c’est bien la main (tag “Hand” par exemple)
        if (!other.CompareTag("PlayerHand")) return;

        switch (type)
        {
            case ButtonType.PlayPause:
                manager.PlayPause();
                break;
            case ButtonType.Next:
                manager.Next();
                break;
            case ButtonType.Prev:
                manager.Prev();
                break;
            case ButtonType.VolUp:
                manager.VolumeUp();
                break;
            case ButtonType.VolDown:
                manager.VolumeDown();
                break;
        }
    }
}
