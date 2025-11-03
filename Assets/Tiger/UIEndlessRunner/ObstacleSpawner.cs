using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public RectTransform track;
    public RectTransform obstaclePrefab;
    public RectTransform powerUpPrefab;
    public RectTransform player;

    public RectTransform bossPrefab;

    public float spawnInterval = 0.8f;
    public float obstacleSpeed = 500f;
    public float powerUpChance = 0.2f;

    [Header("Boss")]
    public float bossSpawnTime = 20f;   // seconds
    public float bossSpeed = 250f;
    public int bossHealth = 60;
    public Vector2 bossSize = new Vector2(300, 300);

    public float roundDuration = 30f;

    float timer;
    float[] lanes;
    float elapsed;
    bool bossSpawned;
    bool useUnscaledTime = false; // For working during pause

    void Start()
    {
        float w = track.rect.width;
        lanes = new[] { -w / 3f, 0f, w / 3f };
    }

    public void SetUnscaledTimeMode(bool enabled)
    {
        useUnscaledTime = enabled;
    }

    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        
        elapsed += deltaTime;

        // stop normal spawning after round ends
        bool roundActive = elapsed < roundDuration;

        // timed boss spawn
        if (!bossSpawned && elapsed >= bossSpawnTime)
        {
            SpawnBoss();
            bossSpawned = true;
        }

        // normal spawns
        if (roundActive)
        {
            timer += deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                Spawn();
            }
        }

        // Get all active objects
        var bullets = track.GetComponentsInChildren<Bullet>();
        var obstacles = track.GetComponentsInChildren<Obstacle>();
        var powerUps = track.GetComponentsInChildren<PowerUp>();
        var bosses = track.GetComponentsInChildren<Boss>();

        // --- Bullet vs Obstacle ---
        foreach (var b in bullets)
        {
            var brt = (RectTransform)b.transform;
            foreach (var o in obstacles)
            {
                var ort = (RectTransform)o.transform;
                if (UIOverlap(brt, ort))
                {
                    var obstacle = o.GetComponent<Obstacle>();
                    o.Hit();
                    
                    // If obstacle is destroyed (health <= 0), add points
                    if (obstacle != null && obstacle.health <= 0)
                    {
                        if (ScoreManager.Instance != null)
                        {
                            ScoreManager.Instance.AddScore(5);
                        }
                    }
                    
                    Destroy(b.gameObject);
                    break;
                }
            }
        }

        // --- Bullet vs Boss ---
        foreach (var b in bullets)
        {
            var brt = (RectTransform)b.transform;
            foreach (var boss in bosses)
            {
                var brtBoss = (RectTransform)boss.transform;
                if (UIOverlap(brt, brtBoss))
                {
                    boss.Hit();
                    Destroy(b.gameObject);
                    break;
                }
            }
        }

        // --- Player vs PowerUp ---
        foreach (var pu in powerUps)
        {
            var purt = (RectTransform)pu.transform;
            if (UIOverlap((RectTransform)player, purt))
            {
                var powerUp = pu.GetComponent<PowerUp>();
                var shooter = player.GetComponent<PlayerShooter>();
                
                if (powerUp.type == PowerUpType.Multiply)
                {
                    shooter.LevelUp();
                }
                else // Addition type
                {
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(powerUp.scoreValue);
                    }
                    // Also boost bullet speed
                    shooter.BoostBulletSpeed();
                }
                
                Destroy(pu.gameObject);
            }
        }

        // --- Player vs Obstacle ---
        foreach (var o in obstacles)
        {
            var ort = (RectTransform)o.transform;
            if (UIOverlap((RectTransform)player, ort))
            {
                Debug.Log("HIT PLAYER! 💥");
                
                // Deduct points equal to obstacle's current health
                var obstacle = o.GetComponent<Obstacle>();
                if (obstacle != null && ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(-obstacle.health);
                }
                
                // Level down the player
                var shooter = player.GetComponent<PlayerShooter>();
                if (shooter != null)
                {
                    shooter.LevelDown();
                }
                
                Destroy(o.gameObject);
                //PLAY AD HERE
            }
        }

        // --- Player vs Boss ---
        foreach (var boss in bosses)
        {
            var brtBoss = (RectTransform)boss.transform;
            if (UIOverlap((RectTransform)player, brtBoss))
            {
                Debug.Log("HIT PLAYER (BOSS)! 💥");
                Destroy(boss.gameObject); // simple rule: remove boss on contact
                //PLAY AD HERE
            }
        }
    }

    void Spawn()
    {
        bool spawnPower = Random.value < powerUpChance;
        int lane = Random.Range(0, 3);
        
        // Check player level to determine if multiply powerup should spawn
        var shooter = player.GetComponent<PlayerShooter>();
        int playerLevel = shooter != null ? shooter.GetLevel() : 0;
        bool canSpawnMultiply = playerLevel < 2; // Not at Quad yet
        
        if (spawnPower)
        {
            // Spawn powerup
            var obj = Instantiate(powerUpPrefab, track);
            obj.anchoredPosition = new Vector2(lanes[lane], track.rect.height / 2 + 150f);
            
            var powerUp = obj.GetComponent<PowerUp>();
            powerUp.speed = obstacleSpeed * 0.8f;
            powerUp.useUnscaledTime = useUnscaledTime; // Pass unscaled time mode
            
            // Randomize type: if player can't level up, always spawn Addition
            PowerUpType type;
            if (canSpawnMultiply)
            {
                // 50/50 chance between Multiply and Addition
                type = Random.value < 0.5f ? PowerUpType.Multiply : PowerUpType.Addition;
            }
            else
            {
                // Player is at max level, only spawn Addition
                type = PowerUpType.Addition;
            }
            
            powerUp.SetType(type);
        }
        else
        {
            // Spawn obstacle
            var obj = Instantiate(obstaclePrefab, track);
            obj.anchoredPosition = new Vector2(lanes[lane], track.rect.height / 2 + 150f);
            var obstacle = obj.GetComponent<Obstacle>();
            obstacle.speed = obstacleSpeed;
            obstacle.useUnscaledTime = useUnscaledTime; // Pass unscaled time mode
        }
    }
    void SpawnBoss()
    {
        var bossRT = Instantiate(bossPrefab, track);
        bossRT.sizeDelta = bossSize;
        bossRT.anchoredPosition = new Vector2(0f, track.rect.height / 2 + 200f);

        var b = bossRT.GetComponent<Boss>();
        b.speed = bossSpeed;
        b.useUnscaledTime = useUnscaledTime; // Pass unscaled time mode
        b.GetType().GetField("health").SetValue(b, bossHealth); 
    }

    bool UIOverlap(RectTransform a, RectTransform b)
    {
        Vector3[] wa = new Vector3[4];
        Vector3[] wb = new Vector3[4];
        a.GetWorldCorners(wa);
        b.GetWorldCorners(wb);
        Rect ra = new Rect(wa[0], wa[2] - wa[0]);
        Rect rb = new Rect(wb[0], wb[2] - wb[0]);
        return ra.Overlaps(rb);
    }
}