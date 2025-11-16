using UnityEngine;

public class ArmTapVFX : MonoBehaviour
{
    [SerializeField] private PlayerController player; // Assign player here

    // This is called from the animation event
    public void OnArmTap()
    {
        if (player != null)
            player.ForceJump();
    }
}
