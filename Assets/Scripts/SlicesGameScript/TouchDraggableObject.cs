using UnityEngine;
using UnityEngine.InputSystem;

public class TouchDraggableObject2D : MonoBehaviour
{
    private Camera camera;
    private Vector2 offset;
    private PlayerInput playerInput;
    private InputAction touchPressed;
    private InputAction touchPos;

    private Transform selectedObject;
    private bool isDragging = false;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupSound; // Sound when touching/picking up object
    [SerializeField] private AudioSource dropSound;   // Sound when placing object down

    void Awake()
    {
        camera = Camera.main;

        playerInput = GetComponent<PlayerInput>();
        touchPressed = playerInput.actions["TouchPressed"];
        touchPos = playerInput.actions["TouchPos"];
    }

    void OnEnable()
    {
        touchPressed.performed += OnTouchPressed;
        touchPressed.canceled += OnTouchReleased;
        touchPressed.Enable();
        touchPos.Enable();
    }

    void OnDisable()
    {
        touchPressed.performed -= OnTouchPressed;
        touchPressed.canceled -= OnTouchReleased;
        touchPressed.Disable();
        touchPos.Disable();
    }

    void OnTouchPressed(InputAction.CallbackContext context)
    {
        // Don't process input if game is paused (during video ads)
        if (Time.timeScale <= 0f)
        {
            return;
        }

        Vector2 screenPos = touchPos.ReadValue<Vector2>();
        Vector2 worldPoint = camera.ScreenToWorldPoint(screenPos);

        // Use OverlapPoint to detect collider at the point
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint);

        if (hitCollider != null && (hitCollider.GetComponent<SlicePiece>() || hitCollider.GetComponent<DualSlices>() || hitCollider.GetComponent<TripleSlices>()))
        {
            selectedObject = hitCollider.transform;
            isDragging = true;

            // Play pickup sound when object is touched/selected
            if (pickupSound != null && pickupSound.clip != null)
                pickupSound.PlayOneShot(pickupSound.clip);
            
            // Medium haptic when slice is touched
            MobileHaptics.ImpactMedium();
        }
    }

    void OnTouchReleased(InputAction.CallbackContext context)
    {
        // Don't process input if game is paused (during video ads)
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (selectedObject != null)
        {
            // Play drop sound when object is placed down
            if (dropSound != null && dropSound.clip != null)
                dropSound.PlayOneShot(dropSound.clip);

            if (selectedObject.TryGetComponent<SlicePiece>(out var slice))
            {
                slice.SetSlice();
            }
            else if (selectedObject.TryGetComponent<TripleSlices>(out var tripleSlice))
            {
                tripleSlice.SetTripleSlice();
            }
            else if (selectedObject.TryGetComponent<DualSlices>(out var dualSlice))
            {
                dualSlice.SetDualSlice();
            }
        }
        
        isDragging = false;
        selectedObject = null;
    }

    void Update()
    {
        // Stop dragging if game gets paused during drag
        if (Time.timeScale <= 0f && isDragging)
        {
            isDragging = false;
            selectedObject = null;
            return;
        }

        if (isDragging && selectedObject != null)
        {
            Vector2 screenPos = touchPos.ReadValue<Vector2>();
            Vector2 worldPoint = camera.ScreenToWorldPoint(screenPos);
            //offset = worldPoint - (Vector2) selectedObject.position;
            selectedObject.position = new Vector2(worldPoint.x, worldPoint.y);
        }
    }
}
