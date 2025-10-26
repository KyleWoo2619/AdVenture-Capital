using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TouchAndShoot : MonoBehaviour
{
    [Header("Character Sprites")]
    [SerializeField] private Sprite waitSprite;  // Default/waiting state
    [SerializeField] private Sprite fireSprite;  // When player shoots
    [SerializeField] private Sprite winSprite;   // When player wins
    [SerializeField] private Sprite loseSprite;  // When player loses

    [Header("Character Image")]
    [SerializeField] private Image characterImage; // The Image component to change sprites

    [Header("Input Settings")]
    [SerializeField] private float initialDisableTime = 3f; // Time to disable input at start

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // AudioSource for sound effects
    [SerializeField] private AudioClip shootSound; // Sound to play when shooting

    private PlayerInput playerInput;
    public InputAction shoot { get; private set; }
    public InputAction shootPos { get; private set; }

    private bool canShoot = false; // Controls if player can shoot

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
        canShoot = false; // Start with shooting disabled

        // Set default wait sprite
        SetCharacterSprite(waitSprite);

        // Start coroutine to enable shooting after delay
        StartCoroutine(EnableShootingAfterDelay());
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
        // Don't allow shooting if disabled or during instructions
        if (!canShoot)
        {
            Debug.Log("Shooting disabled - instructions still showing");
            return;
        }

        // Change to fire sprite when screen is touched and shooting is allowed
        SetCharacterSprite(fireSprite);

        // Play shoot sound effect
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        touch_data.position = shootPos.ReadValue<Vector2>();
        touch_result.Clear();

        ui_raycaster.Raycast(touch_data, touch_result);

        bool hitEnemy = false;

        foreach (RaycastResult result in touch_result)
        {
            GameObject ui_element = result.gameObject;
            Debug.Log(ui_element.name);

            if (ui_element.name == "Enemy" && !ShowdownCountdown.isCountingDown && !isDead)
            {
                OnEnemyDeath?.Invoke();
                hitEnemy = true;
                // Change to win sprite when player hits enemy
                SetCharacterSprite(winSprite);
            }
        }

        // If no enemy was hit, return to wait sprite after a brief moment
        if (!hitEnemy && !isDead)
        {
            Invoke(nameof(ReturnToWaitSprite), 0.2f);
        }
    }

    void SetPlayerDead()
    {
        isDead = true;
        // Change to lose sprite when player dies
        SetCharacterSprite(loseSprite);
    }

    // Helper method to set character sprite
    void SetCharacterSprite(Sprite sprite)
    {
        if (characterImage != null && sprite != null)
        {
            characterImage.sprite = sprite;
        }
    }

    // Helper method to return to wait sprite
    void ReturnToWaitSprite()
    {
        if (!isDead)
        {
            SetCharacterSprite(waitSprite);
        }
    }

    // Coroutine to enable shooting after initial delay
    System.Collections.IEnumerator EnableShootingAfterDelay()
    {
        Debug.Log("Shooting disabled for " + initialDisableTime + " seconds");
        yield return new WaitForSeconds(initialDisableTime);
        canShoot = true;
        Debug.Log("Shooting enabled!");
    }
}
