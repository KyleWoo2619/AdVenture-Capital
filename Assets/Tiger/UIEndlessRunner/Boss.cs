using UnityEngine;
using System;

[RequireComponent(typeof(RectTransform))]
public class Boss : MonoBehaviour
{
    public static event Action<bool> OnBossDefeated; // bool parameter: true = player won, false = boss reached player
    
    public float speed = 1f;   // lol the way i added this first but i set it up in obstacle spawner so this is useless lol
    public int health = 60;      // needs lots of bullets
    
    [Header("Audio")]
    public AudioSource audioSource; // Drag AudioSource component here
    public AudioClip hitSound; // Sound when hit by bullet
    public AudioClip breakSound; // Sound when destroyed (health = 0)
    
    [HideInInspector] public bool useUnscaledTime = false; // Set by spawner

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
        
        if (health <= 0)
        {
            // Play break sound when destroyed
            if (audioSource != null && breakSound != null)
            {
                audioSource.PlayOneShot(breakSound);
            }
            TriggerDefeat(true); // Player won by destroying boss
        }
        else
        {
            // Play hit sound only if not destroyed
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }

    public void TriggerDefeat(bool playerWon)
    {
        OnBossDefeated?.Invoke(playerWon);
        Destroy(gameObject);
    }
}