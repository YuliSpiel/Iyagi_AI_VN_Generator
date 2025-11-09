using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public enum GameLanguage { Korean, English, Japanese }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 진행 중인 상태
    public PlayerStats Stats = new PlayerStats();
    public ChapterProgress Chapter = new ChapterProgress();
    public GameLanguage CurrentLanguage { get; set; }
    
    private async void Awake()
    {
        Init();
        
        await LocalizationSettings.InitializationOperation.Task;

        string savedCode = PlayerPrefs.GetString("LocaleCode", "ko-KR");
        SetLanguageByCode(savedCode, applyLocale: true);
    }

    private async void Start()
    {
        // 언어설정 적용을 위해 일시 대기 후 화면 페이드인
        UIManager.Instance.GlobalFO(2f, 1f);
        SoundManager.Instance.PlayBGM("D960_2");

        // // Localization 초기화 대기
        // await LocalizationSettings.InitializationOperation.Task;
        //
        // string code = PlayerPrefs.GetString("LocaleCode", string.Empty);
        // if (string.IsNullOrEmpty(code)) return; // 저장 없으면 현 상태 유지
        //
        // var loc = LocalizationSettings.AvailableLocales.Locales
        //     .FirstOrDefault(l => l.Identifier.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase));
        // if (loc != null)
        //     LocalizationSettings.SelectedLocale = loc;
    }

    private IEnumerator StartRoutine()
    {
        // 마지막 세이브 슬롯만 로드
        SaveDataBlock lastSave = DataManager.Instance.LoadLastSlot();
        if (lastSave != null)
        {
            Initialize(lastSave);
        }
        yield break;
    }

    void Init()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetLanguageByCode(string code, bool applyLocale = false)
    {
        // 로케일 적용 (UI용)
        if (applyLocale)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales
                .FirstOrDefault(l => l.Identifier.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase));
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        // 내부 언어 정보 저장 (CSV/대사용)
        switch (code)
        {
            case "ko-KR":
            case "ko":
                CurrentLanguage = GameLanguage.Korean;
                break;
            case "en":
            case "en-US":
                CurrentLanguage = GameLanguage.English;
                break;
            case "ja":
            case "ja-JP":
                CurrentLanguage = GameLanguage.Japanese;
                break;
        }

        PlayerPrefs.SetString("LocaleCode", code);
        PlayerPrefs.Save();
    }
    
    // 챕터 변경 시 자동 저장
    public void ChangeChapter(int chapterNumber, int index)
    {
        Chapter.CurChapter = chapterNumber;
        Chapter.CurIndex = index;
        SaveCurrentProgress();
    }

    // 스탯 변경 시 자동 저장
    public void UpdateStats(PlayerStats newStats)
    {
        Stats = newStats;
        SaveCurrentProgress();
    }

    // 현재 진행상황 저장
    public void SaveCurrentProgress()
    {
        if (DataManager.Instance.SaveData.Blocks.Count == 0)
        {
            DataManager.Instance.CreateNewSlot();
        }
        DataManager.Instance.SaveCurrentSlot();
    }

    public void Initialize(SaveDataBlock save)
    {
        Stats = save.Stats;
        Chapter = save.Chapter;
    }

    public SaveDataBlock ToSaveDataBlock()
    {
        return new SaveDataBlock
        {
            Stats = this.Stats,
            Chapter = this.Chapter,
            SaveTime = System.DateTime.Now.ToString("yyyy.MM.dd HH:mm")
        };
    }
    
    public void ExitGame()
    {
        // 게임 종료 시 현재 진행상황 저장
        SaveCurrentProgress();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
