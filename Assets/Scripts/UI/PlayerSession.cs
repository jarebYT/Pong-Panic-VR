using UnityEngine;


public class PlayerSession
{
    public static string Token { get; set; }
    public static string UserId { get; set; }
    public static string Username { get; set; }
    public static string Email { get; set; }
    public static string GameMode { get; set; }
    public static string Rating { get; set; }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public static void Clear()
    {
        Token = null;
        UserId = null;
        Username = null;
        Email = null;
        GameMode = null;
        Rating = null;
    }
}
