using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    /* while the game is going 
     * add y to the platform spawner position, then add x to the platform spawner position
     * spawn a new platform
     * loop this until player loses
     */


    
    void Start()
    {
        if (GameManager.instance.gameIsPlaying)
        {
            InvokeRepeating("MoveSpawner", 1, 5);
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
        

        // transform.position = transform.position + new Vector3(-5, transform.position.y, 0);
    }

    void MoveSpawner()
    {
        transform.position = transform.position + Vector3.up;
    }

    
}
