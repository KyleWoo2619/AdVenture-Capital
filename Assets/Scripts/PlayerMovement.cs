using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    /*
     * player needs rigid body to adhere to physics
     * add impulse force so that player jumps , combine with input
     * jump only when player is on 'ground'
     */

    InputAction jump;
    Rigidbody playerRB;
    LayerMask GroundMask;
    
   [SerializeField] float jumpHeight;

    private void Awake()
    {
        GroundMask = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        jump = InputSystem.actions.FindAction("Jump");
        playerRB = GetComponent<Rigidbody>();
        jumpHeight = 1f;
       
    }

    
    void FixedUpdate()
    {
        //RaycastHit hit;
       
        if(Physics.Raycast(transform.position, Vector3.down, jumpHeight, GroundMask))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.blue);
            //Debug.Log("Did Hit");
            if (jump.IsPressed())
            {
                playerRB.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            }

        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.white);
            //Debug.Log("Didn't Hit");
        }
        


    }
}
