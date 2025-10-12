using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleSlices180_240_300 : TripleSlices
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
            TripleSlotList1.Add(slot);
        }
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotListMinus60)
        {
            TripleSlotList2.Add(slot);
        }
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotListMinus120)
        {
            TripleSlotList3.Add(slot);
        }
    }
}
