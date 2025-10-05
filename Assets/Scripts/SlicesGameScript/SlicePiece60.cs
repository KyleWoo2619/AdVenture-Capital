using UnityEngine;
using System.Collections.Generic;

public class SlicePiece60 : SlicePiece
{

    void OnEnable()
    {
        if (SliceGameManager.instance1 != null)
        {
            foreach (SliceSlot _managerSlice in SliceGameManager.instance1.sliceSlotList60)
            {
                Debug.Log(_managerSlice);
            }
       }
        
    }

}

    
    

