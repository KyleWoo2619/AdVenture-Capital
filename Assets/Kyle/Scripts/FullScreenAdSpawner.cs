using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

// Written by Kyle
// This script spawns a fullscreen ad at random intervals, with a close button that appears after a short delay.
// Attach to a GameObject in your UI Canvas.

[DisallowMultipleComponent]
public class FullscreenAdSpawner : MonoBehaviour
{
    [Header("Hook up your UI")]
    [SerializeField]
    private Image adImage; // Fullscreen Image child

    [SerializeField]
    private Button closeButton; // Transparent Button child (top-right)

    [Header("Ad Content")]
    [SerializeField]
    private List<Sprite> adSprites = new(); // Random pick, repeats allowed

    [Header("Schedule (seconds)")]
    [Tooltip("Random wait before each popup. Set both equal for a fixed interval.")]
    [SerializeField]
    private float minInterval = 30f;

    [SerializeField]
    private float maxInterval = 30f;

    [Header("Close Button Delay (seconds)")]
    [Tooltip("Close button appears after this random delay.")]
    [SerializeField]
    private float minCloseDelay = 1f;

    [SerializeField]
    private float maxCloseDelay = 3f;

    [Header("Behavior")]
    [SerializeField]
    private bool startOnEnable = true;

    [Tooltip("Keep timing even when the game is paused (timeScale = 0).")]
    [SerializeField]
    private bool runDuringPause = true;

    private Coroutine loop;
    private bool isShowing = false;
    private int showToken = 0; // to cancel stale coroutines safely
    public FailMenuManager FailMenuInstance;

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseAd);
        }

        // Ensure children are hidden at start
        SetAdVisible(false);
        SetCloseButtonVisible(false);
    }

    void OnEnable()
    {
        if (startOnEnable && loop == null)
            loop = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
        StopAllCoroutines(); // also cancels any pending close-delay
        // Hide children so you never see leftover UI during scene swaps
        SetAdVisible(false);
        SetCloseButtonVisible(false);
        isShowing = false;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // wait for next popup
            float wait = Mathf.Max(0f, Random.Range(minInterval, maxInterval));
            yield return WaitRealtime(wait);

            // show a random sprite (repeats allowed)
            if (adSprites != null && adSprites.Count > 0 && adImage != null)
            {
                adImage.sprite = adSprites[Random.Range(0, adSprites.Count)];
                ShowAd();
            }

            // wait until the player closes it
            yield return new WaitUntil(() => !isShowing);
        }
    }

    public void ShowAd()
    {
        Time.timeScale = 0f;
        showToken++; // invalidate any old EnableClose coroutines
        isShowing = true;

        SetAdVisible(true);
        SetCloseButtonVisible(false);

        // enable the close button after a small random delay
        float delay = Random.Range(minCloseDelay, maxCloseDelay);
        StartCoroutine(EnableCloseAfter(delay, showToken));
    }

    IEnumerator EnableCloseAfter(float seconds, int token)
    {
        yield return WaitRealtime(seconds);
        // only enable if we are still showing the same ad
        if (isShowing && token == showToken)
            SetCloseButtonVisible(true);
    }

    public void CloseAd()
    {

        if (!isShowing && !GameManager.instance.isDead)
            return;

        isShowing = false;
        SetCloseButtonVisible(false);
        SetAdVisible(false);
        Time.timeScale = 1f;

        if (isShowing &&GameManager.instance.isDead)
        {
            FailMenuInstance.DisplayFailMenu();
        }
    }

    // ---------- helpers ----------
    void SetAdVisible(bool visible)
    {
        if (adImage != null)
            adImage.gameObject.SetActive(visible);
    }

    void SetCloseButtonVisible(bool visible)
    {
        if (closeButton != null)
            closeButton.gameObject.SetActive(visible);
    }

    // Always use realtime so pause menus don't stall the timers.
    WaitForSecondsRealtime WaitRealtime(float seconds)
    {
        return new WaitForSecondsRealtime(Mathf.Max(0f, seconds));
    }

    // Optional utility for QA:
    [ContextMenu("Show Now")]
    public void ShowNowForQA()
    {
        if (adSprites == null || adSprites.Count == 0 || adImage == null)
            return;
        adImage.sprite = adSprites[Random.Range(0, adSprites.Count)];
        ShowAd();
    }
}
