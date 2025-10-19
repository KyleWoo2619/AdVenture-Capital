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
        public Sprite fullscreenImage;  // The fullscreen ad image to show after video
    }

    // --- UI References ---
    [Header("Hook up your UI")]
    [SerializeField] private Canvas videoAdCanvas;      // The canvas that displays the video ad
    [SerializeField] private Canvas videoImageCanvas;   // The canvas that displays the video render texture/raw image
    [SerializeField] private Image videoBackgroundImage; // Background image behind the video to block game scene
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

    // --- External References ---
    [Header("External")]
    [SerializeField] private Canvas failMenuCanvas;   // Assign the Canvas component of your fail menu
    [SerializeField] private GameLogic gameLogic;     // Reference to GameLogic for proper pause/resume

    public FailMenuManager FailMenuInstance; // Reference to fail menu for death flow

    // --- Internal State ---
    private bool isShowing = false;          // Is a video ad currently being shown?
    private Coroutine progressCoroutine;     // Coroutine for slider progress
    private Coroutine skipButtonCoroutine;   // Coroutine for enabling skip button
    private Coroutine videoCompletionCoroutine; // Coroutine for watching video completion
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
        SetVideoBackgroundVisible(false);
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
        SetVideoBackgroundVisible(false);
        SetSkipButtonVisible(false);
        
        // Restore BGM and AdCanvas
        SetBGMAudioMuted(false);
        SetAdCanvasActive(true);
        isShowing = false;

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
        SetVideoBackgroundVisible(true);
        SetSkipButtonVisible(false);
        
        // Mute BGM and disable fullscreen ads during video
        SetBGMAudioMuted(true);
        SetAdCanvasActive(false);

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
        SetVideoBackgroundVisible(false);
        
        // Restore BGM and AdCanvas after video ends
        SetBGMAudioMuted(false);
        SetAdCanvasActive(true);

        // Show the fullscreen ad image instead of immediately completing
        ShowFullscreenAd();
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

    void SetVideoBackgroundVisible(bool visible)
    {
        if (videoBackgroundImage == null)
        {
            Debug.LogWarning("videoBackgroundImage is null!");
            return;
        }

        StartCoroutine(ApplyImageVisibilityAtEndOfFrame(videoBackgroundImage, visible));
    }

    IEnumerator ApplyImageVisibilityAtEndOfFrame(Image image, bool visible)
    {
        // Wait until end of frame to avoid UI collection modification during paint/layout
        yield return new WaitForEndOfFrame();

        if (image == null) yield break;

        try
        {
            image.enabled = visible;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ApplyImageVisibilityAtEndOfFrame exception: {ex.Message}");
        }
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
        Debug.Log("[VideoAdSpawner] ShowFullscreenAd called");
        Debug.Log($"[VideoAdSpawner] currentAdPair: {(currentAdPair != null ? currentAdPair.adName : "NULL")}");
        Debug.Log($"[VideoAdSpawner] fullscreenImage: {(currentAdPair?.fullscreenImage != null ? "assigned" : "NULL")}");
        Debug.Log($"[VideoAdSpawner] fullscreenAdImage: {(fullscreenAdImage != null ? "assigned" : "NULL")}");
        Debug.Log($"[VideoAdSpawner] fullscreenAdCanvas: {(fullscreenAdCanvas != null ? "assigned" : "NULL")}");
        
        if (currentAdPair == null || currentAdPair.fullscreenImage == null)
        {
            Debug.LogWarning("Cannot show fullscreen ad: currentAdPair or fullscreenImage is null");
            CloseFullscreenAd();
            return;
        }

        if (fullscreenAdImage == null)
        {
            Debug.LogWarning("fullscreenAdImage is null!");
            CloseFullscreenAd();
            return;
        }

        Debug.Log($"Showing fullscreen ad for: {currentAdPair.adName}");
        
        // Hide video UI
        SetVideoAdVisible(false);
        SetVideoImageVisible(false);
        SetVideoBackgroundVisible(false);
        SetSkipButtonVisible(false);

        // Re-enable AdCanvas (was disabled during video playback)
        SetAdCanvasActive(true);
        Debug.Log("[VideoAdSpawner] Re-enabled adCanvas for fullscreen ad display");

        // Configure fullscreen ad canvas for proper layering (like FullscreenAdSpawner)
        if (fullscreenAdCanvas != null)
        {
            fullscreenAdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fullscreenAdCanvas.overrideSorting = true;
            fullscreenAdCanvas.sortingOrder = 5000; // High sorting order to be on top
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenAdCanvas.gameObject, true));
            Debug.Log("[VideoAdSpawner] Enabled fullscreenAdCanvas GameObject with high sorting order");
        }

        // Enable fullscreen ad image and its GameObject
        if (fullscreenAdImage != null)
        {
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenAdImage.gameObject, true));
            fullscreenAdImage.enabled = true;
            fullscreenAdImage.sprite = currentAdPair.fullscreenImage;
            Debug.Log("[VideoAdSpawner] Enabled fullscreenAdImage GameObject and set sprite");
        }

        // Show fullscreen ad canvas FIRST, then set raycast blocking
        SetFullscreenAdVisible(true);
        showingFullscreenAd = true;

        // IMPORTANT: Set raycast blocking AFTER canvas is visible to prevent override
        StartCoroutine(SetRaycastBlockingAfterFrame());

        // Start close button delay (like FullscreenAdSpawner: 2-3 seconds)
        if (fullscreenCloseButton != null)
        {
            // Initially hide the close button
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenCloseButton.gameObject, false));
            fullscreenCloseButton.interactable = false;
            
            // Enable close button after random delay between 2-3 seconds
            float closeDelay = UnityEngine.Random.Range(2f, 3f);
            StartCoroutine(EnableFullscreenCloseButtonAfterDelay(closeDelay));
            Debug.Log($"[VideoAdSpawner] Close button will appear in {closeDelay:F1} seconds");
        }
        
        Debug.Log("[VideoAdSpawner] Fullscreen ad should now be visible and blocking input");
    }

    // Set raycast blocking after the canvas visibility changes are applied
    IEnumerator SetRaycastBlockingAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return null; // Wait one more frame to ensure canvas changes are applied
        
        if (fullscreenAdImage != null && showingFullscreenAd)
        {
            fullscreenAdImage.raycastTarget = true;
            Debug.Log("[VideoAdSpawner] Set raycast blocking on fullscreen ad image AFTER canvas setup");
        }
    }

    // --- Enable Fullscreen Close Button After Delay ---
    IEnumerator EnableFullscreenCloseButtonAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real time since game might be paused

        // Only enable if fullscreen ad is still showing
        if (showingFullscreenAd && fullscreenCloseButton != null)
        {
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenCloseButton.gameObject, true));
            fullscreenCloseButton.interactable = true;
            
            // Ensure close button has proper raycast target and is on top (like FullscreenAdSpawner)
            if (fullscreenCloseButton.targetGraphic != null)
            {
                fullscreenCloseButton.targetGraphic.raycastTarget = true;
            }
            fullscreenCloseButton.transform.SetAsLastSibling(); // Bring to front
            
            Debug.Log("[VideoAdSpawner] Enabled fullscreen close button after delay with proper raycast setup");
        }
    }

    void CloseFullscreenAd()
    {
        Debug.Log("[VideoAdSpawner] CloseFullscreenAd called - Starting close process");
        Debug.Log($"[VideoAdSpawner] showFailOnSkip: {showFailOnSkip}");
        Debug.Log($"[VideoAdSpawner] restartOnSkip: {restartOnSkip}");
        Debug.Log($"[VideoAdSpawner] FailMenuInstance: {(FailMenuInstance != null ? "assigned" : "NULL")}");
        
        // Hide fullscreen ad UI components
        SetFullscreenAdVisible(false);
        showingFullscreenAd = false;

        // Disable raycast blocking on the fullscreen ad image (like FullscreenAdSpawner)
        if (fullscreenAdImage != null)
        {
            fullscreenAdImage.raycastTarget = false;
            Debug.Log("[VideoAdSpawner] Disabled raycast blocking on fullscreen ad image");
        }

        // Explicitly disable fullscreen ad GameObjects
        if (fullscreenAdCanvas != null)
        {
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenAdCanvas.gameObject, false));
            Debug.Log("[VideoAdSpawner] Disabled fullscreenAdCanvas GameObject");
        }

        if (fullscreenAdImage != null)
        {
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenAdImage.gameObject, false));
            Debug.Log("[VideoAdSpawner] Disabled fullscreenAdImage GameObject");
        }

        if (fullscreenCloseButton != null)
        {
            StartCoroutine(ApplyGameObjectActiveAtEndOfFrame(fullscreenCloseButton.gameObject, false));
            Debug.Log("[VideoAdSpawner] Disabled fullscreen close button GameObject");
        }

        // Turn off adCanvas (like FullscreenAdSpawner does)
        if (adCanvas != null)
        {
            adCanvas.enabled = false;
            Debug.Log("[VideoAdSpawner] Disabled adCanvas component after fullscreen ad close");
        }

        // Resume game time and unpause
        Time.timeScale = 1f;
        Debug.Log("[VideoAdSpawner] Resumed game time scale");

        // Properly resume the game using GameLogic's resume system
        if (gameLogic != null)
        {
            gameLogic.ResumeGame();
            Debug.Log("[VideoAdSpawner] Called GameLogic.ResumeGame() to restore input");
        }

        // Execute the original completion action (fail menu, restart, or callback)
        if (showFailOnSkip)
        {
            Debug.Log("[VideoAdSpawner] showFailOnSkip is TRUE - attempting to show fail menu");
            
            // Enable the fail menu canvas component directly
            if (failMenuCanvas != null)
            {
                failMenuCanvas.enabled = true;
                Debug.Log("[VideoAdSpawner] Enabled failMenuCanvas component directly");
            }
            else
            {
                Debug.LogWarning("[VideoAdSpawner] failMenuCanvas is null!");
            }
            
            // Also call the FailMenuInstance method
            if (FailMenuInstance != null)
            {
                Debug.Log("[VideoAdSpawner] Calling FailMenuInstance.DisplayFailMenu()");
                FailMenuInstance.DisplayFailMenu();
                Debug.Log("[VideoAdSpawner] DisplayFailMenu() call completed");
            }
            else
            {
                Debug.LogError("[VideoAdSpawner] Cannot show fail menu: FailMenuInstance is null!");
            }
        }
        else if (restartOnSkip)
        {
            Debug.Log("[VideoAdSpawner] restartOnSkip is TRUE - attempting scene restart");
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("Cannot restart scene: targetSceneName is empty");
            }
        }
        else
        {
            Debug.Log("[VideoAdSpawner] No fail or restart action configured");
        }

        // Call completion callback if assigned
        if (onSkipCallback != null)
        {
            Debug.Log("[VideoAdSpawner] Invoking onSkipCallback");
            onSkipCallback?.Invoke();
        }

        // Reset state flags
        showFailOnSkip = false;
        restartOnSkip = false;
        targetSceneName = "";
        currentAdPair = null;
        onSkipCallback = null;
        
        Debug.Log("[VideoAdSpawner] Fullscreen ad closure complete - all state reset");
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
        Debug.Log($"AssignRandomVideoIfNeeded: videoPlayer={videoPlayer}, videoAdPairs count={videoAdPairs?.Count}");
        if (videoPlayer != null && videoAdPairs != null && videoAdPairs.Count > 0)
        {
            currentAdPair = videoAdPairs[UnityEngine.Random.Range(0, videoAdPairs.Count)];
            videoPlayer.clip = currentAdPair.videoClip;
            Debug.Log($"Assigned random video ad pair: {currentAdPair.adName}");
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