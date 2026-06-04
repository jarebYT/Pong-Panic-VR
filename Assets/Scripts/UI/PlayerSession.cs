using UnityEngine;

public class PlayerSession
{
    private const string TOKEN_KEY = "jwt_token";
    private const string USER_ID_KEY = "user_id";
    private const string USERNAME_KEY = "username";
    private const string EMAIL_KEY = "email";
    private const string GAME_MODE_KEY = "gamemode";
    private const string RATING_KEY = "rating";

    public static string Token
    {
        get => PlayerPrefs.GetString(TOKEN_KEY, "");
        set { PlayerPrefs.SetString(TOKEN_KEY, value); PlayerPrefs.Save(); }
    }

    public static string UserId
    {
        get => PlayerPrefs.GetString(USER_ID_KEY, "");
        set { PlayerPrefs.SetString(USER_ID_KEY, value); PlayerPrefs.Save(); }
    }

    public static string Username
    {
        get => PlayerPrefs.GetString(USERNAME_KEY, "");
        set { PlayerPrefs.SetString(USERNAME_KEY, value); PlayerPrefs.Save(); }
    }

    public static string Email
    {
        get => PlayerPrefs.GetString(EMAIL_KEY, "");
        set { PlayerPrefs.SetString(EMAIL_KEY, value); PlayerPrefs.Save(); }
    }

    public static string GameMode
    {
        get => PlayerPrefs.GetString(GAME_MODE_KEY, "");
        set { PlayerPrefs.SetString(GAME_MODE_KEY, value); PlayerPrefs.Save(); }
    }

    public static string Rating
    {
        get => PlayerPrefs.GetString(RATING_KEY, "");
        set { PlayerPrefs.SetString(RATING_KEY, value); PlayerPrefs.Save(); }
    }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(TOKEN_KEY);
        PlayerPrefs.DeleteKey(USER_ID_KEY);
        PlayerPrefs.DeleteKey(USERNAME_KEY);
        PlayerPrefs.DeleteKey(EMAIL_KEY);
        PlayerPrefs.DeleteKey(GAME_MODE_KEY);
        PlayerPrefs.DeleteKey(RATING_KEY);
        PlayerPrefs.Save();
    }
}
