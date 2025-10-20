using System.Collections;
using UnityEngine;

public class TripleSlice0_300_240 : TripleSlices
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
