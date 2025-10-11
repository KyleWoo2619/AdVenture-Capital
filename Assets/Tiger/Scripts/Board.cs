using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour
{

    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offSet;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject destroyParticle;
    private BackgroundTile[,] allTiles;
    public GameObject[,] allDots;
    public Dot currentDot;
    private FindMatches findMatches;

    public Vector2 localOrigin = Vector2.zero;   // lets you nudge the whole grid

    [Header("Audio")]
    [SerializeField] private AudioSource matchOneShot; // Assign in Inspector
    [SerializeField] private AudioSource incorrectMoveOneShot; // Assign in Inspector

    // Use this for initialization
    void Awake()
    {
        Time.timeScale = 1f;
    }
    
    void Start()
    {
        findMatches = FindFirstObjectByType<FindMatches>();
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Spawn background tile as child and set local position
                var bg = Instantiate(tilePrefab, transform);
                bg.transform.localPosition = new Vector2(i, j + offSet) + localOrigin;
                bg.name = $"( {i}, {j} )";

                int dotToUse = Random.Range(0, dots.Length);
                int maxIterations = 0;
                while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }

                // Spawn dot as child and set local position
                var dot = Instantiate(dots[dotToUse], transform);
                dot.transform.localPosition = new Vector2(i, j + offSet) + localOrigin;
                var d = dot.GetComponent<Dot>();
                d.row = j; d.column = i;
                dot.name = $"( {i}, {j} )";
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        // Horizontal: check right
        if (column <= width - 3)
        {
            if (allDots[column, row] != null &&
                allDots[column + 1, row] != null &&
                allDots[column + 2, row] != null)
            {
                if (allDots[column, row].tag == piece.tag &&
                    allDots[column + 1, row].tag == piece.tag &&
                    allDots[column + 2, row].tag == piece.tag)
                    return true;
            }
        }
        // Vertical: check up
        if (row <= height - 3)
        {
            if (allDots[column, row] != null &&
                allDots[column, row + 1] != null &&
                allDots[column, row + 2] != null)
            {
                if (allDots[column, row].tag == piece.tag &&
                    allDots[column, row + 1].tag == piece.tag &&
                    allDots[column, row + 2].tag == piece.tag)
                    return true;
            }
        }
        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        Dot dot = allDots[column, row]?.GetComponent<Dot>();
        if (dot != null && dot.isMatched)
        {
            // Bomb logic
            if (dot.isRowBomb)
            {
                DestroyRow(row);
            }
            else if (dot.isColumnBomb)
            {
                DestroyColumn(column);
            }
            else
            {
                // Normal destruction
                Vector3 pos = allDots[column, row].transform.position;
                if (matchOneShot != null) matchOneShot.Play(); // Play SFX
                GameObject particle = Instantiate(destroyParticle, pos, Quaternion.identity);
                Destroy(particle, .5f);
                Destroy(allDots[column, row]);
                allDots[column, row] = null;
            }
        }
    }

    private void DestroyRow(int row)
    {
        for (int i = 0; i < width; i++)
        {
            if (allDots[i, row] != null)
            {
                Vector3 pos = allDots[i, row].transform.position;
                if (matchOneShot != null) matchOneShot.Play(); // Play SFX
                GameObject particle = Instantiate(destroyParticle, pos, Quaternion.identity);
                Destroy(particle, .5f);
                Destroy(allDots[i, row]);
                allDots[i, row] = null;
            }
        }
    }

    private void DestroyColumn(int column)
    {
        for (int j = 0; j < height; j++)
        {
            if (allDots[column, j] != null)
            {
                Vector3 pos = allDots[column, j].transform.position;
                if (matchOneShot != null) matchOneShot.Play(); // Play SFX
                GameObject particle = Instantiate(destroyParticle, pos, Quaternion.identity);
                Destroy(particle, .5f);
                Destroy(allDots[column, j]);
                allDots[column, j] = null;
            }
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.currentMatches.Clear();

        // Wait for VFX to play before shifting rows
        StartCoroutine(WaitAndDecreaseRow());
    }

    private IEnumerator WaitAndDecreaseRow()
    {
        yield return new WaitForSeconds(0.3f); // Wait for VFX duration
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    int dotToUse = Random.Range(0, dots.Length);
                    var piece = Instantiate(dots[dotToUse], transform);
                    piece.transform.localPosition = new Vector2(i, j + offSet) + localOrigin;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                    piece.name = $"( {i}, {j} )";
                    allDots[i, j] = piece;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(.5f);

        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }

    public void HandleDotSwap(Dot otherDot)
    {
        // Cache the current row and column
        int previousRow = currentDot.row;
        int previousColumn = currentDot.column;

        // Swap the dots
        currentDot.row = otherDot.GetComponent<Dot>().row;
        currentDot.column = otherDot.GetComponent<Dot>().column;
        otherDot.GetComponent<Dot>().row = previousRow;
        otherDot.GetComponent<Dot>().column = previousColumn;

        // Check for matches
        findMatches.FindAllMatches();

        // If no matches found, swap back
        if (!currentDot.isMatched && !otherDot.GetComponent<Dot>().isMatched)
        {
            // Play incorrect move sound
            if (incorrectMoveOneShot != null)
                incorrectMoveOneShot.Play();

            // Swap back
            otherDot.GetComponent<Dot>().row = currentDot.row;
            otherDot.GetComponent<Dot>().column = currentDot.column;
            currentDot.row = previousRow;
            currentDot.column = previousColumn;
            StartCoroutine(ResetCurrentDot());
        }
    }

    private IEnumerator ResetCurrentDot()
    {
        yield return new WaitForSeconds(.5f);
        currentDot = null;
        currentState = GameState.move;
    }
}