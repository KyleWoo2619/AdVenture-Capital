using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuManager : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        canvas = this.GetComponentInParent<Canvas>();

        canvas.enabled = false;
    }

    

    void OnEnable()
    {
        //Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }

    
    public void LoadBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Continue()
    {
        canvas.enabled = false;
        this.gameObject.SetActive(false);
    }

    public void Pause()
    {
        Time.timeScale = 0;
        canvas.enabled = true;
        this.gameObject.SetActive(true);
    }


}
