using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    InputAction jump;
    Rigidbody player_RB;
    [SerializeField] private float jumpValue;
    LayerMask groundLayer;

    //gravity settings
    

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
        if (Physics.Raycast(transform.position, Vector3.down, 1.2f, groundLayer))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.green);

            if (jump.WasPressedThisFrame())
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
