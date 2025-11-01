using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PowerUp : MonoBehaviour
{
    public float speed = 400f;

    void Update()
    {
        var rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.down * speed * Time.deltaTime;
        if (rt.anchoredPosition.y < -1300f)
            Destroy(gameObject);
    }
}
