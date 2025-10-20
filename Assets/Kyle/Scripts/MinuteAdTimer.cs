using System.Collections;
using UnityEngine;
using TMPro;

public class AdAfterMinuteTMP : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FullscreenAdSpawner spawner;   // Drag your FullscreenAdSpawner here
    [SerializeField] private VideoAdSpawner videoAdSpawner; // Drag your VideoAdSpawner here
    [SerializeField] private TMP_Text countdownLabel;       // TMP text for the countdown
    [SerializeField] private GameObject failMenuRoot;      // Reference to the fail menu GameObject
    [SerializeField] private AudioSource mainBGM;         // Assign your main background music AudioSource
    [SerializeField] private AudioSource deathOneShot;    // Assign your one-shot death AudioSource

    [Header("Settings")]
    [SerializeField, Min(1f)] private float seconds = 60f;   // How long to wait before showing ad
    [SerializeField] private bool startOnEnable = true;      // Auto start when object enables
    [SerializeField] private bool repeatEveryMinute = false; // Trigger again every minute
    [SerializeField, Range(0f, 1f)] private float duckVolume = 0.2f; // Volume to duck to
    [SerializeField] private float duckDuration = 2f;     // How long to duck (seconds)

    private Coroutine routine;

    void Reset()
    {
        if (!spawner) spawner = FindFirstObjectByType<FullscreenAdSpawner>();
        if (!videoAdSpawner) videoAdSpawner = FindFirstObjectByType<VideoAdSpawner>();
    }

    void OnEnable()
    {
        if (startOnEnable) StartTimer();
    }

    void OnDisable()
    {
        StopTimer();
        SetLabel(string.Empty);
    }

    public void StartTimer()
    {
        if (routine != null) return;
        routine = StartCoroutine(TimerLoop());
    }

    public void StopTimer()
    {
        if (routine == null) return;
        StopCoroutine(routine);
        routine = null;
    }

    IEnumerator TimerLoop()
    {
        do
        {
            float remaining = Mathf.Max(0.01f, seconds);

            while (remaining > 0f)
            {
                SetLabel(Format(remaining));
                yield return null;
                remaining -= Time.deltaTime; // stops when paused
            }

            SetLabel("00:00");
            
            // Use VideoAdSpawner to show video ad which will then show fail menu
            if (videoAdSpawner != null)
            {
                videoAdSpawner.ShowVideoAdForDeath();
                PlayDeathSoundWithDuck();
            }
            else
            {
                // Fallback to direct fail menu if no VideoAdSpawner assigned
                Time.timeScale = 0f; // pause the game

                if (failMenuRoot != null)
                {
                    failMenuRoot.SetActive(true);
                    PlayDeathSoundWithDuck();

                    // force-enable Canvas if something else disabled it
                    var canvas = failMenuRoot.GetComponent<Canvas>();
                    if (canvas != null && !canvas.enabled)
                        canvas.enabled = true;
                }
            }

            // Wait until video ad and fail menu flow is complete before repeating
            if (videoAdSpawner != null)
            {
                // Wait for video ad to finish and fail menu to be handled
                while ((videoAdSpawner.IsVideoAdShowing() || (failMenuRoot != null && failMenuRoot.activeSelf)) && repeatEveryMinute)
                {
                    yield return null;
                }
            }
            else
            {
                // Fallback wait condition for direct fail menu
                while (failMenuRoot != null && failMenuRoot.activeSelf) yield return null;
            }

            yield return null; // small buffer

        } while (repeatEveryMinute);

        routine = null;
    }

    bool IsAdShowing()
    {
        // crude but effective: ad image is active while showing
        return spawner != null && spawner.gameObject.activeInHierarchy && spawner.enabled;
    }

    private void TriggerAdAndFail()
    {
        if (spawner != null)
        {
            // This will show the fullscreen ad,
            // then open the fail menu after the ad closes
            spawner.ShowAdForDeath();
        }
        else
        {
            Debug.LogWarning("AdAfterMinuteTMP: No FullscreenAdSpawner assigned!");
        }
    }

    private void SetLabel(string s)
    {
        if (countdownLabel) countdownLabel.text = s;
    }

    private static string Format(float t)
    {
        int sec = Mathf.Max(0, Mathf.CeilToInt(t));
        return $"{sec/60:00}:{sec%60:00}";
    }

    public void PlayDeathSoundWithDuck()
    {
        if (mainBGM != null && deathOneShot != null)
            StartCoroutine(DuckAndPlay());
    }

    private IEnumerator DuckAndPlay()
    {
        float originalVolume = mainBGM.volume;
        mainBGM.volume = duckVolume;

        deathOneShot.Play();

        yield return new WaitForSeconds(duckDuration);

        mainBGM.volume = originalVolume;
    }
}
