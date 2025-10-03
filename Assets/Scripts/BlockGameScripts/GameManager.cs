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

    public UnityEvent onPlayerDeath;
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
        onPlayerDeath.Invoke(); 
        if (isDead) return;
        isDead = true;
        Time.timeScale = 0f;
        StartCoroutine(DuckAndPlayDeathSound()); // <-- play death sound and duck BGM
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
