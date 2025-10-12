using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;


public class TripleSlices : MonoBehaviour
{
    private bool isDragging;
    public bool isPlaced { get; private set; }

    Vector2 offset; //offsets how much the slice's position is the mouse's position
    Vector2 originalPos; //used to set the slice back to its original selection if its not dropped in

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

    void Start()
    {
        StartCoroutine(CallEndGame());
    }

    /*void OnMouseDown()
    {
        isDragging = true;

        offset = GetMousePos() - (Vector2)transform.position;
    }
    */

    

    void Update()
    {
        if (isPlaced) return;


        //var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //transform.position = mousePosition - offset;

        foreach (SliceSlot _slot1 in TripleSlotList1)
        {


            if (Vector2.Distance(transform.position, _slot1.transform.position) < 1.25f)
            {
                //Debug.Log(_slot1);
                slot1 = _slot1;

            }
        }

        foreach (SliceSlot _slot2 in TripleSlotList2)
        {
            if (Vector2.Distance(transform.position, _slot2.transform.position) < 1.25f)
            {
                //Debug.Log(_slot2);
                slot2 = _slot2;
            }
        }

        foreach (SliceSlot _slot3 in TripleSlotList3)
        {
            if (Vector2.Distance(transform.position, _slot3.transform.position) < 1.25f)
            {
                //Debug.Log(_slot3);
                slot3 = _slot3;
            }
        }


    }
    
    public void SetTripleSlice()
    {
        if (slot1 != null && slot2 != null && slot3 != null)
        {
            if (!slot1.GetIsFilledState(slot1) && !slot2.GetIsFilledState(slot2) && !slot3.GetIsFilledState(slot3) && Vector2.Distance(transform.position, slot1.transform.position) < 1)
            {
                transform.position = slot1.transform.position;

                isPlaced = true;

                slot1.SetIsFilledtoTrue(slot1);
                slot2.SetIsFilledtoTrue(slot2);
                slot3.SetIsFilledtoTrue(slot3);

                slot1.ObjectOnSlot = this.gameObject;
                slot3.ObjectOnSlot = this.gameObject;
                slot2.ObjectOnSlot = this.gameObject;
            }

            else
            {
                transform.position = originalPos;
                //isDragging = false;
            }
        }
    }

   

    public bool CanPlaceTripleSliceAnywhere() //handles checking if each pie has a valid spot for the slice
    {
        foreach (var pie in SliceGameManager.instance1.wholePieList)
        {
            if (pie == null) continue;

            var slots = pie.GetComponentsInChildren<SliceSlot>();

            var slotGroup1 = slots.Where(s => TripleSlotList1.Contains(s)).ToList();
            var slotGroup2 = slots.Where(s => TripleSlotList2.Contains(s)).ToList();
            var slotGroup3 = slots.Where(s => TripleSlotList3.Contains(s)).ToList();

            foreach (var s1 in slotGroup1)
            {
                foreach (var s2 in slotGroup2)
                {
                    foreach (var s3 in slotGroup3)
                    {
                        if (!s1.GetIsFilledState(s1) && !s2.GetIsFilledState(s2) && !s3.GetIsFilledState(s3))
                        {
                            return true; // A valid pair exists
                        }
                    }

                }
            }
        }

        return false; // No valid dual-slot pairs found
    }
    
    IEnumerator CallEndGame()
    {
        yield return new WaitForSeconds(0.3f); // Small delay to allow setup (if needed)

        if (!CanPlaceTripleSliceAnywhere())
        {
            Debug.Log("This slice can't be placed anywhere");
            SliceGameManager.instance1.EndGame();
        }
    }
}
