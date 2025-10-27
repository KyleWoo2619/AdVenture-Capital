using System.Collections;
using UnityEngine;

public class ShooterAdversary : MonoBehaviour
{
    [SerializeField] int shootDelay; // time after "draw" before shooting at player
    
    [Header("Enemy Sprites")]
    [SerializeField] private Sprite waitSprite;  // Default/waiting state
    [SerializeField] private Sprite fireSprite;  // When enemy shoots
    [SerializeField] private Sprite shotSprite;  // When enemy dies
    [SerializeField] private Sprite winSprite;   // When enemy wins
    
    [Header("Enemy Image")]
    [SerializeField] private UnityEngine.UI.Image enemyImage; // The Image component to change sprites
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // AudioSource for sound effects
    [SerializeField] private AudioClip deathSound; // Sound to play when enemy dies
    
    [Header("Face Management")]
    [SerializeField] private RandomEnemyFace enemyFaceController; // Reference to the face controller
    
    [Header("Interactive Ad Settings")]
    [SerializeField] private bool useUnscaledTime = false; // Set to true when running as interactive ad
    
    Coroutine TimeToShootCoroutine;
    private bool isDead = false; // Track if enemy is dead
    public delegate void PlayerDeath();
    public static event PlayerDeath OnPlayerDeath;

    void Start()
    {
        // Set default wait sprite
        SetEnemySprite(waitSprite);
    }

    void OnEnable()
    {
        ShowdownCountdown.OnDraw += ShootPlayer;
        TouchAndShoot.OnEnemyDeath += NotShootPlayer;
        shootDelay = Random.Range(1, 4);
    }
    
    void OnDisable()
    {
        ShowdownCountdown.OnDraw -= ShootPlayer;
        TouchAndShoot.OnEnemyDeath -= NotShootPlayer;
    }
    
    void ShootPlayer()
    {
        // Change to fire sprite when enemy shoots
        SetEnemySprite(fireSprite);
        TimeToShootCoroutine = StartCoroutine(TimeToShootPlayer(shootDelay));
    }
    
    void NotShootPlayer()
    {
        isDead = true; // Mark enemy as dead
        
        // Change to shot sprite when enemy dies
        SetEnemySprite(shotSprite);
        
        // Play death sound when enemy is killed
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Switch to death face when enemy dies
        if (enemyFaceController != null)
        {
            enemyFaceController.ShowDeathFace();
        }
        
        if (TimeToShootCoroutine != null)
        {
            StopCoroutine(TimeToShootCoroutine);
            TimeToShootCoroutine = null;
        }
    }
    
    IEnumerator TimeToShootPlayer(int delay)
    {
        // Use unscaled time if running as interactive ad (game paused)
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return new WaitForSeconds(delay);
            
        // Only shoot if enemy is still alive
        if (!isDead)
        {
            OnPlayerDeath?.Invoke();
            // After shooting (winning), wait 1 second then show win sprite
            StartCoroutine(ShowWinSpriteAfterDelay(1f));
        }
    }
    
    /// <summary>
    /// Enable unscaled time mode for interactive ads (called by CowboyGameWrapper)
    /// </summary>
    public void SetUnscaledTimeMode(bool enabled)
    {
        useUnscaledTime = enabled;
        Debug.Log($"[ShooterAdversary] Unscaled time mode: {enabled}");
    }
    
    /// <summary>
    /// Reset enemy state for new game (called by CowboyGameWrapper)
    /// </summary>
    public void ResetEnemyState()
    {
        isDead = false;
        if (TimeToShootCoroutine != null)
        {
            StopCoroutine(TimeToShootCoroutine);
            TimeToShootCoroutine = null;
        }
        // Reset to wait sprite
        SetEnemySprite(waitSprite);
        Debug.Log("[ShooterAdversary] Enemy state reset for new game");
    }

    // Helper method to set enemy sprite
    void SetEnemySprite(Sprite sprite)
    {
        if (enemyImage != null && sprite != null)
        {
            enemyImage.sprite = sprite;
        }
    }

    // Coroutine to show win sprite after delay
    IEnumerator ShowWinSpriteAfterDelay(float seconds)
    {
        // Use unscaled time if running as interactive ad (game paused)
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
            
        // Only show win sprite if enemy is still alive
        if (!isDead)
        {
            SetEnemySprite(winSprite);
        }
    }
}
