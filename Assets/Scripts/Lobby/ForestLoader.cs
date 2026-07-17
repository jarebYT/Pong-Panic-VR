using UnityEngine;
using UnityEngine.SceneManagement;

public class ForestLoader : MonoBehaviour
{
    // Fonction publique pour qu'elle soit visible par ton bouton ou ton objet XR
    public void LoadForest()
    {
        Debug.Log("Bouton activé : Chargement de la map Forest !");
        
        // Charge la scène "Forest" en fermant la précédente
        SceneManager.LoadScene("Forest", LoadSceneMode.Single);
    }
}