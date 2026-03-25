using UnityEngine;
public class Ball : MonoBehaviour
{
    private BoxCollider lastCornerHitted;
    private GameObject lastPaddleHitted;
    public PingPongManager pingPongManager;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Table"))
        {
            //si la balle touche l'autre côté
            if (collision.gameObject != lastCornerHitted)
            {
                if (pingPongManager.currentState == PingPongManager.GameState.Game)
                {
                    pingPongManager.SwitchActivePlayer();
                    pingPongManager.lastCornerHitted = pingPongManager.inactivePlayer.sideCollider;
                }
                else if (pingPongManager.currentState == PingPongManager.GameState.Service)
                {
                    if(pingPongManager.activePlayer.countServiceSideTouch == 1)
                    {
                        pingPongManager.activePlayer.countServiceSideTouch = 0;
                        pingPongManager.currentState = PingPongManager.GameState.Game;
                    }
                    else
                    {
                        pingPongManager.Score(pingPongManager.inactivePlayer);
                        pingPongManager.ResetBall(pingPongManager.activePlayer.servicePoint);
                        pingPongManager.currentState = PingPongManager.GameState.Service;
                    }
                }
                
            }
            //si la balle touche le même côté
            else if (collision.gameObject == lastCornerHitted)
            {
                if (pingPongManager.currentState == PingPongManager.GameState.Game)
                {
                    pingPongManager.Score(pingPongManager.inactivePlayer);
                    pingPongManager.ResetBall(pingPongManager.activePlayer.servicePoint);
                    pingPongManager.currentState = PingPongManager.GameState.Service;
                }
                else if (pingPongManager.currentState == PingPongManager.GameState.Service)
                {
                    pingPongManager.activePlayer.countServiceSideTouch++;
                    if (pingPongManager.activePlayer.countServiceSideTouch > 2)
                    {
                        pingPongManager.Score(pingPongManager.inactivePlayer);
                        pingPongManager.ResetBall(pingPongManager.activePlayer.servicePoint);
                        pingPongManager.currentState = PingPongManager.GameState.Service;
                    }
                }
                
            }
            Debug.Log("La balle touche la table !");
        }

        if (collision.gameObject.CompareTag("Paddle"))
        {
            if (collision.gameObject != lastPaddleHitted)
            {
                lastPaddleHitted = collision.gameObject;
                pingPongManager.SwitchActivePlayer();
            }
            else if (collision.gameObject == lastPaddleHitted)
            {
                pingPongManager.IncreaseBallTouch();
            }
            Debug.Log("La balle touche la raquette !");
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("La balle touche le sol !");
            pingPongManager.TouchGround();
            Destroy(gameObject);
        }
    }
}
