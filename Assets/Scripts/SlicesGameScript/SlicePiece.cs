
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.InputSystem;


public class SlicePiece : MonoBehaviour
{
    private bool isDragging;
    public bool isPlaced { get; private set; }

    Vector2 offset; //offsets how much the slice's position is the mouse's position
    Vector2 originalPos; //used to set the slice back to its original selection if its not dropped in

    protected SliceSlot slot; //will hold the reference of one slot when a slot from the list is found

    protected List<SliceSlot> sliceSlotList = new List<SliceSlot>(); //will contain a list that reference each slot from each whole pizza
    //for example, for a pizza slice at 60 degrees, all slots that are 60 degrees are in this list. 

    protected PlayerInput playerInput;
    protected InputAction TouchPressedAction;
    protected InputAction TouchPosAction;

    protected Collider2D sliceCollider;

    [SerializeField] protected Transform childSlice;
    [SerializeField] protected SpriteRenderer childRenderer;

    void Awake()
    {
        originalPos = transform.position;
        sliceCollider = GetComponent<Collider2D>();

        childSlice = transform.GetChild(0); //get child game object
        childRenderer = childSlice.GetComponent<SpriteRenderer>(); //references its sprite renderer
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
        if (isPlaced)
        {
            sliceCollider.enabled = false;
            childRenderer.sortingLayerName = "OnPie"; //Debug.Log("sorting layer changed"); //change sorting order to 'OnPie' when placed  
            return;
        }


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
   

    public void SetSlice()
    {
        
         if (slot != null && !slot.GetIsFilledState(slot) && Vector2.Distance(transform.position, slot.transform.position) < 0.5)
        {
            transform.position = slot.transform.position;
            isPlaced = true;

            slot.ObjectOnSlot = this.gameObject;
            slot.SetIsFilledtoTrue(slot);
            
            // Medium haptic when slice is placed into socket
            MobileHaptics.ImpactMedium();
        }
        else
        {
            transform.position = originalPos;
            //isDragging = false;
        }
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


