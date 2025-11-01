using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : MonoBehaviour
{
    public RectTransform bulletPrefab;
    public RectTransform track;
    public RectTransform firePoint;
    public float shootInterval = 0.4f;
    public float bulletSpeed = 1400f;
    public Sprite defaultSprite, duoSprite, quadSprite;

    float shootTimer;
    Image image;
    int level = 0; // 0 = default, 1 = duo, 2 = quad

    void Awake()
    {
        image = GetComponent<Image>();
        UpdateSprite();
    }

    void Update()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        int count = level == 0 ? 1 : (level == 1 ? 2 : 4);
        float spread = 20f;

        for (int i = 0; i < count; i++)
        {
            
            var b = Instantiate(bulletPrefab, track);
            var brt = b.GetComponent<RectTransform>();

            // where thebullet comes from
            brt.position = firePoint.position;

            // Multi shot sped? if not can change to just change fire speed
            float offset = (count == 1) ? 0 :
                           (count == 2) ? (i == 0 ? -spread / 2 : spread / 2) :
                                          (-spread + i * (spread / 2));

            brt.anchoredPosition += Vector2.right * offset;

            // Set speed
            b.GetComponent<Bullet>().speed = bulletSpeed;
        }
    }

    public void LevelUp()
    {
        level = Mathf.Clamp(level + 1, 0, 2);
        UpdateSprite();
    }

    void UpdateSprite()
    {
        image.sprite = level switch
        {
            0 => defaultSprite,
            1 => duoSprite,
            _ => quadSprite
        };
    }
}
