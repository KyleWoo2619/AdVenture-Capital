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


}
