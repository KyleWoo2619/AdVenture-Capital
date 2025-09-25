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

    public UnityEvent onPlayerDeath;
    private void Awake()
    {
        forgivenessValue = 0.3f;
        platformSpeedScalar = 0.4f;
        platformSpeedTime = 60f;
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
            yield return new WaitForSeconds(platformSpeedTime);
            platformSpeed += platformSpeedScalar;
        }
    }

    public void TriggerPlayerDeath()
    {
        if (isDead) return;          // <-- gate
        isDead = true;
        onPlayerDeath.Invoke();      // <-- fires once
        Time.timeScale = 0f;         // or let the ad spawner pause, your choice
    }
}
