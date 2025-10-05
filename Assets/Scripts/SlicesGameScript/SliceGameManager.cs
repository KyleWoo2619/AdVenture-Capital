using System.Collections.Generic;
using UnityEngine;

public class SliceGameManager : MonoBehaviour
{
    public static SliceGameManager instance1;
    public List<SliceSlot> sliceSlotList60 = new List<SliceSlot>();


    void Awake()
    {
        if (instance1 != null && instance1 != this)
        {
            Destroy(this);
        }
        else
        {
            instance1 = this;
        }

    }

    

}
