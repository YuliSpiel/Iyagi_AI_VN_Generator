using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioSource _BGMAudio;
    [SerializeField] private AudioSource _SFXAudio;
    
    [SerializeField] private AudioClip[] _bgms;
    [SerializeField] private AudioClip[] _sfxs;
    
    private Dictionary<string, AudioClip> _bgmDict;
    private Dictionary<string, AudioClip> _sfxDict;

    public float MasterVolume;
    public float BGMVolume;
    public float SFXVolume;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        else if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        _bgmDict = new Dictionary<string, AudioClip>();
        foreach (var x in _bgms)
        {
            _bgmDict.Add(x.name, x);
        }

        _sfxDict = new Dictionary<string, AudioClip>();
        foreach (var x in _sfxs)
        {
            _sfxDict.Add(x.name, x);
        }
        
    }

    private void Start()
    {
        // 저장된 볼륨 설정 최초 1회 가져오기
        LoadVolumeSettings();
        // 저장된 볼륨 설정 적용하기
        ApplyVolumeSettings();
    }

    public AudioClip GetBGMClip(string clipName)
    {
        AudioClip clip = _bgmDict[clipName];
        if (clip == null)
        {
            Debug.LogError(clipName + "을(를) 찾을 수 없음");
        }
        return clip;
    }
    
    public AudioClip GetSFXClip(string clipName)
    {
        AudioClip clip = _sfxDict[clipName];
        if (clip == null)
        {
            Debug.LogError(clipName + "을(를) 찾을 수 없음");
        }
        return clip;
    }

    public void PlayBGM(string clipName)
    {
        // if (_BGMAudio.isPlaying)
        // {
        //     _BGMAudio.Stop();
        // }
        
        _BGMAudio.clip = GetBGMClip(clipName);
        _BGMAudio.Play();  
        _BGMAudio.loop = true;
    }

    public void StopBGM()
    {
        _BGMAudio.Stop();
    }

    public void PauseBGM()
    {
        if (!_BGMAudio.isPlaying)
        {
            return;
        }
        _BGMAudio.Pause();
    }

    public void PlaySFX(string clipName)
    {
        AudioClip clip = GetSFXClip(clipName);
        _SFXAudio.PlayOneShot(clip);               
    }

    public void StopSFX()
    {
        _SFXAudio.Stop();
    }

    public void LoadVolumeSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        Debug.Log("볼륨 로드됨");
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("BGMVolume", BGMVolume);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        PlayerPrefs.Save();
        Debug.Log("볼륨 저장됨");
    }
    
    // 불러온 볼륨값을 AudioMixer에 적용
    public void ApplyVolumeSettings()
    {
        if (_audioMixer == null)
        {
            Debug.LogWarning("AudioMixer가 연결되지 않음");
            return;
        }

        _audioMixer.SetFloat("Master", Mathf.Log10(MasterVolume) * 20);
        _audioMixer.SetFloat("BGMs", Mathf.Log10(BGMVolume) * 20);
        _audioMixer.SetFloat("SFXs", Mathf.Log10(SFXVolume) * 20);
        Debug.Log("볼륨 초기설정 완료");
    }
}
