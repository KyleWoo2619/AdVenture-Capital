using System.Collections;
using UnityEngine;

public class DualSlice180_240 : DualSlices
{
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
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotListMinus120)
        {
            DualSlotList2.Add(slot);
        }
    }
}
