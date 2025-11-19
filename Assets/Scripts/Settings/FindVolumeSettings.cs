using UnityEngine;
using UnityEngine.UI;

public class FindVolumeSettings : MonoBehaviour
{
    [SerializeField] private VolumeSettings volumeSettingsRef;
    //private Canvas volumeSettingsRefCanvas;
    private Button optionsButton;
    void Start()
    {
        volumeSettingsRef = VolumeSettings.instance;
        optionsButton = GetComponent<Button>();
        optionsButton.onClick.AddListener(volumeSettingsRef.RendererOn);
        
        //volumeSettingsRefCanvas = VolumeSettings.instance.vRenderer;

        
    }

    

    
    
    
}
