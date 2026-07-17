using UnityEngine;
using UnityEngine.SceneManagement;

public class AquariumLoader : MonoBehaviour
{
    // Fonction publique pour qu'elle soit visible par ton bouton ou ton objet XR
    public void LoadAquarium()
    {
        Debug.Log("Bouton aquarium activé : Chargement de la map Aquarium !");
        
        // Charge la scène "Aquarium" en fermant la précédente
        SceneManager.LoadScene("Aquarium", LoadSceneMode.Single);
    }
}