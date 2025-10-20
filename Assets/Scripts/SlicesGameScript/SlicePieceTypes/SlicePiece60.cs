using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SlicePiece60 : SlicePiece
{
    void OnEnable()
    {
        StartCoroutine(InitializeList());
    }



    IEnumerator InitializeList()
    {
        yield return new WaitForEndOfFrame();
        if (SliceGameManager.instance1 != null)
        {
            foreach (SliceSlot _managerSlice in SliceGameManager.instance1.sliceSlotList60)
            {
                //Debug.Log(_managerSlice);
                sliceSlotList.Add(_managerSlice);
            }
        }
        

    }

}

    
    

