using UnityEngine;
using UnityEngine.SceneManagement;


public class PingPongManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private BoxCollider playerSideCollider;
    [SerializeField] private BoxCollider ennemySideCollider;
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject playerPaddle;
    [SerializeField] private GameObject enemyPaddle;
    [SerializeField] private int playerScore;
    [SerializeField] private int enemyScore;
    enum GameState
    {
        Start,
        Game,
        Score,
        End
    }

    private GameState currentState;

    void Start()
    {
        //Set la position des joueurs et/ou éléments de jeu
        //mettre GameState à Start avec un élément ex:bouton
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case GameState.Start:
                Service();
                break;
            case GameState.Game:
                Game();
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

    void Service()
    {
        //Lancer la balle
        //Affichage de qui démarre le service

        //Gérer le service de la balle

        //Si le serive est bon, passer à l'état Game
        currentState = GameState.Game;
    }

    void Game()
    {
        //Jeu en cours
        //ne peux plus attraper la balle pendant le jeu
        ball.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = false;

        //Vérifier si un joueur a marqué
        currentState = GameState.Score;

        //Vérifier si la partie est terminée
        if (playerScore >= 11 || enemyScore >= 11)
        {
            currentState = GameState.End;
        }
    }

    int Score()
    {
        //Gérer le score
        return 0;
    }

    void End()
    {
        //Afficher le gagnant
        //Proposer de rejouer ou de quitter
        SceneManager.LoadScene("MainMenu");
    }
}
