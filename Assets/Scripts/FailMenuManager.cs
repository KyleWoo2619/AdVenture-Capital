using UnityEngine;
using UnityEngine.SceneManagement;

public class FailMenuManager : MonoBehaviour
{
    private Canvas canvas;
    void Awake()
    {
        canvas = this.GetComponentInParent<Canvas>();

        canvas.enabled = false;

        //subscribe to unity event that involves player dying
    }
    

    public void LoadBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void DisplayFailMenu()
    {
        canvas.enabled = true;
    }
}
