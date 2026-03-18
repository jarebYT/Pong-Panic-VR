using UnityEngine;
using UnityEngine.SceneManagement;


public class PingPongManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private BoxCollider playerSideCollider;
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject player1Paddle;
    [SerializeField] private GameObject player2Paddle;
    [SerializeField] private Transform player1ServicePoint;
    [SerializeField] private Transform player2ServicePoint;
    [SerializeField] private BoxCollider player1SideCollider;
    [SerializeField] private BoxCollider player2SideCollider;
    [SerializeField] private int player1Score;
    [SerializeField] private int player2Score;
    [SerializeField] private GameObject ground;

    private Player currentServer;
    private Player otherPlayer;
    private Player lastHitter;
    private Player player1;
    private Player player2;
    private int serviceCount = 0;

    enum GameState
    {
        Service,
        Game,
        Score,
        End
    }

    private GameState currentState;

    void Start()
    {
        //Set la position des joueurs et/ou éléments de jeu
        //mettre GameState à Start avec un élément ex:bouton
        player1 = new(player1Score, player1Paddle, player1SideCollider, player1ServicePoint);
        player2 = new(player2Score, player2Paddle, player2SideCollider, player2ServicePoint);
        currentState = GameState.Service;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GameState.Service:
                Service(currentServer, otherPlayer);
                break;
            case GameState.Game:
                Game(currentServer, otherPlayer);
                break;
            case GameState.Score:
                Score();
                break;
            case GameState.End:
                End();
                break;
            default:
                break;
        }
    }

    void Service(Player player, Player ennemy)
    {
        SpawnBall(player.servicePoint);
        if (ball.GetComponent<Collider>().bounds.Intersects(player.sideCollider.bounds))
        {
            if (ball.GetComponent<Collider>().bounds.Intersects(ennemy.sideCollider.bounds))
            {
                player.score++;
                currentState = GameState.Game;
            }
            else
            {
                ennemy.score++;
                serviceCount++;
                if (serviceCount >= 2)
                {
                    serviceCount = 0;
                    currentServer = (currentServer == player1) ? player2 : player1;
                    otherPlayer = (currentServer == player1) ? player2 : player1;
                    Service(currentServer, otherPlayer);
                }
                else
                {
                    Service(currentServer, otherPlayer);
                }
            }
        }
    }

    void Game(Player player, Player ennemy)
    {
        lastHitter = player;
        //ne peux plus attraper la balle pendant le jeu
        ball.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = false;

        //Vérifier si un joueur a marqué
        currentState = GameState.Score;
    }

    int Score()
    {
        //afficher les scores pendant 3 secondes
        Debug.Log("Player 1 Score: " + player1.score);
        Debug.Log("Player 2 Score: " + player2.score);
        if (player1.score >= 11 || player2.score >= 11)
        {
            currentState = GameState.End;
            return 0;
        }
        else
        {
            currentState = GameState.Service;
            return 0;
        }
    }

    void End()
    {
        //Afficher le gagnant
        //Proposer de rejouer ou de quitter
        SceneManager.LoadScene("MainMenu");
    }

    public void SpawnBall(Transform servicePoint)
    {
        Instantiate(ball, servicePoint);
    }
}
