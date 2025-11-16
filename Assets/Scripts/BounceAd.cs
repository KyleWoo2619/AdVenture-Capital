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

        

        GameObject point = new GameObject("EdgePoint", typeof(RectTransform));

        for (int i = -500; i < 600; i += 200) //points at top edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(i, hBound/2);
            topEdgePoints.Add(p);
            
        }

        for (int i = -500; i < 600; i += 200) //points at left edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(wBound/2, i);
            leftEdgePoints.Add(p);
        }

        for (int i = -500; i < 600; i += 200) //points at bottom edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(i, -hBound/2);
            bottomEdgePoints.Add(p);
        }
        
        for(int i = -500; i<600; i += 200) //points at right edge
        {
            GameObject p = Instantiate(point, canvas.transform);
            p.GetComponent<RectTransform>().anchoredPosition = new Vector2(-wBound/2, i);
            rightEdgePoints.Add(p);
        }

        
    }

    void Start()
    {
        StartCoroutine(SearchForPoint());
    }

    void Update()
    {
        
        adRectBorder.anchoredPosition = Vector2.MoveTowards(
            adRectBorder.anchoredPosition,
            pointToGoTo,
            speed * Time.deltaTime
        );
        
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
                    pointToGoTo = topEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;

                case 2: 
                    randPointNum = Random.Range(0, bottomEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = bottomEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;

                case 3: 
                    randPointNum = Random.Range(0, rightEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = rightEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;

                case 4:
                    randPointNum = Random.Range(0, leftEdgePoints.Count);
                    Debug.Log(randPointNum);
                    pointToGoTo = leftEdgePoints[randPointNum].GetComponent<RectTransform>().anchoredPosition;
                break;
                
            }
            yield return new WaitUntil(() => AtPoint() == true);
        }
        }
    
    bool AtPoint()
    {
        return (adRectBorder.anchoredPosition - pointToGoTo).sqrMagnitude < 0.1f; 
    }

    
}
 
 
 
