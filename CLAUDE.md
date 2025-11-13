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

**Last Updated**: 2025-01-13
**Document Version**: 3.1 (Choice Button Listener Bug Prevention Guide Added)
