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
    [SerializeField] private bool useUnscaledTime = false; // Set to true when running as interactive ad

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

        var canvas = GetComponentInParent<Canvas>();
        ui_raycaster = canvas ? canvas.GetComponent<GraphicRaycaster>() : null;

        touch_data = new PointerEventData(EventSystem.current);
        touch_result = new List<RaycastResult>();

        InitializeGame();
    }

    void InitializeGame()
    {
        isDead = false;
        canShoot = false; // Start with shooting disabled

        // Set default wait sprite
        SetCharacterSprite(waitSprite);

        // Start coroutine to enable shooting after delay
        StartCoroutine(EnableShootingAfterDelay());
        
        Debug.Log("[TouchAndShoot] Game initialized, starting disable timer");
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
        // If player has cheated or is dead, disable all shooting
        if (ShowdownCountdown.hasCheated || isDead)
        {
            return;
        }
        
        // Always show the fire pose briefly
        SetCharacterSprite(fireSprite);
        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);

        // If we're not allowed to shoot yet, restore sprite and let ShowdownCountdown handle cheat detection
        if (!canShoot)
        {
            Invoke(nameof(ReturnToWaitSprite), 0.2f);
            return;
        }

        touch_data.position = shootPos.ReadValue<Vector2>();
        touch_result.Clear();

        if (ui_raycaster == null)
        {
            // fail-safe: treat as miss, restore wait pose
            Invoke(nameof(ReturnToWaitSprite), 0.2f);
            return;
        }

        ui_raycaster.Raycast(touch_data, touch_result);

        bool hitEnemy = false;

        foreach (RaycastResult result in touch_result)
        {
            GameObject ui_element = result.gameObject;
            Debug.Log(ui_element.name);

            if (ui_element.name == "Enemy" && !isDead)
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
        
        // Use unscaled time if running as interactive ad (game paused)
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(initialDisableTime);
        else
            yield return new WaitForSeconds(initialDisableTime);
            
        canShoot = true;
        Debug.Log("Shooting enabled!");
    }

    /// <summary>
    /// Enable unscaled time mode for interactive ads (called by CowboyGameWrapper)
    /// </summary>
    public void SetUnscaledTimeMode(bool enabled)
    {
        useUnscaledTime = enabled;
        Debug.Log($"[TouchAndShoot] Unscaled time mode: {enabled}");
    }

    /// <summary>
    /// Restart the game (called by CowboyGameWrapper when starting interactive ad)
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[TouchAndShoot] Restarting game for interactive ad");
        
        // Stop any running coroutines
        StopAllCoroutines();
        
        // Reset and restart the game
        InitializeGame();
    }
}
