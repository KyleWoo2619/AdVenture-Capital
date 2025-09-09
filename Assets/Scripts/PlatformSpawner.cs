using System.Security.Cryptography;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public Transform groundPlatform;
   [SerializeField] float hValue;
    [SerializeField] float vValue;

    private float spawnerPosX;
    private float spawnerPosY;

    private int RandNum;
    
    void Start()
    {
        hValue = 5; vValue = 1;
        spawnerPosX = groundPlatform.transform.position.x + hValue; spawnerPosY = groundPlatform.transform.position.y + vValue;
        RandNum = Random.Range(0, 5);
        Debug.Log(RandNum);

        if (RandNum < 3)
        {
            spawnerPosX = spawnerPosX * 1;
        }
        else
        {
            spawnerPosX = -1*spawnerPosX;
        }
        
        transform.position = new Vector3(spawnerPosX, spawnerPosY, groundPlatform.transform.position.z); //sets initial position of spawner
        
        if (GameManager.instance.gameIsPlaying)
        {
           InvokeRepeating("MoveSpawner", 2, 5);
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
        

        
    }

    void MoveSpawner()
    {

        spawnerPosY += 1;
        spawnerPosX = -(spawnerPosX); //sets spawnerPosX to negative of itself to move to otherside
        transform.position = new Vector3(spawnerPosX, spawnerPosY, groundPlatform.transform.position.z); //moves the transform to the otherside
    }

    void SpawnLeftMovingPlatform()
    {
        
    }

    void SpawnRightMovingPlatform()
    {
        
    }

}
