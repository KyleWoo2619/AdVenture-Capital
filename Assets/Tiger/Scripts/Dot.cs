using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private FindMatches findMatches;
    private Board board;
    [HideInInspector] public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0f;
    public float swipeResist = 1f;

    [Header("Playability")]
    [Tooltip("Row index that cannot be swiped from or swiped into (typically 0).")]
    [SerializeField] private int blockedRow = 0;
    [Tooltip("Board/visibility may also set this at runtime. If false, all swipes are ignored.")]
    public bool canSwipe = true;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;

    void Awake()
    {
        board = Object.FindFirstObjectByType<Board>();
        findMatches = Object.FindFirstObjectByType<FindMatches>();
    }

    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;

        // (Re)acquire just in case
        if (!board) board = Object.FindFirstObjectByType<Board>();
        if (!findMatches) findMatches = Object.FindFirstObjectByType<FindMatches>();
    }

    private bool IsBlockedRow(int r) => r == blockedRow;

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isColorBomb = true;
            GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
            color.transform.parent = this.transform;
        }
    }

    void Update()
    {
        // Drive transform toward (column,row) in LOCAL space
        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.localPosition.x) > .1f)
        {
            tempPosition = new Vector2(targetX, transform.localPosition.y);
            transform.localPosition = Vector2.Lerp(transform.localPosition, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
                board.allDots[column, row] = this.gameObject;

            findMatches.FindAllMatches();
        }
        else
        {
            tempPosition = new Vector2(targetX, transform.localPosition.y);
            transform.localPosition = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.localPosition.y) > .1f)
        {
            tempPosition = new Vector2(transform.localPosition.x, targetY);
            transform.localPosition = Vector2.Lerp(transform.localPosition, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
                board.allDots[column, row] = this.gameObject;

            findMatches.FindAllMatches();
        }
        else
        {
            tempPosition = new Vector2(transform.localPosition.x, targetY);
            transform.localPosition = tempPosition;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        if (isColorBomb)
        {
            // This piece is a color bomb; destroy the color of the other piece
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }
        else if (otherDot != null && otherDot.GetComponent<Dot>().isColorBomb)
        {
            // Other piece is a color bomb
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
        }

        yield return new WaitForSeconds(.5f);

        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                // no match → revert
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;

                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                // matched → resolve
                board.DestroyMatches();
            }
        }
    }

    private void OnMouseDown()
    {
        if (board.currentState != GameState.move) return;
        if (!canSwipe) return;
        if (IsBlockedRow(row)) return; // cannot begin a swipe from the blocked row

        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (board.currentState != GameState.move) return;
        if (!canSwipe) return;
        if (IsBlockedRow(row)) return; // still on blocked row; ignore

        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                                     finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

            // Try to initiate a move
            otherDot = null; // ensure clean state
            MovePieces();

            // Only enter WAIT if a move actually got scheduled
            if (otherDot != null)
            {
                board.currentState = GameState.wait;
                board.currentDot = this;
            }
            else
            {
                board.currentState = GameState.move; // no move happened
            }
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        // Right
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            var target = board.allDots[column + 1, row];
            if (target == null) return;

            // lateral move doesn't change row; allowed
            otherDot = target;
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        }
        // Up
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            var target = board.allDots[column, row + 1];
            if (target == null) return;

            // moving up: otherDot moves down one; ensure it wouldn't land on blocked row
            int otherNewRow = row; // target goes from row+1 down to row
            if (IsBlockedRow(otherNewRow)) return; // would push neighbor into blocked row (only if row==0)

            otherDot = target;
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        }
        // Left
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            var target = board.allDots[column - 1, row];
            if (target == null) return;

            // lateral move doesn't change row; allowed
            otherDot = target;
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        }
        // Down
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // DOWN would land us on row-1
            int myNewRow = row - 1;
            if (IsBlockedRow(myNewRow)) return; // disallow swiping into blocked row

            var target = board.allDots[column, row - 1];
            if (target == null) return;

            otherDot = target;
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }

        // If we started a valid move, mark that the player moved so scoring can apply
        if (otherDot != null && findMatches != null)
        {
            findMatches.playerHasMoved = true;
            StartCoroutine(CheckMoveCo());
        }
    }

    // (Optional) local helper for 3-in-a-row marking; unchanged from your original
    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
    }
}
