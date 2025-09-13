using UnityEngine;

public class PlayerKillers : MonoBehaviour
{
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.isDead = true;
            Debug.Log("Player is dead");
        }
    }
}
