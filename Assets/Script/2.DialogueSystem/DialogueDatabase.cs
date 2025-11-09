using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타이틀 씬에서 1회 초기화되는 대사 DB.
/// - 싱글턴
/// - 에디터/테스트: 온라인 시트(옵션)
/// - 릴리즈/일반: Resources/dialogue.csv
/// - 중복 초기화 방지(텍스트 해시로 동일 소스면 재파싱 생략)
/// </summary>
public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase I { get; private set; }

    [Header("Source")]
    [Tooltip("에디터에서만 온라인 시트를 사용(릴리즈에선 무시)")]
    public bool useOnlineSheetInEditor = true;

    [Tooltip("온라인 시트 로더(같은 오브젝트에 붙이기)")]
    public DialogueLoader loader;

    [Tooltip("Assets/Resources/dialogue.csv → \"dialogue\"")]
    public string resourcesPath = "dialogue";

    /// <summary>ID → DialogueRecord</summary>
    public Dictionary<int, DialogueRecord> Dict { get; private set; }

    /// <summary>데이터 준비 완료</summary>
    public bool IsReady { get; private set; }

    // 내부 상태
    private string _lastSourceHash; // 마지막 로딩 텍스트 해시
    private bool _initialized;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // 타이틀에서 자동 초기화
        EnsureInitialized(this);
    }

    /// <summary>
    /// 어디서 호출해도 안전. 이미 초기화되어 있으면 바로 리턴.
    /// 타이틀씬에서는 Awake에서 자동 호출됨.
    /// </summary>
    public void EnsureInitialized(MonoBehaviour host)
    {
        if (_initialized) return;

#if UNITY_EDITOR
        if (useOnlineSheetInEditor && loader != null && !string.IsNullOrEmpty(loader.csvUrl))
        {
            host.StartCoroutine(loader.LoadCSV(OnCsvGot));
            return;
        }
#endif
        LoadFromResources();
    }

    private void LoadFromResources()
    {
        var csv = Resources.Load<TextAsset>(resourcesPath);
        if (csv == null)
        {
            Debug.LogError($"[DialogueDatabase] Resources/{resourcesPath}.csv not found");
            return;
        }
        if (IsSameHash(csv.text))
        {
            IsReady = (Dict != null && Dict.Count > 0);
            _initialized = true;
            return;
        }

        Dict = DialogueParser.ParseToDict(csv.text, idColumnIndex: 45, skipDescriptionRow: true);
        _lastSourceHash = ComputeHash(csv.text);
        IsReady = (Dict != null && Dict.Count > 0);
        _initialized = true;

        Debug.Log($"[DialogueDatabase] Loaded from Resources. Count={Dict?.Count ?? 0}");
    }

    private void OnCsvGot(string csvText)
    {
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError("[DialogueDatabase] Online CSV empty.");
            return;
        }
        if (IsSameHash(csvText))
        {
            IsReady = (Dict != null && Dict.Count > 0);
            _initialized = true;
            return;
        }

        Dict = DialogueParser.ParseToDict(csvText, idColumnIndex: 45, skipDescriptionRow: true);
        _lastSourceHash = ComputeHash(csvText);
        IsReady = (Dict != null && Dict.Count > 0);
        _initialized = true;

        Debug.Log($"[DialogueDatabase] Loaded from Sheet. Count={Dict?.Count ?? 0}");
    }

    private bool IsSameHash(string text)
        => !string.IsNullOrEmpty(_lastSourceHash) && _lastSourceHash == ComputeHash(text);

    private string ComputeHash(string s)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(s ?? "");
            var hash = md5.ComputeHash(bytes);
            return System.BitConverter.ToString(hash);
        }
    }

    public DialogueRecord GetById(int id)
        => (Dict != null && Dict.TryGetValue(id, out var rec)) ? rec : null;

    public int FirstId
    {
        get
        {
            if (Dict == null || Dict.Count == 0) return 0;
            int min = int.MaxValue;
            foreach (var k in Dict.Keys) if (k < min) min = k;
            return (min == int.MaxValue) ? 0 : min;
        }
    }
}
