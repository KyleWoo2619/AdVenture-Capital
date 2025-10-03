using Unity.VisualScripting;
using UnityEngine;

public class SlicePiece : MonoBehaviour
{
    private bool isDragging, isPlaced;

    Vector2 offset;
    Vector2 originalPos;

    [SerializeField] private SliceSlot slot;


    public void Init(SliceSlot slot)
    {
        this.slot = slot;
    }


    void Awake()
    {
        originalPos = transform.position;
    }

    void OnMouseDown()
    {
        isDragging = true;

        offset = GetMousePos() - (Vector2)transform.position;
    }

    void OnMouseUp()
    {
        if (Vector2.Distance(transform.position, slot.transform.position) < 3)
        {
            transform.position = slot.transform.position;
            isPlaced = true;
        }
        else
        {
            transform.position = originalPos;
            isDragging = false;
        }
        
    }

    void Update()
    {
        if (isPlaced) return;

        if (!isDragging)
            return;
        

        var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = mousePosition - offset;
    }


    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }

}


