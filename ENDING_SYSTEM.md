# 엔딩 및 친밀도 시스템 설계 문서

> **Iyagi AI VN Generator - Ending & Affection System**
>
> Core Value 기반 엔딩 + Romance Achievement 시스템

---

## 📊 시스템 개요

### 핵심 원칙

1. **Core Value**: 스토리 분기 결정 (챕터 캐싱 키)
2. **Affection (친밀도)**: NPC 반응 톤 + Romance Achievement 결정 (캐싱 키 제외)
3. **분리된 엔딩 판정**:
   - **메인 엔딩**: Value 기반 (True/Value/Normal)
   - **Romance Achievement**: 친밀도 80+ NPC들 (별도 표시)
4. **조합 폭발 방지**: N개 Value 엔딩 + M개 Romance Achievement (독립적)

---

## 🎯 엔딩 결정 시스템

### 엔딩 타입 (3가지)

1. **트루 엔딩 (TrueEnding)** ✨
   - 조건: 특정 Core Value 70+
   - 설정: `VNProjectData.trueValueName` (예: "정의")
   - 설명: 가장 이상적인 결말, 특정 가치를 완벽히 구현

2. **Value 엔딩 (ValueEnding)** ⭐
   - 조건: Dominant Core Value 60+
   - 예시: "Justice Ending", "Ambition Ending"
   - 설명: 플레이어가 추구한 가치에 따른 엔딩

3. **일반 엔딩 (NormalEnding)** 🌟
   - 조건: 모든 조건 미달 (Value 60 미만)
   - 설명: 뚜렷한 방향 없이 끝나는 평범한 엔딩

### Romance Achievement (별도 판정)

- **조건**:
  - NPC 호감도 80+
  - `CharacterData.isRomanceable = true`
- **특징**:
  - 메인 엔딩과 독립적으로 표시
  - 여러 NPC와 동시에 Achievement 가능
  - 엔딩 텍스트에 추가로 표시됨

**예시**:
```
[Main Ending]
Your journey ends with Justice as your guiding principle.

[Romance Achievements]
✨ You have formed a deep bond with Hans.
✨ You have formed a deep bond with Emma.
```

---

## 💕 친밀도 시스템 (Affection System)

### 데이터 구조

```csharp
// GameStateSnapshot.cs
public class GameStateSnapshot
{
    // Core Values (챕터 캐싱에 사용)
    public Dictionary<string, int> coreValueScores; // "정의": 50, "출세": 30

    // Affection (캐싱 키에서 제외)
    public Dictionary<string, int> characterAffections; // "Hans": 85, "Emma": 45
}
```

### 친밀도 범위

| 범위 | 상태 | NPC 태도 | 효과 |
|------|------|---------|------|
| **80~100** | 매우 높음 | 친밀하고 따뜻한 태도 | Romance Achievement 획득 |
| **50~79** | 높음 | 우호적이고 협력적 | 일반 대화 |
| **30~49** | 보통 | 중립적, 예의 바른 태도 | 일반 대화 |
| **0~29** | 낮음 | 거리를 두는 태도 | 냉담한 대화 |
| **-100~-1** | 매우 낮음 | 적대적이거나 회피 | (미구현) |

### 친밀도 변화 방식

#### 1. 선택지를 통한 변화

**AI JSON 포맷**:
```json
{
  "text": "Hans에게 진실을 말한다",
  "value_impact": [
    {"value_name": "정의", "change": 10}
  ],
  "affection_impact": [
    {"character_name": "Hans", "change": 15}
  ],
  "next_id": 1050
}
```

**DialogueRecord 변환**:
```csharp
// AIDataConverter.cs
record.Fields["Choice1_ENG"] = "Hans에게 진실을 말한다";
record.Fields["Choice1_ValueImpact_정의"] = "10";
record.Fields["Choice1_AffectionImpact_Hans"] = "15"; // ✅
```

**GameController 적용**:
```csharp
public void OnChoiceSelected(int choiceIndex)
{
    // 1. Core Value 업데이트 (챕터 분기에 영향)
    string valueKey = $"Choice{choiceIndex}_ValueImpact_정의";
    if (currentRecord.Fields.ContainsKey(valueKey))
    {
        int change = int.Parse(currentRecord.Fields[valueKey]);
        currentGameState.coreValueScores["정의"] += change;
    }

    // 2. Affection 업데이트 (엔딩에만 영향)
    string affectionKey = $"Choice{choiceIndex}_AffectionImpact_Hans";
    if (currentRecord.Fields.ContainsKey(affectionKey))
    {
        int change = int.Parse(currentRecord.Fields[affectionKey]);
        currentGameState.characterAffections["Hans"] += change;

        Debug.Log($"Hans 호감도: {currentGameState.characterAffections["Hans"]}");
    }
}
```

#### 2. AI가 친밀도를 참고하여 대사 생성

**ChapterGenerationManager 프롬프트**:
```
# Current State
Core Values: 정의=50, 출세=30
Character Affections: Hans=85, Emma=45

Note on Affection System:
- affection_impact in choices should reflect how the choice affects NPC relationships
- Affection does NOT affect chapter branching (only Core Values do)
- Affection is used for dialogue tone and ending determination only
- You can reference current affection scores when writing NPC dialogue/reactions
```

**AI 대사 생성 예시**:
- **Hans (호감도 85)**: "You always know the right thing to say. I'm glad we met." (따뜻하고 친밀한 톤)
- **Emma (호감도 45)**: "I see. Well, do what you think is best." (중립적이고 예의바른 톤)

---

## 🎮 엔딩 판정 플로우

### EndingManager.DetermineEnding() 순서

```csharp
public EndingResult DetermineEnding(GameStateSnapshot state)
{
    EndingResult result = new EndingResult();

    // 1. Dominant Core Value 찾기
    string dominantValue = GetDominantCoreValue(state);

    // 2. 트루 엔딩 체크 (특정 Value 70+)
    if (IsTrueEnding(state, dominantValue))
    {
        result.endingType = EndingType.TrueEnding;
        result.endingTitle = "True Ending";
        result.endingDescription = $"You have mastered {dominantValue}...";
    }
    // 3. Value 엔딩 체크 (Dominant Value 60+)
    else if (IsValueEnding(state, dominantValue))
    {
        result.endingType = EndingType.ValueEnding;
        result.endingTitle = $"{dominantValue} Ending";
        result.endingDescription = $"Your journey ends with {dominantValue}...";
    }
    // 4. 일반 엔딩 (모든 조건 미달)
    else
    {
        result.endingType = EndingType.NormalEnding;
        result.endingTitle = "Normal Ending";
        result.endingDescription = "Your journey ends...";
    }

    // 5. Romance Achievement 체크 (별도 판정)
    result.romanceCharacters = GetRomanceCharacters(state);

    return result;
}
```

### GetRomanceCharacters() 구현

```csharp
private List<string> GetRomanceCharacters(GameStateSnapshot state)
{
    List<string> romanceChars = new List<string>();

    if (state.characterAffections == null) return romanceChars;

    foreach (var kvp in state.characterAffections)
    {
        string npcName = kvp.Key;
        int affection = kvp.Value;

        // 1. 호감도 80 이상인지 확인
        if (affection < 80) continue;

        // 2. 로맨스 가능한 NPC인지 확인
        var npc = projectData.npcs.Find(n => n.characterName == npcName);
        if (npc == null || !npc.isRomanceable) continue;

        romanceChars.Add(npcName);
    }

    return romanceChars;
}
```

---

## 📊 엔딩 예시

### 예시 1: Justice Value Ending + 2 Romance Achievements

**게임 상태**:
- Core Values: 정의=65, 출세=40, 명예=30
- Affections: Hans=85, Emma=82, Leo=45

**판정 결과**:
```
[EndingType] ValueEnding
[Title] Justice Ending
[Description] Your journey ends with Justice as your guiding principle.

[Romance Achievements]
✨ Hans (Affection: 85)
✨ Emma (Affection: 82)
```

### 예시 2: True Ending + 1 Romance Achievement

**게임 상태**:
- Core Values: 정의=75 (True Value), 출세=50, 명예=40
- Affections: Hans=90, Emma=60

**판정 결과**:
```
[EndingType] TrueEnding
[Title] True Ending
[Description] You have mastered Justice and reached the ultimate ending.

[Romance Achievement]
✨ Hans (Affection: 90)
```

### 예시 3: Normal Ending + 0 Romance

**게임 상태**:
- Core Values: 정의=45, 출세=50, 명예=40 (모두 60 미만)
- Affections: Hans=60, Emma=55

**판정 결과**:
```
[EndingType] NormalEnding
[Title] Normal Ending
[Description] Your journey ends, though your path remains unclear.

[Romance Achievements]
(없음)
```

---

## 🎨 엔딩 UI 구현 가이드

### EndingUI 컴포넌트

```csharp
public class EndingUI : MonoBehaviour
{
    [Header("Main Ending")]
    public TMP_Text endingTitleText;
    public TMP_Text endingDescriptionText;

    [Header("Romance Achievements")]
    public GameObject romancePanel;
    public Transform romanceListPanel;
    public GameObject romanceItemPrefab;

    public void DisplayEnding(EndingResult result)
    {
        // 1. 메인 엔딩 표시
        endingTitleText.text = result.endingTitle;
        endingDescriptionText.text = result.endingDescription;

        // 2. Romance Achievement 표시
        if (result.romanceCharacters.Count > 0)
        {
            romancePanel.SetActive(true);

            foreach (string npcName in result.romanceCharacters)
            {
                GameObject item = Instantiate(romanceItemPrefab, romanceListPanel);
                item.GetComponentInChildren<TMP_Text>().text = $"✨ {npcName}";
            }
        }
        else
        {
            romancePanel.SetActive(false);
        }
    }
}
```

### 레이아웃 예시

```
┌─────────────────────────────────────────┐
│          [Ending Title]                 │
│          Justice Ending                 │
├─────────────────────────────────────────┤
│                                         │
│  Your journey ends with Justice as      │
│  your guiding principle. You have       │
│  fought for what is right, even when    │
│  the cost was high.                     │
│                                         │
├─────────────────────────────────────────┤
│  [Romance Achievements]                 │
│                                         │
│  ✨ You have formed a deep bond with    │
│     Hans.                               │
│                                         │
│  ✨ You have formed a deep bond with    │
│     Emma.                               │
│                                         │
└─────────────────────────────────────────┘
```

---

## 🔄 챕터 캐싱과의 관계

### 캐시 키 생성 (친밀도 제외)

```csharp
// GameStateSnapshot.GetCacheHash()
public string GetCacheHash()
{
    // Core Value만 사용 (친밀도 제외!)
    var sortedList = new List<KeyValuePair<string, int>>(coreValueScores);
    sortedList.Sort((a, b) => string.Compare(a.Key, b.Key));

    var roundedValues = new List<string>();
    foreach (var kv in sortedList)
    {
        int roundedValue = (kv.Value / 10) * 10; // 10단위 반올림
        roundedValues.Add($"{kv.Key}:{roundedValue}");
    }

    string stateString = string.Join(",", roundedValues.ToArray());
    return stateString.GetHashCode().ToString("X8");
}
```

**결과**:
- 같은 Core Value 루트에서 다양한 친밀도 조합 가능
- 예: "정의=60, 출세=30" 루트에서 Hans 호감도만 다른 플레이 → 같은 챕터 재사용

---

## 📝 구현 체크리스트

### 완료된 작업 ✅

- [x] `GameStateSnapshot.characterAffections` 필드 추가
- [x] `AIDataConverter.AffectionImpact` 파싱
- [x] `EndingManager.DetermineEnding()` 로직 변경 (Value 우선 + Romance 추가)
- [x] `EndingResult.romanceCharacters` 리스트 추가
- [x] `EndingManager.GetRomanceCharacters()` 메서드 작성
- [x] `EndingType` enum 3개로 축소 (TrueEnding, ValueEnding, NormalEnding)
- [x] `VNProjectData.trueValueName` 필드 추가

### 미구현 작업 🚧

- [ ] `GameController.OnChoiceSelected()` - Affection 업데이트 적용
- [ ] `EndingUI` 컴포넌트 작성 (메인 엔딩 + Romance Achievement 표시)
- [ ] `ChapterGenerationManager` 프롬프트에 Affection 가이드라인 추가 (이미 포함됨)
- [ ] Setup Wizard Step2에서 `trueValueName` 선택 UI 추가
- [ ] 엔딩 CG 표시 (선택적)

---

## 🎚️ 밸런스 조절 시스템

### 조절 가능한 파라미터

#### 1. EndingManager 임계값

```csharp
[Header("Ending Thresholds")]
public int trueEndingThreshold = 70;      // 트루 엔딩 최소 점수
public int valueEndingThreshold = 60;     // Value 엔딩 최소 점수
public int romanceThreshold = 80;         // Romance Achievement 최소 호감도
```

#### 2. Unity Editor 밸런스 계산기

```csharp
// Assets/Script/Editor/EndingBalanceCalculator.cs
using UnityEngine;
using UnityEditor;

public class EndingBalanceCalculator : EditorWindow
{
    [MenuItem("Iyagi/Balance Calculator")]
    static void ShowWindow()
    {
        GetWindow<EndingBalanceCalculator>("Ending Balance Calculator");
    }

    private VNProjectData projectData;
    private EndingManager endingManager;

    void OnGUI()
    {
        GUILayout.Label("Ending Balance Calculator", EditorStyles.boldLabel);

        projectData = (VNProjectData)EditorGUILayout.ObjectField("Project Data", projectData, typeof(VNProjectData), false);
        endingManager = (EndingManager)EditorGUILayout.ObjectField("Ending Manager", endingManager, typeof(EndingManager), true);

        if (projectData == null || endingManager == null) return;

        EditorGUILayout.Space();
        GUILayout.Label("Current Thresholds", EditorStyles.boldLabel);

        EditorGUILayout.IntField("True Ending", endingManager.trueEndingThreshold);
        EditorGUILayout.IntField("Value Ending", endingManager.valueEndingThreshold);
        EditorGUILayout.IntField("Romance Achievement", endingManager.romanceThreshold);

        EditorGUILayout.Space();

        // 최대 점수 계산 (총 챕터 수 × 선택지당 평균 증가량 × 선택지 수)
        int totalChapters = projectData.totalChapters;
        int avgValueIncrease = 10; // 선택지당 평균 Value 증가량
        int choicesPerChapter = 2; // 챕터당 선택지 수 (가정)
        int maxValueScore = totalChapters * avgValueIncrease * choicesPerChapter;

        GUILayout.Label($"Estimated Max Value Score: {maxValueScore}", EditorStyles.helpBox);

        EditorGUILayout.Space();

        if (GUILayout.Button("임계값 자동 조정 (70% 기준)"))
        {
            endingManager.trueEndingThreshold = (int)(maxValueScore * 0.7f);
            endingManager.valueEndingThreshold = (int)(maxValueScore * 0.5f);
            EditorUtility.SetDirty(endingManager);
            Debug.Log($"Thresholds adjusted: True={endingManager.trueEndingThreshold}, Value={endingManager.valueEndingThreshold}");
        }
    }
}
```

#### 3. 난이도 프리셋

```csharp
// EndingManager.cs에 추가
[Header("Difficulty Presets")]
public DifficultyPreset currentDifficulty = DifficultyPreset.Normal;

public enum DifficultyPreset
{
    Easy,    // True=50, Value=40, Romance=60
    Normal,  // True=70, Value=60, Romance=80
    Hard     // True=90, Value=80, Romance=90
}

void Start()
{
    ApplyDifficultyPreset(currentDifficulty);
}

void ApplyDifficultyPreset(DifficultyPreset preset)
{
    switch (preset)
    {
        case DifficultyPreset.Easy:
            trueEndingThreshold = 50;
            valueEndingThreshold = 40;
            romanceThreshold = 60;
            break;
        case DifficultyPreset.Normal:
            trueEndingThreshold = 70;
            valueEndingThreshold = 60;
            romanceThreshold = 80;
            break;
        case DifficultyPreset.Hard:
            trueEndingThreshold = 90;
            valueEndingThreshold = 80;
            romanceThreshold = 90;
            break;
    }

    Debug.Log($"[EndingManager] Difficulty set to {preset}");
}
```

---

## 🔗 관련 파일

| 파일 | 역할 |
|------|------|
| [EndingManager.cs](Assets/Script/Runtime/EndingManager.cs) | 엔딩 판정 로직 |
| [GameStateSnapshot.cs](Assets/Script/Runtime/GameStateSnapshot.cs) | 게임 상태 저장 (Core Value + Affection) |
| [AIDataConverter.cs](Assets/Script/AISystem/AIDataConverter.cs) | AI JSON → DialogueRecord 변환 (affection_impact 파싱) |
| [ChapterGenerationManager.cs](Assets/Script/Runtime/ChapterGenerationManager.cs) | 챕터 생성 프롬프트 (Affection 참고) |
| [VNProjectData.cs](Assets/Script/Runtime/VNProjectData.cs) | 프로젝트 데이터 (trueValueName 포함) |

---

**Last Updated**: 2025-01-10
**Document Version**: 3.0 (Value 우선 엔딩 + Romance Achievement 시스템)
