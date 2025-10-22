using System.Collections;
using UnityEditor;
using UnityEngine;

public class ShowdownCountdown : MonoBehaviour
{
    int startTime;
    [SerializeField] int delayTime;
    public static bool isCountingDown { get; private set; }
    Coroutine coroutineCountdown;
   
    TouchAndShoot shootRef;
    void Awake()
    {
        startTime = 3;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<TouchAndShoot>())
            {
                shootRef = child.GetComponent<TouchAndShoot>();
            }
        }

    }
   
    void OnEnable()
    {
        TouchAndShoot.OnEnemyDeath += DisplayWinMenu;
    }
    void OnDisable()
    {
        TouchAndShoot.OnEnemyDeath -= DisplayWinMenu;
    }
   
    void Start()
    {
        coroutineCountdown = StartCoroutine(CountDown());
        
    }

    void Update()
    {
        if (isCountingDown)
        {
            if (shootRef.shoot.WasPerformedThisFrame())
            {
                Debug.Log("You can't fire until the 'Draw!' "); //Display 'misfire' menu here, retry option
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
            Debug.Log(startTime);
            startTime--;
        }
        if (startTime == 0)
        {
            isCountingDown = false;
            yield return new WaitForSeconds(1);
            Debug.Log("Draw!");
        }
    }
    
    void DisplayWinMenu()
    {
        //pull up win menu
        Debug.Log("You Win!");
    }
}
