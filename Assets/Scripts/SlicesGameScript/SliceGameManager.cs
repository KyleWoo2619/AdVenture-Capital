using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceGameManager : MonoBehaviour
{
    public static SliceGameManager instance1;

    public List<WholePie> wholePieList = new List<WholePie>();
    public List<SliceSlot> sliceSlotList60 = new List<SliceSlot>();
    public List<SliceSlot> sliceSlotList0 = new List<SliceSlot>();
    public List<SliceSlot> sliceSlotList180 = new List<SliceSlot>();
    public List<SliceSlot> sliceSlotList120 = new List<SliceSlot>();
    public List<SliceSlot> sliceSlotListMinus60 = new List<SliceSlot>();
    public List<SliceSlot> sliceSlotListMinus120 = new List<SliceSlot>();

    [SerializeField] int score = 0;

    [Header("Audio")]
    [SerializeField] private AudioSource loseSound; // Sound when player loses

    [Header("Ad System")]
    [SerializeField] private FullscreenAdSpawner adSpawner; // Reference to ad spawner

    void Awake()
    {
        if (instance1 != null && instance1 != this)
        {
            Destroy(this);
        }
        else
        {
            instance1 = this;
        }

        // Debug.Log(sliceSlotList60[0]);

    }

    public void addToScore(int numDestroyedSlices)
    {
        score += numDestroyedSlices;
        Debug.Log(score);
    }

    // Public method to get current score (for UI display)
    public int GetScore()
    {
        return score;
    }
    
    public void EndGame()
    {
        // Pause the game
        Time.timeScale = 0;
        Debug.Log("Game Over - Player Lost");

        // Play lose sound
        if (loseSound != null && loseSound.clip != null)
            loseSound.PlayOneShot(loseSound.clip);

        // Show ad for death/lose, then fail menu after delay
        StartCoroutine(ShowLoseAdAfterDelay(1.5f));
    }

    private IEnumerator ShowLoseAdAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real time since game is paused

        // Use the death-specific ad method that shows fail menu after
        if (adSpawner != null)
        {
            adSpawner.ShowAdForDeath();
        }
    }

    

}
