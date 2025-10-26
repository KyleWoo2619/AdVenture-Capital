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
    
    Coroutine TimeToShootCoroutine;
    public delegate void PlayerDeath();
    public static event PlayerDeath OnPlayerDeath;

    void OnEnable()
    {
        ShowdownCountdown.OnDraw += ShootPlayer;
        TouchAndShoot.OnEnemyDeath += NotShootPlayer;
    }
    void OnDisable()
    {
        ShowdownCountdown.OnDraw -= ShootPlayer;
        TouchAndShoot.OnEnemyDeath += NotShootPlayer;
    }
    void Start()
    {
        shootDelay = Random.Range(1, 4);   
    }
    void ShootPlayer()
    {
        TimeToShootCoroutine = StartCoroutine(TimeToShootPlayer(shootDelay));
    }
    
    void NotShootPlayer()
    {
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
        
        StopCoroutine(TimeToShootCoroutine);
    }
    
    IEnumerator TimeToShootPlayer(int delay)
    {
        yield return new WaitForSeconds(delay);
        OnPlayerDeath?.Invoke();
    }
}
