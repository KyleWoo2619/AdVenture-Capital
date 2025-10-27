using System;

/// <summary>
/// Interface for interactive ad scripts that can be managed by InteractiveAdManager
/// </summary>
public interface IInteractiveAd
{
    /// <summary>
    /// Start the interactive ad with a completion callback
    /// </summary>
    /// <param name="onComplete">Callback to invoke when the ad is complete</param>
    void StartInteractiveAd(System.Action onComplete);
    
    /// <summary>
    /// Hide the ad UI elements (called when win condition is triggered)
    /// </summary>
    void HideAdUI();
}