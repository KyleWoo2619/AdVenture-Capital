using UnityEngine;

public class Board : MonoBehaviour
{

    public int width;
    public int height;
    private BackgroundTile[,] allTiles;
    public GameObject tilePrefab;

    void Start()
    {
        allTiles = new BackgroundTile[width, height];
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
                Instantiate(tilePrefab, tempPosition, Quaternion.identity);
            }
        }
    }

}
