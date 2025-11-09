# Iyagi AI VN Generator - Project Structure

## 📁 Current Project Structure (After Cleanup)

```
Iyagi_AI_VN_Generator/
├── Assets/
│   ├── AddressableAssetsData/           # Unity Addressables (유지)
│   ├── Localization/                    # Unity Localization (유지)
│   ├── Resources/                       # 런타임 로드 리소스
│   │   ├── Generated/                   # AI 생성 리소스 저장 위치
│   │   │   └── Characters/              # 캐릭터별 폴더
│   │   │       ├── {CharName}/          # 캐릭터 이름별 폴더
│   │   │       │   ├── face_preview.png         # 얼굴 프리뷰 (CG 레퍼런스용)
│   │   │       │   ├── neutral_normal.png       # 스탠딩 (중립+일반)
│   │   │       │   ├── happy_normal.png         # 스탠딩 (행복+일반)
│   │   │       │   └── {expression}_{pose}.png  # 추가 조합
│   │   ├── Image/                       # 기존 이미지 리소스 (유지)
│   │   │   ├── BG/                      # 배경 이미지 (Setup Wizard 생성)
│   │   │   ├── CG/                      # CG 일러스트 (챕터 생성 시)
│   │   │   └── Standing/                # 기존 스탠딩 (사용 안 함, 참고용)
│   │   ├── Sound/                       # 오디오 리소스 (유지)
│   │   │   ├── BGM/                     # Setup Wizard 생성
│   │   │   └── SFX/                     # Setup Wizard 생성 (선택적)
│   │   ├── Prefabs/                     # UI 프리팹 (유지)
│   │   └── Material/                    # 머티리얼 (유지)
│   ├── Scenes/                          # Unity Scene 파일
│   │   └── (새로 생성 예정)
│   ├── Script/                          # C# 스크립트
│   │   ├── 0.Managers/                  # 싱글톤 매니저
│   │   │   ├── SoundManager.cs          ✅ (유지)
│   │   │   └── UIManager.cs             ✅ (유지)
│   │   ├── 1.UI/                        # UI 컴포넌트
│   │   │   └── GlobalCanvas.cs          ✅ (유지)
│   │   ├── 2.DialogueSystem/            # 대화 시스템
│   │   │   ├── DialogueRecord.cs        ✅ (유지 - 기본 클래스)
│   │   │   └── DialogueUI.cs            ✅ (유지 - 렌더링)
│   │   ├── AISystem/                    # AI 통합 (새로 작성)
│   │   │   └── (GeminiClient, NanoBananaClient, AIDataConverter)
│   │   ├── SetupWizard/                 # Setup Wizard (새로 작성)
│   │   │   └── (SetupWizardManager, Step1~6, CharacterFaceGenerator)
│   │   ├── Runtime/                     # 런타임 시스템 (새로 작성)
│   │   │   └── (ChapterGenerationManager, GameController)
│   │   ├── Editor/                      # Unity Editor 확장
│   │   └── Dummy/                       # 개발용 더미
│   ├── TextMesh Pro/                    # TextMesh Pro (유지)
│   └── VNProjects/                      # 생성된 프로젝트 저장 (새로 생성)
├── Library/                             # Unity 캐시 (무시)
├── Logs/                                # 로그 (무시)
├── Packages/                            # Package Manager
├── ProjectSettings/                     # Unity 설정
├── UserSettings/                        # 사용자 설정
├── CLAUDE.md                            # 기술 설계 문서 ✅
├── PROJECT_STRUCTURE.md                 # 이 파일
├── README.md                            # 프로젝트 소개
└── TECHNICAL_DOCUMENTATION.md           # 기존 문서 (참고용)
```

---

## ✅ 유지된 컴포넌트

### 1. **Managers (재사용)**
- `SoundManager.cs` - BGM/SFX 재생
- `UIManager.cs` - 글로벌 페이드 효과

### 2. **DialogueSystem (부분 재사용)**
- `DialogueRecord.cs` - 기본 데이터 컨테이너 (AI 변환 레이어에서 사용)
- `DialogueUI.cs` - 대화 렌더링, 타이핑 애니메이션, Standing 배치

### 3. **UI**
- `GlobalCanvas.cs` - 페이드 패널

### 4. **Resources**
- `Image/`, `Sound/`, `Prefabs/`, `Material/` - 기존 리소스 재사용

---

## ❌ 삭제된 컴포넌트

### 1. **CSV 기반 시스템 (더 이상 필요 없음)**
- ❌ `DialogueDatabase.cs` - CSV 저장소
- ❌ `DialogueParser.cs` - CSV 파싱
- ❌ `DialogueLoader.cs` - Google Sheets 다운로드
- ❌ `DialogueSystem.cs` - CSV 기반 플로우 컨트롤러
- ❌ `DialogueEvents.cs` - CSV 이벤트 트리거
- ❌ `Assets/Resources/Scenario/` - CSV 파일

### 2. **기존 Manager (재작성 예정)**
- ❌ `GameManager.cs` - 게임 상태 관리 (새로운 구조로 재작성)
- ❌ `DataManager.cs` - 세이브/로드 (AI 시스템에 맞춰 재작성)
- ❌ `DialogueManager.cs` - Placeholder

### 3. **기존 LLM 시스템 (완전 재설계)**
- ❌ `Assets/Script/3.LLMSystem/` 전체
  - `LLMConfig.cs`, `LLMStoryGenerator.cs`, `LLMGameController.cs`, `DynamicDialogueBuilder.cs`
  - CLAUDE.md 설계에 맞춰 완전히 새로 작성

### 4. **기존 Scene UI**
- ❌ `Assets/Script/1.UI/1.TitleScene/` - 타이틀 Scene UI (재작성)
- ❌ `Assets/Script/1.UI/2.GameScene/` - 게임 Scene UI (재작성)

### 5. **Scene 파일**
- ❌ `01_TitleScene.unity`
- ❌ `02_GameScene.unity`
- ❌ `LLMGameScene.unity`
- 새로운 Scene 구조로 재작성 예정:
  - `SetupWizardScene.unity` (Editor 전용)
  - `GameScene.unity` (Runtime)

### 6. **기존 LLM 설정 파일**
- ❌ `LLMConfig.asset` (ScriptableObject로 재작성)
- ❌ `README_LLM_Setup.txt`

---

## 🆕 새로 생성된 폴더

### 1. **Assets/Script/AISystem/**
AI API 클라이언트 및 변환 레이어
- `GeminiClient.cs` - Gemini API 통합
- `NanoBananaClient.cs` - 이미지 생성 API
- `AIDataConverter.cs` - JSON → DialogueRecord 변환

### 2. **Assets/Script/SetupWizard/**
Setup Wizard UI 시스템
- `SetupWizardManager.cs` - 위자드 플로우 관리
- `Step1_GameOverview.cs` - 게임 개요 입력
- `Step2_CoreValues.cs` - 가치 설정
- `Step3_StoryStructure.cs` - 스토리 구조
- `Step4_PlayerCharacter.cs` - 플레이어 캐릭터
- `Step5_NPCs.cs` - NPC 생성
- `Step6_Finalize.cs` - 최종 확인
- `CharacterFaceGenerator.cs` - 얼굴 프리뷰 생성
- `StandingSpriteGenerator.cs` - 스탠딩 5종 생성

### 3. **Assets/Script/Runtime/**
런타임 게임 시스템
- `ChapterGenerationManager.cs` - 챕터 생성/캐싱
- `GameController.cs` - 게임 플로우 제어
- `VNProjectData.cs` - ScriptableObject 정의
- `CharacterData.cs` - ScriptableObject 정의
- `ChapterData.cs` - 런타임 챕터 데이터

### 4. **Assets/VNProjects/**
생성된 VN 프로젝트 저장
- `{ProjectName}.asset` - VNProjectData ScriptableObject
- `Characters/{CharName}.asset` - CharacterData

### 5. **Assets/Resources/Generated/**
AI 생성 리소스 자동 저장 (캐릭터별 폴더 구조)
- `Characters/{CharName}/face_preview.png` - 얼굴 프리뷰 (CG 레퍼런스용)
- `Characters/{CharName}/{expression}_{pose}.png` - 스탠딩 스프라이트 (예: `happy_normal.png`)

### 6. **Assets/Resources/Image/CG/**
런타임 생성되는 CG 일러스트 (수채화/페인터리 스타일)
- `Ch{N}_CG{M}.png` - 챕터별 CG (예: `Ch1_CG1.png`)

---

## 📝 다음 단계

### Phase 1: 데이터 구조 정의 (우선순위 1)
```bash
Assets/Script/Runtime/
├── VNProjectData.cs           # ScriptableObject 정의
├── CharacterData.cs           # ScriptableObject 정의
└── DataModels.cs              # Enum, Serializable 클래스
```

### Phase 2: AI 클라이언트 (우선순위 2)
```bash
Assets/Script/AISystem/
├── GeminiClient.cs            # Gemini API 통합
├── NanoBananaClient.cs        # 이미지 생성 (또는 대체 API)
└── AIDataConverter.cs         # JSON → DialogueRecord 변환
```

### Phase 3: Setup Wizard (우선순위 3)
```bash
Assets/Script/SetupWizard/
├── SetupWizardManager.cs
├── Step1_GameOverview.cs
├── Step4_PlayerCharacter.cs
└── CharacterFaceGenerator.cs
```

### Phase 4: 런타임 시스템 (우선순위 4)
```bash
Assets/Script/Runtime/
├── ChapterGenerationManager.cs
└── GameController.cs
```

---

## 🔐 .gitignore 업데이트 완료

다음 항목이 Git에서 무시됩니다:
```gitignore
# IDE 설정 파일
.idea/
.vscode/

# 크래시 리포트 파일
mono_crash.*.json

# LLM API 설정 파일 (API 키 포함)
Assets/Resources/LLMConfig.asset
Assets/Resources/LLMConfig.asset.meta
```

---

**정리 완료일**: 2025-01-09
**다음 작업**: Phase 1 - 데이터 구조 정의부터 시작
