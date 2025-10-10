using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

public class DualSlices : MonoBehaviour
{
    private bool isDragging;
    public bool isPlaced { get; private set; }

    UnityEngine.Vector2 offset; //offsets how much the slice's position is the mouse's position
    UnityEngine.Vector2 originalPos; //used to set the slice back to its original selection if its not dropped in

    protected SliceSlot slot1;
    protected SliceSlot slot2;
    protected List<SliceSlot> DualSlotList1 = new List<SliceSlot>(); //mainly contain 0, 180
    protected List<SliceSlot> DualSlotList2 = new List<SliceSlot>(); //mainly contain 60,120,240,300

    bool allFilled;
    bool allFilled2;
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
        if (slot1 != null && slot2 != null)
        {
            if (!slot1.GetIsFilledState(slot1) && !slot2.GetIsFilledState(slot2) && UnityEngine.Vector2.Distance(transform.position, slot1.transform.position) < 1)
            {
                transform.position = slot1.transform.position;
                
                isPlaced = true;

                slot1.SetIsFilledtoTrue(slot1);
                slot2.SetIsFilledtoTrue(slot2);
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

        foreach (SliceSlot _slot1 in DualSlotList1)
        {
            if (UnityEngine.Vector2.Distance(transform.position, _slot1.transform.position) < 1)
            {
                //Debug.Log(_slot1);
                slot1 = _slot1;

            }
        }

        foreach (SliceSlot _slot2 in DualSlotList2)
        {
            if (UnityEngine.Vector2.Distance(transform.position, _slot2.transform.position) < 1)
            {
                //Debug.Log(_slot2);
                slot2 = _slot2;
            }
        }

        allFilled = DualSlotList1.All(_slot1 => _slot1.GetIsFilledState(_slot1));
        allFilled2 = DualSlotList2.All(_slot2 => _slot2.GetIsFilledState(_slot2));
        if(allFilled || allFilled2) Debug.Log("the slots are filled");
    }
    
    UnityEngine.Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }
    
}
