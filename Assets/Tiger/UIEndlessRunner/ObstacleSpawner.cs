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

    void Start()
    {
        float w = track.rect.width;
        lanes = new[] { -w / 3f, 0f, w / 3f };
    }

    void Update()
    {
        elapsed += Time.deltaTime;

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
            timer += Time.deltaTime;
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
                    o.Hit();
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
                player.GetComponent<PlayerShooter>().LevelUp();
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
        var prefab = spawnPower ? powerUpPrefab : obstaclePrefab;

        var obj = Instantiate(prefab, track);
        obj.anchoredPosition = new Vector2(lanes[lane], track.rect.height / 2 + 150f);

        if (spawnPower)
            obj.GetComponent<PowerUp>().speed = obstacleSpeed * 0.8f;
        else
            obj.GetComponent<Obstacle>().speed = obstacleSpeed;
    }
    void SpawnBoss()
    {
        var bossRT = Instantiate(bossPrefab, track);
        bossRT.sizeDelta = bossSize;
        bossRT.anchoredPosition = new Vector2(0f, track.rect.height / 2 + 200f);

        var b = bossRT.GetComponent<Boss>();
        b.speed = bossSpeed;
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