using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BounceAd - Bouncing ad animation that moves to random edge points.
/// Only active in NoAdMode (interactive ad mode).
/// Randomly selected with 80% probability. Mutually exclusive with BannerAdGrow.
/// After 30 seconds, plays an unskippable video ad and returns to Normal Mode.
/// Uses static flags to ensure all banners make the same choice.
/// </summary>
public class BounceAd : MonoBehaviour
{
    // Static coordination with BounceAd - all banners use same random choice
    private static bool? useBounceForThisSession = null;
    private static bool hasRolled = false;
    
    /// <summary>
    /// Resets static coordination flags. Called by GameModeController when entering NoAdMode.
    /// </summary>
    public static void ResetStaticFlags()
    {
        useBounceForThisSession = null;
        hasRolled = false;
        Debug.Log("BounceAd: Static flags reset");
    }
    
    public Canvas canvas;
    private RectTransform rectBorder;
    private RectTransform adRectBorder;
    private Vector2 pointToGoTo;
    

    public float wBound;
    public float hBound;
    public float speed = 240;
    int lastListNum = -1;

    [SerializeField] private List<GameObject> topEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> leftEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> bottomEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> rightEdgePoints = new List<GameObject>();

    [Header("Takeover Settings")]
    [Tooltip("Time in seconds before playing video ad")]
    [SerializeField, Min(1f)] private float bounceTimeBeforeAd = 30f;
    
    [Header("Video Ad Integration")]
    [Tooltip("Reference to VideoAdSpawner to play unskippable ad")]
    [SerializeField] private VideoAdSpawner videoAdSpawner;
    
    [Tooltip("Reference to GameModeController to switch back to Normal Mode")]
    [SerializeField] private GameModeController gameModeController;

    private int randListNum;
    private int randPointNum;
    private Coroutine bounceCoroutine;
    private bool hasStarted = false;
    private float elapsedBounceTime = 0f;
    private Vector2 originalPosition;
    private bool videoAdTriggered = false; // Prevents multiple video ad triggers
    
    void Awake()
    {
        rectBorder = canvas.GetComponent<RectTransform>();
        adRectBorder = GetComponent<RectTransform>();

        wBound = rectBorder.rect.width;
        hBound = rectBorder.rect.height;
        
        // Save original position
        originalPosition = adRectBorder.anchoredPosition;
        
        // Ensure Canvas has GraphicRaycaster for button clicks to work
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log($"BounceAd: Added GraphicRaycaster to {canvas.name}");
            }
            raycaster.enabled = true;
        }
        
        // Ensure EventSystem exists in scene
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("BounceAd: Created EventSystem");
        }
        
        // Try to find VideoAdSpawner if not assigned
        if (videoAdSpawner == null)
            videoAdSpawner = FindFirstObjectByType<VideoAdSpawner>();
        
        // Try to find GameModeController if not assigned
        if (gameModeController == null)
            gameModeController = GameModeController.Instance;

        

        GameObject point = new GameObject("EdgePoint", typeof(RectTransform));

        for (int i = -500; i < 600; i += 200) //points at top edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(i, hBound/2);
            topEdgePoints.Add(p);
            
        }

        for (int i = -500; i < 600; i += 200) //points at left edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(wBound/2, i);
            leftEdgePoints.Add(p);
        }

        for (int i = -500; i < 600; i += 200) //points at bottom edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(i, -hBound/2);
            bottomEdgePoints.Add(p);
        }
        
        for(int i = -500; i<600; i += 200) //points at right edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(-wBound/2, i);
            rightEdgePoints.Add(p);
        }

        
    }

    void Start()
    {
        hasStarted = true;
        CheckModeAndActivate();
    }

    void Update()
    {
        // Check if we should be enabled based on current game mode
        CheckModeAndActivate();
        
        // Only move if we're supposed to be active
        if (!enabled || bounceCoroutine == null) return;
        
        // Ensure button stays interactable during animation
        EnsureButtonInteractable();
        
        // Track elapsed time for video ad trigger (use unscaled time to continue during pause)
        elapsedBounceTime += Time.unscaledDeltaTime;
        
        // Check if it's time to play video ad (only trigger once)
        if (!videoAdTriggered && elapsedBounceTime >= bounceTimeBeforeAd)
        {
            Debug.Log($"BounceAd: {bounceTimeBeforeAd} seconds elapsed! Playing unskippable video ad...");
            videoAdTriggered = true; // Prevent multiple triggers
            PlayVideoAdAndReturnToNormalMode();
            return;
        }
        
        // Use unscaled time to continue moving during pause
        adRectBorder.anchoredPosition = Vector2.MoveTowards(
            adRectBorder.anchoredPosition,
            pointToGoTo,
            speed * Time.unscaledDeltaTime
        );
    }
    
    private void CheckModeAndActivate()
    {
        var controller = GameModeController.Instance;
        if (controller == null) return;
        
        // Check if we should be active: NoAdMode AND randomly selected (80% chance)
        bool shouldBeActive = controller.currentMode == GameMode.NoAdMode;
        
        if (shouldBeActive && bounceCoroutine == null && hasStarted)
        {
            // Use static flag to ensure all banners make the same choice
            if (!hasRolled)
            {
                float roll = Random.Range(0f, 1f);
                useBounceForThisSession = roll <= 0.8f;
                hasRolled = true;
                Debug.Log($"BounceAd: Global roll = {roll:F2}, useBounce = {useBounceForThisSession}");
            }
            
            var growAd = GetComponent<BannerAdGrow>();
            
            if (useBounceForThisSession == true)
            {
                Debug.Log($"BounceAd: Activating on {gameObject.name}");
                enabled = true;
                if (growAd != null) growAd.enabled = false;
                elapsedBounceTime = 0f; // Reset timer
                videoAdTriggered = false; // Reset video trigger flag
                
                Debug.Log($"BounceAd: Timer started. Will trigger video ad after {bounceTimeBeforeAd} seconds");
                
                // Ensure button stays interactable
                EnsureButtonInteractable();
                
                bounceCoroutine = StartCoroutine(SearchForPoint());
            }
            else if (growAd != null)
            {
                Debug.Log($"BounceAd: Deferring to BannerAdGrow on {gameObject.name}");
                enabled = false;
            }
        }
        else if (!shouldBeActive && bounceCoroutine != null)
        {
            Debug.Log($"BounceAd: Deactivating because not in NoAdMode");
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
            elapsedBounceTime = 0f; // Reset timer
            
            // Return to original position
            adRectBorder.anchoredPosition = originalPosition;
            
            // Reset static flags for next time
            hasRolled = false;
            useBounceForThisSession = null;
            
            enabled = false;
        }
    }
    
    private void EnsureButtonInteractable()
    {
        // Disable raycast blocking on non-button UI elements
        var images = GetComponentsInChildren<UnityEngine.UI.Image>(true);
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
    
    private void PlayVideoAdAndReturnToNormalMode()
    {
        // Stop bouncing
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }
        
        elapsedBounceTime = 0f; // Reset timer
        
        // Return to original position before playing video
        adRectBorder.anchoredPosition = originalPosition;
        
        // Play unskippable video ad
        if (videoAdSpawner != null)
        {
            videoAdSpawner.ShowVideoAdFullLengthThenImageThen(() => 
            {
                Debug.Log("BounceAd: Video ad completed. Switching to Normal Mode.");
                
                // Return to Normal Mode after video completes (only way to exit NoAdMode)
                if (gameModeController != null)
                {
                    gameModeController.CompleteNoAdModeAndReturnToNormal();
                }
            });
        }
        else
        {
            Debug.LogWarning("BounceAd: VideoAdSpawner not found! Cannot play ad.");
            
            // Fallback: just switch to Normal Mode
            if (gameModeController != null)
            {
                gameModeController.CompleteNoAdModeAndReturnToNormal();
            }
        }
    }

     IEnumerator SearchForPoint()
     {
        for(;;){
            
            int newListNum;
            do
            {
                newListNum = Random.Range(1, 5);  
            }
            while (newListNum == lastListNum);

            randListNum = newListNum;
            lastListNum = randListNum;

           // Debug.Log($"the random list number is {randListNum}");

            switch(randListNum)
            {
                case 1: //pick a random point in this list, same for all of them
                    randPointNum = Random.Range(0, topEdgePoints.Count);
                //    Debug.Log(randPointNum);
                    pointToGoTo = topEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;

                case 2: 
                    randPointNum = Random.Range(0, bottomEdgePoints.Count);
                //    Debug.Log(randPointNum);
                    pointToGoTo = bottomEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;

                case 3: 
                    randPointNum = Random.Range(0, rightEdgePoints.Count);
                //    Debug.Log(randPointNum);
                    pointToGoTo = rightEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;

                case 4:
                    randPointNum = Random.Range(0, leftEdgePoints.Count);
                //   Debug.Log(randPointNum);
                    pointToGoTo = leftEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;
                
            }
            yield return new WaitUntil(() => AtPoint() == true);
            
            // Play haptic when reaching the bounce point
            MobileHaptics.ImpactLight();
        }
        }
    
    bool AtPoint()
    {
        return (adRectBorder.anchoredPosition - pointToGoTo).sqrMagnitude < 0.1f; 
    }
}
 
 
 
