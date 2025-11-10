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
        MobileHaptics.ImpactMedium(); // Haptic feedback
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Continue()
    {
        MobileHaptics.ImpactMedium(); // Haptic feedback
        
        // Enable tile input
        GameLogic gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic != null)
            gameLogic.ResumeGame();
        
        canvas.enabled = false;
        this.gameObject.SetActive(false);
    }

    public void Pause()
    {
        MobileHaptics.ImpactMedium(); // Haptic feedback
        Time.timeScale = 0;
        canvas.enabled = true;
        this.gameObject.SetActive(true);
    }

    // In your pause menu script (wherever you handle pause button clicks)
    public void OnPauseButtonClick()
    {
        // Pause the game
        Time.timeScale = 0f;

        // Disable tile input
        GameLogic gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic != null)
            gameLogic.SetPauseState(true);

        // Show pause menu
        canvas.enabled = true;
    }

    public void OnResumeButtonClick()
    {
        // Resume the game
        Time.timeScale = 1f;

        // Enable tile input
        GameLogic gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic != null)
            gameLogic.SetPauseState(false);

        // Hide pause menu
        canvas.enabled = false;
    }
}
