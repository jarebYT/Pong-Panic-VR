using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;



public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetLocomotion(bool locomotion)
    {
        LocomotionMediator locomotionMediator = FindFirstObjectByType<LocomotionMediator>();

        if (locomotion)
        {
            locomotionMediator.enabled = true;
            Debug.Log("Locomotion Enabled");
        }
        else
        {
            locomotionMediator.enabled = false;   
            Debug.Log("Locomotion Disabled");
        }
    }
}
