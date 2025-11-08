using UnityEngine;
using UnityEngine.UI;

public class FindVolumeSettings : MonoBehaviour
{
    [SerializeField] private VolumeSettings volumeSettingsRef;
    //private Canvas volumeSettingsRefCanvas;
    private Button optionsButton;
    void Awake()
    {
        volumeSettingsRef = VolumeSettings.instance;
        optionsButton = GetComponent<Button>();
        
        //volumeSettingsRefCanvas = VolumeSettings.instance.vRenderer;

        
    }

    void OnEnable()
    {
    
        
        optionsButton.onClick.AddListener(volumeSettingsRef.RendererOn);
    }

    
    
    
}
