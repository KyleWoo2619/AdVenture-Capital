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

    

    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        jump = InputSystem.actions.FindAction("Jump");
        player_RB = GetComponent<Rigidbody>();
        jumpValue = 1.3f;
        
    }

    void FixedUpdate()
    {
        PlayerJump();
       
        
    }

    void PlayerJump()
    {
        if (Physics.Raycast(transform.position, Vector3.down, jumpValue, groundLayer))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.green);

            if (jump.IsPressed())
            {
                player_RB.AddForce(Vector3.up * jumpValue, ForceMode.Impulse);


            }
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);

        }
    }

    


}
