# Iyagi AI VN Generator - Technical Overview

> **AI가 시나리오와 캐릭터를 자동 생성하는 비주얼노벨 제작 도구**
> Unity 2022.3.4f1 / Gemini API / NanoBanana API

---

## 🎯 프로젝트 개요

Iyagi AI VN Generator는 최소한의 입력(제목 + 줄거리)만으로 완전한 비주얼노벨을 자동 생성하는 Unity 기반 도구입니다.

### 핵심 목표

1. **최소 입력으로 완전한 VN 생성**: 제목 + 줄거리만 입력하면 전체 게임 생성
2. **일관된 캐릭터 비주얼**: Seed 기반 이미지 생성으로 동일 캐릭터 유지
3. **동적 스토리 분기**: 플레이어 선택에 따라 실시간 챕터 생성
4. **빠른 프로토타이핑**: 개발자가 아이디어를 즉시 테스트 가능
5. **효율적 리소스 관리**: 초기 생성 + 필요 시 추가 생성 + 재사용 최대화

---

## 📐 시스템 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│                    Setup Wizard (Editor)                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ STEP 1   │→ │ STEP 2   │→ │ STEP 3   │→ │ STEP 4-6 │   │
│  │ 게임개요  │  │ 가치설정  │  │ 구조설정  │  │ 캐릭터   │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│         ↓ Gemini API                ↓ NanoBanana API       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            VNProjectData.asset (ScriptableObject)    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    Runtime Game System                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          ChapterGenerationManager                     │  │
│  │  - Scene-based generation (3 scenes per chapter)     │  │
│  │  - Parallel asset generation                         │  │
│  └─────────────┬────────────────────────────────────────┘  │
│                ↓ Gemini API                                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         AIDataConverter                              │  │
│  │  - FromAIJson(string) → List<DialogueRecord>         │  │
│  └─────────────┬────────────────────────────────────────┘  │
│                ↓                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         GameController                                │  │
│  │  - Core Value System (derived skills → core values) │  │
│  │  - Choice handling & state management               │  │
│  │  - SaveFile auto-update                             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 핵심 기술 스택

| 영역 | 사용 기술 | 역할 |
|------|----------|------|
| **엔진** | Unity 2022.3.4f1 | 전체 개발 환경 |
| **시나리오 생성** | Gemini 1.5 Flash API | 플롯, 대사, 분기 자동 생성 |
| **이미지 생성** | NanoBanana API (가정) | 캐릭터 얼굴/스탠딩 이미지 생성 |
| **오디오 생성** | ElevenLabs API (선택) | BGM/SFX 생성 |
| **데이터 저장** | ScriptableObject + JSON | 프로젝트 설정 및 세이브 파일 |
| **UI** | Unity UI (uGUI) | 위자드 및 게임 UI |

---

## 📚 상세 문서

### 시스템 설계 (systemdocs/)

- **[데이터 구조](systemdocs/data-structures.md)** - VNProjectData, CharacterData, SaveFile 등
- **[API 통합](systemdocs/api-integration.md)** - Gemini, NanoBanana, ElevenLabs API 클라이언트
- **[이미지 생성](systemdocs/image-generation.md)** - 캐릭터/배경/CG 생성 파이프라인
- **[챕터 생성](systemdocs/chapter-generation.md)** - Scene-based generation 시스템
- **[세이브/로드](systemdocs/save-load-system.md)** - 프로젝트 슬롯 및 SaveFile 관리
- **[리소스 관리](systemdocs/resource-management.md)** - 폴더 구조 및 리소스 재사용 전략
- **[개발 도구](systemdocs/development-tools.md)** - F5 Auto-Fill, GameScene 자동 설정 등
- **[구현 히스토리](systemdocs/implementation-history.md)** - 주요 변경사항 기록

### 사용자 가이드 (userdocs/)

- **[README](userdocs/README.md)** - 프로젝트 소개 및 시작 가이드
- **[설치 가이드](userdocs/SETUP_GUIDE.md)** - Unity 프로젝트 설정
- **[씬 설정 가이드](userdocs/SCENE_SETUP_GUIDE.md)** - TitleScene, GameScene 설정

---

## 🚀 빠른 시작

1. **API 키 설정**: `Assets/Resources/APIConfig.asset` 생성 및 API 키 입력
2. **Setup Wizard 실행**: Unity Editor > Iyagi > Setup Wizard
3. **프로젝트 생성**: F5로 각 단계 자동 완성 (테스트 모드)
4. **게임 플레이**: TitleScene에서 프로젝트 선택 → SaveFile 선택 → 게임 시작

자세한 내용은 [userdocs/SETUP_GUIDE.md](userdocs/SETUP_GUIDE.md)를 참조하세요.

---

## 📝 최신 업데이트

### Core Value System (2025-01-11)
- **변경**: 선택지가 Core Value를 직접 수정하지 않음
- **새 로직**: 선택지 → Derived Skills 증가 → Core Value = Derived Skills 합계
- **예시**: "용기" Core Value = "검술" + "방어" + "돌격" 스킬 합계

### SaveFile Auto-Update (2025-01-11)
- 챕터 완료 시 자동으로 SaveFile 업데이트
- 저장 내용: currentChapter, gameState (skills, values, affections), lastPlayedDate

### Scene-Based Chapter Generation (2025-01-11)
- JSON 잘림 문제 해결을 위해 챕터를 3개 씬으로 분할 생성
- 각 씬당 3-5개 대사만 생성하여 안정성 향상

### Character Speech Style System (2025-01-13)
- Setup Wizard에서 캐릭터별 말투 예시 입력 가능
- LLM 챕터 생성 시 각 캐릭터의 말투 예시를 프롬프트에 포함
- 캐릭터별 일관된 말투 유지

### Major-Choice-Driven Branching (2025-01-14)
- 기존 Alternating Branching을 Major-Choice-Driven Branching으로 전면 교체
- 이벤트 기반 분기로 몰입감 유지 (정량적 점수 비교 → 정성적 플래그)
- 분기 수 감소: 32개 → 10-20개 (6챕터 기준)
- FlagImpact 데이터 구조 추가 (AIDataConverter, GameController)
- 자세한 내용은 [BRANCHING_SOLUTION.md](BRANCHING_SOLUTION.md) 참조

자세한 내용은 [systemdocs/implementation-history.md](systemdocs/implementation-history.md)를 참조하세요.

---

## ⚠️ 중요: 선택지 버그 방지 가이드

### 문제: 선택지 클릭 시 잘못된 선택지 처리

**증상**: 사용자가 선택지 A를 클릭했는데 선택지 B의 결과가 나타남

**원인**: Unity UI Button의 onClick 리스너가 **한 번만 등록**되고, 선택지 개수가 변할 때마다 **갱신되지 않아** 발생하는 클로저 문제

#### 잘못된 구현 (❌ 절대 금지!)

```csharp
// DialogueUI.cs - Start() 메서드
void Start() {
    for (int i = 0; i < choiceButtons.Length; i++) {
        int index = i; // 클로저 캡처
        choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(index));
    }
}

// DisplayChoices() 메서드
void DisplayChoices(DialogueRecord record) {
    for (int i = 0; i < choiceButtons.Length; i++) {
        if (i < choiceCount) {
            choiceButtons[i].gameObject.SetActive(true); // ❌ 리스너 재등록 안 함!
        }
    }
}
```

**문제점**:
1. Start()에서 4개 버튼 모두 리스너 등록 (index = 0, 1, 2, 3)
2. 첫 번째 씬: 선택지 2개만 표시 → 버튼 0, 1 활성화
3. 두 번째 씬: 선택지 3개 표시 → 버튼 0, 1, 2 활성화
4. **버그 발생**: 버튼 2를 클릭했는데 Start()에서 등록한 잘못된 리스너가 호출됨

#### 올바른 구현 (✅ 필수!)

```csharp
// DisplayChoices() 메서드
void DisplayChoices(DialogueRecord record) {
    int choiceCount = record.GetChoiceCount();

    for (int i = 0; i < choiceButtons.Length; i++) {
        // ✅ 기존 리스너 완전히 제거
        choiceButtons[i].onClick.RemoveAllListeners();

        if (i < choiceCount) {
            int capturedIndex = i; // 클로저 캡처 (매번 새로 생성!)
            string choiceText = record.GetString($"Choice{i + 1}");

            // ✅ 리스너 다시 등록 (현재 선택지에 맞춰)
            choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(capturedIndex));
            choiceTexts[i].text = choiceText;
            choiceButtons[i].gameObject.SetActive(true);
        } else {
            choiceButtons[i].gameObject.SetActive(false);
        }
    }
}
```

#### 핵심 원칙

1. **선택지를 표시할 때마다 리스너를 재등록**해야 함
2. `RemoveAllListeners()` → `AddListener()` 순서로 처리
3. 클로저 변수는 **for 루프 안에서 매번 새로 캡처** (`int capturedIndex = i;`)
4. Start()에서 한 번만 등록하는 방식은 **절대 금지**

#### 테스트 체크리스트

- [ ] 선택지 2개 → 3개 → 2개로 변하는 시나리오 테스트
- [ ] 각 선택지 클릭 시 Debug.Log로 올바른 index 출력 확인
- [ ] 연속된 선택지 씬에서 모든 버튼이 올바르게 동작하는지 확인

#### 관련 파일

- [Assets/Script/Runtime/DialogueUI.cs](Assets/Script/Runtime/DialogueUI.cs) - 선택지 표시 및 리스너 등록
- [Assets/Script/Runtime/GameController.cs](Assets/Script/Runtime/GameController.cs) - OnChoiceSelected(int choiceIndex) 처리

---

## 🎯 Major-Choice-Driven Branching (주요 선택 기반 분기 시스템)

### 설계 목표

**문제 1 - 완전 수렴**: 모든 챕터가 동일하면 몰입감 파괴
- Chapter 2에서 "엘런 배신" 선택 → Chapter 3에서 엘런이 친근하게 대함
- ❌ 플레이어가 이질감 느낌

**문제 2 - 교차 분기**: 정량적 점수 기준은 의미 없음
- 용기 49 vs 50의 차이는 플레이어가 인지 불가
- 6챕터 기준: 32개 루트 (여전히 많음)

**해결 - 주요 선택 분기**: 이벤트 기반, 제한적 분기
- **기본 스토리는 모든 플레이어에게 동일**
- **주요 선택(Major Choice) 시에만 분기** (챕터당 0-1개)
- 정량적 점수 대신 정성적 이벤트 플래그 사용
- 6챕터 기준: **10-20개 루트** (실용적)

### 분기 구조

```
Chapter 1: 공통 프롤로그 (1개)
    ↓
Chapter 2:
┌─────────────────┴─────────────────┐
일반 선택              주요 선택 (배신)
(통계만 변경)           "betrayed_Ellen"
    ↓                       ↓
Chapter 3:            Chapter 3 (배신 후):
엘런 친밀                엘런 냉랭, 적대적
    ↓                       ↓
Chapter 4: 수렴 가능      Chapter 4: 배신 유지
    ↓                       ↓
         └──────┬───────────┘
               ↓
        Chapter 5...

최종 분기 수: 10-20개 (주요 선택 조합)
```

### 캐시 키 생성 로직

**핵심**: 주요 플래그만 캐시 키에 포함

```csharp
// ChapterGenerationManager.cs
private string GenerateCacheKey(int chapterId, GameStateSnapshot state)
{
    string projectId = projectData.projectGuid;

    // Chapter 1: 항상 동일 (프롤로그)
    if (chapterId == 1) return $"{projectId}_Ch1";

    // 주요 선택 플래그만 추출
    string majorFlags = GetMajorFlagsForBranching(state);

    // 플래그가 없으면 기본 경로
    if (majorFlags == "none")
        return $"{projectId}_Ch{chapterId}";

    // 플래그가 있으면 분기 경로
    return $"{projectId}_Ch{chapterId}_{majorFlags}";
}

// 주요 플래그만 추출
private string GetMajorFlagsForBranching(GameStateSnapshot state)
{
    if (state == null || state.flags == null) return "none";

    var majorFlags = state.flags
        .Where(kvp => kvp.Value && IsMajorFlag(kvp.Key))
        .Select(kvp => kvp.Key)
        .OrderBy(f => f)  // 안정적인 캐시 키
        .Take(5)          // 최대 5개 (폭발 방지)
        .ToList();

    return majorFlags.Count == 0 ? "none" : string.Join("_", majorFlags);
}

// Major Flag 판별
private bool IsMajorFlag(string flag)
{
    string[] majorPrefixes = {
        "betrayed_",    // 배신
        "saved_",       // 구출/보호
        "killed_",      // 살해
        "romance_",     // 로맨스 루트
        "allied_",      // 동맹
        "rejected_",    // 거절/결별
        "revealed_",    // 비밀 폭로
        "sacrificed_"   // 희생
    };
    return majorPrefixes.Any(prefix => flag.StartsWith(prefix));
}
```

### LLM 프롬프트 - Major Choice 생성 지침

**프롬프트 예시**:
```
**CRITICAL - Major Choice Flag System**:
- Use SPARINGLY (0-1 per chapter) to avoid branching explosion
- Major Flags cause future chapters to branch

**When to use Major Flags**:
✅ Betraying a key character ("betrayed_Ellen")
✅ Entering romance route ("romance_Alice")
✅ Major plot decision ("saved_village")
❌ Minor stat-affecting choices (no flag needed)

**Example - Major Choice**:
{
  "text": "Betray Ellen to save yourself",
  "flag_impact": [{"flag_name": "betrayed_Ellen", "value": true}],
  "skill_impact": [{"skill_name": "Survival", "change": 15}],
  "affection_impact": [{"character_name": "Ellen", "change": -30}]
}

**Example - Normal Choice** (no flag):
{
  "text": "Help Ellen with her task",
  "skill_impact": [{"skill_name": "Empathy", "change": 12}],
  "affection_impact": [{"character_name": "Ellen", "change": 10}]
}
```

### 데이터 구조

```csharp
// AIDataConverter.cs
[System.Serializable]
public class ChoiceData
{
    public string text;
    public int next_id;
    public SkillImpact[] skill_impact;
    public AffectionImpact[] affection_impact;
    public FlagImpact[] flag_impact;  // ← 새로 추가
}

[System.Serializable]
public class FlagImpact
{
    public string flag_name;  // 예: "betrayed_Ellen"
    public bool value;        // true = 설정, false = 제거
}
```

```csharp
// GameController.cs - 플래그 처리
foreach (var key in allKeys)
{
    if (key.StartsWith($"Choice{choiceIndex + 1}_FlagImpact_"))
    {
        string flagName = key.Substring($"Choice{choiceIndex + 1}_FlagImpact_".Length);
        bool flagValue = bool.Parse(currentLine.GetString(key));
        currentState.flags[flagName] = flagValue;
    }
}
```

### 장점

1. **몰입감 유지**: 주요 선택의 결과가 다음 챕터에 실제로 반영됨
2. **실용적 분기 수**: 10-20개 (6챕터 × 0-1 주요 선택)
3. **이벤트 기반**: 의미있는 선택만 분기 ("배신했다/안 했다")
4. **캐시 효율**: 같은 플래그 조합이면 재사용
5. **확장 용이**: 새로운 주요 선택 추가 시 선형 증가

### 비교표

| 접근법 | 분기 수 | 몰입감 | 선택 무게 | API 비용 | 구현 복잡도 |
|--------|---------|--------|----------|----------|------------|
| 완전 수렴 | 1/챕터 | ❌ 파괴 | ❌ 없음 | 최소 | 낮음 |
| 교차 분기 | 32개 | ⚠️ 보통 | ⚠️ 점수 | 중간 | 높음 |
| **주요 선택** | **10-20개** | **✅ 유지** | **✅ 이벤트** | **중간** | **중간** |

### 관련 파일

- [Assets/Script/Runtime/ChapterGenerationManager.cs](Assets/Script/Runtime/ChapterGenerationManager.cs#L780-L876) - 캐시 키 생성
- [Assets/Script/Runtime/GameController.cs](Assets/Script/Runtime/GameController.cs#L466-L491) - 플래그 처리
- [Assets/Script/AISystem/AIDataConverter.cs](Assets/Script/AISystem/AIDataConverter.cs#L303-L340) - FlagImpact 데이터 구조
- [BRANCHING_SOLUTION.md](BRANCHING_SOLUTION.md) - 3가지 접근법 상세 비교

---

## 🗣️ Character Speech Style System (말투 시스템)

### 개요

각 캐릭터의 고유한 말투를 일관되게 유지하기 위해, Setup Wizard에서 캐릭터별 **말투 예시(Sample Dialogue)**를 입력받아 LLM 챕터 생성 시 프롬프트에 포함합니다.

### 구현 방식

#### 1. 데이터 구조

```csharp
// CharacterData.cs
public class CharacterData : ScriptableObject
{
    [Header("Speech Style")]
    [TextArea(2, 4)]
    public string sampleDialogue; // 말투 예시 (1-2문장)
}
```

#### 2. Setup Wizard 입력

**Step 4 (Player Character) / Step 5 (NPCs)**:
- `sampleDialogueInput` 필드 추가
- 사용자가 캐릭터별로 대표적인 대사 1-2문장 입력
- 예시:
  - 플레이어: "이건 내 방식이야. 이기든, 지든 내 선택으로 끝내겠어."
  - NPC: "그건 좀 무리야. 차라리 이렇게 해보는 건 어때?"

#### 3. LLM 프롬프트 생성

```csharp
// ChapterGenerationManager.cs - BuildScenePrompt()
string characterList = "";

// Player 캐릭터
characterList += $"\n  - Player: {playerCharacter.characterName}";
if (!string.IsNullOrEmpty(playerCharacter.sampleDialogue))
{
    characterList += $"\n    Speech Style: \"{playerCharacter.sampleDialogue}\"";
}

// NPCs
foreach (var npc in npcs)
{
    characterList += $"\n  - NPC: {npc.characterName}";
    if (!string.IsNullOrEmpty(npc.sampleDialogue))
    {
        characterList += $"\n    Speech Style: \"{npc.sampleDialogue}\"";
    }
}
```

**프롬프트 예시**:
```
# Game Information
- Characters:
  - Player: 이시혁
    Speech Style: "이건 내 방식이야. 이기든, 지든 내 선택으로 끝내겠어."
  - NPC: 유해리
    Speech Style: "그건 좀 무리야. 차라리 이렇게 해보는 건 어때?"
```

### 효과

1. **일관된 캐릭터성**: LLM이 각 캐릭터의 말투를 참고하여 대사 생성
2. **작가 의도 반영**: 사용자가 원하는 캐릭터 성격을 말투로 표현 가능
3. **빠른 설정**: 긴 성격 설명 대신 1-2문장으로 말투 정의

### 말투 예시 작성 가이드

- **길이**: 1-2문장 (짧고 명확하게)
- **특징 강조**: 존댓말/반말, 말끝 습관, 어휘 선택 등
- **감정 표현**: 캐릭터의 기본 태도나 성격 드러내기

**좋은 예시**:
- "네, 알겠습니다! 제가 도와드릴게요!" (밝고 적극적인 성격)
- "...그럴 수도 있겠네. 뭐, 상관없지만." (무덤덤하고 소극적)
- "하! 웃기는 소리 하고 있네. 네 실력으로?" (도발적이고 자신감 넘침)

**나쁜 예시**:
- "안녕하세요." (너무 평범, 특징 없음)
- "저는 친절하고 착한 사람입니다." (설명문, 대사가 아님)

### 관련 파일

- [Assets/Script/Runtime/CharacterData.cs](Assets/Script/Runtime/CharacterData.cs#L37-L39) - sampleDialogue 필드
- [Assets/Script/SetupWizard/Step4_PlayerCharacter.cs](Assets/Script/SetupWizard/Step4_PlayerCharacter.cs#L27) - 플레이어 말투 입력
- [Assets/Script/SetupWizard/Step5_NPCs.cs](Assets/Script/SetupWizard/Step5_NPCs.cs#L27) - NPC 말투 입력
- [Assets/Script/Runtime/ChapterGenerationManager.cs](Assets/Script/Runtime/ChapterGenerationManager.cs#L242-L258) - 프롬프트에 말투 포함

---

**Last Updated**: 2025-01-13
**Document Version**: 3.3 (Character Speech Style System Added)
