using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class FindMatches : MonoBehaviour
{

    private Board board;
    [HideInInspector] public List<GameObject> currentMatches = new List<GameObject>();

    public int score = 0; //

    public TMP_Text scoreText;

    public int Score
    {
        get { return score; }
        set { score = value; }
    }
    [HideInInspector] public bool playerHasMoved = false;

    // Use this for initialization
    void Start()
    {
        board = FindFirstObjectByType<Board>();
        score = 0;           // Reset score
        UpdateScoreUI();     // Update UI to show 0
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
        }

        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
        }

        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.column));
        }

        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.column));
        }

        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.column));
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    public void OnPlayerMove()
    {
        playerHasMoved = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        // Add matched dots to the list and mark them
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        currentMatches.Clear();
        yield return new WaitForSeconds(.2f);

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];
                if (currentDot == null) continue; // Null guard

                Dot currentDotDot = currentDot.GetComponent<Dot>();

                // Horizontal match check
                if (i > 0 && i < board.width - 1)
                {
                    GameObject left = board.allDots[i - 1, j];
                    GameObject right = board.allDots[i + 1, j];
                    if (left != null && right != null)
                    {
                        Dot l = left.GetComponent<Dot>();
                        Dot r = right.GetComponent<Dot>();
                        if (left.tag == currentDot.tag && right.tag == currentDot.tag)
                        {
                            GetNearbyPieces(left, currentDot, right);
                        }
                    }
                }

                // Vertical match check
                if (j > 0 && j < board.height - 1)
                {
                    GameObject up = board.allDots[i, j + 1];
                    GameObject down = board.allDots[i, j - 1];
                    if (up != null && down != null)
                    {
                        Dot u = up.GetComponent<Dot>();
                        Dot d = down.GetComponent<Dot>();
                        if (up.tag == currentDot.tag && down.tag == currentDot.tag)
                        {
                            GetNearbyPieces(up, currentDot, down);
                        }
                    }
                }
            }
        }

        // SCORING LOGIC
        int matchedCount = currentMatches.Count;
        if (matchedCount >= 3 && playerHasMoved)
        {
            // Award points for all matches found in this chain
            score += matchedCount;

            // Combo bonus: +2 for 4-match, +5 for 5-match, etc.
            if (matchedCount > 3)
                score += (matchedCount - 3) * 2;

            Debug.Log($"Score is : {score}");
            UpdateScoreUI();

            // Reset after scoring so only matches after player moves count
            //playerHasMoved = false;
        }
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    //Check the tag on that dot
                    if (board.allDots[i, j].tag == color)
                    {
                        //Set that dot to be matched
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column, i] != null)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void CheckBombs()
    {
        //Did the player move something?
        if (board.currentDot != null)
        {
            //Is the piece they moved matched?
            if (board.currentDot.isMatched)
            {
                //make it unmatched
                board.currentDot.isMatched = false;
                //Decide what kind of bomb to make
                /*
                int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50){
                    //Make a row bomb
                    board.currentDot.MakeRowBomb();
                }else if(typeOfBomb >= 50){
                    //Make a column bomb
                    board.currentDot.MakeColumnBomb();
                }
                */
                if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                   || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                }
                else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
            //Is the other piece matched?
            else if (board.currentDot.otherDot != null)
            {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                //Is the other Dot matched?
                if (otherDot.isMatched)
                {
                    //Make it unmatched
                    otherDot.isMatched = false;
                    /*
                    //Decide what kind of bomb to make
                    int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        //Make a row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        //Make a column bomb
                        otherDot.MakeColumnBomb();
                    }
                    */
                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                   || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }

        }
    }

    private void Update()
    {
        //     // Check for special case: horizontal or vertical match of 4
        //     if (currentMatches.Count == 4)
        //     {
        //         // Pick a random dot from the match to become a bomb
        //         Dot bombDot = currentMatches[Random.Range(0, currentMatches.Count)].GetComponent<Dot>();
        //         if (bombDot != null)
        //         {
        //             // Decide based on the swipe angle
        //             if ((bombDot.swipeAngle > -45 && bombDot.swipeAngle <= 45)
        //                || (bombDot.swipeAngle < -135 || bombDot.swipeAngle >= 135))
        //             {
        //                 bombDot.MakeRowBomb();
        //             }
        //             else
        //             {
        //                 bombDot.MakeColumnBomb();
        //             }
        //         }
        //     }
    }
}