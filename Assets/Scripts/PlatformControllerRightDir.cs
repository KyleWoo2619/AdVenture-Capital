using UnityEngine;

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
            Destroy(gameObject, 2);
        }
    }
}
