using System.Collections;
using UnityEngine;

/// <summary>
/// 게임 씬 전용 대사 진행 컨트롤러.
/// - 타이틀에서 준비된 DB를 그대로 사용
/// - 화자명: DialogueRecord.GetSpeakerByLang() 사용
/// - 이벤트 트리거: 유연 키 조회(EventTrigger/Event Trigger/이벤트 트리거 등)
/// - 스페이스/엔터: 타이핑중이면 스킵, 아니면 다음 대사
/// - 선택지: 타이핑이 끝나야 표시, 표시 중에는 스페이스 진행 금지
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    [Header("Refs")]
    public DialogueUI ui;
    public DialogueRecord currentRecord;

    [Header("Settings")]
    [Tooltip("시작 ID. 0이거나 유효하지 않으면 DB의 FirstId로 시작")]
    public int startId = 0;

    private int currentId;

    private IEnumerator Start()
    {
        // 안전: DB 보장
        DialogueDatabase.I?.EnsureInitialized(this);
        while (DialogueDatabase.I == null || !DialogueDatabase.I.IsReady)
            yield return null;

        int begin = (startId > 0 && DialogueDatabase.I.GetById(startId) != null)
                  ? startId
                  : DialogueDatabase.I.FirstId;

        if (begin <= 0)
        {
            Debug.LogError("[DialogueSystem] No valid start ID.");
            yield break;
        }

        currentId = begin;
        Show(currentId);
    }

    private void Update()
    {
        // 스페이스/엔터
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            // 1) 타이핑 중이면 먼저 스킵
            if (ui != null && ui.IsTyping)
            {
                ui.SkipTyping();
                return;
            }
            // 2) 선택지 떠 있으면 진행 금지
            if (ui != null && ui.HasActiveChoices())
            {
                return;
            }
            // 3) 평상시에는 다음 대사
            ProceedNext();
        }

        // 숫자키로 선택
        if (Input.GetKeyDown(KeyCode.Alpha1)) { if (ui.HasActiveChoices()) OnChoose(1); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { if (ui.HasActiveChoices()) OnChoose(2); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { if (ui.HasActiveChoices()) OnChoose(3); }
    }

    // ───────────────────── 표시/타이핑/연출 ─────────────────────
    private void Show(int id)
    {
        currentRecord = DialogueDatabase.I.GetById(id);
        if (currentRecord == null)
        {
            Debug.Log($"[DialogueSystem] End or invalid ID: {id}");
            ui.HideChoices();
            ui.ShowLine("", "");
            return;
        }

        // 라인 출력 직전에 선택지 숨김
        ui.HideChoices();

        // 화자/본문
        string speaker = currentRecord.GetSpeakerByLang();
        string text    = currentRecord.GetTextByLang(GameManager.Instance.CurrentLanguage);

        ui.ShowLine(speaker, text);

        // 효과음
        PlaySFX(currentRecord);

        // 스탠딩, 배경
        ui.ApplyStanding(currentRecord);
        ui.ApplyBackground(currentRecord);

        // 자동 실행형 이벤트가 흐름을 소비하면(예: jumpindex) 여기서 종료
        // if (TryExecuteEvent(currentRecord)) return;

        // 선택지는 "타이핑 완료 콜백"에서만 표시한다 (여기선 표시 금지)
    }

    /// <summary>타이핑이 끝났을 때만 선택지를 띄운다.</summary>
    public void OnLineFinishedTyping()
    {
        if (currentRecord == null) return;

        // 기존 로직 유지: 언어별 접미사로 시트 컬럼 접근
        var lang = GameManager.Instance.CurrentLanguage;
        string suffix = lang switch
        {
            GameLanguage.Korean   => "KOR",
            GameLanguage.English  => "ENG",
            GameLanguage.Japanese => "JPN",
            _ => "ENG"
        };

        // 시트 컬럼: C1KOR / C2KOR / C3KOR (언어별)
        string c1 = currentRecord.GetFirst($"C1_{suffix}");
        string c2 = currentRecord.GetFirst($"C2_{suffix}");
        string c3 = currentRecord.GetFirst($"C3_{suffix}");

        bool hasAnyChoice = !string.IsNullOrEmpty(c1) || !string.IsNullOrEmpty(c2) || !string.IsNullOrEmpty(c3);
        if (!hasAnyChoice)
        {
            // 선택지 없으면 아무 것도 안 함 → 스페이스로 다음 진행
            return;
        }

        // 타이핑이 끝났을 때만 선택지 표시
        ui.SetupChoices(
            c1, c2, c3,
            () => OnChoose(1),
            () => OnChoose(2),
            () => OnChoose(3)
        );
    }


    // ───────────────────── 진행/분기 ─────────────────────
    public void Next()
    {
        var rec = DialogueDatabase.I.GetById(currentId);
        if (rec == null) return;

        // 1) 이벤트 우선 (소비되면 종료)
        if (TryExecuteEvent(rec)) return;

        // 2) NextIndex1 우선
        if (int.TryParse(rec.GetFirst("NextIndex1", "Next Index 1", "Next_Index1"), out int to1)
            && DialogueDatabase.I.GetById(to1) != null)
        {
            JumpTo(to1);
            return;
        }

        // 3) 기본 +1
        JumpTo(currentId + 1);
    }

    public void OnChoice(int index)
    {
        var rec = DialogueDatabase.I.GetById(currentId);
        if (rec == null) return;

        // 선택지에서도 이벤트 먼저 처리(있으면 소비)
        if (TryExecuteEvent(rec)) return;

        // Choice1→Next1, Choice2→Next2, Choice3→Next3
        if (TryGetNextForChoice(rec, index, out int to))
            JumpTo(to);
        else
            Next(); // 지정이 없으면 일반 흐름
    }

    public bool JumpTo(int id)
    {
        if (DialogueDatabase.I.GetById(id) == null)
        {
            Debug.LogWarning("[DialogueSystem] jump target missing: " + id);
            return false;
        }
        currentId = id;
        ui.HideChoices();
        Show(currentId);
        return true;
    }

    private bool TryGetNextForChoice(DialogueRecord rec, int choiceIndex, out int nextId)
    {
        nextId = -1;
        if (rec == null) return false;

        // 여러 표기 대응
        string keyA = $"Next{choiceIndex}";
        string keyB = $"Next {choiceIndex}";
        string keyC = $"Next_Index{choiceIndex}";
        string keyD = $"NextIndex{choiceIndex}";

        var raw = rec.GetFirst(keyA, keyB, keyC, keyD);
        if (int.TryParse(raw, out var to) && DialogueDatabase.I.GetById(to) != null)
        {
            nextId = to;
            return true;
        }
        return false;
    }

    /// <summary>이벤트 트리거 실행. true면 이후 흐름을 여기서 소비.</summary>
    private bool TryExecuteEvent(DialogueRecord rec)
    {
        string trigRaw = rec.GetFirst("EventTrigger", "Event Trigger", "Trigger", "이벤트트리거", "이벤트 트리거");
        if (string.IsNullOrEmpty(trigRaw)) return false;

        string trig = trigRaw.Trim().ToLowerInvariant();
        string p1 = rec.GetFirst("Param1", "Param 1", "이벤트파라미터1", "파라미터1");
        string p2 = rec.GetFirst("Param2", "Param 2", "이벤트파라미터2", "파라미터2");

        IDialogueEvent ev = null;

        switch (trig)
        {
            case "jumpindex":
                if (int.TryParse(p1, out int id)) ev = new JumpIndexEvent(id);
                break;

            case "statinc":
                if (int.TryParse(p2, out int dv)) ev = new StatDeltaEvent(p1, +dv);
                else Debug.LogWarning($"[DialogueEvent] StatInc invalid Param2 '{p2}'");
                break;

            case "statdec":
                if (int.TryParse(p2, out int dv2)) ev = new StatDeltaEvent(p1, -dv2);
                else Debug.LogWarning($"[DialogueEvent] StatDec invalid Param2 '{p2}'");
                break;

            case "changebgm":
                if (!string.IsNullOrEmpty(p1)) ev = new ChangeBGMEvent(p1);
                else Debug.LogWarning("[DialogueEvent] ChangeBGM missing Param1 (BGM name)");
                break;

            // TODO: 확장 이벤트들 (playsfx, setstat, loadscene 등)
            default:
                break;
        }

        return ev != null && ev.Execute(this);
    }

    private void PlaySFX(DialogueRecord rec)
    {
        string sfxName = rec.GetFirst("SFX", "Sound", "효과음");
        if (string.IsNullOrEmpty(sfxName)) return;

        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[DialogueSystem] SoundManager.Instance not found.");
            return;
        }
        SoundManager.Instance.PlaySFX(sfxName.Trim());
    }

    // ───────────────────── 래퍼(이름 호환용) ─────────────────────
    // 기존 코드에서 사용한 이름을 호환시키기 위한 래퍼

    private void ProceedNext() => Next();

    private void OnChoose(int index) => OnChoice(index);
}
