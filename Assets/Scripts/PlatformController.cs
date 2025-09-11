using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
  //Controls a platform that moves in the 'left' direction
    [SerializeField] private float speed;
    void Start()
    {
        speed = 3f;
    }

    
    void Update()
    {
        transform.Translate(Vector3.left * speed*Time.deltaTime);
        
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            speed = 0;
            GameManager.instance.AddtoScore();
           UnityEngine.Debug.Log("Your score is " + GameManager.instance.score);
            // Destroy(gameObject, 2);
        }
    }
}
