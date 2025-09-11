using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Manages game state (win, lose, isPlaying)
    public static GameManager instance { get; private set; }
    //public bool gameIsPlaying { get; private set; }
    public bool isDead;
    public int score { get; private set; }

    private void Awake()
    {
        if(instance != null && instance != this)
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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            Time.timeScale = 0;
        }
    }

    public void AddtoScore()
    {
        score++;
    }

    public void AddFourtoScore()
    {
        score += 4;
    }

    
}
