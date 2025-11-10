using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Written by Kyle
// This script rotates through a list of banner images at random intervals, with optional crossfade effect.
// Attach to a GameObject with an Image component (or assign one in the inspector).

[DisallowMultipleComponent]
public class AdRotator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private List<AdWithLink> banners = new();
    [SerializeField] private Button bannerClickButton; // Button for clicking banner

    [Header("Timing (seconds)")]
    [SerializeField] private float minInterval = 30f;
    [SerializeField] private float maxInterval = 60f;

    [Header("Behavior")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool randomizeFirst = true;
    [Tooltip("Run even while game is paused (timeScale=0).")]
    [SerializeField] private bool runDuringPause = true;

    [Header("Optional: Crossfade")]
    [SerializeField] private bool crossfade = true;
    [SerializeField, Range(0f, 2f)] private float fadeDuration = 0.25f;

    private int currentIndex = -1;
    private Coroutine loop;
    private AdWithLink currentBanner;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<Image>();
        
        // Setup banner click button
        if (bannerClickButton != null)
        {
            bannerClickButton.onClick.RemoveAllListeners();
            bannerClickButton.onClick.AddListener(OnBannerClicked);
            // Keep banner button always active
        }
    }

    void OnEnable()
    {
        if (playOnEnable) StartRotator();
    }

    void OnDisable()
    {
        StopRotator();
    }

    public void StartRotator()
    {
        if (loop == null && banners != null && banners.Count > 0)
            loop = StartCoroutine(Run());
    }

    public void StopRotator()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
    }

    IEnumerator Run()
    {
        if (randomizeFirst) ApplyImmediate(PickNewIndex());
        else if (currentIndex < 0) ApplyImmediate(0);

        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);

            // use scaled or unscaled time depending on setting
            if (runDuringPause) yield return new WaitForSecondsRealtime(wait);
            else yield return new WaitForSeconds(wait);

            int next = PickNewIndex();
            yield return SwapTo(next);
        }
    }

    int PickNewIndex()
    {
        if (banners.Count <= 1) return 0;
        int next;
        do { next = Random.Range(0, banners.Count); } while (next == currentIndex);
        return next;
    }

    void ApplyImmediate(int idx)
    {
        currentIndex = idx;
        currentBanner = banners[idx];
        targetImage.sprite = currentBanner.adImage;
        UpdateBannerClickButton();
        // targetImage.SetNativeSize(); // optional
    }

    IEnumerator SwapTo(int newIndex)
    {
        float dt() => runDuringPause ? Time.unscaledDeltaTime : Time.deltaTime;

        if (crossfade && targetImage)
        {
            // fade out
            float t = 0f;
            while (t < fadeDuration)
            {
                t += dt();
                var c = targetImage.color;
                c.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / fadeDuration));
                targetImage.color = c;
                yield return null;
            }

            // swap sprite
            currentBanner = banners[newIndex];
            targetImage.sprite = currentBanner.adImage;
            UpdateBannerClickButton();

            // fade in
            t = 0f;
            while (t < fadeDuration)
            {
                t += dt();
                var c = targetImage.color;
                c.a = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t / fadeDuration));
                targetImage.color = c;
                yield return null;
            }
        }
        else
        {
            currentBanner = banners[newIndex];
            targetImage.sprite = currentBanner.adImage;
            UpdateBannerClickButton();
        }

        currentIndex = newIndex;
    }

    public void NextNow()
    {
        if (banners == null || banners.Count == 0) return;
        if (loop != null) StopCoroutine(loop);
        StartCoroutine(SwapTo(PickNewIndex()));
        if (loop == null) loop = StartCoroutine(Run());
    }

    // --- Banner Click Handling ---
    /// <summary>
    /// Called when the banner is clicked (opens URL)
    /// </summary>
    public void OnBannerClicked()
    {
        if (currentBanner != null && !string.IsNullOrEmpty(currentBanner.clickUrl))
        {
            Application.OpenURL(currentBanner.clickUrl);
            MobileHaptics.ImpactLight(); // Light haptic for banner click
            Debug.Log($"[AdRotator] Opened URL: {currentBanner.clickUrl}");
        }
    }

    /// <summary>
    /// Update banner click button based on current banner's clickable state
    /// </summary>
    private void UpdateBannerClickButton()
    {
        if (bannerClickButton == null) return;

        // Keep button always active, just update interactivity
        bool shouldBeInteractable = currentBanner != null && currentBanner.isClickable && !string.IsNullOrEmpty(currentBanner.clickUrl);
        bannerClickButton.interactable = shouldBeInteractable;
        
        if (bannerClickButton.targetGraphic != null)
        {
            bannerClickButton.targetGraphic.raycastTarget = shouldBeInteractable;
        }
    }
}
