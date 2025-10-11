using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Collections;

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

    void Start()
    {
        StartCoroutine(CallEndGame());
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

                slot1.ObjectOnSlot = this.gameObject;
                slot2.ObjectOnSlot = this.gameObject;
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
        
        var mousePosition = (UnityEngine.Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = mousePosition - offset;
        

       
    }

    UnityEngine.Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }

    public bool CanPlaceDualSliceAnywhere()
    {
        foreach (var pie in SliceGameManager.instance1.wholePieList)
        {
            if (pie == null) continue;

            var slots = pie.GetComponentsInChildren<SliceSlot>();

            var slotGroup1 = slots.Where(s => DualSlotList1.Contains(s)).ToList();
            var slotGroup2 = slots.Where(s => DualSlotList2.Contains(s)).ToList();

            foreach (var s1 in slotGroup1)
            {
                foreach (var s2 in slotGroup2)
                {
                    if (!s1.GetIsFilledState(s1) && !s2.GetIsFilledState(s2))
                    {
                        return true; // A valid pair exists
                    }
                }
            }
        }

        return false; // No valid dual-slot pairs found
    }
    
    IEnumerator CallEndGame()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to allow setup (if needed)

        if (!CanPlaceDualSliceAnywhere())
        {
            Debug.Log("This slice can't be placed anywhere");
            SliceGameManager.instance1.EndGame();
        }
    }


    
}
