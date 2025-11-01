using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Bullet : MonoBehaviour
{
    public float speed = 1200f;
    void Update()
    {
        var rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.up * speed * Time.deltaTime;
        if (rt.anchoredPosition.y > 1300f)
            Destroy(gameObject);
    }
}
