using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlicePiece0 : SlicePiece
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
            foreach (SliceSlot _managerSlice in SliceGameManager.instance1.sliceSlotList0)
            {
                //Debug.Log(_managerSlice);
                sliceSlotList.Add(_managerSlice);
            }
        }
        

    }
}
