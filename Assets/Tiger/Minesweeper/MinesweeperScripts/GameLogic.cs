using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/*
Written by Tiger Britt
Edited by Kyle Woo
This script manages the overall game logic for a Minesweeper game, including board generation, cell interactions, and win/loss conditions.
Kyle edited touch controls to differentiate between short tap (flag) and long press (reveal).
*/

public class GameLogic : MonoBehaviour
{
    InputAction touchPress;
    public int width = 10;
    public int height = 10;
    public int mineCount = 10;
    private MineBoard Board;
    private Cell[,] state;

    [Header("Audio")]
    public AudioSource deathSound;
    public AudioSource winSound;
    public AudioSource touchSound;

    [Header("Ad System")]
    public FullscreenAdSpawner adSpawner;

    [Header("Game State")]
    private bool isGamePaused = false; // Add pause state tracking

    private void Awake()
    {
        Board = GetComponentInChildren<MineBoard>();
        touchPress = InputSystem.actions.FindAction("TouchPress"); // Move here
    }

    private void OnEnable()
    {
        if (touchPress != null) // Add null check
            touchPress.Enable();
    }

    private void OnDisable()
    {
        if (touchPress != null) // Add null check
            touchPress.Disable();
    }

    private void Start()
    {
        // Remove touchPress initialization from here
        NewGame();
    }

    private void NewGame()
    {
        state = new Cell[width, height];
        GenerateCells();
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10);
        Board.Draw(state);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
        GenerateMines();
        GenerateNumbers();
    }

    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            while (state[x, y].type == Cell.Type.Mine)
            {
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                    if (y >= height) { y = 0; }
                }
            }
            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type == Cell.Type.Mine) { continue; }
                cell.number = CountMines(x, y);
                if (cell.number > 0) { cell.type = Cell.Type.Number; }
                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) { continue; }
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;
                if (GetCell(x, y).type == Cell.Type.Mine) { count++; }
            }
        }
        return count;
    }

    private void Update()
    {
        // Don't process ANY input if game is paused
        if (isGamePaused) return;

        // Desktop input
        if (Input.GetMouseButtonDown(1)) { Flag(); }
        if (Input.GetMouseButtonDown(0)) { Reveal(); }

        // Mobile touch input - only process if not paused
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                StartCoroutine(HandleTouch(touch.position));
            }
        }
    }

    private IEnumerator HandleTouch(Vector2 touchPosition)
    {
        float holdTime = 0f;
        float longPressThreshold = 0.6f;
        bool actionExecuted = false;

        while (Input.touchCount > 0 && Input.GetTouch(0).phase != UnityEngine.TouchPhase.Ended)
        {
            // Check if game gets paused during touch
            if (isGamePaused)
            {
                yield break; // Exit if paused during touch
            }

            holdTime += Time.deltaTime;

            if (holdTime >= longPressThreshold && !actionExecuted)
            {
                RevealTouch(touchPosition);
                actionExecuted = true;
                yield break;
            }

            yield return null;
        }

        // Only execute flag if game is still not paused
        if (!actionExecuted && !isGamePaused)
        {
            FlagTouch(touchPosition);
        }
    }

    private void Flag()
    {
        if (isGamePaused) return; // Block input if paused

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPosition.z = 0;
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed) { return; }
        
        // Play touch sound
        if (touchSound != null)
            touchSound.Play();
            
        cell.isFlagged = !cell.isFlagged;
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private void FlagTouch(Vector2 screenPosition)
    {
        if (isGamePaused) return; // Block input if paused

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPosition.z = 0;
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed) { return; }
        
        // Play touch sound
        if (touchSound != null)
            touchSound.Play();
            
        cell.isFlagged = !cell.isFlagged;
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private void Reveal()
    {
        if (isGamePaused) return; // Block input if paused

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPosition.z = 0;
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed || cell.isFlagged) { return; }

        // Play touch sound
        if (touchSound != null)
            touchSound.Play();

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Debug.Log("Game Over!");
                GameOver();
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.isRevealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        cell.isRevealed = true;
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private void RevealTouch(Vector2 screenPosition)
    {
        if (isGamePaused) return; // Block input if paused

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPosition.z = 0;
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed || cell.isFlagged) { return; }

        // Play touch sound
        if (touchSound != null)
            touchSound.Play();

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Debug.Log("Game Over!");
                GameOver();
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.isRevealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        cell.isRevealed = true;
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private void GameOver()
    {
        // Play death sound
        if (deathSound != null)
            deathSound.Play();

        // Reveal all mines
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell c = state[x, y];
                if (c.type == Cell.Type.Mine)
                {
                    c.isRevealed = true;
                    state[x, y] = c;
                }
            }
        }

        Board.Draw(state);

        // Show ad for death - this will show fail menu after ad closes
        StartCoroutine(ShowAdForDeathAfterDelay(1.5f));
    }

    private IEnumerator ShowAdForDeathAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Use the death-specific ad method that shows fail menu after
        if (adSpawner != null)
        {
            adSpawner.ShowAdForDeath();
        }
    }

    private void CheckWinCondition()
    {
        // Check if all non-mine cells are revealed (standard minesweeper win condition)
        int revealedCount = 0;
        int totalNonMineCells = (width * height) - mineCount;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                
                // Count revealed non-mine cells
                if (cell.type != Cell.Type.Mine && cell.isRevealed)
                {
                    revealedCount++;
                }
            }
        }

        // Win condition: All non-mine cells revealed
        if (revealedCount == totalNonMineCells)
        {
            Debug.Log("You Win!");
            GameWon();
        }
    }

    private void GameWon()
    {
        // Play win sound
        if (winSound != null)
            winSound.Play();

        // Show ad after a brief delay - for win screen
        StartCoroutine(ShowAdForWinAfterDelay(2f));
    }

    private IEnumerator ShowAdForWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Use the win-specific ad method that shows win menu after
        if (adSpawner != null)
        {
            adSpawner.ShowAdForWin();
        }
    }

    private IEnumerator ShowAdAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Show ad if available
        if (adSpawner != null)
        {
            adSpawner.ShowAd();
        }
    }

    // Remove the old Explode method since GameOver handles it now

    private void Flood(Cell cell)
    {
        if (cell.isRevealed || cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid)
            return;

        cell.isRevealed = true;
        state[(int)cell.position.x, (int)cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
            {
                for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
                {
                    if (adjacentX == 0 && adjacentY == 0) continue;

                    int x = (int)cell.position.x + adjacentX;
                    int y = (int)cell.position.y + adjacentY;

                    if (IsValid(x, y))
                        Flood(state[x, y]);
                }
            }
        }
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y)) { return state[x, y]; }
        else { return new Cell(); }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Call this function directly from your PAUSE BUTTON
    public void PauseGame()
    {
        isGamePaused = true;
        
        // Disable touch input completely
        if (touchPress != null)
            touchPress.Disable();
            
        Debug.Log("Game Paused - Touch input disabled");
    }
    
    // Call this function directly from your RESUME BUTTON  
    public void ResumeGame()
    {
        isGamePaused = false;
        
        // Re-enable touch input
        if (touchPress != null)
            touchPress.Enable();
            
        Debug.Log("Game Resumed - Touch input enabled");
    }
    
    // Generic method for other scripts if needed
    public void SetPauseState(bool paused)
    {
        if (paused)
            PauseGame();
        else
            ResumeGame();
    }
}
