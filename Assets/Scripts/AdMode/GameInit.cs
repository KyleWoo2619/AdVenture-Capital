using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private GameModeController gameModeController;

    private void Start()
    {
        gameModeController = FindObjectOfType<GameModeController>();

        if (gameModeController != null)
        {
            // Don't force reset the mode - let it persist from previous scene
            // The GameModeController already loads the persistent mode in Awake()
            Debug.Log($"GameInitializer: GameModeController found. Current mode: {gameModeController.currentMode}");
        }
        else
        {
            Debug.LogWarning("GameModeController not found in scene!");
        }
    }
}
