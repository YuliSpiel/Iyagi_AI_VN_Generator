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

## 🎯 Alternating Branching Strategy (교차 분기 시스템)

### 설계 목표

**문제**: 모든 기준(Core Value + NPC Affection)을 동시 적용 시 분기 폭발
- Core Values 2개 × NPCs 2명 = 4개 조합
- 6챕터 기준: 4⁵ = **1,024개 루트** (관리 불가능)

**해결**: 챕터별 단일 기준 교차 분기
- 짝수 챕터: Core Value 기준 분기
- 홀수 챕터: NPC Affection 기준 분기
- 6챕터 기준: 2⁵ = **32개 루트** (97% 감소)

### 분기 구조

```
Chapter 1: 공통 프롤로그 (1개)
Chapter 2: Core Value 분기 (A, B) → 2개
Chapter 3: NPC Affection 분기 (X, Y) → 4개 (AX, AY, BX, BY)
Chapter 4: Core Value 재분기 → 8개
Chapter 5: NPC Affection 재분기 → 16개
Chapter 6: Core Value 확정 → 32개
Ending: 4개 엔딩 (AX, AY, BX, BY)
```

### Chapter State Key 구조

```csharp
chapter_state_key = (
    chapter_index,      // 챕터 번호
    core_route,         // "A" or "B" (코어 밸류 축)
    love_route,         // "X" or "Y" or "None" (NPC 공략 축)
    core_bucket,        // "LOW" / "MID" / "HIGH" (점수 양자화)
    affinity_bucket,    // "X_HIGH" / "Y_HIGH" / "BALANCED"
    major_flags         // ["helped_X", "lied_to_Y", ...] (중요 플래그만)
)
```

**핵심 원칙**: 같은 state_key → 같은 챕터 내용 (Deterministic)

### 캐시 키 생성 로직

```csharp
private string GenerateCacheKey(int chapterId, GameStateSnapshot state) {
    string projectId = projectData.projectGuid;

    // Chapter 1: 항상 동일 (프롤로그)
    if (chapterId == 1) {
        return $"{projectId}_Ch1";
    }

    // 짝수 챕터: Core Value 기준 (2, 4, 6...)
    if (chapterId % 2 == 0) {
        string coreRoute = GetDominantCoreValue(state);           // "A" or "B"
        string coreBucket = GetCoreValueBucket(state);            // "LOW"/"MID"/"HIGH"
        string flags = GetMajorFlagsHash(state);                  // "helped_X_lied_Y"
        return $"{projectId}_Ch{chapterId}_{coreRoute}_{coreBucket}_{flags}";
    }

    // 홀수 챕터: NPC Affection 기준 (3, 5, 7...)
    else {
        string coreRoute = GetDominantCoreValue(state);           // 이전 경로 유지
        string loveRoute = GetDominantNPC(state);                 // "X" or "Y"
        string affBucket = GetAffectionBucket(state);             // "X_HIGH"/"Y_HIGH"/"BALANCED"
        string flags = GetMajorFlagsHash(state);
        return $"{projectId}_Ch{chapterId}_{coreRoute}_{loveRoute}_{affBucket}_{flags}";
    }
}

// Core Value 중 가장 높은 값 반환
private string GetDominantCoreValue(GameStateSnapshot state) {
    return state.coreValueScores
        .OrderByDescending(kvp => kvp.Value)
        .First().Key;
}

// NPC 중 가장 호감도 높은 캐릭터 반환
private string GetDominantNPC(GameStateSnapshot state) {
    return state.characterAffections
        .OrderByDescending(kvp => kvp.Value)
        .First().Key;
}

// Core Value를 LOW/MID/HIGH로 양자화
private string GetCoreValueBucket(GameStateSnapshot state) {
    int score = state.coreValueScores.Values.Max();
    if (score < 30) return "LOW";
    if (score < 70) return "MID";
    return "HIGH";
}

// Affection을 X_HIGH/Y_HIGH/BALANCED로 양자화
private string GetAffectionBucket(GameStateSnapshot state) {
    var affs = state.characterAffections;
    if (!affs.ContainsKey("X") || !affs.ContainsKey("Y")) return "BALANCED";

    int x = affs["X"];
    int y = affs["Y"];
    int diff = Math.Abs(x - y);

    if (diff < 20) return "BALANCED";
    return x > y ? "X_HIGH" : "Y_HIGH";
}

// 중요 플래그만 추출하여 해시 생성
private string GetMajorFlagsHash(GameStateSnapshot state) {
    // 실제로 스토리에 영향을 주는 플래그만 필터링
    var majorFlags = state.flags
        .Where(f => IsMajorFlag(f))
        .OrderBy(f => f)
        .ToList();

    return string.Join("_", majorFlags);
}

private bool IsMajorFlag(string flag) {
    // 예: "helped_X", "lied_to_Y", "failed_performance" 등
    string[] majorPrefixes = { "helped_", "lied_", "saved_", "failed_", "betrayed_" };
    return majorPrefixes.Any(prefix => flag.StartsWith(prefix));
}
```

### LLM 프롬프트 구조

#### 입력 (System → LLM)

```json
{
  "chapter_index": 3,
  "core_route": "A",
  "love_route": null,
  "core_score": 72,
  "affinity_x": 55,
  "affinity_y": 18,
  "core_bucket": "HIGH",
  "affinity_bucket": "X_HIGH",
  "major_flags": ["helped_X"],
  "previous_summary": "이전까지 일어난 사건 요약"
}
```

#### 출력 (LLM → System)

```json
{
  "chapter_script": "3장은 주인공과 X가 공연 리허설에서...",
  "choices": [
    {
      "id": "CHOICE_1",
      "text": "X에게 진심으로 사과한다",
      "effect": {
        "core_delta": -2,
        "affinity_x_delta": +3,
        "affinity_y_delta": 0,
        "flags_add": ["apologized_X"],
        "flags_remove": []
      }
    },
    {
      "id": "CHOICE_2",
      "text": "프로답게 문제를 논리적으로 지적한다",
      "effect": {
        "core_delta": +4,
        "affinity_x_delta": -1,
        "affinity_y_delta": 0,
        "flags_add": ["asserted_logic"],
        "flags_remove": []
      }
    }
  ]
}
```

### 장점

1. **분기 복잡도 97% 감소**: 1,024개 → 32개 (6챕터 기준)
2. **API 비용 절감**: 캐시 재사용으로 동일 경로 재플레이 시 무료
3. **Deterministic 출력**: 같은 state_key → 같은 챕터 보장
4. **명확한 챕터 테마**:
   - 짝수 챕터: "당신의 가치관은?" (Core Value 집중)
   - 홀수 챕터: "누구를 신뢰할 것인가?" (NPC 관계 집중)
5. **확장 가능**: Core Value 3개, NPC 3명으로 확장해도 선형 증가

### 제약사항

- **Bucket 양자화**: 점수를 LOW/MID/HIGH로 뭉개므로 세밀한 분기 불가
- **Major Flags만 반영**: 모든 플래그를 반영하면 상태 폭발 → 중요한 것만 선별
- **Convergence 구조**: 최종 4개 엔딩 (AX, AY, BX, BY)으로 수렴

### 관련 파일

- [Assets/Script/Runtime/ChapterGenerationManager.cs](Assets/Script/Runtime/ChapterGenerationManager.cs) - 캐시 키 생성 및 챕터 로드
- [Assets/Script/Runtime/GameStateSnapshot.cs](Assets/Script/Runtime/GameStateSnapshot.cs) - 상태 스냅샷 구조
- [systemdocs/chapter-generation.md](systemdocs/chapter-generation.md) - 상세 알고리즘

---

**Last Updated**: 2025-01-13
**Document Version**: 3.2 (Alternating Branching Strategy Added)
