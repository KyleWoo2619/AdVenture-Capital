using UnityEngine;

public class BackgroundTile : MonoBehaviour
{

    public GameObject[] dots;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Initialize()
    {
        int dotTouse = Random.Range(0, dots.Length);
        GameObject dot = Instantiate(dots[dotTouse], transform.position, Quaternion.identity);
        dot.transform.parent = this.transform;
        dot.name = this.gameObject.name;
    }
   
}
