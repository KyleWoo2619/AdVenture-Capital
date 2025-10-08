using UnityEngine;

public class SliceSlot : MonoBehaviour
{
    [SerializeField] private bool isFilled;
    private SlicePiece piece;


    void Awake()
    {
        isFilled = false;
    }

    public void SetIsFilledtoTrue(SliceSlot slot)
    {
        slot.isFilled = true;
    }

    public bool GetIsFilledState(SliceSlot slot)
    {
        return slot.isFilled;
    }

    
}
