using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    //Manages game state (win, lose, isPlaying)
    public static GameManager instance { get; private set; }
    //public bool gameIsPlaying { get; private set; }
    public bool isDead;
    public int score { get; private set; }
    public float forgivenessValue { get; [SerializeField] private set; }


    // Coroutine IncreasePlatformSpeedRoutine;
    // Coroutine DisplayScoreRoutine;

    public float platformSpeed { get; private set; }
    [SerializeField] float platformSpeedScalar;
    [SerializeField] float platformSpeedTime;
    [SerializeField] private AudioSource mainBGM;
    [SerializeField] private AudioSource deathOneShot;
    [SerializeField, Range(0f, 1f)] private float duckVolume = 0.2f;
    [SerializeField] private float duckDuration = 2f;
    
    [Header("Video Ad & Fail Menu")]
    [SerializeField] private VideoAdSpawner videoAdSpawner;
    [SerializeField] private GameObject failMenuCanvas; // Assign the fail menu Canvas/GameObject

    private void Awake()
    {
        Time.timeScale = 1f;
        forgivenessValue = 0.3f;
        
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        //gameIsPlaying = true;
        score = 0;
        isDead = false;
    }


    void Start()
    {
        platformSpeed = 3f;

        StartCoroutine(IncreasePlatformSpeed());
    }

    // Update is called once per frame

    public void AddtoScore()
    {
        if (!isDead)
        {
            score++;
        }
    }


    public void AddFourtoScore()
    {
        if (!isDead)
        {
            score += 4;
        }
    }

    public IEnumerator IncreasePlatformSpeed()
    {
        for (; ; )
        {
            platformSpeed += platformSpeedScalar;
            yield return new WaitForSeconds(platformSpeedTime);
            
        }
    }

    public void TriggerPlayerDeath()
    {
        if (isDead) return;
        isDead = true;
        Time.timeScale = 0f;
        StartCoroutine(DuckAndPlayDeathSound()); // <-- play death sound and duck BGM
        
        // Handle death video ad or fail menu based on game mode
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        // Wait a moment for death sound to start
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Check if we're in NoAdMode or AdFreeMode - if so, skip video ads and show fail menu directly
        var gameModeController = GameModeController.Instance;
        if (gameModeController != null && gameModeController.currentMode != GameMode.NormalMode)
        {
            Debug.Log($"GameManager: In {gameModeController.currentMode} - skipping video ad, showing fail menu after delay");
            
            // Wait 2 seconds then show fail menu directly
            yield return new WaitForSecondsRealtime(2f);
            
            if (failMenuCanvas != null)
            {
                // Enable the Canvas component (don't use SetActive on GameObject)
                var canvas = failMenuCanvas.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.enabled = true;
            }
            else
            {
                Debug.LogWarning("GameManager: Fail menu canvas not assigned!");
            }
        }
        // Normal mode - show video ad
        else if (videoAdSpawner != null)
        {
            videoAdSpawner.ShowVideoAdForDeath();
        }
        else
        {
            Debug.LogWarning("GameManager: VideoAdSpawner not assigned! Showing fail menu directly.");
            
            // Fallback: show fail menu directly if no video ad spawner
            yield return new WaitForSecondsRealtime(2f);
            
            if (failMenuCanvas != null)
            {
                // Enable the Canvas component (don't use SetActive on GameObject)
                var canvas = failMenuCanvas.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.enabled = true;
            }
        }
    }

    private IEnumerator DuckAndPlayDeathSound()
    {
        if (mainBGM != null && deathOneShot != null)
        {
            float originalVolume = mainBGM.volume;
            mainBGM.volume = duckVolume;
            deathOneShot.Play();
            yield return new WaitForSecondsRealtime(duckDuration);
            mainBGM.volume = originalVolume;
        }
    }
}
