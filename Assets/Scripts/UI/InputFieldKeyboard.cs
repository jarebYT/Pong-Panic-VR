using TMPro;
using UnityEngine;

public class InputFieldKeyboardHook : MonoBehaviour
{
    public GameObject keyboard;
    public Transform playerCamera;
    private TMP_InputField field;
    private bool keyboardActive = false;

    void Awake()
    {
        field = GetComponent<TMP_InputField>();
        field.onSelect.AddListener(OnSelected);
        field.onDeselect.AddListener(OnDeselected);
    }

    void Update()
    {
        // Le clavier suit la tête du joueur en temps réel
        if (keyboardActive && keyboard.activeSelf)
        {
            Vector3 position =
                playerCamera.position +
                playerCamera.forward * 1.2f +
                playerCamera.up * -0.6f;

            keyboard.transform.position = position;

            Vector3 lookDir = keyboard.transform.position - playerCamera.position;
            lookDir.y = 0;
            keyboard.transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    void OnSelected(string text)
    {
        keyboard.SetActive(true);
        keyboardActive = true;
        field.ActivateInputField();
        keyboard.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
    }

    void OnDeselected(string text)
    {
        keyboard.SetActive(false);
        keyboardActive = false;
    }
}