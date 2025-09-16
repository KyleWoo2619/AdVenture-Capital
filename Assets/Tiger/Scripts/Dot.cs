using UnityEngine;

public class Dots : MonoBehaviour
{
    public int column;
    public int row;
    private Board board;
    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    public float swipeAngle = 0;
  


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
    }

    void MovePieces()
    {
        if(swipeAngle > -45 && swipeAngle <= 45)
        {
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Dots>().column -= 1;
            column += 1;
        }
    }
}

