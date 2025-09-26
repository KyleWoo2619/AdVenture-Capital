using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Pool;

public class NewPlatformSpawner : MonoBehaviour
{

    private GameObject player;
    public Transform groundPlatform;
    [SerializeField] float hValue;
    [SerializeField] float vValue;

    private float spawnerPosX;
    private float spawnerPosY;
    [SerializeField] private float distanceBetweenPlatforms;

    private float platformSpawnRate;

    public GameObject[] platformArray; //reference each left moving platform prefab
    public List<GameObject> platformInstList; //List that stores instances of each prefab
    private GameObject platformInst; //reference to an instance

    public GameObject backUpPlatform;
    private GameObject backUpPlatformInst;

    
    

    void Start()
    {
       
        IntializePool();

        player = GameObject.FindWithTag("Player");

        platformSpawnRate = 3f;

        hValue = 5; vValue = 1;
        spawnerPosX = groundPlatform.transform.position.x + hValue; spawnerPosY = groundPlatform.transform.position.y + vValue;

        transform.position = new Vector3(spawnerPosX, spawnerPosY, player.transform.position.z); //sets initial position of spawner

        StartCoroutine(StartPlatformSpawner(3));
    }

    void IntializePool()
    {
        for (int i = 0; i < platformArray.Length; i++)
        {
            platformInst = Instantiate(platformArray[i]);

            platformInst.gameObject.SetActive(false);

            platformInstList.Add(platformInst);
        }

        backUpPlatformInst = Instantiate(backUpPlatform);
        backUpPlatformInst.SetActive(false);
        platformInstList.Add(backUpPlatformInst);

    }

    GameObject GetPooledPlatform()
    {
        foreach (GameObject tempPlatform in platformInstList)
        {
            if (!tempPlatform.activeInHierarchy)
            {
                return tempPlatform;
            }
        }

        return backUpPlatformInst;

    }

    void MoveSpawner()
    {

        spawnerPosY += distanceBetweenPlatforms;
        transform.position = new Vector3(spawnerPosX, spawnerPosY, player.transform.position.z);
    }

    void SpawnLeftMovingPlatform()
    {
        GameObject tempPlatform = GetPooledPlatform();

        tempPlatform.transform.position = transform.position;

        tempPlatform.gameObject.SetActive(true);

        StartCoroutine(DisableTempPlatform(tempPlatform)); //after a certain amount of time, return that platform into the pool to be reused
    }

    IEnumerator StartPlatformSpawner(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        InvokeRepeating("MoveSpawner", 2, platformSpawnRate);

        InvokeRepeating("SpawnLeftMovingPlatform", 0, platformSpawnRate);

    }

    IEnumerator DisableTempPlatform(GameObject tempPlatform)
    {
        yield return new WaitForSeconds(10);
        tempPlatform.SetActive(false);
    }

}
