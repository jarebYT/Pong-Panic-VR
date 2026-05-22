using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public static class JsonHelper
{
    public static T[] FromJsonArray<T>(string json)
    {
        string wrappedJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

public class LeaderboardManager : MonoBehaviour
{
    private const string BASE_URL = "https://api-pong-panic.onrender.com/api/matches/leaderboard";

    public GameObject leaderboardPanel;
    public Transform leaderboardContent; // le container pour les entrées
    public GameObject leaderboardEntryPrefab; // un prefab avec une ligne du classement

    public void ShowLeaderboard()
    {
        Debug.Log("Leaderboard called");
        leaderboardPanel.SetActive(true);
        StartCoroutine(FetchLeaderboard());
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    IEnumerator FetchLeaderboard()
    {
        var request = new UnityWebRequest(BASE_URL, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            LeaderboardResponse[] entries = JsonHelper.FromJsonArray<LeaderboardResponse>(request.downloadHandler.text);

            foreach (Transform child in leaderboardContent)
                Destroy(child.gameObject);

            int rank = 1;
            foreach (var entry in entries)
            {
                GameObject entryGO = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                LeaderboardEntry entryUI = entryGO.GetComponent<LeaderboardEntry>();
                entryUI.SetData(rank, entry.username, entry.stats.rating);
                rank++;
            }
        }
    }
}

[System.Serializable]
public class LeaderboardResponse
{
    public LeaderboardData stats;
    public string _id;
    public string username;
}

[System.Serializable]
public class LeaderboardData
{
    public int total_wins;
    public int total_games;
    public int rating;
}