using System.Collections;
using UnityEngine;

public class DualSlice0_60 : DualSlices
{
     void OnEnable()
    {
        StartCoroutine(InitializeList());
    }

    IEnumerator InitializeList()
    {
        yield return new WaitForEndOfFrame();
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotList0)
        {
            DualSlotList1.Add(slot);
        }
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotList60)
        {
            DualSlotList2.Add(slot);
        }
    }

}
