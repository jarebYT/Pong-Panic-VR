using UnityEngine;

public class TVPanelManager : MonoBehaviour
{
    public static TVPanelManager Instance { get; private set; }

    public GameObject LoginPanel;
    public GameObject RegisterPanel;
    public GameObject GamePanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ShowLogin();
    }

    public void ShowLogin()
    {
        LoginPanel.SetActive(true);
        RegisterPanel.SetActive(false);
        GamePanel.SetActive(false);
        //Leaderboard.setActive(false);
    }

    public void ShowRegister()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(true);
        GamePanel.SetActive(false);
        //Leaderboard.setActive(false);
    }

    public void ShowModeSelection()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        GamePanel.SetActive(true);
        //Leaderboard.setActive(false);
    }

    //public void ShowLeaderboard()
    //{
    //    LoginPanel.SetActive(false);
    //    RegisterPanel.SetActive(false);
    //    GamePanel.SetActive(false);
    //    Leaderboard.setActive(true);
    //}
}