using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Attach this to the X button on the Ad Icon.
/// Tracks clicks - on each click the Ad Icon shakes.
/// After 5 quick clicks (within clickWindowDuration), the ad falls off and switches to AdFreeMode.
/// </summary>
[RequireComponent(typeof(Button))]
public class AdIconXButton : MonoBehaviour, IPointerDownHandler
{
    [Header("References")]
    [Tooltip("The Ad Icon GameObject that will shake and fall off")]
    [SerializeField] private GameObject adIconObject;
    
    [Tooltip("Reference to GameModeController to switch to AdFreeMode")]
    [SerializeField] private GameModeController gameModeController;
    
    [Header("Click Settings")]
    [Tooltip("Number of clicks required to trigger fall-off")]
    [SerializeField] private int requiredClicks = 5;
    
    [Tooltip("Time window in seconds to complete all clicks")]
    [SerializeField] private float clickWindowDuration = 3f;
    
    [Header("Shake Settings")]
    [Tooltip("How much the ad icon shakes (in units)")]
    [SerializeField] private float shakeIntensity = 10f;
    
    [Tooltip("Duration of each shake")]
    [SerializeField] private float shakeDuration = 0.3f;
    
    [Header("Fall Settings")]
    [Tooltip("Duration of the fall-off animation")]
    [SerializeField] private float fallDuration = 1f;
    
    [Tooltip("How far off-screen the ad should fall (in screen heights)")]
    [SerializeField] private float fallDistance = 2f;
    
    [Tooltip("Rotation amount during fall (in degrees)")]
    [SerializeField] private float fallRotation = 360f;
    
    [Header("Audio Settings")]
    [Tooltip("Audio source to play the sound effect")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Sound to play when switching to AdFreeMode (optional)")]
    [SerializeField] private AudioClip modeSwitchSound;
    
    private Button button;
    private RectTransform adIconRectTransform;
    private Vector3 originalAdIconPosition;
    private Quaternion originalAdIconRotation;
    private int currentClickCount = 0;
    private float lastClickTime = 0f;
    private bool isFalling = false;
    private Coroutine shakeCoroutine;
    
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnXButtonClicked);
        
        // Try to find GameModeController if not assigned
        if (gameModeController == null)
            gameModeController = GameModeController.Instance;
        
        // If adIconObject not assigned, try to find it by going up the hierarchy
        if (adIconObject == null)
        {
            // Look for a parent or ancestor with "Ad" in the name
            Transform current = transform.parent;
            while (current != null)
            {
                if (current.name.Contains("Ad") || current.name.Contains("Banner"))
                {
                    adIconObject = current.gameObject;
                    break;
                }
                current = current.parent;
            }
            
            if (adIconObject == null)
            {
                Debug.LogWarning("AdIconXButton: Could not find Ad Icon GameObject. Please assign manually.");
                adIconObject = transform.parent?.gameObject;
            }
        }
        
        if (adIconObject != null)
        {
            adIconRectTransform = adIconObject.GetComponent<RectTransform>();
            if (adIconRectTransform != null)
            {
                originalAdIconPosition = adIconRectTransform.localPosition;
                originalAdIconRotation = adIconRectTransform.localRotation;
            }
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Play haptic feedback
        MobileHaptics.ImpactMedium();
    }
    
    private void OnXButtonClicked()
    {
        if (isFalling || adIconObject == null) return;
        
        float currentTime = Time.time;
        
        // Reset counter if too much time has passed since last click
        if (currentTime - lastClickTime > clickWindowDuration)
        {
            currentClickCount = 0;
        }
        
        lastClickTime = currentTime;
        currentClickCount++;
        
        Debug.Log($"AdIconXButton: Click {currentClickCount}/{requiredClicks}");
        
        // Shake the ad icon with increasing intensity
        float intensityMultiplier = 1f + (currentClickCount * 0.2f); // Shake gets stronger with each click
        TriggerShake(intensityMultiplier);
        
        // Check if we've reached the required clicks
        if (currentClickCount >= requiredClicks)
        {
            Debug.Log("AdIconXButton: Required clicks reached! Triggering fall-off and switching to AdFreeMode.");
            StartCoroutine(FallOffAndSwitchMode());
        }
    }
    
    private void TriggerShake(float intensityMultiplier = 1f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeAnimation(intensityMultiplier));
    }
    
    private IEnumerator ShakeAnimation(float intensityMultiplier)
    {
        if (adIconRectTransform == null) yield break;
        
        Vector3 startPos = adIconRectTransform.localPosition;
        float elapsed = 0f;
        float adjustedIntensity = shakeIntensity * intensityMultiplier;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            
            // Create a decreasing shake effect
            float shake = (1f - progress) * adjustedIntensity;
            
            // Random shake offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0f
            );
            
            adIconRectTransform.localPosition = originalAdIconPosition + randomOffset;
            
            yield return null;
        }
        
        // Return to original position
        adIconRectTransform.localPosition = originalAdIconPosition;
        shakeCoroutine = null;
    }
    
    private IEnumerator FallOffAndSwitchMode()
    {
        isFalling = true;
        
        // Disable button to prevent further clicks
        button.interactable = false;
        
        // Play stronger haptic
        MobileHaptics.ImpactHeavy();
        
        if (adIconRectTransform != null)
        {
            Vector3 startPos = adIconRectTransform.localPosition;
            Quaternion startRot = adIconRectTransform.localRotation;
            
            // Calculate fall target position (off-screen)
            float screenHeight = Screen.height;
            Vector3 fallTarget = startPos + new Vector3(
                Random.Range(-screenHeight * 0.3f, screenHeight * 0.3f), // Random horizontal drift
                -screenHeight * fallDistance, // Fall down
                0f
            );
            
            float elapsed = 0f;
            
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fallDuration;
                
                // Use ease-in curve for falling (accelerating)
                float easedProgress = progress * progress;
                
                // Interpolate position
                adIconRectTransform.localPosition = Vector3.Lerp(startPos, fallTarget, easedProgress);
                
                // Rotate during fall
                float rotationAmount = fallRotation * easedProgress;
                adIconRectTransform.localRotation = startRot * Quaternion.Euler(0f, 0f, rotationAmount);
                
                yield return null;
            }
            
            // Disable the ad icon object
            adIconObject.SetActive(false);
        }
        
        // Play sound effect before switching mode
        if (audioSource != null && modeSwitchSound != null)
        {
            audioSource.PlayOneShot(modeSwitchSound);
            // Wait a brief moment for the sound to start playing
            yield return new WaitForSeconds(0.1f);
        }
        
        // Switch to AdFreeMode
        if (gameModeController != null)
        {
            Debug.Log("AdIconXButton: Switching to AdFreeMode!");
            gameModeController.SetGameMode(GameMode.AdFreeMode);
        }
        else
        {
            Debug.LogError("AdIconXButton: GameModeController reference is missing!");
        }
        
        isFalling = false;
    }
    
    /// <summary>
    /// Reset the ad icon to its original state (useful if you want to re-enable it)
    /// </summary>
    public void ResetAdIcon()
    {
        if (adIconRectTransform != null)
        {
            adIconRectTransform.localPosition = originalAdIconPosition;
            adIconRectTransform.localRotation = originalAdIconRotation;
        }
        
        if (adIconObject != null)
        {
            adIconObject.SetActive(true);
        }
        
        currentClickCount = 0;
        lastClickTime = 0f;
        isFalling = false;
        button.interactable = true;
    }
}
