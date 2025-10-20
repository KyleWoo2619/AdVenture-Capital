using System.Collections;
using UnityEngine;

public class TripleSlice180_120_60 : TripleSlices
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
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotList120)
        {
            TripleSlotList2.Add(slot);
        }
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotList60)
        {
            TripleSlotList3.Add(slot);
        }
    }
}
