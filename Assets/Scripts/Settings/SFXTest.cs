using UnityEngine;

public class SFXTest : MonoBehaviour
{
    public AudioSource sfx;

    void Awake()
    {
        sfx = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sfx.Play();
        }
    }
}
