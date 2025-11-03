using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class Obstacle : MonoBehaviour
{
    public float speed = 500f;
    public int health = 5;
    public TMP_Text healthText; // Drag TMP component here in Inspector
    
    [HideInInspector] public bool useUnscaledTime = false; // Set by spawner

    void Start()
    {
        UpdateHealthDisplay();
    }

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
        UpdateHealthDisplay();
        
        if (health <= 0)
            Destroy(gameObject);
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = health.ToString();
        }
    }
}
