using UnityEngine;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine.UIElements.Experimental;
using UnityEngine.InputSystem.Interactions;

public class WholePie : MonoBehaviour
{
    bool allSlotsFilled;


    private int slotsDestroyedCounter;

    void Update()
    {
        CheckIfAllSlotsFilled();
        if (allSlotsFilled)
        {
            Invoke(nameof(ClearAllSlices), 0.05f); // slight delay
        }
 
    }

    void CheckIfAllSlotsFilled() //checks if all slots are filled in a whole pie
    {
        allSlotsFilled = gameObject.transform
            .Cast<Transform>()
            .Select(child => child.GetComponent<SliceSlot>())
            .Where(slot => slot != null)
            .All(slot => slot.GetIsFilledState(slot));

    }

    void ClearAllSlices()
    {
        int slotsDestroyedCounter = 0; 
        var childSlots = gameObject.transform.Cast<Transform>()
            .Select(t => t.GetComponent<SliceSlot>())
            .Where(slot => slot != null);

        foreach (var slot in childSlots)
        {
            if (slot.ObjectOnSlot != null)
            {
                Destroy(slot.ObjectOnSlot);
                slotsDestroyedCounter++;
                slot.ObjectOnSlot = null;
                slot.SetisFilledtoFalse(slot);
            }
        }

        SliceGameManager.instance1.addToScore(slotsDestroyedCounter);
    }

    
}
