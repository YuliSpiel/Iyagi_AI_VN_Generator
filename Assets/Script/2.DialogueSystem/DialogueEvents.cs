using System;
using System.Reflection;
using UnityEngine;

/// 전체 대화 이벤트 공통 인터페이스
public interface IDialogueEvent
{
    /// true면 흐름(점프 등)을 여기서 소비했다는 의미
    bool Execute(DialogueSystem system);
}

/* ───────────────────────────── 점프 예시 ───────────────────────────── */

public class JumpIndexEvent : IDialogueEvent
{
    private readonly int _targetId;
    public JumpIndexEvent(int id) { _targetId = id; }

    public bool Execute(DialogueSystem system)
    {
        if (system == null) return false;
        return system.JumpTo(_targetId);
    }
}

/* ───────────────────────────── 스탯 증/감 ─────────────────────────────
 * - Trigger: "StatInc" or "StatDec"
 * - Param1: Yeom, Sinche, Jineung, Insung, Jikkam, Luck, Sanity
 * - Param2: int
 * - GameManager.Instance.Stats 를 수정한 뒤 GameManager.Instance.UpdateStats(stats) 호출
 * - PlayerStats가 필드/프로퍼티(정수형)로 위 이름을 갖고 있다고 가정.
 *   만약 이름이 다르다면 아래 StatName 을 매핑 수정.
 */

public class StatDeltaEvent : IDialogueEvent
{
    private readonly string _statName; // ex) "Yeom"
    private readonly int _delta;       // +n / -n

    public StatDeltaEvent(string statName, int delta)
    {
        _statName = statName;
        _delta = delta;
    }

    public bool Execute(DialogueSystem system)
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[DialogueEvent] GameManager.Instance not found.");
            return false;
        }

        var stats = gm.Stats ?? new PlayerStats();

        if (!TryApplyDelta(stats, _statName, _delta))
        {
            Debug.LogWarning($"[DialogueEvent] Failed to modify stat '{_statName}' by Δ{_delta}.");
            return false; // 흐름은 계속 진행
        }

        // 변경된 스탯 저장 반영(자동 저장 트리거)
        gm.UpdateStats(stats);
        return false; // 이후 정상 흐름 계속
    }

    /// PlayerStats 내부에서 대상 스탯을 찾아 Δ를 적용한다.
    /// - 우선 정확히 같은 이름의 int 프로퍼티/필드 탐색(대소문자 무시)
    /// - 실패 시, 알려진 별칭(소문자 비교) 매핑 활용
    private bool TryApplyDelta(object statsObj, string statName, int delta)
    {
        if (statsObj == null || string.IsNullOrEmpty(statName)) return false;

        Type t = statsObj.GetType();
        string resolved = StatName(statName); // 표준화된 대상명 (예: "Yeom")

        // 1) 프로퍼티 우선
        var prop = FindIntProperty(t, resolved);
        if (prop == null) prop = FindIntPropertyLoose(t, resolved); // 대소문자/별칭 허용

        if (prop != null)
        {
            int cur = (int) (prop.GetValue(statsObj) ?? 0);
            int after = cur + delta;
            prop.SetValue(statsObj, after);
            Debug.Log($"[Stats] {resolved} (prop): {cur} -> {after} (Δ{delta})");
            return true;
        }

        // 2) 필드 시도
        var field = FindIntField(t, resolved);
        if (field == null) field = FindIntFieldLoose(t, resolved);

        if (field != null)
        {
            int cur = (int) (field.GetValue(statsObj) ?? 0);
            int after = cur + delta;
            field.SetValue(statsObj, after);
            Debug.Log($"[Stats] {resolved} (field): {cur} -> {after} (Δ{delta})");
            return true;
        }

        // 3) 실패
        Debug.LogWarning($"[DialogueEvent] Stat member not found on PlayerStats: '{statName}'(resolved '{resolved}')");
        return false;
    }

    private PropertyInfo FindIntProperty(Type t, string name)
        => t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is PropertyInfo p
           && p.PropertyType == typeof(int) ? p : null;

    private FieldInfo FindIntField(Type t, string name)
        => t.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is FieldInfo f
           && f.FieldType == typeof(int) ? f : null;

    // 느슨한 탐색(별칭/소문자 비교)
    private PropertyInfo FindIntPropertyLoose(Type t, string normalized)
    {
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.PropertyType != typeof(int)) continue;
            if (string.Equals(p.Name, normalized, StringComparison.OrdinalIgnoreCase)) return p;
            if (string.Equals(p.Name, StatName(p.Name), StringComparison.OrdinalIgnoreCase)) return p;
        }
        return null;
    }

    private FieldInfo FindIntFieldLoose(Type t, string normalized)
    {
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (f.FieldType != typeof(int)) continue;
            if (string.Equals(f.Name, normalized, StringComparison.OrdinalIgnoreCase)) return f;
            if (string.Equals(f.Name, StatName(f.Name), StringComparison.OrdinalIgnoreCase)) return f;
        }
        return null;
    }

    /// 입력 별칭을 표준 이름으로 정규화한다.
    /// 시트에는 Yeom/Sinche/Jineung/Insung/Jikkam/Luck/Sanity 로 온다고 가정.
    private string StatName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        string k = s.Trim().ToLowerInvariant();

        // 필요시 여기서 PlayerStats 실제 멤버명과 매핑해줘.
        // 예: "sanity" -> "Sanity" (PlayerStats 멤버명도 Sanity라고 가정)
        switch (k)
        {
            case "yeom":    return "Yeom";
            case "sinche":  return "Sinche";
            case "jineung": return "Jineung";
            case "insung":  return "Insung";
            case "jikkam":  return "Jikkam";
            case "luck":    return "Luck";
            case "sanity":  return "Sanity";
            default:        return s; // 그대로 반환하고 느슨 탐색에서 한 번 더 시도
        }
    }
}

// ───────────────────────────── BGM 변경 이벤트 ─────────────────────────────
public class ChangeBGMEvent : IDialogueEvent
{
    private readonly string _bgmName;

    public ChangeBGMEvent(string bgmName)
    {
        _bgmName = bgmName;
    }

    public bool Execute(DialogueSystem system)
    {
        if (string.IsNullOrEmpty(_bgmName))
        {
            Debug.LogWarning("[DialogueEvent] ChangeBGM: Param1 is empty.");
            return false;
        }

        // SoundManager가 존재하고 PlayBGM(string) 메서드가 있다고 가정
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[DialogueEvent] SoundManager.Instance not found.");
            return false;
        }

        Debug.Log($"[DialogueEvent] Changing BGM to '{_bgmName}'");
        SoundManager.Instance.PlayBGM(_bgmName);
        return false; // 대사 흐름은 계속 진행
    }
}

