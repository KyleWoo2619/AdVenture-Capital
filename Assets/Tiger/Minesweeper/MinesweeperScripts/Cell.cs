using UnityEngine;

public struct Cell 
{
   public enum Type
   {
      Empty,
      Mine,
      Number
    }

    public Vector3 position;
    public Type type;
    public int number; // Number of adjacent mines (only relevant if type is Number)
    public bool isRevealed;
    public bool isFlagged;
    public bool isExploded;

}
