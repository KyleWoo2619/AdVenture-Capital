using UnityEngine;

public class ArmTapVFX : MonoBehaviour
{
    [Header("References")]
    public VFXManager vfxController; 

    [Header("Settings")]
    public float interval = 7f; // Trigger every 7 seconds

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            Debug.Log("[AutoJump] Interval reached (" + interval + "s). Spawning VFX for jump.");

            if (vfxController != null)
                vfxController.SpawnVFX(); // This will move VFX and trigger jump on arrival
            else
                Debug.LogWarning("[AutoJump] VFX Controller not assigned!");

            timer = 0f;
        }
    }
}
