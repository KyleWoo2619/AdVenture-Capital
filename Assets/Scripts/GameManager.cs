using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Manages game state (win, lose, isPlaying)
    public static GameManager instance { get; private set; }
    public bool gameIsPlaying { get; private set; }

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

        gameIsPlaying = true;
    }


   
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
