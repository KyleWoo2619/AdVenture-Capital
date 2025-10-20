using System.Collections;
using UnityEngine;

public class DualSlices240_300 : DualSlices
{
    void OnEnable()
    {
        StartCoroutine(InitializeList());
    }

    IEnumerator InitializeList()
    {
        yield return new WaitForEndOfFrame();
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotListMinus120)
        {
            DualSlotList1.Add(slot);
        }
        foreach (SliceSlot slot in SliceGameManager.instance1.sliceSlotListMinus60)
        {
            DualSlotList2.Add(slot);
        }
    }
}
