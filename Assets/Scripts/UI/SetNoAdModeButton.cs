using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

/// <summary>
/// Attach to the "No Ads" UI Button. Assign the GameModeController.
/// On click switches the game to NoAdMode.
/// Optionally can toggle back if clicked again.
/// </summary>
[RequireComponent(typeof(Button))]
public class SetNoAdModeButton : MonoBehaviour
{
    [SerializeField] private GameModeController gameModeController;
    [SerializeField] private bool toggleInsteadOfForce = false;
    [Header("Optional: Gate with Video Ad")]
    // Intentionally typed as MonoBehaviour to avoid compile-time ambiguity
    // when multiple VideoAdSpawner classes exist. We'll call its methods via reflection.
    [SerializeField] private MonoBehaviour videoAdSpawner; // if assigned, play video first
    [SerializeField] private bool requireFullLength = true; // disables skip for this run
    [SerializeField] private bool showFullscreenImageAfter = true; // show post-video image

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (gameModeController == null)
        {
            Debug.LogWarning("SetNoAdModeButton: GameModeController reference not assigned.");
            return;
        }

        // If a video ad spawner is provided, handle the ad flow first
        if (videoAdSpawner != null)
        {
            System.Action setMode = () => gameModeController.SetGameMode(GameMode.NoAdMode);

            bool invoked = false;
            if (showFullscreenImageAfter)
            {
                if (requireFullLength)
                    invoked = TryInvoke(videoAdSpawner, "ShowVideoAdFullLengthThenImageThen", setMode);
                if (!invoked)
                    invoked = TryInvoke(videoAdSpawner, "ShowVideoAdThenImageThen", setMode);
            }
            // Fallback: no fullscreen image, or previous attempts failed
            if (!invoked)
                invoked = TryInvoke(videoAdSpawner, "ShowVideoAdThen", setMode);

            // If no compatible method exists, switch immediately
            if (!invoked)
            {
                Debug.LogWarning("SetNoAdModeButton: VideoAdSpawner assigned, but no compatible method found. Switching mode immediately.");
                setMode();
            }
        }
        else
        {
            // No video gating; change immediately
            if (toggleInsteadOfForce)
            {
                if (gameModeController.currentMode == GameMode.NoAdMode)
                    gameModeController.SetGameMode(GameMode.NormalMode);
                else
                    gameModeController.SetGameMode(GameMode.NoAdMode);
            }
            else
            {
                gameModeController.SetGameMode(GameMode.NoAdMode);
            }
        }

        MobileHaptics.ImpactLight();
    }

    private bool TryInvoke(MonoBehaviour target, string methodName, System.Action arg)
    {
        if (target == null) return false;
        var t = target.GetType();
        var mi = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mi == null) return false;
        var ps = mi.GetParameters();
        if (ps.Length == 1 && ps[0].ParameterType == typeof(System.Action))
        {
            try { mi.Invoke(target, new object[] { arg }); return true; }
            catch { return false; }
        }
        return false;
    }
}
