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

    InputAction touchPress;  // New input action for touch events
    public int width = 10;
    public int height = 10;
    public int mineCount = 10;
    private MineBoard Board;
    private Cell[,] state;

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
        // Desktop input
        if (Input.GetMouseButtonDown(1)) { Flag(); }
        if (Input.GetMouseButtonDown(0)) { Reveal(); }

        // Mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                // Start timing for long press
                StartCoroutine(HandleTouch(touch.position));
            }
        }
    }

    private IEnumerator HandleTouch(Vector2 touchPosition)
    {
        float holdTime = 0f;
        float longPressThreshold = 0.6f; // 0.6 seconds for long press
        bool actionExecuted = false;

        while (Input.touchCount > 0 && Input.GetTouch(0).phase != UnityEngine.TouchPhase.Ended)
        {
            holdTime += Time.deltaTime;

            // If long press threshold reached, execute reveal immediately
            if (holdTime >= longPressThreshold && !actionExecuted)
            {
                RevealTouch(touchPosition); // Long press = reveal with touch position
                actionExecuted = true;
                yield break; // Exit the coroutine immediately
            }

            yield return null;
        }

        // If touch ended without long press, flag the cell
        if (!actionExecuted)
        {
            FlagTouch(touchPosition); // Short tap = flag with touch position
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPosition.z = 0; // Convert to 2D space
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed) { return; }
        cell.isFlagged = !cell.isFlagged;  // Toggle flagged state
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private void FlagTouch(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPosition.z = 0; // Convert to 2D space
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed) { return; }
        cell.isFlagged = !cell.isFlagged;  // Toggle flagged state
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPosition.z = 0;
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed || cell.isFlagged) { return; }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Debug.Log("Game Over!");
                Explode(cell);
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
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPosition.z = 0;
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.isRevealed || cell.isFlagged) { return; }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Debug.Log("Game Over!");
                Explode(cell);
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

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type != Cell.Type.Mine && !cell.isRevealed) { return; }
            }
        }
        Debug.Log("You Win!");
    }

    private void Flood(Cell cell)
    {
        cell.isRevealed = true;
        state[(int)cell.position.x, (int)cell.position.y] = cell;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) { continue; }
                int x = (int)cell.position.x + adjacentX;
                int y = (int)cell.position.y + adjacentY;
                Cell adjacentCell = GetCell(x, y);

                if (adjacentCell.type == Cell.Type.Invalid || adjacentCell.isRevealed || adjacentCell.isFlagged) { continue; }
                if (adjacentCell.type == Cell.Type.Empty)
                {
                    Flood(adjacentCell);
                }
                else
                {
                    adjacentCell.isRevealed = true;
                    state[x, y] = adjacentCell;
                }
            }
        }
    }

    private void Explode(Cell cell)
    {
        cell.isRevealed = true;
        state[(int)cell.position.x, (int)cell.position.y] = cell;

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
}
