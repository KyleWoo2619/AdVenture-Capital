using System.Collections.Generic;
using UnityEngine;

public class BounceAd : MonoBehaviour
{
    private Transform adTransform;
    
    public Canvas canvas;
    private RectTransform rectBorder;
    private RectTransform adrectBorder;
    private Vector2 initialPos;

    public float wBound;
    public float hBound;

    [SerializeField] private List<GameObject> topEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> leftEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> bottomEdgePoints = new List<GameObject>();
    [SerializeField] private List<GameObject> rightEdgePoints = new List<GameObject>();


    void Awake()
    {
        adTransform = transform;
        rectBorder = canvas.GetComponent<RectTransform>();
        adrectBorder = GetComponent<RectTransform>();

        wBound = rectBorder.rect.width;
        hBound = rectBorder.rect.height;

        initialPos = transform.position;

        GameObject point = new GameObject();

        for (int i = 100; i < 1000; i += 100) //points at top edge
        {
            topEdgePoints.Add(Instantiate(point, new Vector3(i, hBound, 0), Quaternion.identity));
        }

        for (int i = 100; i < 2000; i += 100) //points at left edge
        {
            leftEdgePoints.Add(Instantiate(point, new Vector3(wBound, i, 0), Quaternion.identity));
        }

        for (int i = 100; i < 1000; i += 100)
        {
            bottomEdgePoints.Add(Instantiate(point, new Vector3(i, 0, 0), Quaternion.identity));
        }
        
        for(int i = 100; i<2000; i += 100)
        {
            rightEdgePoints.Add(Instantiate(point, new Vector3(0, i, 0), Quaternion.identity));
        }

        
    }
}
