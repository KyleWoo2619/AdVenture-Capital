using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    InputAction jump;
    Rigidbody player_RB;
    [SerializeField] private float jumpValue;
    LayerMask groundLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource jumpOneShot; // Assign in Inspector

    [Header("Sprite Changing")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer; // Assign the sprite renderer component
    [SerializeField] private Sprite groundedSprite; // Sprite when on ground
    [SerializeField] private Sprite jumpingSprite;  // Sprite when in air

    private bool hasJumped = false;

    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        jump = InputSystem.actions.FindAction("Jump");
        player_RB = GetComponent<Rigidbody>();
        jumpValue = 9.5f;
    }

    void Update()
    {
        PlayerJump();
    }

    void PlayerJump()
    {
        // Prevent jumping while paused
        if (Time.timeScale == 0f) return;

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f, groundLayer);

        if (isGrounded)
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.green);

            // Reset jump flag when grounded
            hasJumped = false;

            // Change to grounded sprite
            if (playerSpriteRenderer != null && groundedSprite != null)
                playerSpriteRenderer.sprite = groundedSprite;

            if (jump.WasPressedThisFrame() && !hasJumped)
            {
                player_RB.AddForce(Vector3.up * jumpValue, ForceMode.Impulse);

                // Play jump sound only once per jump
                if (jumpOneShot != null)
                    jumpOneShot.Play();

                // Haptic feedback for jump
                MobileHaptics.ImpactMedium();

                hasJumped = true;

                // Change to jumping sprite immediately when jump starts
                if (playerSpriteRenderer != null && jumpingSprite != null)
                    playerSpriteRenderer.sprite = jumpingSprite;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);

            // Change to jumping sprite when in air
            if (playerSpriteRenderer != null && jumpingSprite != null)
                playerSpriteRenderer.sprite = jumpingSprite;
        }
    }
}
