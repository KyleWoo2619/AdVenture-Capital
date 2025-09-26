using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PlatformController : MonoBehaviour
{
  //Controls a platform that moves in the 'left' direction
    [SerializeField] private float speed;
    private LayerMask ground;
    RaycastHit hit;
    Transform hitTransform;

    IObjectPool<PlatformController> _pool;


    public void SetPool(IObjectPool<PlatformController> pool) => _pool = pool;
    void Start()
    {
        ground = LayerMask.GetMask("Ground");
        speed = 3f;
        
    }

    void OnEnable()
    {
        speed = GameManager.instance.platformSpeed;


        //UnityEngine.Debug.Log(speed);
    }

    
    void Update()
    {
        transform.Translate(Vector3.left * speed*Time.deltaTime);
        
        
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
                    UnityEngine.Debug.Log("it has moved");
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

            UnityEngine.Debug.Log(GameManager.instance.score);
            DisplayScore.UpdateScore();
        }    
             
            
    }
}
