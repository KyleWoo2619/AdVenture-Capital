using System.Collections;
using UnityEngine;

public class ShooterAdversary : MonoBehaviour
{
    [SerializeField] int shootDelay; // time after "draw" before shooting at player
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
        StopCoroutine(TimeToShootCoroutine);
    }
    
    IEnumerator TimeToShootPlayer(int delay)
    {
        yield return new WaitForSeconds(delay);
        OnPlayerDeath?.Invoke();
    }
}
