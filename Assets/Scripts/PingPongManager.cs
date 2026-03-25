using UnityEngine;
using UnityEngine.SceneManagement;


public class PingPongManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Paddle;
    [SerializeField] private GameObject player2Paddle;
    [SerializeField] private Transform player1ServicePoint;
    [SerializeField] private Transform player2ServicePoint;
    [SerializeField] private BoxCollider player1SideCollider;
    [SerializeField] private BoxCollider player2SideCollider;
    [SerializeField] private int player1Score;
    [SerializeField] private int player2Score;
    [SerializeField] private GameObject ground;
    [SerializeField] private Ball pingPongBall;
    public Player activePlayer;
    public Player inactivePlayer;
    public Player player1;
    public Player player2;
    private Player tempPlayer;
    public BoxCollider lastCornerHitted;

    public GameState currentState;

    public enum GameState
    {
        Service,
        Game,
        Inactive
    }

    public Ball ResetBall(Transform servicePoint)
    {
        return Instantiate(pingPongBall, servicePoint);
    }

    public void CheckScore()
    {
        if (activePlayer.score > 11 && inactivePlayer.score <= activePlayer.score - 2)
        {
            //TODO fonction fin de jeu
            currentState = GameState.Inactive;
            End();
        }
    }

    //Change de joueur actif
    public void SwitchActivePlayer()
    {
        activePlayer.countBallTouch = 0;
        tempPlayer = activePlayer;
        activePlayer = inactivePlayer;
        inactivePlayer = tempPlayer;
        activePlayer.countBallTouch++;
    }

    public void Score(Player player)
    {
        player.score++;
        CheckScore();
        ResetBall(activePlayer.servicePoint);
        Debug.Log("Player 1 Score: " + activePlayer.score);
        Debug.Log("Player 2 Score: " + inactivePlayer.score);
    }

    public  void TouchGround()
    {
        if (lastCornerHitted == activePlayer.sideCollider)
        {
            Score(inactivePlayer);
        }
        else
        {
            Score(activePlayer);
        }
        ResetBall(activePlayer.servicePoint);
    }

    public void IncreaseBallTouch()
    {
        activePlayer.countBallTouch++;
    }

    void Start()
    {
        activePlayer = player1;
        inactivePlayer = player2;
        ResetBall(activePlayer.servicePoint);
        currentState = GameState.Service;
        //désactiver de pouvoir attraper la balle
    }

    void Update()
    {
        if (currentState == GameState.Game)
        {
            //Si le joueur actif touche la balle plusieurs fois
            if (activePlayer.countBallTouch > 1)
            {
                Score(inactivePlayer);
                SwitchActivePlayer();
                currentState = GameState.Service;
            }
        }else if (currentState == GameState.Service)
        {
            if (activePlayer.countBallTouch > 2)
            {
                Score(inactivePlayer);
                SwitchActivePlayer();
                currentState = GameState.Service;
            } 
        }
    }

    void End()
    {
        Debug.Log("Le gagnant est : " + activePlayer.name + " avec un score de " + activePlayer.score);
    }
}