using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board instance;  // Singleton instance
    public int width;
    public int height;
    private BackgroundTile[,] allTiles;
    public GameObject tilePrefab;
    public GameObject[] dot; // Field for storing dot prefabs
    public GameObject[,] allDots; // 2D array for storing the dots on the board

    void Awake()
    {
        // Check if the instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate
        }
        else
        {
            instance = this;  // Set the instance to this
            DontDestroyOnLoad(gameObject);  // Prevent this object from being destroyed on scene change
        }
    }
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
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + ", " + j + " )";

                int dotToUse = Random.Range(0, dot.Length);

                GameObject dots = Instantiate(dot[dotToUse], tempPosition, Quaternion.identity);
                dots.GetComponent<Dot>().row = j;
                dots.GetComponent<Dot>().column = i;
                dots.transform.parent = this.transform;
                dots.name = "( " + i + ", " + j + " )";
                allDots[i, j] = dots;
            }
        }
    }
}
