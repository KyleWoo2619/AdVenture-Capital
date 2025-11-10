using UnityEngine;

/// <summary>
/// Handles native sharing functionality for mobile platforms
/// Call ShareText() or ShareWithUrl() to open the device's share sheet
/// </summary>
public class NativeShare : MonoBehaviour
{
    [Header("Share Content")]
    [SerializeField] private string shareText = "Check out AdVenture Capital! Download it here:";
    [SerializeField] private string shareUrl = "https://play.google.com/store/apps/details?id=com.yourcompany.adventurecapital";
    
    /// <summary>
    /// Share text only (opens Android share sheet)
    /// </summary>
    public void ShareText()
    {
        ShareContent(shareText, "");
    }
    
    /// <summary>
    /// Share text with URL (opens Android share sheet)
    /// </summary>
    public void ShareWithUrl()
    {
        string fullText = $"{shareText}\n{shareUrl}";
        ShareContent(fullText, "");
    }
    
    /// <summary>
    /// Share with custom text
    /// </summary>
    public void ShareCustom(string text)
    {
        ShareContent(text, "");
    }

    /// <summary>
    /// Core sharing method - opens native Android share sheet
    /// </summary>
    private void ShareContent(string text, string url)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        
        // Set action to ACTION_SEND
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        
        // Set the text to share
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
        
        // Set type to plain text
        intentObject.Call<AndroidJavaObject>("setType", "text/plain");
        
        // Create chooser (the "Share via..." dialog)
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share via");
        
        // Start the activity
        currentActivity.Call("startActivity", chooser);
        
        // Play haptic feedback
        MobileHaptics.ImpactMedium();
        
        Debug.Log("[NativeShare] Opened share sheet");
#elif UNITY_EDITOR
        // In editor, just copy to clipboard
        GUIUtility.systemCopyBuffer = text;
        Debug.Log($"[NativeShare] (Editor) Copied to clipboard: {text}");
#else
        // For other platforms, copy to clipboard
        GUIUtility.systemCopyBuffer = text;
        Debug.Log($"[NativeShare] Copied to clipboard: {text}");
#endif
    }
}
