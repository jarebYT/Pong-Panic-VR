using UnityEngine;

public class ControleVagueBaleine : MonoBehaviour
{
    [Header("Vitesse Globale de la Nage")]
    public float vitesseNage = 4f;

    public enum AxeRotation { AxeX, AxeY, AxeZ }

    [System.Serializable]
    public class ReglageOs
    {
        public Transform os;
        [Range(-60f, 60f)] public float amplitude = 15f;
        [Tooltip("Le retard de l'onde (ex: 0, puis -0.5, puis -1...)")]
        public float decalagePhase = 0f;
        [Tooltip("Si l'os ne tourne pas dans le bon sens, change l'axe ici")]
        public AxeRotation axe = AxeRotation.AxeX;
        
        [HideInInspector] public Quaternion rotationInitiale;
    }

    [Header("Contrôle Indépendant des 6 Os")]
    public ReglageOs os1; // Tête
    public ReglageOs os2;
    public ReglageOs os3;
    public ReglageOs os4; // Milieu (Pivot)
    public ReglageOs os5;
    public ReglageOs os6; // Bout de la queue

    void Start()
    {
        // On sauvegarde la position de repos des 6 os
        MemoriserRotation(os1);
        MemoriserRotation(os2);
        MemoriserRotation(os3);
        MemoriserRotation(os4);
        MemoriserRotation(os5);
        MemoriserRotation(os6);
    }

    void Update()
    {
        float temps = Time.time * vitesseNage;

        // Animation des 6 os au cas par cas
        AnimerOs(os1, temps);
        AnimerOs(os2, temps);
        AnimerOs(os3, temps);
        AnimerOs(os4, temps);
        AnimerOs(os5, temps);
        AnimerOs(os6, temps);
    }

    void MemoriserRotation(ReglageOs reglage)
    {
        if (reglage != null && reglage.os != null)
        {
            reglage.rotationInitiale = reglage.os.localRotation;
        }
    }

    void AnimerOs(ReglageOs reglage, float temps)
    {
        if (reglage == null || reglage.os == null) return;

        // Calcul du mouvement de cet os précis
        float angle = Mathf.Sin(temps + reglage.decalagePhase) * reglage.amplitude;

        // Application sur le bon axe local choisi dans l'Inspecteur
        Vector3 vecteurRotation = Vector3.zero;
        if (reglage.axe == AxeRotation.AxeX) vecteurRotation = new Vector3(angle, 0, 0);
        else if (reglage.axe == AxeRotation.AxeY) vecteurRotation = new Vector3(0, angle, 0);
        else if (reglage.axe == AxeRotation.AxeZ) vecteurRotation = new Vector3(0, 0, angle);

        reglage.os.localRotation = reglage.rotationInitiale * Quaternion.Euler(vecteurRotation);
    }
}