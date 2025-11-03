using System;
using System.Collections;
using UnityEngine;

public class UCFRunnerWrapper : MonoBehaviour, IInteractiveAd
{
    [Header("Game References")]
    [SerializeField] private Canvas runnerCanvas; // The main runner game canvas
    [SerializeField] private ObstacleSpawner obstacleSpawner; // Manages spawning and collisions
    [SerializeField] private PlayerShooter playerShooter; // Player shooting mechanics
    [SerializeField] private LaneRunner laneRunner; // Player lane movement
    [SerializeField] private ScoreManager scoreManager; // Score tracking
    
    private Action onComplete;
    private InteractiveAdManager adManager;
    private bool gameStarted = false;

    void Awake()
    {
        adManager = FindFirstObjectByType<InteractiveAdManager>();
        
        // Subscribe to boss defeat event
        Boss.OnBossDefeated += OnBossDefeated;
    }

    void OnDestroy()
    {
        Boss.OnBossDefeated -= OnBossDefeated;
    }

    public void StartInteractiveAd(Action onAdComplete)
    {
        Debug.Log("[UCFRunnerWrapper] Starting UCF IT infinite runner game");
        
        onComplete = onAdComplete;
        gameStarted = true;
        
        // Show canvas
        if (runnerCanvas != null)
            runnerCanvas.gameObject.SetActive(true);

        // Reset score
        if (scoreManager != null)
            scoreManager.ResetScore();

        // Enable unscaled time mode for all runner game scripts (so they work during pause)
        if (obstacleSpawner != null)
        {
            obstacleSpawner.SetUnscaledTimeMode(true);
            obstacleSpawner.enabled = true;
        }

        if (playerShooter != null)
        {
            playerShooter.SetUnscaledTimeMode(true);
        }

        if (laneRunner != null)
        {
            laneRunner.SetUnscaledTimeMode(true);
        }

        Debug.Log("[UCFRunnerWrapper] Runner game started with unscaled time - works during pause!");
    }

    private void OnBossDefeated()
    {
        if (!gameStarted) return;
        Debug.Log("[UCFRunnerWrapper] Boss defeated! Player wins!");
        gameStarted = false;

        // Pause spawning
        if (obstacleSpawner != null)
            obstacleSpawner.enabled = false;

        // After 3s, show fullscreen ad
        StartCoroutine(EndWithFullscreenAfter(3f));
    }

    private IEnumerator EndWithFullscreenAfter(float seconds)
    {
        // Use realtime so it works while timeScale == 0
        yield return new WaitForSecondsRealtime(seconds);

        if (adManager != null)
        {
            adManager.TriggerWinCondition();
        }
        else
        {
            Debug.LogError("[UCFRunnerWrapper] No InteractiveAdManager found!");
            // Fallback: close out if manager missing
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("[UCFRunnerWrapper] Ending runner game (fallback)");
        gameStarted = false;
        
        // Hide the runner canvas
        if (runnerCanvas != null)
            runnerCanvas.gameObject.SetActive(false);

        // Stop spawning
        if (obstacleSpawner != null)
            obstacleSpawner.enabled = false;

        // Notify completion
        onComplete?.Invoke();
    }

    public void HideAdUI()
    {
        Debug.Log("[UCFRunnerWrapper] Hiding runner UI");
        
        // Hide the canvas
        if (runnerCanvas != null)
            runnerCanvas.gameObject.SetActive(false);

        // Stop spawning
        if (obstacleSpawner != null)
            obstacleSpawner.enabled = false;

        // Clear any spawned objects
        ClearSpawnedObjects();
    }

    private void ClearSpawnedObjects()
    {
        if (obstacleSpawner == null || obstacleSpawner.track == null) return;

        // Destroy all obstacles, powerups, bullets, and bosses
        var obstacles = obstacleSpawner.track.GetComponentsInChildren<Obstacle>();
        var powerUps = obstacleSpawner.track.GetComponentsInChildren<PowerUp>();
        var bullets = obstacleSpawner.track.GetComponentsInChildren<Bullet>();
        var bosses = obstacleSpawner.track.GetComponentsInChildren<Boss>();

        foreach (var o in obstacles) Destroy(o.gameObject);
        foreach (var p in powerUps) Destroy(p.gameObject);
        foreach (var b in bullets) Destroy(b.gameObject);
        foreach (var boss in bosses) Destroy(boss.gameObject);
    }
}
