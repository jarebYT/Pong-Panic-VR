using UnityEngine;

public class Player : MonoBehaviour
{

    public int score;
    public GameObject paddle;
    public BoxCollider sideCollider;
    public Transform servicePoint;

    public Player(int initialScore, GameObject paddleObject, BoxCollider collider, Transform serviceLocation)
    {
        score = initialScore;
        paddle = paddleObject;
        sideCollider = collider;
        servicePoint = serviceLocation;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}