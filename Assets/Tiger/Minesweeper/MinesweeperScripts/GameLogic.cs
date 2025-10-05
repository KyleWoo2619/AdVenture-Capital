using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameLogic : MonoBehaviour
{
    
    InputAction touchPress;  // New input action for touch events
    public int width = 10;
    public int height = 10;
    public int mineCount = 10;
    private MineBoard Board;
    private Cell[,] state;

    private void OnEnable()
    {
       
        touchPress.Enable();  // Enable the new touch action
    }

    private void OnDisable()
    {
    
        touchPress.Disable();  // Disable the new touch action
    }

    private void Awake()
    {
        Board = GetComponentInChildren<MineBoard>();
    }

    private void Start()
    {
       
        touchPress = InputSystem.actions.FindAction("TouchPress");  // Initialize the touch action
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
        
        if ( Input.GetMouseButtonDown(1)) { Flag(); }
        if (Input.GetMouseButtonDown(0)) { Reveal(); }

        // Handling the TouchPress input action for both tap and tap-and-hold
        if (touchPress.triggered)
        {
            if (touchPress.phase == InputActionPhase.Started)
            {
                // Detect tap (short press)
                Reveal();
            }
            else if (touchPress.phase == InputActionPhase.Performed)
            {
                // Detect tap-and-hold (long press)
                Flag();
            }
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
