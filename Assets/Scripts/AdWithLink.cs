using UnityEngine;

/// <summary>
/// Data structure for ads that can be clicked to open URLs
/// </summary>
[System.Serializable]
public class AdWithLink
{
    [Tooltip("Name of the ad (for organization)")]
    public string adName;
    
    [Tooltip("The sprite/image for this ad")]
    public Sprite adImage;
    
    [Tooltip("URL to open when ad is clicked (e.g., https://www.example.com)")]
    public string clickUrl;
    
    [Tooltip("Whether this ad can be clicked")]
    public bool isClickable = true;
}
