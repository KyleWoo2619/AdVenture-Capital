using Unity.VisualScripting;
using UnityEngine;

public class SliceSlot : MonoBehaviour
{
    [SerializeField] private bool isFilled;
    public GameObject ObjectOnSlot; //the piece on the slot


    void Awake()
    {
        isFilled = false;
    }

    public void SetIsFilledtoTrue(SliceSlot slot)
    {
        slot.isFilled = true;
    }

    public void SetisFilledtoFalse(SliceSlot slot)
    {
        slot.isFilled = false;
    }

    public bool GetIsFilledState(SliceSlot slot)
    {
        return slot.isFilled;
    }
 
}
