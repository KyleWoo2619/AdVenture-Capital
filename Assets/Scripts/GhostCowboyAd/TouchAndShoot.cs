using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TouchAndShoot : MonoBehaviour
{
    private PlayerInput playerInput;
    public InputAction shoot { get; private set; }
    public InputAction shootPos { get; private set; }

    private GameObject CanvasUI;
    GraphicRaycaster ui_raycaster;
    PointerEventData touch_data;
    List<RaycastResult> touch_result;

    public delegate void EnemyDeath();
    public static event EnemyDeath OnEnemyDeath;

    bool isDead;
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shoot = playerInput.actions.FindAction("Shoot");
        shootPos = playerInput.actions.FindAction("ShootPos");

        CanvasUI = transform.parent.gameObject;
        ui_raycaster = CanvasUI.GetComponent<GraphicRaycaster>();
        touch_data = new PointerEventData(EventSystem.current);
        touch_result = new List<RaycastResult>();

        isDead = false;
    }

    void OnEnable()
    {
        shoot.performed += OnShootPressed;
        shoot.Enable();
        shootPos.Enable();

        ShooterAdversary.OnPlayerDeath += SetPlayerDead;
        
    }

    void OnDisable()
    {
        shoot.performed -= OnShootPressed;
        shoot.Disable();
        shootPos.Disable();

        ShooterAdversary.OnPlayerDeath -= SetPlayerDead;
    }

    void OnShootPressed(InputAction.CallbackContext context)
    {

        touch_data.position = shootPos.ReadValue<Vector2>();
        touch_result.Clear();

        ui_raycaster.Raycast(touch_data, touch_result);

        foreach (RaycastResult result in touch_result)
        {
            GameObject ui_element = result.gameObject;
            Debug.Log(ui_element.name);

            if (ui_element.name == "Enemy" && !ShowdownCountdown.isCountingDown && !isDead)
            {
                OnEnemyDeath?.Invoke();
            }

        }

    }

    void SetPlayerDead()
    {
        isDead = true;
    }

}
