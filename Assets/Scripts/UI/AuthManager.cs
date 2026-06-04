using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using System;

public class AuthManager : MonoBehaviour
{
    private const string BASE_URL = "https://api-pong-panic.onrender.com/api/users";

    [Header("Login Fields")]
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;
    public TextMeshProUGUI loginErrorText;

    [Header("Register Fields")]
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerEmailField;
    public TMP_InputField registerPasswordField;
    public TextMeshProUGUI registerErrorText;


    [System.Serializable] public class LoginRequest { public string email, password;}
    [System.Serializable] public class LoginResponse { public string message, token; public UserData user; }
    [System.Serializable] public class RegisterRequest { public string username, email, password; }
    [System.Serializable] public class UserData { public string id, username, email; }


    // Login 

    public void OnLoginClick()
    {
        string email = loginEmailField.text;
        string password = loginPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginErrorText.text = "Please fill in all fields.";
            return;
        }

        StartCoroutine(Login(email, password));
    }

    IEnumerator Login(string email, string password)
    {
        loginErrorText.text = "Connecting...";
        string json = JsonUtility.ToJson(new LoginRequest { email = email, password = password });
        var request = new UnityWebRequest(BASE_URL + "/login", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            PlayerSession.Token = response.token;
            PlayerSession.UserId = response.user.id;
            PlayerSession.Username = response.user.username;
            PlayerSession.Email = response.user.email;

            loginEmailField.text = "";
            loginErrorText.text = "";
            loginPasswordField.text = "";

            Debug.Log("Login OK ->" + PlayerSession.Username);
            TVPanelManager.Instance.ShowModeSelection();
        } 
        else
        {
            loginErrorText.text = request.responseCode == 401 ? "Invalid email or password" : "Connection error. Please try again!";
        }
    }

    // Register
    public void OnRegisterClick()
    {
        string username = registerUsernameField.text;
        string email = registerEmailField.text;
        string password = registerPasswordField.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            registerErrorText.text = "Please fill in all fields.";
            return;
        }
        StartCoroutine(Register(username, email, password));
    }

    IEnumerator Register (string username, string email, string password)
    {
        AnimateLoadingText();
        string json = JsonUtility.ToJson(new RegisterRequest { username = username,email = email, password = password });
        var request = new UnityWebRequest(BASE_URL + "/register", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            Debug.Log("Register OK ->" + username);
            registerUsernameField.text = "";
            registerEmailField.text = "";
            registerPasswordField.text = "";
            registerErrorText.text = "Registration successful! Please log in.";
        }
        else
        {
            registerErrorText.text = request.responseCode == 400 ? "Email already in use" : "Connection error. Please try again!";
        }
    }

    IEnumerator AnimateLoadingText()
    {
        while (true)
        {
            for (int i = 1; i <= 3; i++)
            {
                registerErrorText.text = "Creating account" + new string('.', i);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
