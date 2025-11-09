using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 대사/스피커 표시 + 타이핑 + 선택지(최대 3) + 스탠딩 이미지 2슬롯.
/// - StandingImg1/StandingImg2 에 스프라이트 적용
/// - 포지션: 시트의 Char1/2Pos("Left|Center|Right") → 인스펙터 프리셋(left/center/right) 적용
/// - 사이즈: 프리셋의 sizeDelta 사용
/// - 스케일: 시트의 Char1/2Size("Small|Medium|Large") → 인스펙터 스케일 프리셋 적용
/// - 파츠(Eyes/EyesAnim/Mouth/ETC)는 아직 미구현: 변경사항만 Debug.Log
/// - 스프라이트 로드: Resources/Standing/{Name} 또는 {Name}_{Look}
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Text")]
    [Tooltip("본문 텍스트(TMP)")]
    public TMP_Text dialogueText;

    [Tooltip("스피커명 텍스트(TMP)")]
    public TMP_Text speakerText;

    [Header("Choices (최대 3개)")]
    public Button choice1;
    public Button choice2;
    public Button choice3;

    [Header("Typing")]
    [Tooltip("한 글자 타이핑 간격(초)")]
    public float typingSpeed = 0.015f;

    [Tooltip("텍스트 내의 \"\\n\"을 실제 개행으로 렌더링")]
    public bool renderEscapedNewlines = true;

    /// <summary>현재 타이핑 중인지</summary>
    public bool IsTyping { get; private set; }

    [Header("Standing Images")]
    [Tooltip("왼쪽/우선 슬롯")]
    public Image StandingImg1;

    [Tooltip("오른쪽/두번째 슬롯")]
    public Image StandingImg2;

    [Header("Standing Presets (Inspector-controlled)")]
    [Tooltip("왼쪽 프리셋 위치")]
    public Vector2 leftAnchoredPos = new Vector2(-500, -50);
    [Tooltip("중앙 프리셋 위치")]
    public Vector2 centerAnchoredPos = new Vector2(0, -50);
    [Tooltip("오른쪽 프리셋 위치")]
    public Vector2 rightAnchoredPos = new Vector2(500, -50);

    [Header("Scale Presets")]
    [Tooltip("CharXSize=Small 일 때 localScale 배수")]
    public float smallScale = 0.95f;
    [Tooltip("CharXSize=Medium 일 때 localScale 배수")]
    public float mediumScale = 1.0f;
    [Tooltip("CharXSize=Large 일 때 localScale 배수")]
    public float largeScale = 1.05f;

    // 내부 상태
    private Coroutine typingCo;
    private string lastFullText = string.Empty;

    // 슬롯별 이전 상태(변경 로그용)
    private StandingState state1 = new StandingState();
    private StandingState state2 = new StandingState();

    // 스프라이트 캐시
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    
    // DialogueUI 클래스 내부 어딘가 적당한 위치 (예: Standing 아래)
    [Header("Background")]
    [Tooltip("씬의 배경 Image. BG 열에 값이 있으면 이 스프라이트를 교체한다.")]
    public Image BGImage;

    [Tooltip("로드한 배경에 대해 Image.preserveAspect를 켤지 여부")]
    public bool bgPreserveAspect = true;

// (선택) BG 스프라이트 캐시
    private readonly Dictionary<string, Sprite> bgCache = new Dictionary<string, Sprite>();


    private void Awake()
    {
        HideChoices();
        HideImage(StandingImg1);
        HideImage(StandingImg2);
    }

    // ────────────────────── Dialogue Text ──────────────────────
    public void ShowLine(string speaker, string content)
    {
        StopTypingIfAny();
        HideChoices();

        if (speakerText != null)
        {
            bool showSpeaker = !string.IsNullOrEmpty(speaker);
            speakerText.gameObject.SetActive(showSpeaker);
            if (showSpeaker) speakerText.text = speaker;
        }

        if (renderEscapedNewlines && !string.IsNullOrEmpty(content))
            content = content.Replace("\\n", "\n");

        lastFullText = content ?? string.Empty;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        typingCo = StartCoroutine(TypeRoutine(lastFullText));
    }

    private IEnumerator TypeRoutine(string full)
    {
        IsTyping = true;
        if (dialogueText != null) dialogueText.text = string.Empty;

        for (int i = 0; i < full.Length; i++)
        {
            if (dialogueText != null) dialogueText.text += full[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        IsTyping = false;
        typingCo = null;

        var sys = FindObjectOfType<DialogueSystem>();
        if (sys) sys.OnLineFinishedTyping();
    }

    public void SkipTyping()
    {
        if (!IsTyping) return;
        StopTypingIfAny();
        if (dialogueText != null) dialogueText.text = lastFullText;

        var sys = FindObjectOfType<DialogueSystem>();
        if (sys) sys.OnLineFinishedTyping();
    }

    private void StopTypingIfAny()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
            IsTyping = false;
        }
    }

    // ────────────────────── Choices ──────────────────────
    public void SetupChoices(string c1, string c2, string c3,
                             System.Action onClick1, System.Action onClick2, System.Action onClick3)
    {
        SetupChoice(choice1, c1, onClick1);
        SetupChoice(choice2, c2, onClick2);
        SetupChoice(choice3, c3, onClick3);
    }

    public void HideChoices()
    {
        if (choice1) choice1.gameObject.SetActive(false);
        if (choice2) choice2.gameObject.SetActive(false);
        if (choice3) choice3.gameObject.SetActive(false);
    }

    private void SetupChoice(Button btn, string label, System.Action onClick)
    {
        if (!btn) return;

        bool show = !string.IsNullOrEmpty(label) && onClick != null;
        btn.gameObject.SetActive(show);
        if (!show) return;

        var t = btn.GetComponentInChildren<TMP_Text>(true);
        if (t) t.text = label;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick.Invoke());
    }

    // ────────────────────── Standing Images ──────────────────────

    /// <summary>
    /// CSV 레코드로부터 Char1/Char2 정보를 읽어 스탠딩 이미지를 갱신한다.
    /// 규칙:
    /// 1) Char1Name, Char2Name 모두 비면 → 두 이미지 모두 비우기
    /// 2) 한 명만 있으면 → 그 캐릭터를 StandingImg1에 표시(StandingImg2 비움)
    ///    - 위치는 CharXPos("Left/Center/Right") 또는 기본 Center
    /// 3) 둘 다 있으면 → 각각 StandingImg1/StandingImg2에 표시
    ///    - 위치는 각자의 CharXPos 또는 기본 (1:Left, 2:Right)
    /// 스케일: CharXSize("Small|Medium|Large") → 인스펙터 프리셋 적용
    /// </summary>
    public void ApplyStanding(DialogueRecord rec)
    {
        // Char1
        string n1     = rec.GetFirst("Char1Name", "Char1", "Character1", "캐릭터1");
        string look1  = rec.GetFirst("Char1Look", "Char1_Look", "Look1");
        string pos1   = rec.GetFirst("Char1Pos",  "Char1_Pos",  "Pos1");   // Left|Center|Right
        string size1  = rec.GetFirst("Char1Size", "Char1_Size", "Size1");  // Small|Medium|Large
        string eyes1  = rec.GetFirst("Char1Eyes", "Char1_Eyes", "Eyes1");
        string eyesA1 = rec.GetFirst("Char1EyesAnim","Char1_EyesAnim","EyesAnim1");
        string mouth1 = rec.GetFirst("Char1Mouth","Char1_Mouth","Mouth1");
        string etc1   = rec.GetFirst("Char1ETC",  "Char1_ETC",  "ETC1");

        // Char2
        string n2     = rec.GetFirst("Char2Name", "Char2", "Character2", "캐릭터2");
        string look2  = rec.GetFirst("Char2Look", "Char2_Look", "Look2");
        string pos2   = rec.GetFirst("Char2Pos",  "Char2_Pos",  "Pos2");
        string size2  = rec.GetFirst("Char2Size", "Char2_Size", "Size2");
        string eyes2  = rec.GetFirst("Char2Eyes", "Char2_Eyes", "Eyes2");
        string eyesA2 = rec.GetFirst("Char2EyesAnim","Char2_EyesAnim","EyesAnim2");
        string mouth2 = rec.GetFirst("Char2Mouth","Char2_Mouth","Mouth2");
        string etc2   = rec.GetFirst("Char2ETC",  "Char2_ETC",  "ETC2");

        bool has1 = !string.IsNullOrEmpty(n1);
        bool has2 = !string.IsNullOrEmpty(n2);

        if (!has1 && !has2)
        {
            ClearSlot(StandingImg1, ref state1);
            ClearSlot(StandingImg2, ref state2);
            return;
        }

        if (has1 && !has2)
        {
            // 단독 → 기본 Center (시트 Pos 있으면 우선)
            var ap = ResolvePreset(pos1, defaultIfEmpty: "Center");
            ApplySlot(StandingImg1, ref state1, n1, look1, eyes1, eyesA1, mouth1, etc1, ap, size1);
            ClearSlot(StandingImg2, ref state2);
            return;
        }

        if (!has1 && has2)
        {
            // Char2 단독 → 기본 Center
            var ap = ResolvePreset(pos2, defaultIfEmpty: "Center");
            ApplySlot(StandingImg1, ref state1, n2, look2, eyes2, eyesA2, mouth2, etc2, ap, size2);
            ClearSlot(StandingImg2, ref state2);
            return;
        }

        // 둘 다 존재 → 기본 (1:Left, 2:Right), 시트 Pos 있으면 우선
        var ap1 = ResolvePreset(pos1, defaultIfEmpty: "Left");
        var ap2 = ResolvePreset(pos2, defaultIfEmpty: "Right");

        ApplySlot(StandingImg1, ref state1, n1, look1, eyes1, eyesA1, mouth1, etc1, ap1, size1);
        ApplySlot(StandingImg2, ref state2, n2, look2, eyes2, eyesA2, mouth2, etc2, ap2, size2);
    }

    /// <summary>시트의 Pos 문자열을 프리셋으로 변환</summary>
    private Vector2 ResolvePreset(string posField, string defaultIfEmpty)
    {
        string p = (posField ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(p)) p = defaultIfEmpty.ToLowerInvariant();

        switch (p)
        {
            case "left":
                return leftAnchoredPos;
            case "center":
            case "centre":
            case "middle":
                return centerAnchoredPos;
            case "right":
                return rightAnchoredPos;
            default:
                Debug.LogWarning($"[Standing] Unknown Pos '{posField}', fallback to {defaultIfEmpty}.");
                if (defaultIfEmpty.Equals("Left", System.StringComparison.OrdinalIgnoreCase))
                    return leftAnchoredPos;
                if (defaultIfEmpty.Equals("Right", System.StringComparison.OrdinalIgnoreCase))
                    return rightAnchoredPos;
                return centerAnchoredPos;
        }
    }

    /// <summary>시트의 Size 문자열을 스케일 배수로 변환(인스펙터 프리셋 사용)</summary>
    private float ResolveScale(string sizeField)
    {
        if (string.IsNullOrEmpty(sizeField)) return mediumScale;
        switch (sizeField.Trim().ToLowerInvariant())
        {
            case "small":  return smallScale;
            case "large":  return largeScale;
            case "medium":
            default:       return mediumScale;
        }
    }

    /// <summary>
    /// 슬롯에 스탠딩 적용 + 스케일 적용 + 변경 로그
    /// </summary>
    private void ApplySlot(
        Image img, ref StandingState state,
        string name, string look, string eyes, string eyesAnim, string mouth, string etc,
        Vector2 anchoredPos, string sizeField // "Small|Medium|Large"
    )
    {
        if (!img) return;

        var rt = img.rectTransform;
        rt.anchoredPosition = anchoredPos;

        // 스케일 적용
        float scale = ResolveScale(sizeField);
        rt.localScale = Vector3.one * scale;

        // 스프라이트 변경(Name/Look 변할 때만)
        if (state.name != name || state.look != look)
        {
            var spr = ResolveSprite(name, look);
            img.sprite = spr;
            img.enabled = spr != null;
        }

        // // 파츠 변경 로그
        // var changed = state.GetDiff(name, look, eyes, eyesAnim, mouth, etc, sizeField);
        // if (changed != null && changed.Count > 0)
        //     Debug.Log($"[Standing] Slot({img.name}) 변경: " + string.Join(", ", changed));
        
        // 상태 갱신
        state.name = name;
        state.look = look;
        state.eyes = eyes;
        state.eyesAnim = eyesAnim;
        state.mouth = mouth;
        state.etc = etc;
        state.sizeField = sizeField;

        // 표시/비표시
        if (img.sprite != null) ShowImage(img);
        else HideImage(img);
    }

    private void ClearSlot(Image img, ref StandingState state)
    {
        if (!img) return;
        img.sprite = null;
        HideImage(img);
        state = new StandingState(); // 리셋
    }

    private void HideImage(Image img)
    {
        if (!img) return;
        img.enabled = false;
        var cg = img.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 0f;
    }

    private void ShowImage(Image img)
    {
        if (!img) return;
        img.enabled = true;
        var cg = img.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;
    }

    private Sprite ResolveSprite(string name, string look)
    {
        if (string.IsNullOrEmpty(name)) return null;
        string key = string.IsNullOrEmpty(look) ? name : (name + "_" + look);

        if (spriteCache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        string path = string.IsNullOrEmpty(look) ? $"Image/Standing/{name}" : $"Image/Standing/{name}_{look}";
        var spr = Resources.Load<Sprite>(path);
        spriteCache[key] = spr;
        if (spr == null)
            Debug.LogWarning($"[Standing] Sprite not found at Resources/{path}");
        return spr;
    }
    
    /// <summary>
    /// CSV의 BG/Background/배경 열을 읽어 배경 이미지를 교체한다.
    /// 값이 비어있으면 아무 작업도 하지 않는다(현재 배경 유지).
    /// 리소스 경로 규약: Resources/Image/BG/{bgKey}.png
    /// </summary>
    public void ApplyBackground(DialogueRecord rec)
    {
        if (BGImage == null || rec == null) return;

        string bgKey = rec.GetFirst("BG", "Background", "배경")?.Trim();
        if (string.IsNullOrEmpty(bgKey))
            return; // 비었으면 그대로 둠

        // 캐시 조회
        if (!bgCache.TryGetValue(bgKey, out var spr) || spr == null)
        {
            string path = $"Image/BG/{bgKey}";
            spr = Resources.Load<Sprite>(path);
            bgCache[bgKey] = spr;

            if (spr == null)
            {
                Debug.LogWarning($"[BG] Sprite not found at Resources/{path}.png");
                return;
            }
        }

        BGImage.sprite = spr;
        BGImage.enabled = true;
        BGImage.preserveAspect = bgPreserveAspect;
    }


    // ────────────────────── Helpers ──────────────────────
    private struct StandingState
    {
        public string name;
        public string look;
        public string eyes;
        public string eyesAnim;
        public string mouth;
        public string etc;
        public string sizeField; // Small/Medium/Large
    
        public List<string> GetDiff(string n, string l, string e, string ea, string m, string et, string s)
        {
            List<string> list = new List<string>();
            if (name != n) list.Add($"Name: {name} → {n}");
            if (look != l) list.Add($"Look: {look} → {l}");
            if (eyes != e) list.Add($"Eyes: {eyes} → {e}");
            if (eyesAnim != ea) list.Add($"EyesAnim: {eyesAnim} → {ea}");
            if (mouth != m) list.Add($"Mouth: {mouth} → {m}");
            if (etc != et) list.Add($"ETC: {etc} → {et}");
            if (sizeField != s && !string.IsNullOrEmpty(s)) list.Add($"Scale: {sizeField} → {s}");
            return list;
        }
    }
    
    // 선택지 표시 상태 확인용 헬퍼
    public bool HasActiveChoices()
    {
        bool a = choice1 && choice1.gameObject.activeSelf;
        bool b = choice2 && choice2.gameObject.activeSelf;
        bool c = choice3 && choice3.gameObject.activeSelf;
        return a || b || c;
    }

}
