using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    InputAction jump;
    Rigidbody player_RB;
    [SerializeField] private float jumpValue;
    [SerializeField] private float playerGravity;
    LayerMask groundLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource jumpOneShot; // Assign in Inspector

    private bool hasJumped = false;

    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        jump = InputSystem.actions.FindAction("Jump");
        player_RB = GetComponent<Rigidbody>();
        jumpValue = 9.5f;

        Physics.gravity = new Vector3(0, playerGravity, 0); //affects all rigid bodies in scene
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

            if (jump.WasPressedThisFrame() && !hasJumped)
            {
                player_RB.AddForce(Vector3.up * jumpValue, ForceMode.Impulse);

                // Play jump sound only once per jump
                if (jumpOneShot != null)
                    jumpOneShot.Play();

                hasJumped = true;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);
        }
    }
}
