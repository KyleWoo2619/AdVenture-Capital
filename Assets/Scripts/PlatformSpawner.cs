using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

public class PlatformSpawner : MonoBehaviour
{
    private GameObject player;
    public Transform groundPlatform;
    [SerializeField] float hValue;
    [SerializeField] float vValue;

    private float spawnerPosX;
    private float spawnerPosY;

    private int RandNum;

    [SerializeField] private GameObject platformR;
    [SerializeField] private GameObject platformL;

    [SerializeField] private float distanceBetweenPlatforms;

    private float platformSpawnRate;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        distanceBetweenPlatforms = 0.7f;
        platformSpawnRate = 3f;

        hValue = 5; vValue = 1;
        spawnerPosX = groundPlatform.transform.position.x + hValue; spawnerPosY = groundPlatform.transform.position.y + vValue;
       
        RandNum = Random.Range(0, 5);
        //Debug.Log(RandNum);

        if (RandNum < 3)
        {
            spawnerPosX = spawnerPosX * 1;
        }
        else
        {
            spawnerPosX = -1*spawnerPosX;
        }
        
        transform.position = new Vector3(spawnerPosX, spawnerPosY, player.transform.position.z); //sets initial position of spawner

        

        if (!GameManager.instance.isDead)
        {
           
           InvokeRepeating("MoveSpawner", 2, platformSpawnRate);

           if(spawnerPosX > 0)
            {
              //  Debug.Log("spawned L platform");
                InvokeRepeating("SpawnLeftMovingPlatform", 0, platformSpawnRate);
                
            }
            else
            {
                //Debug.Log("spawned R platform");
                InvokeRepeating("SpawnRightMovingPlatform", 0, platformSpawnRate);
                
            }

           
        }
    }

    

    // Update is called once per frame
    void Update()
    {

        


    }

    void MoveSpawner()
    {

        spawnerPosY += distanceBetweenPlatforms;
        //spawnerPosX = -1*spawnerPosX; //sets spawnerPosX to negative of itself to move to otherside
        transform.position = new Vector3(spawnerPosX, spawnerPosY, player.transform.position.z); //moves the transform to the otherside
    }

    void SpawnLeftMovingPlatform()
    {
       
       
      Instantiate(platformL, transform.position, Quaternion.identity);
     // Destroy(platformL_Instance, 2);
        
        
    }

    void SpawnRightMovingPlatform()
    {
        Instantiate(platformR, transform.position, Quaternion.identity);
       // Destroy(platformR_Instance, 2);
        
    }

   

}
