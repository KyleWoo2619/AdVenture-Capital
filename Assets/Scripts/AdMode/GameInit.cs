using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private GameModeController gameModeController;

    private void Start()
    {
        gameModeController = FindObjectOfType<GameModeController>();

        if (gameModeController != null)
        {
            gameModeController.SetGameMode(GameMode.NormalMode);
        }
        else
        {
            Debug.LogWarning("GameModeController not found in scene!");
        }
    }
}
