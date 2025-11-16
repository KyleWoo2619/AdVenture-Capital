using UnityEngine;

public enum GameMode
{
    NormalMode,
    AdFreeMode,
    NoAdMode
}

public class GameModeController : MonoBehaviour
{
    [Header("Hide these in Normal Mode")]
    public GameObject[] hideInNormal;

    [Header("Hide these in Ad Free Mode")]
    public GameObject[] hideInAdFree;

    [Header("Hide these in No Ad Mode")]
    public GameObject[] hideInNoAd;

    [Header("Current Mode")]
    public GameMode currentMode = GameMode.NormalMode;

    private void Update()
    {
        ApplyModeSettings();
    }

    public void SetGameMode(GameMode newMode)
    {
        Debug.Log($"Switching mode to: {newMode}");
        currentMode = newMode;
        ApplyModeSettings();
    }

    private void ApplyModeSettings()
    {
        // reset all objects to active
        SetAllActive();

        // disable only the ones that should be hidden for this mode
        switch (currentMode)
        {
            case GameMode.NormalMode:
              //  SetActiveForObjects(hideInNormal, false);
               foreach(GameObject bannerAd in hideInNormal)
                {
                    bannerAd.GetComponent<BounceAd>().enabled = false;
                    bannerAd.GetComponent<Canvas>().enabled = true;
                }
                break;

            case GameMode.AdFreeMode:
              //  SetActiveForObjects(hideInAdFree, false);
               foreach(GameObject bannerAd in hideInNormal)
                {
                    bannerAd.GetComponent<BounceAd>().enabled = true;
                    bannerAd.GetComponent<Canvas>().enabled = true;
                }
                break;

            case GameMode.NoAdMode:
             //   SetActiveForObjects(hideInNoAd, false);
               foreach(GameObject bannerAd in hideInNormal)
                {
                    bannerAd.GetComponent<BounceAd>().enabled = false;
                    bannerAd.GetComponent<Canvas>().enabled = false;
                }
                break;
        }
    }

    private void SetAllActive()
    {
        // edge case to ensure all are active before disabling specific ones lol
        SetActiveForObjects(hideInNormal, true);
        SetActiveForObjects(hideInAdFree, true);
        SetActiveForObjects(hideInNoAd, true);
    }

    private void SetActiveForObjects(GameObject[] objects, bool state)
    {
        if (objects == null) return;

        foreach (var obj in objects)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }


    [ContextMenu("Set → Normal Mode")]
    private void TestSetNormal() => SetGameMode(GameMode.NormalMode);

    [ContextMenu("Set → Ad Free Mode")]
    private void TestSetAdFree() => SetGameMode(GameMode.AdFreeMode);

    [ContextMenu("Set → No Ad Mode")]
    private void TestSetNoAd() => SetGameMode(GameMode.NoAdMode);
}
