using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ShowdownCountdown : MonoBehaviour
{
    int startTime;
    [SerializeField] int delayTime;
    public static bool isCountingDown { get; private set; }
    public static bool hasCheated { get; private set; }
    Coroutine coroutineCountdown;
    public delegate void Draw();
    public static Draw OnDraw;
    public static event Action OnCheat; // tell others a misfire happened
    TouchAndShoot shootRef;

    [SerializeField] private GameObject InstructionObject;
    
    [Header("Interactive Ad Settings")]
    [SerializeField] private bool useUnscaledTime = false; // Set to true when running as interactive ad

    
    TMP_Text countdownText;
    void Awake()
    {
        startTime = 3;
        InstructionObject.SetActive(true);

        foreach (Transform child in transform)
        {
            if (child.GetComponent<TouchAndShoot>())
            {
                shootRef = child.GetComponent<TouchAndShoot>();
            }
            if (child.GetComponent<TMP_Text>())
            {
                countdownText = child.GetComponent<TMP_Text>();
            }
        }

    }
   
    void OnEnable()
    {
        TouchAndShoot.OnEnemyDeath += DisplayWinMenu;
        ShooterAdversary.OnPlayerDeath += DisplayFailMenu;
    }
    void OnDisable()
    {
        TouchAndShoot.OnEnemyDeath -= DisplayWinMenu;
        ShooterAdversary.OnPlayerDeath -= DisplayFailMenu;
    }
   
    void Start()
    {
        StartCoroutine(DisableInstructionMenu());
        coroutineCountdown = StartCoroutine(CountDown());
        
    }

    void Update()
    {
        if (isCountingDown && !hasCheated && shootRef.shoot.WasPerformedThisFrame())
        {
            countdownText.text = "You cheater!";
            isCountingDown = false;
            hasCheated = true;

            if (coroutineCountdown != null)
            {
                StopCoroutine(coroutineCountdown);
                coroutineCountdown = null;
            }
            StopAllCoroutines();
            
            // Wait 3 seconds to show cheater message, then broadcast loss
            Invoke(nameof(TriggerCheatLoss), 3f);
        }
    }
    
    void TriggerCheatLoss()
    {
        OnCheat?.Invoke();
    }
    IEnumerator CountDown()
    {
        // Use unscaled time if running as interactive ad (game paused)
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(delayTime);
        else
            yield return new WaitForSeconds(delayTime);
            
        isCountingDown = true;

        // Use a local counter to avoid modifying startTime
        int currentCount = startTime;
        while (currentCount > 0)
        {
            Debug.Log(currentCount);
            countdownText.text = currentCount.ToString();
            
            // Use unscaled time if running as interactive ad (game paused)
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(1);
            else
                yield return new WaitForSeconds(1);
                
            currentCount--;
        }
        
        // When countdown reaches 0, show "Draw!"
        isCountingDown = false;
        OnDraw?.Invoke();
        Debug.Log("Draw!");
        countdownText.text = "Draw!";
    }
    
    IEnumerator DisableInstructionMenu()
    {
        // Use unscaled time if running as interactive ad (game paused)
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(delayTime);
        else
            yield return new WaitForSeconds(delayTime);
            
        InstructionObject.SetActive(false);
    }

    /// <summary>
    /// Enable unscaled time mode for interactive ads (called by CowboyGameWrapper)
    /// </summary>
    public void SetUnscaledTimeMode(bool enabled)
    {
        useUnscaledTime = enabled;
        Debug.Log($"[ShowdownCountdown] Unscaled time mode: {enabled}");
    }

    /// <summary>
    /// Restart the countdown for interactive ads (called by CowboyGameWrapper)
    /// </summary>
    public void RestartCountdown()
    {
        Debug.Log("[ShowdownCountdown] Restarting countdown for interactive ad");
        
        // Stop any running coroutines
        if (coroutineCountdown != null)
        {
            StopCoroutine(coroutineCountdown);
        }
        StopAllCoroutines();
        
        // Reset values
        startTime = 3;
        isCountingDown = false;
        hasCheated = false;
        
        // Show instruction object
        if (InstructionObject != null)
            InstructionObject.SetActive(true);
        
        // Restart the countdown
        StartCoroutine(DisableInstructionMenu());
        coroutineCountdown = StartCoroutine(CountDown());
    }

    void DisplayWinMenu()
    {
        //pull up win menu
        countdownText.text = "You Win!";  Debug.Log("You Win!");
    }
    
    void DisplayFailMenu()
    {
        //pull up fail menu
        countdownText.text = "You Lose!"; Debug.Log("You lose");
    }
}
