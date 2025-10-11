
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;


public class SlicePiece : MonoBehaviour
{
    private bool isDragging;
    public bool isPlaced { get; private set; }

    Vector2 offset; //offsets how much the slice's position is the mouse's position
    Vector2 originalPos; //used to set the slice back to its original selection if its not dropped in

    protected SliceSlot slot; //will hold the reference of one slot when a slot from the list is found

    protected List<SliceSlot> sliceSlotList = new List<SliceSlot>(); //will contain a list that reference each slot from each whole pizza
    //for example, for a pizza slice at 60 degrees, all slots that are 60 degrees are in this list. 

    bool allfilled;

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

        offset = GetMousePos() - (Vector2)transform.position;
    }

    void OnMouseUp()
    {

        if (slot != null && !slot.GetIsFilledState(slot) && Vector2.Distance(transform.position, slot.transform.position) < 0.5)
        {
            transform.position = slot.transform.position;
            isPlaced = true;

            slot.ObjectOnSlot = this.gameObject;
            slot.SetIsFilledtoTrue(slot);
        }
        else
        {
            transform.position = originalPos;
            isDragging = false;
        }
        
    }

    void Update()
    {
        if (isPlaced) return;

        if (!isDragging)
            return;
        

        var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = mousePosition - offset;


        foreach (SliceSlot _slot in sliceSlotList)
        {
            if (Vector2.Distance(transform.position, _slot.transform.position) < 0.5)
            {
                //Debug.Log(_slot);
                slot = _slot;
                if (slot == _slot)
                {
                    break;
                }
            }
        }
        
        
    }
    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }

    public bool CanPlaceSliceAnywhere()
    {
        foreach (var pie in SliceGameManager.instance1.wholePieList)
        {
            if (pie == null) continue;

            var slots = pie.GetComponentsInChildren<SliceSlot>();

            var slotGroup1 = slots.Where(s => sliceSlotList.Contains(s)).ToList();


            foreach (var s1 in slotGroup1)
            {
                if (!s1.GetIsFilledState(s1))
                {
                    return true; // A valid pair exists
                }

            }
        }

        return false; // No valid slots found
    }
    
    IEnumerator CallEndGame()
    {
        yield return new WaitForSeconds(0.2f); // Small delay to allow setup (if needed)

        if (!CanPlaceSliceAnywhere())
        {
            Debug.Log("This slice can't be placed anywhere");
            SliceGameManager.instance1.EndGame();
        }
    }

}


