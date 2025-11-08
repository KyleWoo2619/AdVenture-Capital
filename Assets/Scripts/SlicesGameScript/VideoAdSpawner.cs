using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

// Written by Kyle
// This script manages video ads with progress slider and skip functionality
// Forces players to watch video ads before accessing fail menus or restart options

[DisallowMultipleComponent]
public class VideoAdSpawner : MonoBehaviour
{
    // --- Paired Video + Image Ad Data ---
    [System.Serializable]
    public class VideoAdPair
    {
        public string adName;           // Name for identification
        public VideoClip videoClip;     // The video to play

        public AudioSource audioSource;
        public Sprite fullscreenImage;  // The fullscreen ad image to show after video
    }

    // --- UI References ---
    [Header("Hook up your UI")]
    [SerializeField] private Canvas videoAdCanvas;      // The canvas that displays the video ad
    [SerializeField] private Canvas videoImageCanvas;   // The canvas that displays the video render texture/raw image
    [SerializeField] private Image backgroundBlockerImage; // Black image to block game background during video
    [SerializeField] private Canvas fullscreenAdCanvas; // The canvas that displays the fullscreen image ad
    [SerializeField] private Image fullscreenAdImage;   // The Image component for the fullscreen ad
    [SerializeField] private VideoPlayer videoPlayer;   // The video player component
    [SerializeField] private Slider progressSlider;     // The progress slider (0 to 1)
    [SerializeField] private Button skipButton;         // The skip button
    [SerializeField] private Button fullscreenCloseButton; // Close button for the fullscreen ad
    
    // --- Audio References ---
    [Header("Audio Management")]
    [SerializeField] private AudioSource bgmAudioSource; // BGM audio source to mute during video ads
    
    // --- Ad Management ---
    [Header("Ad Canvas Management")]
    [SerializeField] private Canvas adCanvas;            // AdCanvas to disable fullscreen ads during video

    // --- Video Content ---
    [Header("Video Content")]
    [SerializeField] private List<VideoAdPair> videoAdPairs = new(); // List of paired video + image ads

    // --- Timing Configuration ---
    [Header("Video Settings")]
    [SerializeField] private float skipButtonDelay = 5f;        // Time for slider to fill and skip button to appear
    [SerializeField] private bool pauseGameOnShow = true;       // If true, game pauses when ad is shown

    [Header("Fullscreen Ad Settings")]
    [SerializeField] private float fullscreenCloseButtonDelay = 2.5f; // Time before fullscreen close button appears (2-3 seconds)

    // --- External References ---
    [Header("External")]
    [SerializeField] private Canvas failMenuCanvas;   // Assign the Canvas component of your fail menu
    [SerializeField] private GameLogic gameLogic;     // Reference to GameLogic for proper pause/resume
    [SerializeField] private FullscreenAdSpawner fullscreenAdSpawner; // Reference to pause/resume fullscreen ad intervals

    public FailMenuManager FailMenuInstance; // Reference to fail menu for death flow

    // --- Internal State ---
    private bool isShowing = false;          // Is a video ad currently being shown?
    private Coroutine progressCoroutine;     // Coroutine for slider progress
    private Coroutine skipButtonCoroutine;   // Coroutine for enabling skip button
    private Coroutine videoCompletionCoroutine; // Coroutine for watching video completion
    private Coroutine fullscreenCloseButtonCoroutine; // Coroutine for enabling fullscreen close button
    private Action onSkipCallback;           // Callback to run after skip is pressed
    private bool showFailOnSkip = false;     // Should fail menu show after skip?
    private bool restartOnSkip = false;      // Should restart scene after skip?
    private string targetSceneName = "";     // Scene name to load when restarting
    private RenderTexture videoRenderTexture; // temporary render texture for video playback
    private VideoAdPair currentAdPair;       // Currently selected ad pair
    private bool showingFullscreenAd = false; // Is the fullscreen ad currently showing?

    // --- Unity Lifecycle ---
    void Awake()
    {
        // Assign canvas if not set
        if (!videoAdCanvas)
            videoAdCanvas = GetComponentInParent<Canvas>();

        // Setup skip button listener
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipVideoAd);
        }

        // Setup fullscreen close button listener
        if (fullscreenCloseButton != null)
        {
            fullscreenCloseButton.onClick.RemoveAllListeners();
            fullscreenCloseButton.onClick.AddListener(CloseFullscreenAd);
        }

        // Hide video ad and skip button at start
        SetVideoAdVisible(false);
        SetVideoImageVisible(false);
        SetBackgroundBlockerVisible(false);
        SetSkipButtonVisible(false);
        SetFullscreenAdVisible(false);
        
        // Ensure BGM is not muted and AdCanvas is active at start
        SetBGMAudioMuted(false);
        SetAdCanvasActive(true);

        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false; // Don't loop the video ad
        }

        // Setup slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.interactable = false; // Player can't interact with slider
        }
    }

    void OnDisable()
    {
        // Stop all coroutines and hide video ad UI
        StopAllCoroutines();
        SetVideoAdVisible(false);
        SetVideoImageVisible(false);
        SetBackgroundBlockerVisible(false);
        SetSkipButtonVisible(false);
        SetFullscreenAdVisible(false);
        
        // Restore BGM and AdCanvas
        SetBGMAudioMuted(false);
        SetAdCanvasActive(true);
        isShowing = false;
        
        // Resume fullscreen ad interval in case we were disabled during video
        if (fullscreenAdSpawner != null)
        {
            fullscreenAdSpawner.ResumeInterval();
        }

        // Stop video if playing
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
    }

    // --- Show Video Ad for Death/Fail ---
    public void ShowVideoAdForDeath()
    {
        Debug.Log("ShowVideoAdForDeath called");
        if (isShowing) 
        {
            Debug.Log("Video ad already showing, returning");
            return; // Prevent double-show
        }

        // Properly pause the game using GameLogic's pause system to disable all input
        if (gameLogic != null)
        {
            gameLogic.PauseGame();
            Debug.Log("Called GameLogic.PauseGame() to disable input during death video");
        }

        onSkipCallback = null;
        showFailOnSkip = true;
        restartOnSkip = false;
        ShowVideoAd();
    }

    // --- Show Video Ad for Restart ---
    public void ShowVideoAdForRestart()
    {
        if (isShowing) return; // Prevent double-show

        onSkipCallback = null;
        showFailOnSkip = false;
        restartOnSkip = true;
        targetSceneName = ""; // Use current scene
        ShowVideoAd();
    }

    // --- Show Video Ad and Load Specific Scene (for Button OnClick events) ---
    public void ShowVideoAdAndLoadScene(string sceneName)
    {
        if (isShowing) return; // Prevent double-show

        onSkipCallback = null;
        showFailOnSkip = false;
        restartOnSkip = true;
        targetSceneName = sceneName; // Set the specific scene to load
        ShowVideoAd();
    }

    // --- Show Video Ad with Custom Callback ---
    public void ShowVideoAdThen(Action afterSkip)
    {
        if (isShowing) return; // Prevent double-show

        onSkipCallback = afterSkip;
        showFailOnSkip = false;
        restartOnSkip = false;
        ShowVideoAd();
    }

    // --- Main Show Video Ad Method ---
    private void ShowVideoAd()
    {
        Debug.Log("ShowVideoAd method called");
        if (isShowing) 
        {
            Debug.Log("Already showing video ad, returning");
            return;
        }

        // Select random video clip
        AssignRandomVideoIfNeeded();
        Debug.Log("Video ad setup starting...");

        isShowing = true;
        Debug.Log($"Setting video ad visible: videoAdCanvas={videoAdCanvas}");
        SetVideoAdVisible(true);
        Debug.Log($"Setting video image visible: videoImageCanvas={videoImageCanvas}");
        SetVideoImageVisible(true);
        SetBackgroundBlockerVisible(true);
        SetSkipButtonVisible(false);
        
        // Mute BGM and disable fullscreen ads during video
        SetBGMAudioMuted(true);
        SetAdCanvasActive(false);
        
        // Pause fullscreen ad spawning interval during video
        if (fullscreenAdSpawner != null)
        {
            fullscreenAdSpawner.PauseInterval();
        }

        // Reset and show slider
        if (progressSlider != null)
            progressSlider.value = 0f;

        // Prepare and play the video. The coroutine will handle pausing the game
        // after playback starts and will begin the progress/skip/completion coroutines.
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            // Ensure any previous coroutines are cleared
            if (progressCoroutine != null) { StopCoroutine(progressCoroutine); progressCoroutine = null; }
            if (skipButtonCoroutine != null) { StopCoroutine(skipButtonCoroutine); skipButtonCoroutine = null; }
            if (videoCompletionCoroutine != null) { StopCoroutine(videoCompletionCoroutine); videoCompletionCoroutine = null; }

            StartCoroutine(PrepareAndPlay());
        }
        else
        {
            Debug.LogWarning($"Cannot start video: videoPlayer={videoPlayer}, clip={videoPlayer?.clip}");
            // Cleanup / restore state so the game isn't left in a partial state
            SetBGMAudioMuted(false);
            SetAdCanvasActive(true);
            SetVideoAdVisible(false);
            SetVideoImageVisible(false);
            isShowing = false;
        }
    }

    // --- Update Progress Slider ---
    IEnumerator UpdateProgressSlider()
    {
        float elapsed = 0f;

        while (elapsed < skipButtonDelay && isShowing)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time since game might be paused
            
            if (progressSlider != null)
            {
                progressSlider.value = elapsed / skipButtonDelay;
            }

            yield return null;
        }

        // Ensure slider reaches full value
        if (progressSlider != null)
            progressSlider.value = 1f;
    }

    // --- Prepare and Play Video ---
    IEnumerator PrepareAndPlay()
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("PrepareAndPlay: videoPlayer is null");
            yield break;
        }

        // Start preparing the clip
        try
        {
            videoPlayer.Prepare();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"VideoPlayer.Prepare threw: {e.Message}");
        }

        // Wait until prepared or timeout
        float timeout = 5f;
        float waited = 0f;
        while (!videoPlayer.isPrepared && waited < timeout)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("PrepareAndPlay: video did not prepare within timeout, attempting to play anyway");
        }

        // Ensure a render target is available for the player (use RawImage on videoImageCanvas)
        try
        {
            var raw = videoImageCanvas != null ? videoImageCanvas.GetComponentInChildren<UnityEngine.UI.RawImage>() : null;
            if (raw != null)
            {
                // Create a temporary render texture matching screen size (or clip size)
                if (videoRenderTexture != null)
                {
                    if (videoPlayer.targetTexture != videoRenderTexture)
                        videoPlayer.targetTexture = videoRenderTexture;
                }
                else
                {
                    int w = Math.Max(256, Screen.width);
                    int h = Math.Max(256, Screen.height);
                    videoRenderTexture = new RenderTexture(w, h, 0, RenderTextureFormat.Default);
                    videoRenderTexture.Create();
                    videoPlayer.targetTexture = videoRenderTexture;
                    raw.texture = videoRenderTexture;
                }
            }
            else
            {
                Debug.LogWarning("PrepareAndPlay: No RawImage found under videoImageCanvas; video may not be visible without target texture.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"PrepareAndPlay render target setup failed: {ex.Message}");
        }

        // Start playback
        Debug.Log($"PrepareAndPlay: Playing video {videoPlayer.clip?.name} (isPrepared={videoPlayer.isPrepared})");
        videoPlayer.Play();

        // Give one frame for the player/texture/UI to initialize
        yield return null;

        // Pause game AFTER playback has started (preserve real-time coroutines)
        if (pauseGameOnShow)
        {
            Debug.Log("PrepareAndPlay: Pausing game after playback started");
            Time.timeScale = 0f;
        }

        // Start progress/skip/completion coroutines
        if (progressCoroutine != null) { StopCoroutine(progressCoroutine); progressCoroutine = null; }
        progressCoroutine = StartCoroutine(UpdateProgressSlider());

        if (skipButtonCoroutine != null) { StopCoroutine(skipButtonCoroutine); skipButtonCoroutine = null; }
        skipButtonCoroutine = StartCoroutine(EnableSkipAfterDelay(skipButtonDelay));

        if (videoCompletionCoroutine != null) { StopCoroutine(videoCompletionCoroutine); videoCompletionCoroutine = null; }
        videoCompletionCoroutine = StartCoroutine(WatchForVideoCompletion());
    }

    // --- Enable Skip Button After Delay ---
    IEnumerator EnableSkipAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real time since game might be paused

        // Only enable if video is still showing
        if (isShowing)
        {
            SetSkipButtonVisible(true);
        }
    }

    // --- Watch For Video Completion ---
    IEnumerator WatchForVideoCompletion()
    {
        // Wait while the video is playing. If it's not playing yet, wait for it to start
        float waitForStartTimeout = 5f;
        float waited = 0f;

        // If video hasn't started, wait a short time for it to start playing
        while (isShowing && videoPlayer != null && !videoPlayer.isPlaying && waited < waitForStartTimeout)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log($"WatchForVideoCompletion: waitedForStart={waited}, isPlaying={videoPlayer?.isPlaying}, isPrepared={videoPlayer?.isPrepared}");

        // Now wait until the video stops playing naturally
        while (isShowing && videoPlayer != null && videoPlayer.isPlaying)
        {
            yield return null;
        }

        // If we're still showing and the video stopped naturally (not manually stopped)
        if (isShowing && videoPlayer != null && !videoPlayer.isPlaying)
        {
            Debug.Log("WatchForVideoCompletion: video ended naturally, calling SkipVideoAd()");
            SkipVideoAd();
        }
    }

    // --- Skip Video Ad ---
    public void SkipVideoAd()
    {
        if (!isShowing) return;
        isShowing = false;

        // Stop all coroutines
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }
        if (skipButtonCoroutine != null)
        {
            StopCoroutine(skipButtonCoroutine);
            skipButtonCoroutine = null;
        }
        if (videoCompletionCoroutine != null)
        {
            StopCoroutine(videoCompletionCoroutine);
            videoCompletionCoroutine = null;
        }

        // Stop video
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        // Release temporary render texture
        if (videoRenderTexture != null)
        {
            try
            {
                if (videoPlayer != null && videoPlayer.targetTexture == videoRenderTexture)
                    videoPlayer.targetTexture = null;
                UnityEngine.Object.Destroy(videoRenderTexture);
            }
            catch (Exception) { }
            videoRenderTexture = null;
        }

        // Hide video ad and skip button
        SetSkipButtonVisible(false);
        SetVideoAdVisible(false);
        SetVideoImageVisible(false);
        SetBackgroundBlockerVisible(false);
        
        // Restore BGM and AdCanvas after video ends
        SetBGMAudioMuted(false);
        SetAdCanvasActive(true);
        
        // Resume fullscreen ad spawning interval with fresh timer
        if (fullscreenAdSpawner != null)
        {
            fullscreenAdSpawner.ResumeInterval();
        }

        // For death videos (showFailOnSkip = true), keep the game paused - let the fail menu handle unpausing
        // Only unpause for non-death videos like restart ads
        if (pauseGameOnShow && showFailOnSkip)
        {
            // Keep paused for death videos - don't unpause
            Debug.Log("SkipVideoAd: Keeping game paused for death video");
        }
        else if (pauseGameOnShow)
        {
            // Unpause for non-death videos
            Time.timeScale = 1f;
        }

        // Execute appropriate action
        if (showFailOnSkip)
        {
            // Show paired fullscreen ad first, then fail menu after close
            ShowFullscreenAd();
        }
        else if (restartOnSkip)
        {
            // Restart scene
            RestartScene();
        }
        else if (onSkipCallback != null)
        {
            // Custom callback
            var cb = onSkipCallback;
            onSkipCallback = null;
            cb?.Invoke();
        }

        // Reset flags for next video
        showFailOnSkip = false;
        restartOnSkip = false;
    }

    // --- Scene Management ---
    private void RestartScene()
    {
        // Resume time before scene change
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // Load specified scene
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }
        else
        {
            // Restart current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
    }

    // --- UI Helpers ---
    void SetVideoAdVisible(bool visible)
    {
        if (videoAdCanvas == null)
        {
            Debug.LogWarning("videoAdCanvas is null!");
            return;
        }

        // Run at end of frame to avoid modifying UI collections during UI processing
        StartCoroutine(ApplyCanvasVisibilityAtEndOfFrame(videoAdCanvas, visible));
    }

    void SetVideoImageVisible(bool visible)
    {
        if (videoImageCanvas == null)
        {
            Debug.LogWarning("videoImageCanvas is null!");
            return;
        }

        StartCoroutine(ApplyCanvasVisibilityAtEndOfFrame(videoImageCanvas, visible));
    }

    IEnumerator ApplyCanvasVisibilityAtEndOfFrame(Canvas canvas, bool visible)
    {
        // Wait until end of frame to avoid UI collection modification during paint/layout
        yield return new WaitForEndOfFrame();

        if (canvas == null) yield break;

        try
        {
            // Only toggle the Canvas.enabled flag. Do NOT add/remove components or toggle
            // GameObject.active state here — changing serialized objects at runtime from
            // inspector/UI callbacks can cause the InvalidOperation/SerializedObject errors.
            canvas.enabled = visible;
            var cg = canvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = visible ? 1f : 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;
            }
            // If there's no CanvasGroup we simply leave the GameObject active but
            // rely on Canvas.enabled to hide the UI. This avoids modifying the
            // GameObject active state which can trigger editor serialization errors.
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ApplyCanvasVisibilityAtEndOfFrame exception: {ex.Message}");
        }
    }

    void SetSkipButtonVisible(bool visible)
    {
        if (skipButton == null) return;

        // Defer toggling active state to end of frame to avoid UI collection modification errors
        StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(skipButton.gameObject, visible));
        skipButton.interactable = visible;
    }

    void SetBGMAudioMuted(bool muted)
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.mute = muted;
        }
    }

    void SetAdCanvasActive(bool active)
    {
        if (adCanvas == null) return;

        // Use same deferred method as other canvases to avoid modifying serialized UI state mid-frame
        StartCoroutine(ApplyCanvasVisibilityAtEndOfFrame(adCanvas, active));
    }

    void SetBackgroundBlockerVisible(bool visible)
    {
        if (backgroundBlockerImage == null) return;

        // Defer toggling active state to end of frame to avoid UI collection modification errors
        StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(backgroundBlockerImage.gameObject, visible));
    }

    void SetFullscreenAdVisible(bool visible)
    {
        if (fullscreenAdCanvas == null)
        {
            Debug.LogWarning("fullscreenAdCanvas is null!");
            return;
        }

        StartCoroutine(ApplyCanvasVisibilityAtEndOfFrame(fullscreenAdCanvas, visible));
    }

    void ShowFullscreenAd()
    {
        Debug.Log("ShowFullscreenAd: Starting to display paired fullscreen ad");
        
        if (currentAdPair == null || currentAdPair.fullscreenImage == null)
        {
            Debug.LogWarning("ShowFullscreenAd: No current ad pair or fullscreen image available");
            return;
        }

        if (fullscreenAdImage == null)
        {
            Debug.LogWarning("ShowFullscreenAd: fullscreenAdImage is null");
            return;
        }

        showingFullscreenAd = true;
        
        // Set up the fullscreen ad canvas with high priority overlay
        if (fullscreenAdCanvas != null)
        {
            fullscreenAdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fullscreenAdCanvas.overrideSorting = true;
            fullscreenAdCanvas.sortingOrder = 5000; // High priority to be on top
        }

        // Set the paired image sprite
        fullscreenAdImage.sprite = currentAdPair.fullscreenImage;
        Debug.Log($"ShowFullscreenAd: Set sprite to {currentAdPair.fullscreenImage.name}");

        // Show the fullscreen ad
        SetFullscreenAdVisible(true);

        // Hide the close button initially
        if (fullscreenCloseButton != null)
        {
            fullscreenCloseButton.gameObject.SetActive(false);
            fullscreenCloseButton.interactable = false;
        }

        // Start coroutine to enable close button after delay
        if (fullscreenCloseButtonCoroutine != null)
            StopCoroutine(fullscreenCloseButtonCoroutine);
        fullscreenCloseButtonCoroutine = StartCoroutine(EnableFullscreenCloseButtonAfterDelay(fullscreenCloseButtonDelay));
        
        Debug.Log($"ShowFullscreenAd: Fullscreen ad visible, close button will appear in {fullscreenCloseButtonDelay}s");
    }

    // --- Enable Fullscreen Close Button After Delay ---
    IEnumerator EnableFullscreenCloseButtonAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real time since game might be paused

        // Only enable if fullscreen ad is still showing
        if (showingFullscreenAd && fullscreenCloseButton != null)
        {
            fullscreenCloseButton.gameObject.SetActive(true);
            fullscreenCloseButton.interactable = true;
            
            // Ensure it's on top and can receive clicks
            if (fullscreenCloseButton.targetGraphic != null)
            {
                fullscreenCloseButton.targetGraphic.raycastTarget = true;
            }
            fullscreenCloseButton.transform.SetAsLastSibling();
            
            Debug.Log("EnableFullscreenCloseButtonAfterDelay: Close button is now active and interactable");
        }
    }

    void CloseFullscreenAd()
    {
        Debug.Log("CloseFullscreenAd: Closing fullscreen ad and showing fail menu");
        showingFullscreenAd = false;

        // Stop close button coroutine
        if (fullscreenCloseButtonCoroutine != null)
        {
            StopCoroutine(fullscreenCloseButtonCoroutine);
            fullscreenCloseButtonCoroutine = null;
        }

        // Hide fullscreen ad
        SetFullscreenAdVisible(false);
        
        // Hide close button
        if (fullscreenCloseButton != null)
        {
            fullscreenCloseButton.gameObject.SetActive(false);
            fullscreenCloseButton.interactable = false;
        }

        // Show fail menu (time should stay paused)
        if (FailMenuInstance != null)
        {
            FailMenuInstance.DisplayFailMenu();
            Debug.Log("CloseFullscreenAd: Displayed fail menu");
        }
        else
        {
            Debug.LogWarning("CloseFullscreenAd: FailMenuInstance is null!");
        }
        
        // Reset state for next ad
        currentAdPair = null;
        
        Debug.Log("CloseFullscreenAd: Fullscreen ad closed successfully");
    }

    IEnumerator ApplyGameObjectActiveAtEndOfFrame(GameObject go, bool active)
    {
        yield return new WaitForEndOfFrame();
        if (go == null) yield break;
        try
        {
            go.SetActive(active);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ApplyGameObjectActiveAtEndOfFrame exception: {ex.Message}");
        }
    }

    // --- Video Selection ---
    private void AssignRandomVideoIfNeeded()
    {
        Debug.Log($"AssignRandomVideoIfNeeded: videoPlayer={videoPlayer}, videoAdPairs={videoAdPairs}, count={videoAdPairs?.Count}");
        if (videoPlayer != null && videoAdPairs != null && videoAdPairs.Count > 0)
        {
            currentAdPair = videoAdPairs[UnityEngine.Random.Range(0, videoAdPairs.Count)];
            videoPlayer.clip = currentAdPair.videoClip;
            videoPlayer.SetTargetAudioSource(0, currentAdPair.audioSource);
            
            Debug.Log($"Assigned random video ad pair: {currentAdPair.adName} (video: {currentAdPair.videoClip?.name}, image: {currentAdPair.fullscreenImage?.name})");
        }
        else
        {
            Debug.LogWarning("Cannot assign video: Missing videoPlayer or videoAdPairs");
        }
    }

    // --- Public Helper Methods ---
    public bool IsVideoAdShowing()
    {
        return isShowing;
    }

    public void SetSkipButtonDelay(float newDelay)
    {
        skipButtonDelay = newDelay;
    }
}