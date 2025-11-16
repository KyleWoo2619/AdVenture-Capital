using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController; // Script with ForceJump()
    public GameObject vfxPrefab;             // Prefab with Rigidbody2D
    public Transform vfxSpawnPoint;          // Where VFX spawns
    public Transform vfxTarget;              // Target transform VFX moves toward

    [Header("Settings")]
    public float moveSpeed = 3f;             // VFX movement speed

    private Rigidbody2D activeVFX;           // The currently moving VFX

    // Call this to spawn the VFX and start the auto-jump process
    public void SpawnVFX()
    {
        if (vfxPrefab == null || vfxSpawnPoint == null || vfxTarget == null)
        {
            Debug.LogWarning("[VFXAutoJump] Missing references!");
            return;
        }

        GameObject obj = Instantiate(vfxPrefab, vfxSpawnPoint.position, Quaternion.identity);
        activeVFX = obj.GetComponent<Rigidbody2D>();

        if (activeVFX == null)
        {
            Debug.LogError("[VFXAutoJump] Prefab is missing Rigidbody2D!");
            return;
        }

        Debug.Log("[VFXAutoJump] VFX spawned at " + vfxSpawnPoint.position);
    }

    private void Update()
    {
        if (activeVFX != null)
        {
            MoveVFX();
        }
    }

    private void MoveVFX()
    {
        Vector2 current = activeVFX.position;
        Vector2 target = vfxTarget.position;

        // Move toward target
        Vector2 newPos = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
        activeVFX.MovePosition(newPos);

        // Debug log
        Debug.Log("[VFXAutoJump] Moving VFX. Pos: " + activeVFX.position);

        // Check if reached target
        if (Vector2.Distance(current, target) < 0.05f)
        {
            Debug.Log("[VFXAutoJump] VFX reached target. Triggering jump.");
            if (playerController != null)
                playerController.ForceJump();
            else
                Debug.LogWarning("[VFXAutoJump] PlayerController not assigned!");

            Destroy(activeVFX.gameObject);
            activeVFX = null;
        }
    }
}
