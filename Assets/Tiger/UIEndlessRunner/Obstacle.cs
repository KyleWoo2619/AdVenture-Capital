using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Obstacle : MonoBehaviour
{
    public float speed = 500f;
    int health = 5;

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
