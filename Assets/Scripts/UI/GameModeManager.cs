using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    public void OnSoloClick()
    {
        Debug.Log("Solo Mode Selected");
        SceneManager.LoadScene("SoloMode");
    }
    public void OnOnlineClick()
    {
        Debug.Log("1v1 Mode Selected");
        SceneManager.LoadScene("Online");
    }
    public void OnTrainingClick()
    {
        Debug.Log("Training Mode Selected");
        SceneManager.LoadScene("Training");
    }
    public void OnClickLeaderBoard()
    {
        Debug.Log("Leaderboard Selected");
        TVPanelManager.Instance.ShowLeaderboard();
    }

}
