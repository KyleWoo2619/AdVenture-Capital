using UnityEngine;
using UnityEngine.UI;

public class LaneRunner : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform track;      // The UI area defining lane width
    public RectTransform player;     // This object�s RectTransform
    public RectTransform safeBottom; // Optional invisible line to clamp Y

    [Header("Lanes")]
    public int currentLane = 1;      // 0=Left, 1=Mid, 2=Right
    public float laneMoveDuration = 0.12f;
    public float bottomY = 150f;     // player�s Y on the track

    [Header("Input")]
    public bool allowKeyboard = true;
    public bool allowSwipe = true;
    public float swipeMinDistance = 50f;

    Vector2[] laneAnchors;           // x positions per lane
    float t;                         // lerp timer
    Vector2 startPos, targetPos;

    Vector2 swipeStartPos;
    bool swiping;
    bool useUnscaledTime = false;    // For working during pause

    void Awake()
    {
        // Precompute 3 lane x positions inside the track rect
        float w = track.rect.width;
        float leftX = -w / 3f;   // visually nice spacing
        float midX = 0f;
        float rightX = w / 3f;
        laneAnchors = new[] { new Vector2(leftX, bottomY), new Vector2(midX, bottomY), new Vector2(rightX, bottomY) };

        // Snap player to starting lane
        currentLane = Mathf.Clamp(currentLane, 0, 2);
        player.anchoredPosition = laneAnchors[currentLane];
        startPos = targetPos = player.anchoredPosition;
    }

    public void SetUnscaledTimeMode(bool enabled)
    {
        useUnscaledTime = enabled;
    }

    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        
        HandleInput();
        // Smooth lane transition
        if (t < laneMoveDuration)
        {
            t += deltaTime;
            float a = Mathf.Clamp01(t / laneMoveDuration);
            player.anchoredPosition = Vector2.Lerp(startPos, targetPos, a);
        }
    }

    void HandleInput()
    {
        // Keyboard / Buttons
        if (allowKeyboard)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                MoveLane(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                MoveLane(+1);
        }

        // Swipe (horizontal)
        if (!allowSwipe) return;

        if (Input.touchCount > 0)
        {
            Touch t0 = Input.GetTouch(0);
            if (t0.phase == TouchPhase.Began)
            {
                swiping = true;
                swipeStartPos = t0.position;
            }
            else if (t0.phase == TouchPhase.Ended && swiping)
            {
                float dx = t0.position.x - swipeStartPos.x;
                if (Mathf.Abs(dx) >= swipeMinDistance)
                    MoveLane(dx > 0 ? +1 : -1);
                swiping = false;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            swiping = true;
            swipeStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) && swiping)
        {
            float dx = ((Vector2)Input.mousePosition).x - swipeStartPos.x;
            if (Mathf.Abs(dx) >= swipeMinDistance)
                MoveLane(dx > 0 ? +1 : -1);
            swiping = false;
        }
    }

    public void MoveLane(int dir)
    {
        int next = Mathf.Clamp(currentLane + dir, 0, 2);
        if (next == currentLane) return;

        currentLane = next;
        startPos = player.anchoredPosition;
        targetPos = new Vector2(laneAnchors[currentLane].x, bottomY);
        t = 0f;
    }
}
