using UnityEngine;
using UnityEngine.SceneManagement;


public class Test : MonoBehaviour
{
    [SerializeField] private BoxCollider playerSideCollider;
    [SerializeField] private GameObject ballPrefab;
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
    private Player activePlayer;
    private Player inactivePlayer;
    private Player tempPlayer;
    private BoxCollider lastCornerHitted;
        //private Ball ball;

    private GameState currentState;

    enum GameState
    {
        Service,
        Game,
        Inactive
    }

    public Ball SpawnBall(Transform servicePoint)
    {
        return Instantiate(pingPongBall, servicePoint);
    }

    public void CheckScore()
    {
        if (activePlayer.score > 11 && inactivePlayer.score <= activePlayer.score - 2)
        {
            //TODO fonction fin de jeu
            currentState = GameState.Inactive;
        }
    }

    public void SwitchActivePlayer()
    {
        tempPlayer = activePlayer;
        activePlayer = inactivePlayer;
        inactivePlayer = tempPlayer;

    }

    void Start()
    {
        activePlayer = new Player(player1Score, player1Paddle, player1SideCollider, player1ServicePoint);
        inactivePlayer = new Player(player2Score, player2Paddle, player2SideCollider, player2ServicePoint);
        SpawnBall(activePlayer.servicePoint);
        //désactiver de pouvoir attraper la balle
    }


    void Update()
    {
        if (currentState == GameState.Game)
        {
            // Si la balle touche le sol
            if (pingPongBall.hasTouchedGround)
            {
                if (lastCornerHitted == activePlayer.sideCollider)
                {
                    inactivePlayer.score++;
                    currentState = GameState.Service;
                }
                else
                {
                    activePlayer.score++;
                    currentState = GameState.Service;
                }
            }

            //Change de joueur actif si le joueur inactif touche la balle
            if (pingPongBall.canSwapPlayer)
            {
                activePlayer.countBallTouch = 0;
                SwitchActivePlayer();
                activePlayer.countBallTouch++;
                pingPongBall.canSwapPlayer = false;
            }

            //Si le joueur actif touche la balle plusieurs fois
            if (activePlayer.countBallTouch > 1)
            {
                inactivePlayer.score++;
                SwitchActivePlayer();
                currentState = GameState.Service;
            }

            //Si la balle touche le côté du joueur actif
            if (pingPongBall.hitSameTable)
            {
                inactivePlayer.score++;
                SwitchActivePlayer();
                currentState = GameState.Service;
            }
            else if (pingPongBall.OnTriggerEnter(inactivePlayer.sideCollider))
            {
                lastCornerHitted = inactivePlayer.sideCollider;
            }
        }
    }
}