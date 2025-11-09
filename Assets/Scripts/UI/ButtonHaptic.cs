using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any Button to add haptic feedback on click.
/// Works automatically - no code needed!
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonHaptic : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("Type of haptic feedback to play")]
    public HapticType hapticType = HapticType.Light;
    
    public enum HapticType
    {
        Tap,
        Light,
        Medium,
        Heavy
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        PlayHaptic();
    }
    
    void PlayHaptic()
    {
        switch (hapticType)
        {
            case HapticType.Tap:
                MobileHaptics.Tap();
                break;
            case HapticType.Light:
                MobileHaptics.ImpactLight();
                break;
            case HapticType.Medium:
                MobileHaptics.ImpactMedium();
                break;
            case HapticType.Heavy:
                MobileHaptics.ImpactHeavy();
                break;
        }
    }
}
