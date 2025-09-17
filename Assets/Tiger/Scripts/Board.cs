using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    private BackgroundTile[,] allTiles;
    public GameObject tilePrefab;
    public GameObject[] dot; // Field for storing dot prefabs
    public GameObject[,] allDots; // 2D array for storing the dots on the board

    void Start()
    {
        allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    void Update()
    {
    }

    private void SetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j);

                // Instantiate the background tile
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform; // Makes the objects spawn as a child of this
                backgroundTile.name = "Tile " + i + "," + j;

                
                int dotToUse = Random.Range(0, dot.Length);
                GameObject newDot = Instantiate(dot[dotToUse], tempPosition, Quaternion.identity);
                newDot.transform.parent = this.transform;
                newDot.name = "(" + i + "," + j + ")";

             
                allDots[i, j] = newDot;
            }
        }
    }
}
