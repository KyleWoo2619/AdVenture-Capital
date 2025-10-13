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
    // --- UI References ---
    [Header("Hook up your UI")]
    [SerializeField] private Canvas videoAdCanvas;      // The canvas that displays the video ad
    [SerializeField] private Canvas videoImageCanvas;   // The canvas that displays the video render texture/raw image
    [SerializeField] private VideoPlayer videoPlayer;   // The video player component
    [SerializeField] private Slider progressSlider;     // The progress slider (0 to 1)
    [SerializeField] private Button skipButton;         // The skip button
    
    // --- Audio References ---
    [Header("Audio Management")]
    [SerializeField] private AudioSource bgmAudioSource; // BGM audio source to mute during video ads
    
    // --- Ad Management ---
    [Header("Ad Canvas Management")]
    [SerializeField] private Canvas adCanvas;            // AdCanvas to disable fullscreen ads during video

    // --- Video Content ---
    [Header("Video Content")]
    [SerializeField] private List<VideoClip> videoClips = new(); // List of video ads

    // --- Timing Configuration ---
    [Header("Video Settings")]
    [SerializeField] private float skipButtonDelay = 5f;        // Time for slider to fill and skip button to appear
    [SerializeField] private bool pauseGameOnShow = true;       // If true, game pauses when ad is shown

    // --- External References ---
    [Header("External")]
    [SerializeField] private Canvas failMenuCanvas;   // Assign the Canvas component of your fail menu

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

        // Hide video ad and skip button at start
        SetVideoAdVisible(false);
        SetVideoImageVisible(false);
        SetSkipButtonVisible(false);
        
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
        SetSkipButtonVisible(false);
        
        // Mute BGM and disable fullscreen ads during video
        SetBGMAudioMuted(true);
        SetAdCanvasActive(false);

        // Reset and show slider
        if (progressSlider != null)
            progressSlider.value = 0f;

        // Start video FIRST
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            Debug.Log($"Starting video playback: {videoPlayer.clip.name}");
            videoPlayer.Play();
            Debug.Log($"Video player state: isPlaying={videoPlayer.isPlaying}");
        }
        else
        {
            Debug.LogWarning($"Cannot start video: videoPlayer={videoPlayer}, clip={videoPlayer?.clip}");
        }

        // Pause game AFTER video starts (if configured)
        if (pauseGameOnShow) 
        {
            Debug.Log("Pausing game after video setup");
            Time.timeScale = 0f;
        }

        // Start progress coroutine
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(UpdateProgressSlider());

        // Start skip button delay coroutine
        if (skipButtonCoroutine != null) StopCoroutine(skipButtonCoroutine);
        skipButtonCoroutine = StartCoroutine(EnableSkipAfterDelay(skipButtonDelay));
        
        // Start video completion watcher coroutine
        if (videoCompletionCoroutine != null) StopCoroutine(videoCompletionCoroutine);
        videoCompletionCoroutine = StartCoroutine(WatchForVideoCompletion());
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
        while (isShowing && videoPlayer != null && videoPlayer.isPlaying)
        {
            yield return null; // Wait one frame
        }

        // If we're still showing and the video stopped naturally (not manually stopped)
        if (isShowing && videoPlayer != null && !videoPlayer.isPlaying)
        {
            // Video ended naturally, execute the appropriate action automatically
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

        // Hide video ad and skip button
        SetSkipButtonVisible(false);
        SetVideoAdVisible(false);
        SetVideoImageVisible(false);
        
        // Restore BGM and AdCanvas after video ends
        SetBGMAudioMuted(false);
        SetAdCanvasActive(true);

        // Resume time only if no fail menu will be shown
        if (pauseGameOnShow && !showFailOnSkip)
            Time.timeScale = 1f;

        // Execute appropriate action
        if (showFailOnSkip && FailMenuInstance != null)
        {
            // Show fail menu (time stays paused unless fail menu resumes)
            FailMenuInstance.DisplayFailMenu();
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
        if (videoAdCanvas != null)
        {
            Debug.Log($"SetVideoAdVisible({visible}): Before - active={videoAdCanvas.gameObject.activeInHierarchy}");
            videoAdCanvas.gameObject.SetActive(visible);
            Debug.Log($"SetVideoAdVisible({visible}): After - active={videoAdCanvas.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("videoAdCanvas is null!");
        }
    }

    void SetVideoImageVisible(bool visible)
    {
        if (videoImageCanvas != null)
        {
            Debug.Log($"SetVideoImageVisible({visible}): Before - active={videoImageCanvas.gameObject.activeInHierarchy}");
            videoImageCanvas.gameObject.SetActive(visible);
            Debug.Log($"SetVideoImageVisible({visible}): After - active={videoImageCanvas.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("videoImageCanvas is null!");
        }
    }

    void SetSkipButtonVisible(bool visible)
    {
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(visible);
            skipButton.interactable = visible;
        }
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
        if (adCanvas != null)
        {
            adCanvas.gameObject.SetActive(active);
        }
    }

    // --- Video Selection ---
    private void AssignRandomVideoIfNeeded()
    {
        Debug.Log($"AssignRandomVideoIfNeeded: videoPlayer={videoPlayer}, videoClips={videoClips}, count={videoClips?.Count}");
        if (videoPlayer != null && videoClips != null && videoClips.Count > 0)
        {
            VideoClip randomClip = videoClips[UnityEngine.Random.Range(0, videoClips.Count)];
            videoPlayer.clip = randomClip;
            Debug.Log($"Assigned random video: {randomClip.name}");
        }
        else
        {
            Debug.LogWarning("Cannot assign video: Missing videoPlayer or videoClips");
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