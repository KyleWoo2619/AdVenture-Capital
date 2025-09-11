using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlatformControllerRightDir : MonoBehaviour
{
    // Controls a platform that moves in the 'right' direction

   [SerializeField] private float speed;
    void Start()
    {
        speed = 3f;
    }


    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
       
        
       
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            speed = 0;
            GameManager.instance.AddtoScore();
            Debug.Log("Your score is " + GameManager.instance.score);
           // Destroy(gameObject, 2);
        }
    }

    
}
