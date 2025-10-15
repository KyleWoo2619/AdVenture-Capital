using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Written by Kyle
// This script manages the spawning and display of fullscreen ads in the game.
// It allows for configuration of ad content, timing, and behavior during gameplay.

[DisallowMultipleComponent]
public class FullscreenAdSpawner : MonoBehaviour
{
    // --- UI References ---
    [Header("Hook up your UI")]
    [SerializeField] private Canvas adCanvas;      // The canvas that displays the ad overlay
    [SerializeField] private Image adImage;        // The fullscreen ad image
    [SerializeField] private Button closeButton;   // The close button for the ad

    // --- Ad Content ---
    [Header("Ad Content")]
    [SerializeField] private List<Sprite> adSprites = new(); // List of possible ad images

    // --- Timing Configuration ---
    [Header("Schedule (seconds)")]
    [SerializeField] private float minInterval = 30f; // Minimum time between ads
    [SerializeField] private float maxInterval = 30f; // Maximum time between ads

    [Header("Close Button Delay (seconds)")]
    [SerializeField] private float minCloseDelay = 1f; // Minimum delay before close button appears
    [SerializeField] private float maxCloseDelay = 3f; // Maximum delay before close button appears

    // --- Behavior Flags ---
    [Header("Behavior")]
    [SerializeField] private bool startOnEnable = false; // If true, ads start automatically on enable
    [SerializeField] private bool runDuringPause = true; // If true, ads spawn even when game is paused
    [SerializeField] private bool pauseGameOnShow = true; // If true, game pauses when ad is shown
    [SerializeField] private int onTopSortingOrder = 5000; // Canvas sorting order for ad overlay

    // --- External References ---
    [Header("External")]
    [SerializeField] private Canvas failMenuCanvas;   // Assign the Canvas component of your fail/death menu
    [SerializeField] private Canvas winMenuCanvas;    // Assign the Canvas component of your win menu
    [SerializeField] private Canvas pauseMenuCanvas;  // (Optional) assign the Canvas of your pause menu

    public FailMenuManager FailMenuInstance; // Reference to fail menu for death flow

    // --- Internal State ---
    private Coroutine loop;                  // Coroutine for ad spawn loop
    private Coroutine closeButtonCoroutine;  // Coroutine for enabling close button
    private bool isShowing = false;          // Is an ad currently being shown?
    private int showToken = 0;               // Token to track current ad instance
    private Action onCloseOneShot;           // Callback to run after ad closes (non-death flow)
    private bool IsPlayerDeadSafe =>         // Helper: safely check if player is dead
        GameManager.instance != null && GameManager.instance.isDead;
    private bool showFailOnClose = false;    // Should fail menu show after ad closes?
    private bool showWinOnClose = false;     // Should win menu show after ad closes?

    //--- Interactive Ads -----

    [Header("Interactive Ad")]
    [SerializeField] private GameObject interactiveAdCanvas;    // might swap to an array of it?
    [SerializeField] private InteractiveAds interactiveAdScript;



    // --- Unity Lifecycle ---
    void Awake()
    {
        // Assign canvas if not set
        if (!adCanvas)
            adCanvas = GetComponentInParent<Canvas>();

        // Setup close button listener
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseAd);
        }

        // Hide ad and close button at start
        SetAdVisible(false);
        SetCloseButtonVisible(false);

        // Prevent ad image from blocking clicks (only close button should)
        if (adImage)
            adImage.raycastTarget = false;
    }

    void OnEnable()
    {
        // Start ad spawn loop if configured
        if (startOnEnable && loop == null)
        {
            loop = StartCoroutine(SpawnLoop());
        }
    }

    void OnDisable()
    {
        // Stop ad spawn loop and hide ad UI
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
        StopAllCoroutines();
        SetAdVisible(false);
        SetCloseButtonVisible(false);
        isShowing = false;
    }

    // --- Ad Spawn Loop ---
    IEnumerator SpawnLoop()
    {  // Continuously spawns ads at random intervals
        while (true)
        {
            float wait = Mathf.Max(0f, UnityEngine.Random.Range(minInterval, maxInterval));
            yield return WaitRealtime(wait);


            bool showInteractive = (interactiveAdCanvas != null && interactiveAdScript != null) && (UnityEngine.Random.value < 0.5f);

            if (showInteractive)
            {
                ShowInteractiveAd();
            }
            else
            {
                if (adSprites != null && adSprites.Count > 0 && adImage != null)
                {
                    adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
                    ShowAd();
                }
            }



            // Wait until ad is closed before spawning next
            yield return new WaitUntil(() => !isShowing && !IsInteractiveAdShowing());
            Debug.Log(showInteractive ? "Interactive ad selected" : "Normal fullscreen ad selected");
        }
    }

    // --- Show Ad ---
    // Called to display an ad (from timer, death, or restart)
    public void ShowAd()
    {
        if (isShowing) return; // Prevent double-show

        // Assign a random sprite if none set
        AssignRandomSpriteIfNeeded();

        // Bring ad canvas to front
        if (adCanvas)
        {
            adCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            adCanvas.overrideSorting = true;
            adCanvas.sortingOrder = onTopSortingOrder;
        }

        // Pause game if configured
        if (pauseGameOnShow) Time.timeScale = 0f;

        showToken++;
        isShowing = true;
        SetAdVisible(true);
        SetCloseButtonVisible(false);

        // Allow ad image to block raycasts while showing
        if (adImage) adImage.raycastTarget = true;

        // Start coroutine to enable close button after delay
        float delay = UnityEngine.Random.Range(minCloseDelay, maxCloseDelay);
        if (closeButtonCoroutine != null) StopCoroutine(closeButtonCoroutine);
        closeButtonCoroutine = StartCoroutine(EnableCloseAfter(delay, showToken));
    }

    // --- Enable Close Button After Delay ---
    IEnumerator EnableCloseAfter(float seconds, int token)
    {
        // Wait for the specified delay (unscaled time)
        yield return WaitRealtime(seconds);

        // Ensure close button is enabled in hierarchy
        if (closeButton != null && !closeButton.gameObject.activeSelf)
            closeButton.gameObject.SetActive(true);

        // Wait one frame for UI update
        yield return null;

        // Only enable if ad is still showing and token matches
        if (isShowing && token == showToken)
        {
            SetCloseButtonVisible(true);
            if (closeButton)
            {
                if (closeButton.targetGraphic)
                    closeButton.targetGraphic.raycastTarget = true;
                closeButton.interactable = true;
                closeButton.transform.SetAsLastSibling();
            }
        }
    }

    // --- Close Ad ---
    // Called when user clicks close button
    public void CloseAd()
    {
        if (!isShowing) return;
        isShowing = false;

        // Stop close button coroutine
        if (closeButtonCoroutine != null)
        {
            StopCoroutine(closeButtonCoroutine);
            closeButtonCoroutine = null;
        }

        // Hide ad and close button
        SetCloseButtonVisible(false);
        SetAdVisible(false);

        // Prevent ad image from blocking raycasts
        if (adImage) adImage.raycastTarget = false;

        // Resume time ONLY for non-death/non-win flows
        // Only unpause if no menu Canvas is enabled
        bool noMenusEnabled = (failMenuCanvas == null || !failMenuCanvas.enabled) &&
                              (winMenuCanvas == null || !winMenuCanvas.enabled) &&
                              (pauseMenuCanvas == null || !pauseMenuCanvas.enabled);

        if (pauseGameOnShow && !showFailOnClose && !showWinOnClose && noMenusEnabled)
            Time.timeScale = 1f;

        // Death flow: show fail menu (time stays paused unless fail menu resumes)
        if (showFailOnClose && FailMenuInstance != null)
            FailMenuInstance.DisplayFailMenu();

        // Win flow: show win menu (time stays paused unless win menu resumes)  
        if (showWinOnClose && winMenuCanvas != null)
            winMenuCanvas.enabled = true;

        // Non-death/non-win flow: run queued callback (e.g., load scene)
        if (!showFailOnClose && !showWinOnClose)
        {
            var cb = onCloseOneShot;
            onCloseOneShot = null;
            cb?.Invoke();
        }

        // Reset flags for next ad
        showFailOnClose = false;
        showWinOnClose = false;
    }

    // --- UI Helpers ---
    // Show/hide ad image
    void SetAdVisible(bool visible)
    {
        if (adImage)
            adImage.gameObject.SetActive(visible);
    }

    // Show/hide close button and set interactable state
    void SetCloseButtonVisible(bool visible)
    {
        if (!closeButton)
            return;
        closeButton.gameObject.SetActive(visible);

        if (closeButton.targetGraphic)
            closeButton.targetGraphic.raycastTarget = visible;
        closeButton.interactable = visible;
    }

    // Wait for real time (unaffected by timeScale)
    WaitForSecondsRealtime WaitRealtime(float seconds)
    {
        return new WaitForSecondsRealtime(Mathf.Max(0f, seconds));
    }

    // --- Ad Show Variants ---
    // Show ad on restart (can be called externally)
    public void ShowAdOnRestart()
    {
        ShowAd();
        float delay = UnityEngine.Random.Range(minCloseDelay, maxCloseDelay);
        StartCoroutine(EnableCloseAfter(delay, showToken));
    }

    // Show ad, then run a callback after close (non-death flow)
    public void ShowAdThen(Action afterClose)
    {
        if (isShowing) return;          // Prevent double-clicks
        onCloseOneShot += afterClose;   // Queue callback
        ShowAd();
    }

    // Pick random sprite, show ad, then run callback after close
    public void ShowAdRandomThen(Action afterClose)
    {
        if (adSprites != null && adSprites.Count > 0 && adImage != null)
            adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
        ShowAdThen(afterClose);
    }

    // For UI Button: show ad, then load scene after close
    public void ShowAdRandomThenLoadScene(string sceneName)
    {
        onCloseOneShot = () => SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        showFailOnClose = false;
        if (adImage && adSprites.Count > 0)
            adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
        ShowAd();
    }

    // Show ad for player death flow (shows fail menu after close)
    public void ShowAdForDeath()
    {
        onCloseOneShot = null;
        showFailOnClose = true;
        showWinOnClose = false;
        AssignRandomSpriteIfNeeded();       // Ensure ad image is set
        ShowAd();
    }

    // Show ad for player win flow (shows win menu after close)
    public void ShowAdForWin()
    {
        onCloseOneShot = null;
        showFailOnClose = false;
        showWinOnClose = true;
        AssignRandomSpriteIfNeeded();       // Ensure ad image is set
        ShowAd();
    }
    
    // Assign a random sprite if ad image is blank
    private void AssignRandomSpriteIfNeeded()
    {
        if (adImage && adImage.sprite == null && adSprites != null && adSprites.Count > 0)
            adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
    }

    public void ShowInteractiveAd()
    {
        if (interactiveAdCanvas == null || interactiveAdScript == null)
        {
            Debug.LogWarning("Interactive ad components missing!");
            return;
        }

        SetAdVisible(false);
        SetCloseButtonVisible(false);

        interactiveAdCanvas.SetActive(true);
        isShowing = true;

        interactiveAdScript.StartInteractiveAd(() =>
        {
            interactiveAdCanvas.SetActive(false);
            isShowing = false;
            if (pauseGameOnShow)
                Time.timeScale = 1f;
        });

        if (pauseGameOnShow)
            Time.timeScale = 0f;
    }

    private bool IsInteractiveAdShowing()
    {
        return interactiveAdCanvas != null && interactiveAdCanvas.activeSelf;
    }
}
