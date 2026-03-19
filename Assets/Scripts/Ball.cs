using UnityEngine;

public class Ball : MonoBehaviour
{
    private bool hasTouchedGround = false;
    private BoxCollider lastCornerHitted;
    private GameObject lastPaddleHitted;
    private bool canSwapPlayer = false;
    private bool hitSameTable = false;
    public ballLastCorner lastCorner;

    public enum ballLastCorner
{
    activePlayerSide,
    inactivePlayerSide,
    none
}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Table"))
        {
            if (collision.gameObject != lastCornerHitted)
            {
                lastCornerHitted = collision.gameObject.GetComponent<BoxCollider>();
            }
            else if (collision.gameObject == lastCornerHitted)
            {
                hitSameTable = true;
            }
            Debug.Log("La balle touche la table !");
        }

        if (collision.gameObject.CompareTag("Paddle"))
        {
            if (collision.gameObject != lastPaddleHitted)
            {
                lastPaddleHitted = collision.gameObject;
                canSwapPlayer = true;
            }
            Debug.Log("La balle touche la raquette !");
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("La balle touche le sol !");
            if(lastCornerHitted == collision.gameObject)
            {
                lastCorner = ballLastCorner.inactivePlayerSide;
            }
            Destroy(gameObject);
        }
    }
}
