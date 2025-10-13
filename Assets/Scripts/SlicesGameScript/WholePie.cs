using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WholePie : MonoBehaviour
{
    bool allSlotsFilled;
    [SerializeField] List<WholePie> adjecentPies = new List<WholePie>();

    [Header("Audio")]
    [SerializeField] private AudioSource matchingSound; // Sound when plate is successfully filled

    private int slotsDestroyedCounter;
    private bool hasPlayedMatchSound = false; // Prevent multiple sound plays

    void Update()
    {
        CheckIfAllSlotsFilled();
        if (allSlotsFilled && !hasPlayedMatchSound)
        {
            // Play matching sound when plate is successfully filled (only once)
            if (matchingSound != null && matchingSound.clip != null)
                matchingSound.PlayOneShot(matchingSound.clip);

            hasPlayedMatchSound = true; // Prevent multiple plays

            Invoke(nameof(ClearAllSlices), 0.05f); // slight delay
            adjecentPies[0].ClearAllSlices();
            adjecentPies[1].ClearAllSlices();
        }
 
    }

    protected void CheckIfAllSlotsFilled() //checks if all slots are filled in a whole pie
    {
        allSlotsFilled = gameObject.transform
            .Cast<Transform>()
            .Select(child => child.GetComponent<SliceSlot>())
            .Where(slot => slot != null)
            .All(slot => slot.GetIsFilledState(slot));

    }

    public void ClearAllSlices() //clears slices off pie
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
        
        // Reset the sound flag so it can play again next time
        hasPlayedMatchSound = false;
    }

    
}
