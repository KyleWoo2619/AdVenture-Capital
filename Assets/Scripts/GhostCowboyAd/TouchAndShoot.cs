using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TouchAndShoot : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction shoot;
    private InputAction shootPos;

    public GameObject CanvasUI;
    GraphicRaycaster ui_raycaster;
    PointerEventData touch_data;
    List<RaycastResult> touch_result;
    
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shoot = playerInput.actions.FindAction("Shoot");
        shootPos = playerInput.actions.FindAction("ShootPos");

        CanvasUI = transform.parent.gameObject;
        ui_raycaster = CanvasUI.GetComponent<GraphicRaycaster>();
        touch_data = new PointerEventData(EventSystem.current);
        touch_result = new List<RaycastResult>();
    }

    void OnEnable()
    {
        shoot.performed += OnShootPressed;
        shoot.Enable();
        shootPos.Enable();
    }

    void OnDisable()
    {
        shoot.performed -= OnShootPressed;
        shoot.Disable();
        shootPos.Disable();
    }
    
    void OnShootPressed(InputAction.CallbackContext context)
    {
        touch_data.position = shootPos.ReadValue<Vector2>();
        touch_result.Clear();

        ui_raycaster.Raycast(touch_data, touch_result);

        foreach(RaycastResult result in touch_result)
        {
            GameObject ui_element = result.gameObject;
            Debug.Log(ui_element.name);
        }
    }
    
    
    
    
    
}
