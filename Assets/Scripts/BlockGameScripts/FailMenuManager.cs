using UnityEngine;
using UnityEngine.SceneManagement;

public class FailMenuManager : MonoBehaviour
{
    private Canvas canvas;
    
    [Header("Win Menu")]
    [SerializeField] private GameObject winMenuObject; // Assign the win menu GameObject in Inspector
    
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
    
    public void DisplayWinMenu()
    {
        // If there's a separate win menu object, enable it
        if (winMenuObject != null)
        {
            winMenuObject.SetActive(true);
        }
        else
        {
            // Fallback: just enable the canvas (same as fail menu)
            canvas.enabled = true;
        }
    }
}
