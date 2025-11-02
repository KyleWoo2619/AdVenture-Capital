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
    [SerializeField] private AudioSource matchOneShot;          // Assign in Inspector
    [SerializeField] private AudioSource incorrectMoveOneShot;  // Assign in Inspector

    [Header("Visibility")]
    [Tooltip("Set to the row index you want hidden; -1 = show all (set to 0 to hide bottom row)")]
    [SerializeField] private int hiddenRow = 0;  // 👈 set to 0 to hide & lock bottom row

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
        ApplyVisibilityForAll(); // ensure hidden row is applied at start
    }

    // ---------- Visibility / Interactability ----------

    private bool ShouldBeVisible(int row)
    {
        if (hiddenRow < 0) return true;
        return row != hiddenRow;
    }

    private bool ShouldBeInteractable(int row)
    {
        // Never interact with the hidden row
        return row != hiddenRow;
    }

    private void ApplyVisibility(GameObject go)
    {
        if (!go) return;
        var d = go.GetComponent<Dot>();
        if (!d) return;

        bool visible = ShouldBeVisible(d.row);
        bool interactable = ShouldBeInteractable(d.row);

        // Toggle all SpriteRenderers (child included)
        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;

        // Toggle colliders (no clicks/touches on hidden row)
        var cols = go.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = interactable;

        // Tell Dot whether it can accept swipe input
        var dotScript = go.GetComponent<Dot>();
        if (dotScript != null)
            dotScript.canSwipe = interactable;
    }

    private void ApplyVisibilityForAll()
    {
        for (int c = 0; c < width; c++)
        {
            for (int r = 0; r < height; r++)
            {
                ApplyVisibility(allDots[c, r]);
            }
        }
    }

    /// <summary>
    /// Centralized setter so the grid, the Dot indices, and visibility all stay in sync.
    /// </summary>
    private void SetDotAt(int col, int row, GameObject go)
    {
        allDots[col, row] = go;
        var d = go.GetComponent<Dot>();
        d.column = col;
        d.row = row;
        ApplyVisibility(go);
    }

    // ---------- Setup & spawning ----------

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

                // Prefer checking only already-placed neighbors (left/down) during setup
                while (MatchesAtForSetup(i, j, dots[dotToUse]) && maxIterations < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }

                // Spawn dot as child and set local position
                var dot = Instantiate(dots[dotToUse], transform);
                dot.transform.localPosition = new Vector2(i, j + offSet) + localOrigin;
                dot.name = $"( {i}, {j} )";

                // Keep grid + model in sync
                SetDotAt(i, j, dot);
            }
        }
    }

    // Setup-time match prevention (checks already-placed cells only: left/down)
    private bool MatchesAtForSetup(int c, int r, GameObject piece)
    {
        string tag = piece.tag;

        // Left two
        if (c >= 2)
        {
            var a = allDots[c - 1, r];
            var b = allDots[c - 2, r];
            if (a && b && a.tag == tag && b.tag == tag) return true;
        }

        // Down two
        if (r >= 2)
        {
            var a = allDots[c, r - 1];
            var b = allDots[c, r - 2];
            if (a && b && a.tag == tag && b.tag == tag) return true;
        }

        return false;
    }

    // ---------- Matching / destroying ----------

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
                if (matchOneShot != null) matchOneShot.Play();
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
                if (matchOneShot != null) matchOneShot.Play();
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
                if (matchOneShot != null) matchOneShot.Play();
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

    // ---------- Gravity / compaction ----------

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;

        for (int i = 0; i < width; i++)
        {
            nullCount = 0;

            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    // Move piece down by nullCount
                    var pieceGO = allDots[i, j];
                    int newRow = j - nullCount;

                    // Update grid + model in sync
                    allDots[i, j] = null;
                    SetDotAt(i, newRow, pieceGO);
                }
            }
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
                    piece.name = $"( {i}, {j} )";

                    SetDotAt(i, j, piece);
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
        ApplyVisibilityForAll(); // newly spawned pieces respect hidden row
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

    // ---------- Swapping ----------

    private bool DestinationHitsHiddenRow(Dot a, Dot b)
    {
        if (hiddenRow < 0) return false;

        // After a swap, 'a' would go to b.row, 'b' would go to a.row
        return (b.row == hiddenRow) || (a.row == hiddenRow);
    }

    public void HandleDotSwap(Dot otherDot)
    {
        if (currentState != GameState.move) return;
        if (currentDot == null || otherDot == null) return;

        // Cancel any swap that would place a dot on the hidden row (e.g., row 0)
        if (DestinationHitsHiddenRow(currentDot, otherDot))
        {
            if (incorrectMoveOneShot != null) incorrectMoveOneShot.Play();
            StartCoroutine(ResetCurrentDot());
            return;
        }

        // Cache original indices
        int aRow = currentDot.row;
        int aCol = currentDot.column;
        int bRow = otherDot.row;
        int bCol = otherDot.column;

        // Swap the model (rows/cols on the Dot components)
        currentDot.row = bRow; currentDot.column = bCol;
        otherDot.row = aRow; otherDot.column = aCol;

        // Swap the board map (GameObject references)
        GameObject temp = allDots[aCol, aRow];
        allDots[aCol, aRow] = allDots[bCol, bRow];
        allDots[bCol, bRow] = temp;

        // Apply visibility to both swapped pieces (in case one crosses the hidden row)
        ApplyVisibility(allDots[aCol, aRow]);
        ApplyVisibility(allDots[bCol, bRow]);

        // Now detect matches
        findMatches.FindAllMatches();

        // If no matches found, swap back
        if (!currentDot.isMatched && !otherDot.isMatched)
        {
            if (incorrectMoveOneShot != null)
                incorrectMoveOneShot.Play();

            // Swap back model
            currentDot.row = aRow; currentDot.column = aCol;
            otherDot.row = bRow; otherDot.column = bCol;

            // Swap back map
            temp = allDots[aCol, aRow];
            allDots[aCol, aRow] = allDots[bCol, bRow];
            allDots[bCol, bRow] = temp;

            // Re-apply visibility after swap-back
            ApplyVisibility(allDots[aCol, aRow]);
            ApplyVisibility(allDots[bCol, bRow]);

            StartCoroutine(ResetCurrentDot());
        }
    }

    private IEnumerator ResetCurrentDot()
    {
        yield return new WaitForSeconds(.15f);
        currentDot = null;
        currentState = GameState.move;
    }
}
