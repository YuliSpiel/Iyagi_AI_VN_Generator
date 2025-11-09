using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerController : MonoBehaviour
{
    [SerializeField] private AudioMixer m_AudioMixer;
    [SerializeField] private Slider m_MusicMasterSlider;
    [SerializeField] private Slider m_MusicBGMSlider;
    [SerializeField] private Slider m_MusicSFXSlider;
	 
    private void Awake()
    {
        m_MusicMasterSlider.onValueChanged.AddListener(SetMasterVolume);
        m_MusicBGMSlider.onValueChanged.AddListener(SetBGMVolume);
        m_MusicSFXSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void Start()
    {
        LoadVolumeSettings();
    }

    // 저장된 볼륨 가져오기 
    private void LoadVolumeSettings()
    {
        float MasterVolume = SoundManager.Instance.MasterVolume;
        m_MusicMasterSlider.value = MasterVolume;
        
        float BGMVolume = SoundManager.Instance.BGMVolume;
        m_MusicBGMSlider.value = BGMVolume;
        
        float SFXVolume = SoundManager.Instance.SFXVolume;
        m_MusicSFXSlider.value = SFXVolume;
    }

    public void SetMasterVolume(float volume)
    {
        m_AudioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
        SoundManager.Instance.MasterVolume = volume;
        SoundManager.Instance.SaveVolumeSettings();
    }
	 
    public void SetBGMVolume(float volume)
    {
        m_AudioMixer.SetFloat("BGMs", Mathf.Log10(volume) * 20);
        SoundManager.Instance.BGMVolume = volume;
        SoundManager.Instance.SaveVolumeSettings();
    }
	 
    public void SetSFXVolume(float volume)
    {
        m_AudioMixer.SetFloat("SFXs", Mathf.Log10(volume) * 20);
        SoundManager.Instance.SFXVolume = volume;
        SoundManager.Instance.SaveVolumeSettings();
    }  
}