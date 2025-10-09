using UnityEngine;
using System.Collections.Generic;

public class TripleSlices : MonoBehaviour
{
    private bool isDragging;
    public bool isPlaced { get; private set; }

    UnityEngine.Vector2 offset; //offsets how much the slice's position is the mouse's position
    UnityEngine.Vector2 originalPos; //used to set the slice back to its original selection if its not dropped in

    protected SliceSlot slot1;
    protected SliceSlot slot2;
    protected SliceSlot slot3;
    protected List<SliceSlot> TripleSlotList1 = new List<SliceSlot>(); //mainly contain 0, 180
    protected List<SliceSlot> TripleSlotList2 = new List<SliceSlot>(); 
    protected List<SliceSlot> TripleSlotList3 = new List<SliceSlot>(); 
    void Awake()
    {
        originalPos = transform.position;
    }

    void OnMouseDown()
    {
        isDragging = true;

        offset = GetMousePos() - (UnityEngine.Vector2)transform.position;
    }

    void OnMouseUp()
    {
        if (slot1 != null && slot2 != null && slot3 != null)
        {
            if (!slot1.GetIsFilledState(slot1) && !slot2.GetIsFilledState(slot2) && !slot3.GetIsFilledState(slot3) && UnityEngine.Vector2.Distance(transform.position, slot1.transform.position) < 1)
            {
                transform.position = slot1.transform.position;

                isPlaced = true;

                slot1.SetIsFilledtoTrue(slot1);
                slot2.SetIsFilledtoTrue(slot2);
                slot3.SetIsFilledtoTrue(slot3);
            }

            else
            {
                transform.position = originalPos;
                isDragging = false;
            }   

        }
        
    }

    void Update()
    {
        if (isPlaced) return;

        if (!isDragging)
            return;


        var mousePosition = (UnityEngine.Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = mousePosition - offset;

        foreach (SliceSlot _slot1 in TripleSlotList1)
        {
            if (UnityEngine.Vector2.Distance(transform.position, _slot1.transform.position) < 1.25f)
            {
                Debug.Log(_slot1);
                slot1 = _slot1;

            }
        }

        foreach (SliceSlot _slot2 in TripleSlotList2)
        {
            if (UnityEngine.Vector2.Distance(transform.position, _slot2.transform.position) < 1.25f)
            {
                Debug.Log(_slot2);
                slot2 = _slot2;
            }
        }

        foreach (SliceSlot _slot3 in TripleSlotList3)
        {
            if (Vector2.Distance(transform.position, _slot3.transform.position) < 1.25f)
            {
                Debug.Log(_slot3);
                slot3 = _slot3;
            }
        }

        
    }
    
    UnityEngine.Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }
}
