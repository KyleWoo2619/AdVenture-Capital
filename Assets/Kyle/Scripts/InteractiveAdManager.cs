using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Written by Kyle
// This script manages multiple interactive ads in a reusable, editor-friendly way.
// It can spawn random interactive ads when called and provides debugging functionality.

[DisallowMultipleComponent]
public class InteractiveAdManager : MonoBehaviour
{
    [System.Serializable]
    public class InteractiveAdEntry
    {
        [Header("Ad Information")]
        public string adName = "Interactive Ad";
        
        [Header("Ad Components")]
        public GameObject adCanvas;           // The canvas GameObject for this ad
        public MonoBehaviour adScript;        // The interactive ad script component (must implement IInteractiveAd)
        
        [Header("Settings")]
        public bool isEnabled = true;         // Whether this ad can be shown
        
        public bool IsValid => adCanvas != null && adScript != null && adScript is IInteractiveAd && isEnabled;
        
        public IInteractiveAd GetAdInterface() => adScript as IInteractiveAd;
    }

    // --- Interactive Ad Configuration ---
    [Header("Interactive Ads Setup")]
    [SerializeField] private List<InteractiveAdEntry> interactiveAds = new();

    // --- Behavior Settings ---
    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnShow = true;   // If true, game pauses when interactive ad is shown
    [SerializeField] private int onTopSortingOrder = 6000;  // Canvas sorting order (higher than fullscreen ads)
    
    // --- Background Settings ---
    [Header("Background UI")]
    [SerializeField] private Image backgroundBlockerImage; // Black background image to cover exposed areas on long phones
    
    // --- Win Condition Settings ---
    [Header("Win Condition UI")]
    [SerializeField] private Image winImage; // Win condition image to show
    [SerializeField] private Button winCloseButton; // Close button for win condition
    
    [Header("Win Condition Settings")]
    [SerializeField] private float minWinDelay = 2f; // Minimum delay before showing close button
    [SerializeField] private float maxWinDelay = 3f; // Maximum delay before showing close button
    [SerializeField] private string sceneToLoad = ""; // Scene name to load when win close button is pressed

    // --- Debug Settings ---
    [Header("Debug")]
    [SerializeField] private bool enableDebugKey = true;    // Enable U key for debugging
    [SerializeField] private KeyCode debugKey = KeyCode.U;  // Key to spawn interactive ad for testing

    // --- Internal State ---
    private bool isShowingInteractiveAd = false;           // Is an interactive ad currently being shown?
    private InteractiveAdEntry currentAd = null;           // Currently active ad
    private Action onCompleteCallback = null;              // Callback when ad completes
    private bool isShowingWinCondition = false;            // Is win condition currently active?
    private Coroutine winCloseButtonCoroutine = null;      // Coroutine for enabling win close button

    // --- Unity Lifecycle ---
    void Awake()
    {
        // Setup win close button listener
        if (winCloseButton != null)
        {
            winCloseButton.onClick.RemoveAllListeners();
            winCloseButton.onClick.AddListener(OnWinCloseButtonPressed);
        }

        // Hide all interactive ads, background blocker, and win UI at start
        HideAllInteractiveAds();
        SetBackgroundBlockerVisible(false);
        SetWinUIVisible(false);
    }

    void Update()
    {
        // Debug key functionality
        if (enableDebugKey && Input.GetKeyDown(debugKey))
        {
            if (!isShowingInteractiveAd)
            {
                Debug.Log("[InteractiveAdManager] Debug key pressed - showing random interactive ad");
                ShowRandomInteractiveAd();
            }
            else
            {
                Debug.Log("[InteractiveAdManager] Debug key pressed but interactive ad already showing");
            }
        }
    }

    void OnDisable()
    {
        // Hide all ads when disabled
        HideAllInteractiveAds();
        isShowingInteractiveAd = false;
        currentAd = null;
    }

    // --- Public Methods ---
    
    /// <summary>
    /// Show a random interactive ad from the available options
    /// </summary>
    public void ShowRandomInteractiveAd()
    {
        ShowRandomInteractiveAd(null);
    }

    /// <summary>
    /// Show a random interactive ad with a callback when completed
    /// </summary>
    /// <param name="onComplete">Callback to run when ad is completed</param>
    public void ShowRandomInteractiveAd(Action onComplete = null)
    {
        if (isShowingInteractiveAd)
        {
            Debug.LogWarning("[InteractiveAdManager] Interactive ad already showing, ignoring request");
            return;
        }

        // Get list of valid ads
        var validAds = GetValidInteractiveAds();
        if (validAds.Count == 0)
        {
            Debug.LogWarning("[InteractiveAdManager] No valid interactive ads available");
            onComplete?.Invoke(); // Still call callback even if no ads
            return;
        }

        // Pick random ad
        var selectedAd = validAds[UnityEngine.Random.Range(0, validAds.Count)];
        Debug.Log($"[InteractiveAdManager] Showing random interactive ad: {selectedAd.adName}");
        
        ShowInteractiveAd(selectedAd, onComplete);
    }

    /// <summary>
    /// Show a specific interactive ad by name
    /// </summary>
    /// <param name="adName">Name of the ad to show</param>
    /// <param name="onComplete">Callback to run when ad is completed</param>
    public void ShowInteractiveAdByName(string adName, Action onComplete = null)
    {
        if (isShowingInteractiveAd)
        {
            Debug.LogWarning("[InteractiveAdManager] Interactive ad already showing, ignoring request");
            return;
        }

        var ad = FindInteractiveAdByName(adName);
        if (ad == null)
        {
            Debug.LogWarning($"[InteractiveAdManager] Interactive ad '{adName}' not found or invalid");
            onComplete?.Invoke(); // Still call callback even if ad not found
            return;
        }

        Debug.Log($"[InteractiveAdManager] Showing specific interactive ad: {adName}");
        ShowInteractiveAd(ad, onComplete);
    }

    /// <summary>
    /// Check if an interactive ad is currently being shown
    /// </summary>
    public bool IsShowingInteractiveAd()
    {
        return isShowingInteractiveAd;
    }

    /// <summary>
    /// Get the number of valid interactive ads available
    /// </summary>
    public int GetValidAdCount()
    {
        return GetValidInteractiveAds().Count;
    }

    /// <summary>
    /// Get list of all interactive ad names
    /// </summary>
    public List<string> GetAdNames()
    {
        var names = new List<string>();
        foreach (var ad in interactiveAds)
        {
            if (ad != null && !string.IsNullOrEmpty(ad.adName))
                names.Add(ad.adName);
        }
        return names;
    }

    /// <summary>
    /// Trigger win condition (called by InteractiveAds scripts)
    /// </summary>
    public void TriggerWinCondition()
    {
        if (isShowingWinCondition)
        {
            Debug.LogWarning("[InteractiveAdManager] Win condition already showing");
            return;
        }

        Debug.Log("[InteractiveAdManager] Win condition triggered!");
        isShowingWinCondition = true;

        // Hide the current interactive ad content (but keep canvas active)
        if (currentAd != null)
        {
            var adInterface = currentAd.GetAdInterface();
            if (adInterface != null)
            {
                // Call a method on the ad script to hide its UI elements
                adInterface.HideAdUI();
            }
        }

        // Hide background blocker when win condition is triggered
        SetBackgroundBlockerVisible(false);

        // Show win UI
        SetWinUIVisible(true);

        // Start coroutine to show close button after delay
        float delay = UnityEngine.Random.Range(minWinDelay, maxWinDelay);
        if (winCloseButtonCoroutine != null) StopCoroutine(winCloseButtonCoroutine);
        winCloseButtonCoroutine = StartCoroutine(EnableWinCloseButtonAfterDelay(delay));
    }

    // --- Private Methods ---

    private void ShowInteractiveAd(InteractiveAdEntry ad, Action onComplete)
    {
        if (ad == null || !ad.IsValid)
        {
            Debug.LogError("[InteractiveAdManager] Invalid ad entry provided");
            onComplete?.Invoke();
            return;
        }

        currentAd = ad;
        onCompleteCallback = onComplete;
        isShowingInteractiveAd = true;

        // Setup canvas properties
        SetupAdCanvas(ad.adCanvas);

        // Pause game if configured
        if (pauseGameOnShow)
            Time.timeScale = 0f;

        // Show the background blocker to cover exposed areas
        SetBackgroundBlockerVisible(true);

        // Show the ad canvas
        ad.adCanvas.SetActive(true);

        // Start the interactive ad with completion callback
        var adInterface = ad.GetAdInterface();
        if (adInterface != null)
        {
            adInterface.StartInteractiveAd(OnInteractiveAdComplete);
        }

        Debug.Log($"[InteractiveAdManager] Interactive ad '{ad.adName}' started");
    }

    private void OnInteractiveAdComplete()
    {
        if (currentAd == null)
        {
            Debug.LogWarning("[InteractiveAdManager] Ad completed but no current ad reference");
            return;
        }

        Debug.Log($"[InteractiveAdManager] Interactive ad '{currentAd.adName}' completed");

        // Hide the ad canvas
        currentAd.adCanvas.SetActive(false);

        // Hide the background blocker
        SetBackgroundBlockerVisible(false);

        // Resume game if we paused it
        if (pauseGameOnShow)
            Time.timeScale = 1f;

        // Reset state
        isShowingInteractiveAd = false;
        currentAd = null;

        // Call completion callback
        var callback = onCompleteCallback;
        onCompleteCallback = null;
        callback?.Invoke();
    }

    private void SetupAdCanvas(GameObject adCanvas)
    {
        if (adCanvas == null) return;

        // Try to get Canvas component and configure it
        var canvas = adCanvas.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = onTopSortingOrder;
        }
    }

    private void HideAllInteractiveAds()
    {
        foreach (var ad in interactiveAds)
        {
            if (ad?.adCanvas != null)
                ad.adCanvas.SetActive(false);
        }
    }

    private List<InteractiveAdEntry> GetValidInteractiveAds()
    {
        var validAds = new List<InteractiveAdEntry>();
        foreach (var ad in interactiveAds)
        {
            if (ad != null && ad.IsValid)
                validAds.Add(ad);
        }
        return validAds;
    }

    private InteractiveAdEntry FindInteractiveAdByName(string adName)
    {
        foreach (var ad in interactiveAds)
        {
            if (ad != null && ad.IsValid && 
                string.Equals(ad.adName, adName, StringComparison.OrdinalIgnoreCase))
            {
                return ad;
            }
        }
        return null;
    }

    // --- Editor Helper Methods ---
    
    [ContextMenu("Hide All Ads")]
    private void EditorHideAllAds()
    {
        HideAllInteractiveAds();
        Debug.Log("[InteractiveAdManager] All interactive ads hidden");
    }

    [ContextMenu("Show Random Ad")]
    private void EditorShowRandomAd()
    {
        if (Application.isPlaying)
            ShowRandomInteractiveAd();
        else
            Debug.Log("[InteractiveAdManager] Can only show ads during play mode");
    }

    [ContextMenu("List Valid Ads")]
    private void EditorListValidAds()
    {
        var validAds = GetValidInteractiveAds();
        Debug.Log($"[InteractiveAdManager] Valid ads ({validAds.Count}): " +
                  string.Join(", ", validAds.ConvertAll(ad => ad.adName)));
    }

    // --- Win Condition Methods ---

    private IEnumerator EnableWinCloseButtonAfterDelay(float seconds)
    {
        // Wait for the specified delay (unscaled time for paused games)
        yield return new WaitForSecondsRealtime(seconds);

        // Show and enable the close button
        if (winCloseButton != null)
        {
            winCloseButton.gameObject.SetActive(true);
            winCloseButton.interactable = true;
            if (winCloseButton.targetGraphic)
                winCloseButton.targetGraphic.raycastTarget = true;
            winCloseButton.transform.SetAsLastSibling();
        }

        Debug.Log("[InteractiveAdManager] Win close button enabled after delay");
    }

    private void OnWinCloseButtonPressed()
    {
        Debug.Log("[InteractiveAdManager] Win close button pressed, loading scene: " + sceneToLoad);

        // Stop the close button coroutine if running
        if (winCloseButtonCoroutine != null)
        {
            StopCoroutine(winCloseButtonCoroutine);
            winCloseButtonCoroutine = null;
        }

        // Hide win UI
        SetWinUIVisible(false);

        // Reset win condition state
        isShowingWinCondition = false;

        // Robustness: clear time scale and disable current ad canvas before scene load
        Time.timeScale = 1f;
        if (currentAd != null && currentAd.adCanvas != null)
        {
            currentAd.adCanvas.SetActive(false);
        }

        // Load the specified scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            // If no scene specified, reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void SetWinUIVisible(bool visible)
    {
        if (winImage) winImage.gameObject.SetActive(visible);
        if (winCloseButton)
        {
            winCloseButton.gameObject.SetActive(visible);
            winCloseButton.interactable = visible;
            if (winCloseButton.targetGraphic)
                winCloseButton.targetGraphic.raycastTarget = visible;
        }
    }

    private void SetBackgroundBlockerVisible(bool visible)
    {
        if (backgroundBlockerImage != null)
        {
            backgroundBlockerImage.gameObject.SetActive(visible);
        }
    }
}