using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Bullet : MonoBehaviour
{
    public float speed = 1200f;
    [HideInInspector] public bool useUnscaledTime = false; // Set by shooter
    
    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        var rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.up * speed * deltaTime;
        if (rt.anchoredPosition.y > 1300f)
            Destroy(gameObject);
    }
}
