using UnityEngine;

public class Board : MonoBehaviour
{

    public int width;
    public int height;
    private BackgroundTile[,] allTiles;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject[,] allDots;
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
                GameObject BackgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                BackgroundTile.transform.parent = this.transform; //makes the objects spawn a child of this
                BackgroundTile.name = "Tile " + i + "," + j + ")";
                int dotToUse = Random.Range(0, dots.Length);
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";

            }
        }
    }

}
