using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SliceSpawner : MonoBehaviour
{
    [SerializeField] private List<SlicePiece> SliceSpawnerSliceList = new List<SlicePiece>(); //get reference to all single slices
    [SerializeField] private List<DualSlices> SliceSpawnerDualSliceList = new List<DualSlices>(); //get reference to all dual slices
    [SerializeField] private List<TripleSlices> SliceSpawnerTripleSliceList = new List<TripleSlices>(); //get reference to all triple slices
    SlicePiece sliceInst;
    DualSlices dualSliceInst;
    TripleSlices tripleSliceInst;

    int[] weightList = {1,1,1,1,1,1,1,2, 3 };
    int randomIndex;
    int random_list_num; //pick a random number between 1 and 3, 1,2,3 represent the reference lists
    int random_num; //represents an element in a list 

    void Awake()
    {
        randomIndex = Random.Range(0, weightList.Length);
        random_list_num = weightList[randomIndex];
        random_num = Random.Range(0, 6);
    }

    
    void Start()
    {
        SpawnSlice(random_list_num, random_num);
        
        if(sliceInst != null || dualSliceInst != null || tripleSliceInst != null)
        {
            StartCoroutine(SpawnNewSlice());
        }
        
    }

    void SpawnSlice(int randomListNum, int randomNum)
    {
        sliceInst = null;
        dualSliceInst = null;
        tripleSliceInst = null;
        
        switch (randomListNum)
        {
            case 1:
                sliceInst = Instantiate(SliceSpawnerSliceList[randomNum], transform.position, Quaternion.identity);
                break;
            case 2:
                dualSliceInst = Instantiate(SliceSpawnerDualSliceList[randomNum], transform.position, Quaternion.identity);
                break;
            case 3:
                tripleSliceInst = Instantiate(SliceSpawnerTripleSliceList[randomNum], transform.position, Quaternion.identity);
                 break;    
        }
        
    }
    
    IEnumerator NewRandomValues(int a, int b)
    {
        random_list_num = Random.Range(1, 3);
        random_num = Random.Range(0, 5);
        yield return new WaitForEndOfFrame();
    }

    IEnumerator SpawnNewSlice()
    {
   
        for(; ; )
        {
            yield return new WaitUntil(() => GetIsPlacedStatus() == true); //wait until a slice is placed 

            randomIndex = Random.Range(0, weightList.Length);
            random_list_num = weightList[randomIndex];
            random_num = Random.Range(0, 6);
            SpawnSlice(random_list_num, random_num); //spawn a new slice 
        }
        
    }
    
    
    bool GetIsPlacedStatus()
    {
        if (sliceInst != null)
            return sliceInst.isPlaced;
        if (dualSliceInst != null)
            return dualSliceInst.isPlaced;
        if (tripleSliceInst != null)
            return tripleSliceInst.isPlaced;

        return false;
    }

    
    
    
}
