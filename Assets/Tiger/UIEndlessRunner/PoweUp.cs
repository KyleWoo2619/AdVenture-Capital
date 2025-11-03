using UnityEngine;
using UnityEngine.UI;

public enum PowerUpType
{
    Multiply,  // Levels up player shooting (duo/quad)
    Addition   // Adds score points
}

[RequireComponent(typeof(RectTransform))]
public class PowerUp : MonoBehaviour
{
    public float speed = 400f;
    public PowerUpType type = PowerUpType.Multiply;
    public int scoreValue = 100; // Only used for Addition type
    public Sprite multiplySprite; // Sprite for multiply powerup
    public Sprite additionSprite; // Sprite for addition powerup

    [HideInInspector] public bool useUnscaledTime = false; // Set by spawner
    
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
        UpdateSprite();
    }

    public void SetType(PowerUpType newType)
    {
        type = newType;
        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (image == null) return;
        
        image.sprite = type == PowerUpType.Multiply ? multiplySprite : additionSprite;
    }

    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        var rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.down * speed * deltaTime;
        if (rt.anchoredPosition.y < -1300f)
            Destroy(gameObject);
    }
}
