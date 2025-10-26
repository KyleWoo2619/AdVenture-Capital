using System;
using UnityEngine;

public class CowboyGameWrapper : MonoBehaviour, IInteractiveAd
{
    [Header("Game References")]
    [SerializeField] private Canvas cowboyCanvas; // The main cowboy game canvas
    [SerializeField] private ShowdownCountdown countdownScript; // Handles countdown and instructions
    [SerializeField] private RandomEnemyFace enemyFace; // Handles enemy face randomization
    [SerializeField] private TouchAndShoot touchAndShootScript; // Handles shooting mechanics
    
    private Action onComplete;
    private InteractiveAdManager adManager;
    private bool gameStarted = false;

    void Awake()
    {
        adManager = FindFirstObjectByType<InteractiveAdManager>();
        
        // Subscribe to win/lose events
        TouchAndShoot.OnEnemyDeath += OnPlayerWins;
        ShooterAdversary.OnPlayerDeath += OnPlayerLoses;
    }

    void OnDestroy()
    {
        TouchAndShoot.OnEnemyDeath -= OnPlayerWins;
        ShooterAdversary.OnPlayerDeath -= OnPlayerLoses;
    }

    public void StartInteractiveAd(Action onAdComplete)
    {
        Debug.Log("[CowboyGameWrapper] Starting cowboy game");
        
        onComplete = onAdComplete;
        gameStarted = true;
        
        // Show canvas first
        if (cowboyCanvas != null)
            cowboyCanvas.gameObject.SetActive(true);

        // Enable unscaled time mode for all cowboy game scripts (so they work during pause)
        if (countdownScript != null)
        {
            countdownScript.SetUnscaledTimeMode(true);
            // Restart the countdown to reset states and start fresh
            countdownScript.RestartCountdown();
        }
        if (touchAndShootScript != null)
        {
            touchAndShootScript.SetUnscaledTimeMode(true);
            // Restart the touch and shoot game to reset states
            touchAndShootScript.RestartGame();
        }
            
        // Reset enemy face
        if (enemyFace != null)
            enemyFace.ChangeToRandomFace();
            
        // The ShowdownCountdown script will handle everything else automatically!
        // It starts countdown, shows instructions, enables shooting, etc.
        Debug.Log("[CowboyGameWrapper] Cowboy game started with unscaled time - works during pause!");
    }

    private void OnPlayerWins()
    {
        if (!gameStarted) return;
        
        Debug.Log("[CowboyGameWrapper] Player won cowboy game!");
        gameStarted = false;
        
        // Trigger win condition (same as Japanese simulator)
        if (adManager != null)
        {
            adManager.TriggerWinCondition();
        }
        else
        {
            Debug.LogError("[CowboyGameWrapper] No InteractiveAdManager found!");
            EndGame(); // Fallback
        }
    }

    private void OnPlayerLoses()
    {
        if (!gameStarted) return;
        
        Debug.Log("[CowboyGameWrapper] Player lost cowboy game!");
        gameStarted = false;
        
        // Wait a moment to show the loss state, then end the ad
        Invoke(nameof(EndGame), 1.5f);
    }

    private void EndGame()
    {
        Debug.Log("[CowboyGameWrapper] Ending cowboy game");
        gameStarted = false;
        
        // Hide the cowboy canvas
        if (cowboyCanvas != null)
            cowboyCanvas.gameObject.SetActive(false);
            
        // Call completion callback
        onComplete?.Invoke();
    }

    public void HideAdUI()
    {
        // Hide UI when win condition triggers (called by InteractiveAdManager)
        Debug.Log("[CowboyGameWrapper] Hiding cowboy game UI for win condition");
        
        // Hide the instruction object and countdown UI
        if (countdownScript != null)
        {
            var instructionObject = countdownScript.GetComponent<ShowdownCountdown>();
            if (instructionObject != null)
            {
                // The ShowdownCountdown manages UI, so we can disable its GameObject
                countdownScript.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Force end the game (for debugging or manual control)
    /// </summary>
    [ContextMenu("Force End Cowboy Game")]
    public void ForceEndGame()
    {
        if (gameStarted)
        {
            Debug.Log("[CowboyGameWrapper] Force ending cowboy game");
            OnPlayerLoses();
        }
    }

    /// <summary>
    /// Force win the game (for debugging)
    /// </summary>
    [ContextMenu("Force Win Cowboy Game")]
    public void ForceWinGame()
    {
        if (gameStarted)
        {
            Debug.Log("[CowboyGameWrapper] Force winning cowboy game");
            OnPlayerWins();
        }
    }
}