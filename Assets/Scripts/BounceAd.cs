using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class BounceAd : MonoBehaviour
{
    
    public Canvas canvas;
    private RectTransform rectBorder;
    private RectTransform adRectBorder;
    private Vector2 pointToGoTo;
    

    public float wBound;
    public float hBound;
    public float speed = 240;
    int lastListNum = -1;

    [SerializeField] private List<GameObject> topEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> leftEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> bottomEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> rightEdgePoints = new List<GameObject>();

    private int randListNum;
    private int randPointNum;
    void Awake()
    {
       
        rectBorder = canvas.GetComponent<RectTransform>();
        adRectBorder = GetComponent<RectTransform>();

        wBound = rectBorder.rect.width;
        hBound = rectBorder.rect.height;

        

        GameObject point = new GameObject();

        for (int i = 100; i < 1000; i += 100) //points at top edge
        {
            topEdgePoints.Add(Instantiate(point, new Vector3(i, hBound, 0), Quaternion.identity));
        }

        for (int i = 100; i < 2000; i += 100) //points at left edge
        {
            leftEdgePoints.Add(Instantiate(point, new Vector3(wBound, i, 0), Quaternion.identity));
        }

        for (int i = 100; i < 1000; i += 100) //points at bottom edge
        {
            bottomEdgePoints.Add(Instantiate(point, new Vector3(i, 0, 0), Quaternion.identity));
        }
        
        for(int i = 100; i<2000; i += 100) //points at right edge
        {
            rightEdgePoints.Add(Instantiate(point, new Vector3(0, i, 0), Quaternion.identity));
        }

        
    }

    void Start()
    {
        StartCoroutine(SearchForPoint());
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, pointToGoTo, speed*Time.deltaTime);
    }

     IEnumerator SearchForPoint()
     {
        for(;;){
            
            int newListNum;
            do
            {
                newListNum = Random.Range(1, 5);  
            }
            while (newListNum == lastListNum);

            randListNum = newListNum;
            lastListNum = randListNum;

            Debug.Log($"the random list number is {randListNum}");

            switch(randListNum)
            {
                case 1: //pick a random point in this list, same for all of them
                    randPointNum = Random.Range(0, topEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = topEdgePoints[randPointNum].transform.position;
                break;

                case 2: 
                    randPointNum = Random.Range(0, bottomEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = bottomEdgePoints[randPointNum].transform.position;
                break;

                case 3: 
                    randPointNum = Random.Range(0, rightEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = rightEdgePoints[randPointNum].transform.position;
                break;

                case 4:
                    randPointNum = Random.Range(0, leftEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = leftEdgePoints[randPointNum].transform.position;
                break;
                
            }
            yield return new WaitUntil(() => AtPoint() == true);
        }
        }
    
    bool AtPoint()
    {
        if((Vector2) transform.position == pointToGoTo) return true;

        return false;
    }

    
}
 
 
 
