using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : MonoBehaviour
{
    public RectTransform bulletPrefab;
    public RectTransform track;
    public RectTransform firePoint;
    public float shootInterval = 0.4f;
    public float bulletSpeed = 1400f;
    public float spread = 50f;
    public float speedBoostPerPowerup = 50f; // Speed added per Addition powerup
    public Sprite defaultSprite, duoSprite, quadSprite;
    
    [Header("Audio")]
    public AudioSource audioSource; // Drag AudioSource component here
    public AudioClip fireSound; // Sound when shooting

    float shootTimer;
    Image image;
    int level = 0; // 0 = default, 1 = duo, 2 = quad
    float currentBulletSpeed; // Actual bullet speed including boosts
    bool useUnscaledTime = false; // For working during pause
    bool canShoot = true; // Control whether shooting is allowed

    void Awake()
    {
        image = GetComponent<Image>();
        currentBulletSpeed = bulletSpeed; // Initialize with base speed
        UpdateSprite();
    }

    public void SetUnscaledTimeMode(bool enabled)
    {
        useUnscaledTime = enabled;
    }

    public void StopShooting()
    {
        canShoot = false;
    }

    public void StartShooting()
    {
        canShoot = true;
    }

    void Update()
    {
        if (!canShoot) return; // Don't shoot if disabled
        
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        shootTimer += deltaTime;
        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        int count = level == 0 ? 1 : (level == 1 ? 2 : 4);

        // Play fire sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Play haptic feedback based on bullet count
        if (count == 1)
        {
            MobileHaptics.Tap(); // Light tap for single shot
        }
        else if (count == 2)
        {
            MobileHaptics.ImpactMedium(); // Medium for duo shot
        }
        else // count == 4
        {
            MobileHaptics.ImpactHeavy(); // Heavy for quad shot
        }

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

            // Set speed (use current boosted speed)
            var bullet = b.GetComponent<Bullet>();
            bullet.speed = currentBulletSpeed;
            bullet.useUnscaledTime = useUnscaledTime; // Pass unscaled time mode to bullet
        }
    }

    public void LevelUp()
    {
        level = Mathf.Clamp(level + 1, 0, 2);
        UpdateSprite();
    }

    public void LevelDown()
    {
        level = Mathf.Clamp(level - 1, 0, 2);
        UpdateSprite();
    }

    public int GetLevel()
    {
        return level;
    }

    public void BoostBulletSpeed()
    {
        currentBulletSpeed += speedBoostPerPowerup;
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
