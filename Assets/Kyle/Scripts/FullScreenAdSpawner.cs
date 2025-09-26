using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FullscreenAdSpawner : MonoBehaviour
{
    [Header("Hook up your UI")]
    [SerializeField]
    private Canvas adCanvas; // Ad canvas (Overlay)
    [SerializeField]

    private Image adImage; // Fullscreen Image child
    [SerializeField]

    private Button closeButton; // Transparent Button child (top-right)

    [Header("Ad Content")]
    [SerializeField]
    private List<Sprite> adSprites = new();

    [Header("Schedule (seconds)")]
    [SerializeField]
    private float minInterval = 30f;
    [SerializeField]
    private float maxInterval = 30f;

    [Header("Close Button Delay (seconds)")]
    [SerializeField]
    private float minCloseDelay = 1f;
    [SerializeField]
    private float maxCloseDelay = 3f;

    [Header("Behavior")]
    [SerializeField]
    private bool startOnEnable = false; // you call ShowAd() on death
    [SerializeField]

    private bool runDuringPause = true; // unscaled timing
    [SerializeField]
    private bool pauseGameOnShow = true; // set timeScale=0 while ad visible
    [SerializeField]
    private int onTopSortingOrder = 5000;

    [Header("External")]
    public FailMenuManager FailMenuInstance;

    private Coroutine loop;
    private Coroutine closeButtonCoroutine;
    private bool isShowing = false;
    private int showToken = 0;
    private Action onCloseOneShot;
    private bool IsPlayerDeadSafe =>
        GameManager.instance != null && GameManager.instance.isDead;

    private bool showFailOnClose = false;

    void Awake()
    {
        if (!adCanvas)
            adCanvas = GetComponentInParent<Canvas>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseAd);
        }

        // start hidden
        SetAdVisible(false);
        SetCloseButtonVisible(false);

        // the ad image shouldn’t steal clicks; the button will
        if (adImage)
            adImage.raycastTarget = false;
    }

    void OnEnable()
    {
        if (startOnEnable && loop == null)
        {
            loop = StartCoroutine(SpawnLoop());
        }
    }

    void OnDisable()
    {
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

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Mathf.Max(0f, UnityEngine.Random.Range(minInterval, maxInterval));
            yield return WaitRealtime(wait);

            if (adSprites != null && adSprites.Count > 0 && adImage != null)
            {
                adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
                ShowAd();
            }

            // wait until closed
            yield return new WaitUntil(() => !isShowing);
        }
    }

    // Called by GameManager On Player Death
    public void ShowAd()
    {
        if (isShowing) return;

        // if nothing assigned yet, pick one (covers death path too)
        AssignRandomSpriteIfNeeded();

        if (adCanvas) { adCanvas.renderMode = RenderMode.ScreenSpaceOverlay; adCanvas.overrideSorting = true; adCanvas.sortingOrder = onTopSortingOrder; }
        if (pauseGameOnShow) Time.timeScale = 0f;

        showToken++; isShowing = true;
        SetAdVisible(true);
        SetCloseButtonVisible(false);
        if (adImage) adImage.raycastTarget = true;

        float delay = UnityEngine.Random.Range(minCloseDelay, maxCloseDelay);
        if (closeButtonCoroutine != null) StopCoroutine(closeButtonCoroutine);
        closeButtonCoroutine = StartCoroutine(EnableCloseAfter(delay, showToken));
    }

    IEnumerator EnableCloseAfter(float seconds, int token)
    {
        yield return WaitRealtime(seconds);

        // Ensure button is enabled before making it visible
        if (closeButton != null && !closeButton.gameObject.activeSelf)
            closeButton.gameObject.SetActive(true);

        // Wait one frame for UI update
        yield return null;

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

    public void CloseAd()
    {
        if (!isShowing) return;
        isShowing = false;
        if (closeButtonCoroutine != null) { StopCoroutine(closeButtonCoroutine); closeButtonCoroutine = null; }

        SetCloseButtonVisible(false);
        SetAdVisible(false);
        if (adImage) adImage.raycastTarget = false;

        // Resume time ONLY for non-death flows
        if (pauseGameOnShow && !showFailOnClose)
            Time.timeScale = 1f;

        // Death flow -> show fail menu (keep time paused unless your fail menu resumes it)
        if (showFailOnClose && FailMenuInstance != null)
            FailMenuInstance.DisplayFailMenu();

        // Non-death flow -> run the queued action (e.g., load scene)
        if (!showFailOnClose)
        {
            var cb = onCloseOneShot;
            onCloseOneShot = null;
            cb?.Invoke();
        }

        // reset flag
        showFailOnClose = false;
    }

    // ------- helpers -------
    void SetAdVisible(bool visible)
    {
        if (adImage)
            adImage.gameObject.SetActive(visible);
    }

    void SetCloseButtonVisible(bool visible)
    {
        if (!closeButton)
            return;
        closeButton.gameObject.SetActive(visible);

        if (closeButton.targetGraphic)
            closeButton.targetGraphic.raycastTarget = visible;
        closeButton.interactable = visible;
    }

    WaitForSecondsRealtime WaitRealtime(float seconds)
    {
        // predictable timing; uses unscaled time so it works while paused
        return new WaitForSecondsRealtime(Mathf.Max(0f, seconds));
    }

    public void ShowAdOnRestart()
    {
        ShowAd();
        float delay = UnityEngine.Random.Range(minCloseDelay, maxCloseDelay);
        StartCoroutine(EnableCloseAfter(delay, showToken));
    }

    // Show ad, then run a callback exactly once when the user closes it
    public void ShowAdThen(Action afterClose)
    {
        if (isShowing) return;          // ignore double-clicks
        onCloseOneShot += afterClose;   // queue once
        ShowAd();
    }

    // Pick a random sprite, then show ad and run callback
    public void ShowAdRandomThen(Action afterClose)
    {
        if (adSprites != null && adSprites.Count > 0 && adImage != null)
            adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
        ShowAdThen(afterClose);
    }

    // Convenience for hooking directly from Button OnClick with a string param
    public void ShowAdRandomThenLoadScene(string sceneName)
    {
        onCloseOneShot = () => SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        showFailOnClose = false;
        if (adImage && adSprites.Count > 0)
            adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
        ShowAd();
    }

    public void ShowAdForDeath()
    {
        onCloseOneShot = null;
        showFailOnClose = true;
        AssignRandomSpriteIfNeeded();       // ensures no blank white panel
        ShowAd();
    }
    
    private void AssignRandomSpriteIfNeeded()
    {
        if (adImage && adImage.sprite == null && adSprites != null && adSprites.Count > 0)
            adImage.sprite = adSprites[UnityEngine.Random.Range(0, adSprites.Count)];
    }

}
