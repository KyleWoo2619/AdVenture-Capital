using UnityEngine;

public enum GameMode
{
    NormalMode,
    AdFreeMode,
    NoAdMode
}

[DisallowMultipleComponent]
public class GameModeController : MonoBehaviour
{
    public static GameModeController Instance { get; private set; }
    
    // Static mode value that persists across scenes without DontDestroyOnLoad
    private static GameMode persistentMode = GameMode.NormalMode;
    [Header("Hide these in Normal Mode")]
    public GameObject[] hideInNormal;

    [Header("Hide these in Ad Free Mode")]
    public GameObject[] hideInAdFree;

    [Header("Hide these in No Ad Mode")]
    public GameObject[] hideInNoAd;

    [Header("Current Mode")]
    public GameMode currentMode = GameMode.NormalMode;

    private GameMode lastAppliedMode;

    private void Awake()
    {
        // Set instance for this scene (no DontDestroyOnLoad)
        Instance = this;
        
        // Load the persistent mode value
        currentMode = persistentMode;

        // Apply mode settings for this scene
        ApplyModeSettings();
    }

    private void Update()
    {
        // Only re-apply if changed to avoid per-frame work
        if (currentMode != lastAppliedMode)
            ApplyModeSettings();
    }

    public void SetGameMode(GameMode newMode)
    {
        Debug.Log($"Switching mode to: {newMode}");
        currentMode = newMode;
        persistentMode = newMode; // Save to static variable
        ApplyModeSettings();
    }

    private void ApplyModeSettings()
    {
        // reset all objects to active
        SetAllActive();

        // disable only the ones that should be hidden for this mode
        switch (currentMode)
        {
            case GameMode.NormalMode:
                // Disable both animation scripts on banner ads
                foreach(GameObject bannerAd in hideInNormal)
                {
                    if (bannerAd == null) continue;
                    
                    var bounceAd = bannerAd.GetComponent<BounceAd>();
                    if (bounceAd != null) bounceAd.enabled = false;
                    
                    var growAd = bannerAd.GetComponent<BannerAdGrow>();
                    if (growAd != null) growAd.enabled = false;
                }
                
                // Hide buttons completely in normal mode
                SetActiveForObjects(hideInNormal, false);
                break;

            case GameMode.AdFreeMode:
                // Disable ALL ads completely (paid ad-free mode)
                SetActiveForObjects(hideInAdFree, false);
                
                // Also disable interactive ad animations on banner ads
                foreach(GameObject bannerAd in hideInNormal)
                {
                    if (bannerAd == null) continue;
                    
                    var bounceAd = bannerAd.GetComponent<BounceAd>();
                    if (bounceAd != null)
                        bounceAd.enabled = false;
                    
                    var growAd = bannerAd.GetComponent<BannerAdGrow>();
                    if (growAd != null)
                        growAd.enabled = false;
                }
                break;

            case GameMode.NoAdMode:
                // Enable interactive ads with random animation (80% Bounce, 20% Grow)
                float randomValue = Random.Range(0f, 1f);
                bool useBounce = randomValue <= 0.8f; // 80% chance
                
                Debug.Log($"NoAdMode: Selected {(useBounce ? "BounceAd" : "BannerAdGrow")} (roll: {randomValue:F2})");
                Debug.Log($"NoAdMode: hideInNormal array has {hideInNormal?.Length ?? 0} objects");
                
                foreach(GameObject bannerAd in hideInNormal)
                {
                    if (bannerAd == null)
                    {
                        Debug.LogWarning("NoAdMode: Found null GameObject in hideInNormal array");
                        continue;
                    }
                    
                    Debug.Log($"NoAdMode: Processing {bannerAd.name}");
                    
                    var bounceAd = bannerAd.GetComponent<BounceAd>();
                    var growAd = bannerAd.GetComponent<BannerAdGrow>();
                    
                    Debug.Log($"  - BounceAd component: {(bounceAd != null ? "Found" : "NOT FOUND")}");
                    Debug.Log($"  - BannerAdGrow component: {(growAd != null ? "Found" : "NOT FOUND")}");
                    
                    // First disable both scripts
                    if (bounceAd != null) bounceAd.enabled = false;
                    if (growAd != null) growAd.enabled = false;
                    
                    // Make sure the GameObject is active
                    if (!bannerAd.activeSelf)
                    {
                        Debug.Log($"  - Activating {bannerAd.name}");
                        bannerAd.SetActive(true);
                    }
                    
                    // Enable canvas
                    var canvas = bannerAd.GetComponent<Canvas>();
                    if (canvas != null)
                        canvas.enabled = true;
                    
                    // Wait one frame for GameObject to become active, then enable selected script
                    StartCoroutine(EnableAdScriptNextFrame(bannerAd, bounceAd, growAd, useBounce));
                }
                
                // Hide objects in hideInNoAd array (actual banner ads)
                SetActiveForObjects(hideInNoAd, false);
                break;
        }

        lastAppliedMode = currentMode;
    }

    private void SetAllActive()
    {
        // edge case to ensure all are active before disabling specific ones lol
        SetActiveForObjects(hideInNormal, true);
        SetActiveForObjects(hideInAdFree, true);
        SetActiveForObjects(hideInNoAd, true);
    }

    private void SetActiveForObjects(GameObject[] objects, bool state)
    {
        if (objects == null) return;

        foreach (var obj in objects)
        {
            if (obj == null) continue;

            // Guard: never allow this controller to deactivate itself
            if (obj == gameObject) continue;

            obj.SetActive(state);
        }
    }


    [ContextMenu("Set → Normal Mode")]
    private void TestSetNormal() => SetGameMode(GameMode.NormalMode);

    [ContextMenu("Set → Ad Free Mode")]
    private void TestSetAdFree() => SetGameMode(GameMode.AdFreeMode);

    [ContextMenu("Set → No Ad Mode")]
    private void TestSetNoAd() => SetGameMode(GameMode.NoAdMode);
    
    /// <summary>
    /// Enables the selected ad script after waiting one frame to ensure GameObject is active
    /// </summary>
    private System.Collections.IEnumerator EnableAdScriptNextFrame(GameObject bannerAd, BounceAd bounceAd, BannerAdGrow growAd, bool useBounce)
    {
        yield return null; // Wait one frame
        
        if (bannerAd == null || !bannerAd.activeInHierarchy)
        {
            Debug.LogWarning($"EnableAdScriptNextFrame: {bannerAd?.name ?? "null"} is not active in hierarchy!");
            yield break;
        }
        
        Debug.Log($"EnableAdScriptNextFrame: Enabling script on {bannerAd.name} (useBounce: {useBounce})");
        
        if (useBounce && bounceAd != null)
        {
            bounceAd.enabled = true;
            Debug.Log($"✓ Enabled BounceAd on {bannerAd.name} (enabled: {bounceAd.enabled}, isActiveAndEnabled: {bounceAd.isActiveAndEnabled})");
        }
        else if (!useBounce && growAd != null)
        {
            growAd.enabled = true;
            Debug.Log($"✓ Enabled BannerAdGrow on {bannerAd.name} (enabled: {growAd.enabled}, isActiveAndEnabled: {growAd.isActiveAndEnabled})");
        }
        else
        {
            Debug.LogWarning($"EnableAdScriptNextFrame: Could not enable script - bounceAd: {bounceAd != null}, growAd: {growAd != null}");
        }
    }
}
