using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Unity.VisualScripting;

public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings instance;
    [SerializeField] private AudioMixer myMixer;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider adSlider;
    
    [Header("Vibration Settings")]
    [SerializeField] private Toggle vibrationToggle; // Drag your Toggle UI element here

    public Canvas vRenderer;

    void Awake()
    {
        vRenderer = GetComponent<Canvas>();
        
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance.gameObject);
    }

    void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume") && PlayerPrefs.HasKey("sfxVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetAdVolume();
        }

        // Load vibration setting
        LoadVibrationSetting();
        
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        myMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void SetAdVolume()
    {
        float volume = adSlider.value;
        myMixer.SetFloat("adVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("adVolume", volume);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        adSlider.value = PlayerPrefs.GetFloat("adVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetAdVolume();

    }

    public void RendererOff()
    {
        vRenderer.enabled = false;
    }

    public void RendererOn()
    {
        vRenderer.enabled = true;
    }

    // Vibration toggle methods
    public void SetVibration(bool enabled)
    {
        MobileHaptics.SetEnabled(enabled);
        PlayerPrefs.SetInt("vibrationEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadVibrationSetting()
    {
        // Default to enabled if no preference exists
        bool vibrationEnabled = PlayerPrefs.GetInt("vibrationEnabled", 1) == 1;
        
        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = vibrationEnabled;
            vibrationToggle.onValueChanged.AddListener(SetVibration);
        }
        
        MobileHaptics.SetEnabled(vibrationEnabled);
    }

}
