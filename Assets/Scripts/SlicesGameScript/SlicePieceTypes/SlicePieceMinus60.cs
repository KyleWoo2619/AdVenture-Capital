using System.Collections;
using UnityEngine;

public class SlicePieceMinus60 : SlicePiece
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
            foreach (SliceSlot _managerSlice in SliceGameManager.instance1.sliceSlotListMinus60)
            {
                //Debug.Log(_managerSlice);
                sliceSlotList.Add(_managerSlice);
            }
        }
        

    }
}
