using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlatformControllerRightDir : MonoBehaviour
{
    // a platform check for the platform below it
    // when a player lands on that platform, while there is a another platform below it, if that platform is x units away from the platform along the x axis
    // set that platform's x position equal to the one below it

    [SerializeField] private float speed;
    private LayerMask ground;
    RaycastHit hit;
    Transform hitTransform;
    

    

    void Start()
    {
        ground = LayerMask.GetMask("Ground");
        speed = 3f;
       
    }


    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3f, ground))
        {
            
            if (hit.transform.CompareTag("Platform"))
            {
                //Debug.Log("that is a platform");
                hitTransform = hit.transform;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            speed = 0;
            if (hitTransform == null)
            {
                GameManager.instance.AddtoScore();
            }

            if (hitTransform != null)
            {
                if (transform.position.x >= hitTransform.position.x && transform.position.x <= hitTransform.position.x + GameManager.instance.forgivenessValue || transform.position.x <= hitTransform.position.x && transform.position.x >= hitTransform.position.x - GameManager.instance.forgivenessValue)
                {
                    transform.position = new Vector3(hit.transform.position.x, transform.position.y, transform.position.z);
                    Debug.Log("it has moved");
                }

                if (transform.position.x == hitTransform.position.x)
                {

                    GameManager.instance.AddFourtoScore();
                }
                else
                {
                    GameManager.instance.AddtoScore();
                }
                
            }
            
             
             Debug.Log(GameManager.instance.score);
        }
    }

    

    
}
