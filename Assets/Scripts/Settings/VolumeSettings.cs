using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Unity.VisualScripting;

public class VolumeSettings : MonoBehaviour
{
    private static VolumeSettings instance;
    [SerializeField] private AudioMixer myMixer;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Awake()
    {
        
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            instance = this;
        }
        
        DontDestroyOnLoad(gameObject);
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

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        SetMusicVolume();
        SetSFXVolume();

    }

}
