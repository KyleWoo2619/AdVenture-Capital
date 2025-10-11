using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    public Vector2 firstPosition;
    private Vector2 secondPosition;
    public Vector2 moveDirection;
    public bool isControlling = false;

    void Update()
    {
        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            isControlling = true;
            firstPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            isControlling = false;
            secondPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleSwipe(secondPosition - firstPosition);
        }

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                isControlling = true;
                firstPosition = touchPos;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isControlling = false;
                secondPosition = touchPos;
                HandleSwipe(secondPosition - firstPosition);
            }
        }
    }

    private void HandleSwipe(Vector2 swipe)
    {
        float minSwipeDist = 0.2f; // Adjust as needed
        if (swipe.magnitude >= minSwipeDist)
        {
            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                moveDirection = swipe.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                moveDirection = swipe.y > 0 ? Vector2.up : Vector2.down;
            }
            // Use moveDirection to trigger moves
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }
}