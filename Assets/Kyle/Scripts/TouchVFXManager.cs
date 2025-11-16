using UnityEngine;
using UnityEngine.InputSystem;

public class TouchVFXManager : MonoBehaviour
{
    [SerializeField] private GameObject tapVFXPrefab;
    private Camera mainCam;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Mobile touch check
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
            SpawnVFX(pos);
        }

        // PC mouse check
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            SpawnVFX(pos);
        }
    }

    private void SpawnVFX(Vector2 screenPos)
    {
        if (mainCam == null)
            mainCam = Camera.main;

        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        Instantiate(tapVFXPrefab, worldPos, Quaternion.identity);
    }
}
