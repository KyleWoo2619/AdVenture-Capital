using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple reusable component: assign a URL, attach to a Button.
/// Opens URL on click with optional haptic feedback and basic validation.
/// </summary>
[RequireComponent(typeof(Button))]
public class UrlOpenerButton : MonoBehaviour
{
    [SerializeField] private string url;
    [Header("Behavior")]
    [SerializeField] private bool addHttpsIfMissing = true;
    [SerializeField] private bool hapticOnClick = true;

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OpenUrl);
    }

    public void SetUrl(string newUrl) => url = newUrl;

    private void OpenUrl()
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("UrlOpenerButton: URL is empty.");
            return;
        }

        string finalUrl = url.Trim();
        if (addHttpsIfMissing && !finalUrl.StartsWith("http://") && !finalUrl.StartsWith("https://"))
            finalUrl = "https://" + finalUrl;

        if (hapticOnClick) MobileHaptics.ImpactMedium();

        Application.OpenURL(finalUrl);
        Debug.Log($"UrlOpenerButton: Opened {finalUrl}");
    }
}
