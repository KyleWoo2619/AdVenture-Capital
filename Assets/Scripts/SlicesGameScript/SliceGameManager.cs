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
    
    public void EndGame()
    {
        Time.timeScale = 0;
        Debug.Log("your lose");
        //display fail menu
    }

    

}
