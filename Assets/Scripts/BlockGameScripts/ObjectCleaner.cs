using System.Collections;
using UnityEngine;

public class ObjectCleaner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ObjectCleanerE(3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            Debug.Log("Hit!");
            Destroy(other.gameObject);
        }
    }

    private IEnumerator ObjectCleanerE(float time)
    {
        yield return new WaitForSeconds(time);
        for(; ; )
        {
            transform.Translate(0, 0.5f, 0);
            yield return new WaitForSeconds(time);
        }
    }
}
