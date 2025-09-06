using Unity.VisualScripting;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
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
        }
    }
}
