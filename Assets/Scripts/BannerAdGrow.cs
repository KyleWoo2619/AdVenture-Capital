using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slowly grows a banner overlay to completely take over the screen over 30 seconds.
/// When fully grown, plays an unskippable video ad, then returns to Normal Mode.
/// Attach to each banner ad GameObject and check isTopBanner for positioning.
/// - Top banner grows downward to cover the screen
/// - Bottom banner grows upward to cover the screen
///
/// Only active in NoAdMode (interactive ad mode).
/// Randomly selected with 20% probability. Mutually exclusive with BounceAd.
/// </summary>
[DisallowMultipleComponent]
public class BannerAdGrow : MonoBehaviour
{
    // Static coordination with BounceAd - must use same variable names
    private static bool? useBounceForThisSession = null;
    private static bool hasRolled = false;
    
    [Header("Banner Position")]
    [Tooltip("Check if this is a top banner (grows down). Uncheck for bottom banner (grows up).")]
    [SerializeField] private bool isTopBanner = true;

    [Header("Takeover Settings")]
    [Tooltip("Time in seconds for banner to fully take over the screen")]
    [SerializeField, Min(1f)] private float takeoverDuration = 30f;
    
    [Header("Video Ad Integration")]
    [Tooltip("Reference to VideoAdSpawner to play unskippable ad")]
    [SerializeField] private VideoAdSpawner videoAdSpawner;
    
    [Tooltip("Reference to GameModeController to switch back to Normal Mode")]
    [SerializeField] private GameModeController gameModeController;

    [Header("Behavior")]
    [SerializeField] private bool playOnEnable = true;

    private Coroutine loop;
    private RectTransform myRectTransform;
    private bool hasStarted = false;

    // Original layout snapshot so we can restore cleanly
    private struct Layout
    {
        public Vector2 anchorMin, anchorMax, anchoredPos, sizeDelta, pivot;
        public int siblingIndex;
        public bool active;
    }

    private Layout originalLayout;
    private bool captured;
    
    void Awake()
    {
        // Get the RectTransform of this GameObject
        myRectTransform = GetComponent<RectTransform>();
        
        // Ensure Canvas has GraphicRaycaster for button clicks to work
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log($"BannerAdGrow: Added GraphicRaycaster to {canvas.name}");
            }
            raycaster.enabled = true;
        }
        
        // Ensure EventSystem exists in scene
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("BannerAdGrow: Created EventSystem");
        }
        
        // Try to find VideoAdSpawner if not assigned
        if (videoAdSpawner == null)
            videoAdSpawner = FindFirstObjectByType<VideoAdSpawner>();
        
        // Try to find GameModeController if not assigned
        if (gameModeController == null)
            gameModeController = GameModeController.Instance;
    }

    void Start()
    {
        hasStarted = true;
        CheckModeAndActivate();
    }

    void OnEnable()
    {
        // Don't start automatically - let CheckModeAndActivate handle it
    }
    
    void Update()
    {
        // Continuously check if mode changed
        CheckModeAndActivate();
    }
    
    private void CheckModeAndActivate()
    {
        var controller = GameModeController.Instance;
        if (controller == null) return;
        
        bool shouldBeActive = controller.currentMode == GameMode.NoAdMode;
        
        if (shouldBeActive && loop == null && hasStarted && playOnEnable)
        {
            // Use static flag to ensure all banners make the same choice
            if (!hasRolled)
            {
                float roll = Random.Range(0f, 1f);
                useBounceForThisSession = roll <= 0.8f; // 80% bounce, 20% grow
                hasRolled = true;
                Debug.Log($"BannerAdGrow: Global roll = {roll:F2}, useBounce = {useBounceForThisSession}");
            }
            
            // Only activate if grow was selected (useBounce == false)
            if (useBounceForThisSession == false)
            {
                Debug.Log($"BannerAdGrow: Activating on {gameObject.name}");
                var bounceAd = GetComponent<BounceAd>();
                if (bounceAd != null) bounceAd.enabled = false;
                
                enabled = true;
                
                // Ensure button stays interactable
                EnsureButtonInteractable();
                
                loop = StartCoroutine(SlowTakeoverSequence());
            }
            else
            {
                enabled = false;
            }
        }
        else if (!shouldBeActive && loop != null)
        {
            Debug.Log($"BannerAdGrow: Deactivating because not in NoAdMode - restoring banner");
            
            // Stop the coroutine
            StopCoroutine(loop);
            loop = null;
            
            // Restore banner to original state before disabling
            if (myRectTransform != null && captured)
            {
                Restore(myRectTransform, originalLayout);
                Debug.Log($"BannerAdGrow: Restored {gameObject.name} to original layout");
            }
            
            // Reset static flags for next time
            hasRolled = false;
            useBounceForThisSession = null;
            
            enabled = false;
        }
    }
    
    private void EnsureButtonInteractable()
    {
        // Disable raycast blocking on non-button UI elements
        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            // Check if this image belongs to a button
            bool isButtonGraphic = false;
            var btn = img.GetComponent<Button>();
            if (btn == null)
            {
                // Check if it's a child of a button and is the targetGraphic
                btn = img.GetComponentInParent<Button>();
                if (btn != null && btn.targetGraphic == img)
                    isButtonGraphic = true;
            }
            else
            {
                isButtonGraphic = true;
            }
            
            // Non-button images shouldn't block raycasts
            if (!isButtonGraphic)
                img.raycastTarget = false;
        }
        
        // Disable raycast on Canvas if it's not meant for interaction
        var canvasComponent = GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            var canvasGroup = canvasComponent.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        
        // Keep any buttons on this GameObject interactable so they're annoying
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            btn.interactable = true;
            if (btn.targetGraphic != null)
                btn.targetGraphic.raycastTarget = true;
            
            Debug.Log($"Button {btn.name} - interactable: {btn.interactable}, raycastTarget: {btn.targetGraphic?.raycastTarget}");
        }
    }

    void OnDisable()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
    }

    // Public controls
    public void StartLoop()
    {
        if (loop == null) loop = StartCoroutine(SlowTakeoverSequence());
    }

    public void StopLoop()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
    }

    [ContextMenu("Start Takeover")]
    public void StartTakeover()
    {
        if (myRectTransform != null) StartCoroutine(SlowTakeoverSequence());
    }

    private void CaptureOriginal()
    {
        if (captured || myRectTransform == null) return;
        originalLayout = Snapshot(myRectTransform);
        captured = true;
    }

    private static Layout Snapshot(RectTransform rt)
    {
        return new Layout
        {
            anchorMin = rt.anchorMin,
            anchorMax = rt.anchorMax,
            anchoredPos = rt.anchoredPosition,
            sizeDelta = rt.sizeDelta,
            pivot = rt.pivot,
            siblingIndex = rt.GetSiblingIndex(),
            active = rt.gameObject.activeSelf
        };
    }

    private static void Restore(RectTransform rt, Layout l)
    {
        rt.anchorMin = l.anchorMin;
        rt.anchorMax = l.anchorMax;
        rt.pivot = l.pivot;
        rt.anchoredPosition = l.anchoredPos;
        rt.sizeDelta = l.sizeDelta;
        if (rt.gameObject.activeSelf != l.active)
            rt.gameObject.SetActive(l.active);
        rt.SetSiblingIndex(Mathf.Clamp(l.siblingIndex, 0, rt.parent.childCount - 1));
    }

    /// <summary>
    /// Slowly grows the banner to take over half the screen (meeting in the middle) over 30 seconds,
    /// then plays an unskippable video ad and returns to Normal Mode.
    /// </summary>
    IEnumerator SlowTakeoverSequence()
    {
        if (myRectTransform == null) yield break;

        // Check if we should still be running based on game mode
        var controller = GameModeController.Instance;
        if (controller == null || controller.currentMode != GameMode.NoAdMode)
        {
            enabled = false;
            yield break;
        }

        // Capture original layout
        CaptureOriginal();
        int originalIndex = myRectTransform.GetSiblingIndex();
        myRectTransform.SetAsLastSibling();
        bool originalActive = myRectTransform.gameObject.activeSelf;
        if (!originalActive) myRectTransform.gameObject.SetActive(true);

        // Starting position (banner at edge)
        Vector2 startMin = myRectTransform.anchorMin;
        Vector2 startMax = myRectTransform.anchorMax;
        Vector2 startPos = myRectTransform.anchoredPosition;
        Vector2 startSize = myRectTransform.sizeDelta;
        Vector2 startPivot = myRectTransform.pivot;

        // Goal: Grow to middle of screen (top banner grows to 0.5, bottom banner grows to 0.5)
        // Top banner: covers from top (1.0) to middle (0.5)
        // Bottom banner: covers from bottom (0.0) to middle (0.5)
        Vector2 goalMin = isTopBanner ? new Vector2(0f, 0.5f) : new Vector2(0f, 0f);
        Vector2 goalMax = isTopBanner ? new Vector2(1f, 1f) : new Vector2(1f, 0.5f);
        Vector2 goalPos = Vector2.zero;
        Vector2 goalSize = Vector2.zero;
        Vector2 goalPivot = new Vector2(0.5f, isTopBanner ? 1f : 0f);

        // Slowly grow over takeoverDuration seconds
        float elapsed = 0f;
        while (elapsed < takeoverDuration)
        {
            // Check if mode changed during growth - if so, stop immediately
            if (controller == null || controller.currentMode != GameMode.NoAdMode)
            {
                enabled = false;
                Restore(myRectTransform, originalLayout);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / takeoverDuration);
            
            // Ensure button stays interactable throughout animation
            EnsureButtonInteractable();
            
            // Smooth easing curve for more natural growth
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            myRectTransform.anchorMin = Vector2.Lerp(startMin, goalMin, easedProgress);
            myRectTransform.anchorMax = Vector2.Lerp(startMax, goalMax, easedProgress);
            myRectTransform.pivot = Vector2.Lerp(startPivot, goalPivot, easedProgress);
            myRectTransform.anchoredPosition = Vector2.Lerp(startPos, goalPos, easedProgress);
            myRectTransform.sizeDelta = Vector2.Lerp(startSize, goalSize, easedProgress);

            yield return null;
        }

        // Ensure fully grown to middle
        myRectTransform.anchorMin = goalMin;
        myRectTransform.anchorMax = goalMax;
        myRectTransform.pivot = goalPivot;
        myRectTransform.anchoredPosition = goalPos;
        myRectTransform.sizeDelta = goalSize;

        Debug.Log("BannerAdGrow: Reached middle of screen! Playing unskippable video ad...");

        // Play unskippable video ad
        if (videoAdSpawner != null)
        {
            // Use the full-length video ad that forces watching the entire video
            videoAdSpawner.ShowVideoAdFullLengthThenImageThen(() => 
            {
                Debug.Log("BannerAdGrow: Video ad completed. Switching to Normal Mode.");
                
                // Return to Normal Mode after video completes
                if (gameModeController != null)
                {
                    gameModeController.SetGameMode(GameMode.NormalMode);
                }
                
                // Restore banner to original state
                Restore(myRectTransform, originalLayout);
            });
        }
        else
        {
            Debug.LogWarning("BannerAdGrow: VideoAdSpawner not found! Cannot play ad.");
            
            // Fallback: just switch to Normal Mode
            if (gameModeController != null)
            {
                gameModeController.SetGameMode(GameMode.NormalMode);
            }
            
            // Restore banner
            Restore(myRectTransform, originalLayout);
        }
    }
}
