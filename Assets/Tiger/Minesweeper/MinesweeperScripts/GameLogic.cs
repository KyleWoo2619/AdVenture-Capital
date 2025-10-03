using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class GameLogic : MonoBehaviour
{
    InputAction jump;

    public int width = 10;
  public int height = 10;
    
  public int mineCount = 10;

    private MineBoard Board;

    private Cell[,] state;

    private void OnEnable()
    {
        jump.Enable();
        
    }
    private void OnDisable()
    {
        jump.Disable();
    }
    private void Awake()
    {
        Board = GetComponentInChildren<MineBoard>();
    }

    private void Start()
    {
        jump = InputSystem.actions.FindAction("Jump");
        NewGame();
       
    }
    private void NewGame()
    {
        state = new Cell[width, height];

        GenerateCells();

        Camera.main.transform.position = new Vector3(width / 2f, height/ 2f, -10);
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

            while(state[x, y].type == Cell.Type.Mine) // if the cell already has a mine, find the next cell, if we reach the end of the row, go to the next row
            {
                x++;

                if(x >= width)
                {
                    x = 0;
                    y++;
                    if (y >= height)
                    {
                        y = 0;
                    }
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

                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x, y);

                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                cell.isRevealed = true;
                state[x, y] = cell;
                
            }

        }
    }    
    private int CountMines(int cellX , int cellY)
    {
        int count = 0;
        
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue; // skip the cell itself
                }
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

          

                if (GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void Update()
    {
        if (jump.WasPressedThisFrame() || Input.GetMouseButtonDown(1))
        {
            
            Flag();
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPosition.z = 0; 

        // Convert the world position to tile coordinates
        Vector3Int cellPosition = Board.tilemap.WorldToCell(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));

        // Get the cell at the converted position
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        // If the cell is invalid or revealed, don't flag it
        if (cell.type == Cell.Type.Invalid || cell.isRevealed)
        {
            return;
        }

        // Toggle the flagged state
        cell.isFlagged = !cell.isFlagged;
        state[cellPosition.x, cellPosition.y] = cell;
        Board.Draw(state);
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

}

