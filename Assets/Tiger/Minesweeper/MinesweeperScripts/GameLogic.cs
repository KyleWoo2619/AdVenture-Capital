using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
  public int width = 10;
  public int height = 10;
    
  public int mineCount = 10;

    private MineBoard Board;

    private Cell[,] state;
    private void Awake()
    {
        Board = GetComponentInChildren<MineBoard>();
    }

    private void Start()
    {
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
            state[x, y].isRevealed = true;
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

                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue; // skip out of bounds
                }

                if (state[x,y].type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }

}

