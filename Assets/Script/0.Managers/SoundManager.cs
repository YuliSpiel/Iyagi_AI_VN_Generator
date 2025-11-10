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

    // 현재 재생 중인 BGM 이름 (중복 재생 방지)
    private string _currentBGMName = null;

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
        _sfxDict = new Dictionary<string, AudioClip>();

        // Inspector에 할당된 클립들 등록
        foreach (var x in _bgms)
        {
            if (x != null && !_bgmDict.ContainsKey(x.name))
            {
                _bgmDict.Add(x.name, x);
            }
        }

        foreach (var x in _sfxs)
        {
            if (x != null && !_sfxDict.ContainsKey(x.name))
            {
                _sfxDict.Add(x.name, x);
            }
        }

        // Resources 폴더에서 추가 로드
        LoadResourcesAudio();
    }

    /// <summary>
    /// Resources/Sound/BGM, Resources/Sound/SFX, TestResources 폴더에서 오디오 클립 동적 로드
    /// </summary>
    private void LoadResourcesAudio()
    {
        // BGM 로드 (Sound/BGM)
        AudioClip[] bgmResources = Resources.LoadAll<AudioClip>("Sound/BGM");
        foreach (var clip in bgmResources)
        {
            if (!_bgmDict.ContainsKey(clip.name))
            {
                _bgmDict.Add(clip.name, clip);
                Debug.Log($"[SoundManager] BGM loaded from Resources: {clip.name}");
            }
        }

        // BGM 로드 (TestResources/BGM)
        AudioClip[] testBgmResources = Resources.LoadAll<AudioClip>("TestResources/BGM");
        foreach (var clip in testBgmResources)
        {
            if (!_bgmDict.ContainsKey(clip.name))
            {
                _bgmDict.Add(clip.name, clip);
                Debug.Log($"[SoundManager] BGM loaded from TestResources: {clip.name}");
            }
        }

        // SFX 로드 (Sound/SFX)
        AudioClip[] sfxResources = Resources.LoadAll<AudioClip>("Sound/SFX");
        foreach (var clip in sfxResources)
        {
            if (!_sfxDict.ContainsKey(clip.name))
            {
                _sfxDict.Add(clip.name, clip);
                Debug.Log($"[SoundManager] SFX loaded from Resources: {clip.name}");
            }
        }

        // SFX 로드 (TestResources/SFX)
        AudioClip[] testSfxResources = Resources.LoadAll<AudioClip>("TestResources/SFX");
        foreach (var clip in testSfxResources)
        {
            if (!_sfxDict.ContainsKey(clip.name))
            {
                _sfxDict.Add(clip.name, clip);
                Debug.Log($"[SoundManager] SFX loaded from TestResources: {clip.name}");
            }
        }

        Debug.Log($"[SoundManager] Total BGMs: {_bgmDict.Count}, Total SFXs: {_sfxDict.Count}");
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
        // 1. Dictionary에서 직접 찾기
        if (_bgmDict.ContainsKey(clipName))
        {
            return _bgmDict[clipName];
        }

        // 2. 경로가 포함된 경우 (예: "TestResources/BGM/D960_1" → "D960_1")
        string fileName = System.IO.Path.GetFileNameWithoutExtension(clipName);
        if (_bgmDict.ContainsKey(fileName))
        {
            Debug.Log($"[SoundManager] BGM found by filename: {fileName}");
            return _bgmDict[fileName];
        }

        // 3. Resources에서 직접 로드 시도
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip != null)
        {
            Debug.Log($"[SoundManager] BGM loaded from Resources: {clipName}");
            _bgmDict[clipName] = clip; // 캐시에 추가
            return clip;
        }

        Debug.LogError($"[SoundManager] BGM not found: {clipName}");
        return null;
    }

    public AudioClip GetSFXClip(string clipName)
    {
        // 1. Dictionary에서 직접 찾기
        if (_sfxDict.ContainsKey(clipName))
        {
            return _sfxDict[clipName];
        }

        // 2. 경로가 포함된 경우 (예: "TestResources/SFX/SFX_Knock" → "SFX_Knock")
        string fileName = System.IO.Path.GetFileNameWithoutExtension(clipName);
        if (_sfxDict.ContainsKey(fileName))
        {
            Debug.Log($"[SoundManager] SFX found by filename: {fileName}");
            return _sfxDict[fileName];
        }

        // 3. Resources에서 직접 로드 시도
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip != null)
        {
            Debug.Log($"[SoundManager] SFX loaded from Resources: {clipName}");
            _sfxDict[clipName] = clip; // 캐시에 추가
            return clip;
        }

        Debug.LogError($"[SoundManager] SFX not found: {clipName}");
        return null;
    }

    /// <summary>
    /// BGM 재생 (이미 같은 BGM이 재생 중이면 무시)
    /// </summary>
    public void PlayBGM(string clipName)
    {
        // 빈 문자열이거나 null이면 무시
        if (string.IsNullOrEmpty(clipName))
        {
            return;
        }

        // 이미 같은 BGM이 재생 중이면 무시
        if (_currentBGMName == clipName && _BGMAudio.isPlaying)
        {
            Debug.Log($"[SoundManager] BGM '{clipName}' is already playing, skipping");
            return;
        }

        AudioClip clip = GetBGMClip(clipName);
        if (clip != null)
        {
            _BGMAudio.clip = clip;
            _BGMAudio.Play();
            _BGMAudio.loop = true;
            _currentBGMName = clipName;
            Debug.Log($"[SoundManager] BGM playing: {clipName}");
        }
    }

    /// <summary>
    /// BGM 크로스페이드로 전환 (부드러운 전환)
    /// </summary>
    public void CrossfadeBGM(string clipName, float duration = 1f)
    {
        if (string.IsNullOrEmpty(clipName))
        {
            return;
        }

        if (_currentBGMName == clipName && _BGMAudio.isPlaying)
        {
            return;
        }

        StartCoroutine(CrossfadeBGMCoroutine(clipName, duration));
    }

    private IEnumerator CrossfadeBGMCoroutine(string clipName, float duration)
    {
        float startVolume = _BGMAudio.volume;

        // Fade out
        float elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            _BGMAudio.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
            yield return null;
        }

        // Change clip
        AudioClip clip = GetBGMClip(clipName);
        if (clip != null)
        {
            _BGMAudio.clip = clip;
            _BGMAudio.Play();
            _BGMAudio.loop = true;
            _currentBGMName = clipName;
        }

        // Fade in
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            _BGMAudio.volume = Mathf.Lerp(0f, startVolume, elapsed / (duration / 2f));
            yield return null;
        }

        _BGMAudio.volume = startVolume;
    }

    public void StopBGM()
    {
        _BGMAudio.Stop();
        _currentBGMName = null;
        Debug.Log("[SoundManager] BGM stopped");
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
