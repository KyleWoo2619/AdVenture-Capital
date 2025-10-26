using System.Collections;
using UnityEngine;

public class ShooterAdversary : MonoBehaviour
{
    [SerializeField] int shootDelay; // time after "draw" before shooting at player
    
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
        TimeToShootCoroutine = StartCoroutine(TimeToShootPlayer(shootDelay));
    }
    
    void NotShootPlayer()
    {
        isDead = true; // Mark enemy as dead
        
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
        Debug.Log("[ShooterAdversary] Enemy state reset for new game");
    }
}
