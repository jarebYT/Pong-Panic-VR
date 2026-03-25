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
            if (collision.gameObject != lastCornerHitted)
            {
                pingPongManager.SwitchActivePlayer();
                pingPongManager.lastCornerHitted = pingPongManager.inactivePlayer.sideCollider;
            }
            else if (collision.gameObject == lastCornerHitted)
            {
                pingPongManager.Score(pingPongManager.inactivePlayer);
                pingPongManager.ResetBall(pingPongManager.activePlayer.servicePoint);
                pingPongManager.currentState = PingPongManager.GameState.Service;
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
