using UnityEngine;

public class BirdRandomMovement : MonoBehaviour
{
    public float speed = 15f;                   // Vitesse de déplacement
    public float minX = -60f, maxX = 60f;       // Limites horizontales (Gauche / Droite)
    public float minZ = -60f, maxZ = 60f;       // Limites profondeur (Bas / Haut)

    private Vector3 targetPosition;            // La position cible sur un bord
    private int currentEdge = -1;              // Garde en mémoire le bord actuel (0: Gauche, 1: Droite, 2: Bas, 3: Haut)

    void Start()
    {
        // Initialisation de la toute première destination au lancement
        SetNewTargetPosition();
    }

    void Update()
    {
        // 1. Déplacer l'oiseau vers la position cible
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 2. Faire tourner l'oiseau pour qu'il regarde sa destination
        RotateBirdTowardsTarget();

        // 3. Vérifier si l'oiseau est arrivé EXACTEMENT à sa destination (sur le bord)
        // On utilise la distance pour être sûr qu'il est arrivé à son point de chute
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTargetPosition(); // Il est hors champ, on lui donne un nouveau bord !
        }
    }

    void SetNewTargetPosition()
    {
        // On choisit un bord aléatoire (0 à 3)
        int targetEdge = Random.Range(0, 4);
        
        // On s'assure que le nouveau bord est différent de celui où l'oiseau se trouve
        while (targetEdge == currentEdge)
        {
            targetEdge = Random.Range(0, 4);
        }

        currentEdge = targetEdge;
        
        float newX = 0f;
        float newZ = 0f;

        // On génère le point précis en fonction du bord choisi
        switch (targetEdge)
        {
            case 0: // Bord Gauche (minX) - il voyagera le long de l'axe Z
                newX = minX;
                newZ = Random.Range(minZ, maxZ);
                break;
            case 1: // Bord Droit (maxX)
                newX = maxX;
                newZ = Random.Range(minZ, maxZ);
                break;
            case 2: // Bord Bas (minZ) - il voyagera le long de l'axe X
                newX = Random.Range(minX, maxX);
                newZ = minZ;
                break;
            case 3: // Bord Haut (maxZ)
                newX = Random.Range(minX, maxX);
                newZ = maxZ;
                break;
        }

        // On assigne la nouvelle position (on garde la même hauteur Y)
        targetPosition = new Vector3(newX, transform.position.y, newZ);
    }

    void RotateBirdTowardsTarget()
    {
        // Calculer la direction
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;  // Garder l'oiseau à plat

        // Vérifier que la direction n'est pas nulle
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // J'ai gardé ton ajustement de 90°. (À retirer si jamais l'oiseau vole "en crabe")
            //targetRotation *= Quaternion.Euler(0, 90, 0); 

            // Appliquer la rotation de manière fluide
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }
}