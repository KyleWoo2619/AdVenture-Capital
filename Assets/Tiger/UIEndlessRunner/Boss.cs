using UnityEngine;
using System;

[RequireComponent(typeof(RectTransform))]
public class Boss : MonoBehaviour
{
    public static event Action OnBossDefeated;
    
    public float speed = 1f;   // lol the way i added this first but i set it up in obstacle spawner so this is useless lol
    public int health = 60;      // needs lots of bullets
    
    [HideInInspector] public bool useUnscaledTime = false; // Set by spawner

    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        var rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.down * speed * deltaTime;
        if (rt.anchoredPosition.y < -1300f)
            Destroy(gameObject);
    }

    public void Hit()
    {
        health--;
        if (health <= 0)
        {
            OnBossDefeated?.Invoke();
            Destroy(gameObject);
        }
    }
}