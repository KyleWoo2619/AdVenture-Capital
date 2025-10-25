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
    Coroutine coroutineCountdown;
    public delegate void Draw();
    public static Draw OnDraw;
    TouchAndShoot shootRef;

    [SerializeField] private GameObject InstructionObject;

    
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
        if (isCountingDown)
        {
            if (shootRef.shoot.WasPerformedThisFrame())
            {
                countdownText.text = "You cheater!"; Debug.Log("You can't fire until the 'Draw!' "); //Display 'misfire' menu here, retry option
                StopCoroutine(coroutineCountdown);
            }
        }
    }
    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(delayTime);
        isCountingDown = true;

        while (startTime != 0)
        {
            yield return new WaitForSeconds(1);
            Debug.Log(startTime); countdownText.text = startTime.ToString();
            startTime--;
        }
        if (startTime == 0)
        {
            yield return new WaitForSeconds(1);
            isCountingDown = false;
            OnDraw?.Invoke(); Debug.Log("Draw!"); countdownText.text = "Draw!";
        }
    }
    
    IEnumerator DisableInstructionMenu()
    {
        yield return new WaitForSeconds(delayTime);
        InstructionObject.SetActive(false);
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
