using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Boss : MonoBehaviour
{
    public float speed = 1f;   // lol the way i added this first but i set it up in obstacle spawner so this is useless lol
    public int health = 60;      // needs lots of bullets

    void Update()
    {
        var rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.down * speed * Time.deltaTime;
        if (rt.anchoredPosition.y < -1300f)
            Destroy(gameObject);
    }

    public void Hit()
    {
        health--;
        if (health <= 0)
            Destroy(gameObject);
    }
}