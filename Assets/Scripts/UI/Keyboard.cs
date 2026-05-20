using TMPro;
using UnityEngine;

public class Keyboard
{
    public GameObject keyboard;
    public Transform playerCamera;

    public Vector3 offset = new Vector3(0, -0.3f, 1.2f);

    public void OpenKeyboard(TMP_InputField field)
    {
        keyboard.SetActive(true);

        // Position devant le joueur
        keyboard.transform.position = playerCamera.position + playerCamera.forward * offset.z
                                      + Vector3.up * offset.y;

        // Rotation vers le joueur
        Vector3 lookDir = keyboard.transform.position - playerCamera.position;
        lookDir.y = 0;

        keyboard.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    public void CloseKeyboard()
    {
        keyboard.SetActive(false);
    }
}

