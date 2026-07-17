using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;



public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Choisir la scène à charger au démarrage du jeu
        // LoadScene(2);
    }

    public void LoadScene(int sceneIndex)
    {
        // SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        // Charger la scène spécifiée en mode additif mais va peux être casser quand il faudra changer de scène
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
