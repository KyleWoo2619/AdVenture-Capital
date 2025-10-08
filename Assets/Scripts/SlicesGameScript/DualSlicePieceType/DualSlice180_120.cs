using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualSlice180_120 : DualSlices
{

    //Create List and initialize it with all possible slots it can be 
    
    

    void OnEnable()
    {
        StartCoroutine(InitializeList());
    }

    IEnumerator InitializeList()
    {
        yield return new WaitForEndOfFrame();
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotList180)
        {
            DualSlotList1.Add(slot);
        }
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotList120)
        {
            DualSlotList2.Add(slot);
        }
    }


}
