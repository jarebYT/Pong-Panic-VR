using TMPro;
using UnityEngine;
public class InputFieldKeyboardHook : MonoBehaviour
{
    public GameObject keyboard;
    public Transform playerCamera;

    private TMP_InputField field;

    public Vector3 offset = new Vector3(0, -0.3f, 1.2f);

    void Awake()
    {
        field = GetComponent<TMP_InputField>();
        field.onSelect.AddListener(OnSelected);
    }

    void OnSelected(string text)
    {
        keyboard.SetActive(true);

        field.ActivateInputField();

        Vector3 position =
            playerCamera.position +
            playerCamera.forward * 1.2f +
            playerCamera.up * -0.3f;

        keyboard.transform.position = position;

        Vector3 lookDir = keyboard.transform.position - playerCamera.position;
        lookDir.y = 0;

        keyboard.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}