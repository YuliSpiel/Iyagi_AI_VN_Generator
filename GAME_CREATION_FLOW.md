# 🎮 Iyagi AI VN Generator - 게임 생성 플로우

> AI가 자동으로 비주얼노벨을 생성하는 전체 과정

---

## 📋 목차

1. [전체 플로우 개요](#전체-플로우-개요)
2. [Phase 1: 프로젝트 설정 (Setup Wizard)](#phase-1-프로젝트-설정-setup-wizard)
3. [Phase 2: 에셋 생성 (Asset Generation)](#phase-2-에셋-생성-asset-generation)
4. [Phase 3: 런타임 챕터 생성](#phase-3-런타임-챕터-생성)
5. [데이터 저장 구조](#데이터-저장-구조)
6. [트러블슈팅](#트러블슈팅)

---

## 🎯 전체 플로우 개요

```
┌─────────────────────────────────────────────────────────────────┐
│                    1. TitleScene (시작)                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  New Game    │  │  Load Game   │  │  Settings    │          │
│  └──────┬───────┘  └──────────────┘  └──────────────┘          │
└─────────┼───────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────────────┐
│              2. SetupWizardScene (프로젝트 생성)                 │
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │
│  │Step 1│→│Step 2│→│Step 3│→│Step 4│→│Step 5│→│Step 6│  │
│  │게임  │  │가치  │  │구조  │  │플레이어│ │NPC   │  │완료  │  │
│  │개요  │  │설정  │  │설정  │  │캐릭터│  │캐릭터│  │확인  │  │
│  └──────┘  └──────┘  └──────┘  └──────┘  └──────┘  └──────┘  │
└─────────────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────────────┐
│            3. 에셋 생성 (ParallelAssetGenerator)                 │
│                                                                  │
│  Cycle 1 (병렬)              Cycle 2 (병렬)                      │
│  ┌─────────────────┐         ┌──────────────────┐              │
│  │ 캐릭터 스탠딩    │         │ 챕터1 JSON 생성  │              │
│  │ - Player: 5개   │         │ - Gemini API     │              │
│  │ - NPC들: 각 5개 │         │ - 대사/선택지    │              │
│  └─────────────────┘         └──────────────────┘              │
│          ↓                           ↓                          │
│          └───────── BARRIER (50%) ───┘                          │
│                      ↓                                           │
│  ┌──────────────────────────────────────────────────┐          │
│  │ Cycle 3: 챕터1 에셋 병렬 생성                     │          │
│  │ - 배경 이미지 (NanoBanana API)                   │          │
│  │ - CG 일러스트 (NanoBanana API)                   │          │
│  │ - BGM (ElevenLabs API)                           │          │
│  │ - SFX (ElevenLabs API)                           │          │
│  └──────────────────────────────────────────────────┘          │
│                      ↓                                           │
│             FINAL BARRIER (100%)                                │
└─────────────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────────────┐
│                    4. GameScene (게임 플레이)                     │
│  ┌───────────────────────────────────────────────────┐         │
│  │ Chapter 1 → Chapter 2 → ... → Chapter N → Ending │         │
│  └───────────────────────────────────────────────────┘         │
│  - 캐시된 챕터 로드 또는 새로 생성                             │
│  - 선택지에 따라 스킬/호감도 변화                              │
│  - 챕터 완료 시 AI 요약 생성 → 다음 챕터 맥락 제공             │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📝 Phase 1: 프로젝트 설정 (Setup Wizard)

### Step 1: 게임 개요 (Game Overview)

**위치**: `SetupWizardScene` → `Step1_GameOverview`

**사용자 입력**:
- 게임 제목 (Game Title)
- 태그라인 (Tagline)
- 짧은 줄거리 (Short Synopsis)
- 상세 줄거리 (Detailed Synopsis)
- 장르 (Genre): School / Fantasy / SF / Mystery / Romance / Horror
- 톤 (Tone): Bright / Calm / Dark / Comic
- 배경 설정 (Setting): Modern / Near Future / Medieval / 19th Century / Fantasy
- 키워드 (Keywords)
- 제약사항 (Constraints): 해피엔딩 보장, 폭력 금지, 로맨스 금지 등

**AI 자동 완성 (F5 키 또는 AutoFill 버튼)**:
```csharp
// Gemini API 호출
Prompt: "Based on the user's title and synopsis, suggest tagline, short synopsis, detailed synopsis, genre, and tone."
→ Response: JSON { tagline, shortSynopsis, detailedSynopsis, genre, tone }
```

**출력**:
- `VNProjectData.gameTitle`
- `VNProjectData.gamePremise`
- `VNProjectData.genre`
- `VNProjectData.tone`
- `VNProjectData.keywords`

---

### Step 2: 가치 설정 (Core Values)

**위치**: `SetupWizardScene` → `Step2_CoreValues`

**사용자 입력**:
- 핵심 가치 (Core Values): 2-4개 (예: "정의", "출세", "우정")
- 각 가치의 파생 스킬 (Derived Skills): 3-5개 (예: 정의 → 자긍심, 공감능력, 판단력)
- 트루 엔딩 가치 선택

**AI 자동 제안 (AutoSuggest 버튼)**:
```csharp
// Gemini API 호출
Prompt: "Based on the game context, suggest 2-4 core values and their derived skills."
→ Response: JSON {
    coreValues: [
        { name: "정의", derivedSkills: ["자긍심", "공감능력", "판단력"] },
        { name: "출세", derivedSkills: ["야망", "사교성"] }
    ]
}
```

**출력**:
- `VNProjectData.coreValues[]`
  - `CoreValue.name`
  - `CoreValue.derivedSkills[]`
- `VNProjectData.trueValueName`

---

### Step 3: 스토리 구조 (Story Structure)

**위치**: `SetupWizardScene` → `Step3_StoryStructure`

**사용자 입력**:
- 총 챕터 수 (Total Chapters): 3-10개
- 분기 타입 (Branching Type):
  - Linear: 선형 스토리
  - Route Split: 중반 이후 루트 분기
  - Fully Branched: 완전 분기
- 챕터당 선택지 수 (Choices Per Chapter): 1-4개
- 배드엔딩 빈도 (Bad Ending Frequency): Rare / Sometimes / Frequent
- 예상 플레이타임 (Playtime): 30분 / 1시간 / 2시간 / 3시간+

**AI 자동 제안 (AutoSuggest 버튼)**:
```csharp
// Gemini API 호출
Prompt: "Based on the game context, suggest optimal story structure."
→ Response: JSON {
    totalChapters: 5,
    branchingType: "RouteSplit",
    choicesPerChapter: 2,
    badEndingFreq: "Sometimes",
    playtime: "Hour1"
}
```

**출력**:
- `VNProjectData.totalChapters`
- `VNProjectData.branchingType`
- `VNProjectData.choicesPerChapter`

---

### Step 4: 플레이어 캐릭터 (Player Character)

**위치**: `SetupWizardScene` → `Step4_PlayerCharacter`

**사용자 입력**:
- 캐릭터 이름 (Name)
- 나이 (Age)
- 성별 (Gender): Male / Female / Non-Binary
- 외모 설명 (Appearance Description)
- 성격 (Personality)
- 원형 (Archetype): Hero / Strategist / Innocent / Rebel / Mentor / Trickster
- POV (Point of View): First Person / Second Person / Third Person

**얼굴 프리뷰 생성 플로우**:
```
1. 사용자가 "Generate Preview" 버튼 클릭
   ↓
2. NanoBanana API 호출 (얼굴만, seed 없음)
   Prompt: "A close-up anime-style portrait, shoulders-up, front-facing,
            plain background, clean lineart, flat colors...
            Character: {appearanceDescription}"
   ↓
3. 이미지 생성 (512x512) + seed 반환
   ↓
4. 프리뷰 히스토리에 저장
   - previewHistory[] (최대 5개)
   - seedHistory[] (seed 값 저장)
   ↓
5. 사용자가 ◀/▶ 버튼으로 선택
   ↓
6. "Confirm" 버튼 클릭 시:
   - CharacterData.confirmedSeed 저장
   - CharacterData.facePreview 저장
   - Resources/Generated/Characters/{CharName}/face_preview.png 저장
```

**스탠딩 스프라이트 생성은 Step 6에서 병렬 처리됨**

**출력**:
- `CharacterData` (ScriptableObject)
  - `characterName`
  - `age`, `gender`, `archetype`, `pov`
  - `appearanceDescription`, `personality`
  - `confirmedSeed` (중요! 스탠딩 생성 시 재사용)
  - `facePreview` (CG 레퍼런스로 사용)
  - `resourcePath`: "Generated/Characters/{CharName}"

---

### Step 5: NPC 캐릭터 (NPCs)

**위치**: `SetupWizardScene` → `Step5_NPCs`

**반복**: Step 4와 동일한 프로세스를 NPC마다 반복

**추가 입력**:
- 역할 (Role): "친구", "멘토", "라이벌", "연인" 등
- 로맨스 가능 여부 (Is Romanceable)
- 초기 호감도 (Initial Affection): -100 ~ +100

**얼굴 프리뷰 생성**:
- **첫 번째 NPC**: `isFirst = true` (스타일 기준 설정)
- **추가 NPC**: `isFirst = false` (기존 스타일 통일)

**출력**:
- `VNProjectData.npcs[]` (CharacterData 리스트)

---

### Step 6: 완료 및 확인 (Finalize)

**위치**: `SetupWizardScene` → `Step6_Finalize`

**표시 내용**:
- 게임 제목
- 총 챕터 수
- 캐릭터 수 (Player + NPCs)
- 핵심 가치 목록
- 예상 플레이타임

**"Create Project" 버튼 클릭 시**:
1. `VNProjectData.asset` 저장 (`Assets/Resources/VNProjects/{GameTitle}.asset`)
2. 각 `CharacterData.asset` 저장 (`Assets/Resources/VNProjects/{GameTitle}/Characters/`)
3. SaveFile 생성 (초기 GameState)
4. **ParallelAssetGenerator 시작** → Phase 2로 이동

---

## 🎨 Phase 2: 에셋 생성 (Asset Generation)

### 전체 구조: Fan-Out Barrier 패턴

**파일**: `ParallelAssetGenerator.cs`

```
SetupWizardManager.OnWizardComplete()
    ↓
StartCoroutine(RunParallelAssetGeneration())
    ↓
┌─────────────────────────────────────────┐
│  Cycle 1 & 2 병렬 실행 (0% → 50%)       │
├─────────────────────────────────────────┤
│  Cycle 1: 모든 캐릭터 스탠딩 생성        │
│  Cycle 2: 챕터1 JSON 생성               │
│          ↓                              │
│     BARRIER (50%)                       │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  Cycle 3: 챕터1 에셋 병렬 생성 (50%→100%)│
├─────────────────────────────────────────┤
│  - 배경 이미지 (2-3개)                  │
│  - CG 일러스트 (1-2개)                  │
│  - BGM (3-5개)                          │
│  - SFX (5-10개)                         │
│          ↓                              │
│  FINAL BARRIER (100%)                   │
└─────────────────────────────────────────┘
    ↓
SceneManager.LoadScene("GameScene")
```

---

### Cycle 1: 캐릭터 스탠딩 생성

**메서드**: `ParallelAssetGenerator.GenerateAllStandingSprites()`

**대상**:
- Player Character (1명)
- NPCs (N명)

**각 캐릭터당 5종 기본 스탠딩 생성**:

| Expression | Pose   | 파일명                | API 호출 |
|------------|--------|-----------------------|----------|
| Neutral    | Normal | `neutral_normal.png`  | ✅       |
| Happy      | Normal | `happy_normal.png`    | ✅       |
| Sad        | Normal | `sad_normal.png`      | ✅       |
| Angry      | Normal | `angry_normal.png`    | ✅       |
| Surprised  | Normal | `surprised_normal.png`| ✅       |

**NanoBanana API 호출**:
```csharp
// 첫 번째 캐릭터 (Player)
Prompt: "A full-body standing sprite of a {gender} character for a Japanese-style visual novel.
         High-quality anime illustration style with clean outlines and soft gradient shading.
         Pose: front-facing, full body centered, neutral stance.
         Expression: {expression}.
         Outfit: {appearanceDescription}.
         Background: transparent or solid white.
         Resolution: 2048×4096."
Seed: {confirmedSeed}  // Step 4에서 저장한 seed 재사용
```

```csharp
// 추가 캐릭터 (NPCs)
Prompt: "A full-body standing sprite of a {gender} character for a Japanese-style visual novel.
         Same art style, same proportions, and same camera angle as the previous character.
         Pose: front-facing, full body centered, neutral stance.
         Expression: {expression}.
         Outfit: {appearanceDescription}.
         Background: transparent or solid white.
         Resolution: 2048×4096."
Seed: {confirmedSeed}  // 각 NPC의 고유 seed
```

**저장 위치**:
```
Assets/Resources/Generated/Characters/
├── PlayerName/
│   ├── face_preview.png       (Step 4에서 생성)
│   ├── neutral_normal.png     (Cycle 1에서 생성)
│   ├── happy_normal.png
│   ├── sad_normal.png
│   ├── angry_normal.png
│   └── surprised_normal.png
├── NPC1_Name/
│   ├── face_preview.png
│   ├── neutral_normal.png
│   └── ...
└── NPC2_Name/
    └── ...
```

**예상 시간**: 캐릭터당 5개 × 10초 = 약 50초 (Player + NPC 2명 = 150초)

---

### Cycle 2: 챕터1 JSON 생성

**메서드**: `ParallelAssetGenerator.GenerateChapter1JSON()`

**Gemini API 호출**:
```csharp
Prompt: ChapterGenerationManager.BuildScenePrompt(chapterId: 1, sceneNumber: 1, totalScenes: 3)

// 주요 지시사항:
- Game Title: {projectData.gameTitle}
- Premise: {projectData.gamePremise}
- Characters: Player, NPC1, NPC2...
- Core Values and Derived Skills: {coreValuesInfo}
- Current State: Initial chapter (no previous choices)
- Generate Scene 1 of 3 for Chapter 1
- Output 10 dialogue lines with choices
```

**JSON 응답 예시**:
```json
[
  {
    "speaker": "Narrator",
    "text": "정원에 도착한 순간, 이상한 기운이 느껴졌다.",
    "character1_name": "주인공",
    "character1_expression": "neutral",
    "character1_pose": "normal",
    "character1_position": "Center",
    "bg_name": "EnchantedGarden_Day",
    "bgm_name": "Mysterious_Calm",
    "sfx_name": null,
    "choices": null
  },
  {
    "speaker": "엘런",
    "text": "여기가... 그 전설의 환상정원인가요?",
    "character1_name": "엘런",
    "character1_expression": "surprised",
    "character1_pose": "normal",
    "character1_position": "Right",
    "character2_name": "주인공",
    "character2_expression": "neutral",
    "character2_pose": "normal",
    "character2_position": "Left",
    "bg_name": "EnchantedGarden_Day",
    "bgm_name": "Mysterious_Calm",
    "sfx_name": null,
    "choices": [
      {
        "text": "정원을 탐험하자.",
        "next_id": 5,
        "skill_impact": [
          { "skill_name": "용기", "change": 10 }
        ],
        "affection_impact": [
          { "character_name": "엘런", "change": 5 }
        ]
      },
      {
        "text": "조심스럽게 관찰한다.",
        "next_id": 7,
        "skill_impact": [
          { "skill_name": "판단력", "change": 10 }
        ]
      }
    ]
  }
]
```

**파싱 및 변환**:
```csharp
AIDataConverter.FromAIJson(jsonResponse, chapterId: 1)
→ List<DialogueRecord>
```

**추출되는 에셋 목록**:
- **배경 이름**: `bg_name` 필드에서 추출 (예: "EnchantedGarden_Day", "CastleHall_Night")
- **BGM 이름**: `bgm_name` 필드에서 추출 (예: "Mysterious_Calm", "Battle_Theme")
- **SFX 이름**: `sfx_name` 필드에서 추출 (예: "DoorOpen", "Footstep")
- **CG ID**: `cg_id` 필드에서 추출 (예: "Ch1_CG1")

**예상 시간**: 약 10-20초

---

### Cycle 3: 챕터1 에셋 병렬 생성

**메서드**: `ParallelAssetGenerator.RunCycle3()`

**입력**: Cycle 2에서 생성된 챕터1 JSON

**에셋 목록 파싱**:
```csharp
ParseChapter1Assets(chapter1JSON)
→ AssetList {
    backgrounds: ["EnchantedGarden_Day", "CastleHall_Night"],
    bgmNames: ["Mysterious_Calm", "Battle_Theme"],
    sfxNames: ["DoorOpen", "Footstep", "MagicCast"],
    cgs: [
        { cgId: "Ch1_CG1", description: "...", characters: ["주인공", "엘런"] }
    ]
}
```

**병렬 생성 작업**:

#### 1) 배경 이미지 생성

**API**: NanoBanana API

**프롬프트**:
```
"A high-quality anime-style background for a visual novel.
Scene: {bg_name} (예: Enchanted Garden at daytime)
Style: Soft colors, detailed environment, no characters.
Lighting: Natural and atmospheric.
Resolution: 1920×1080."
```

**저장 위치**: `Assets/Resources/Image/Background/{bg_name}.png`

---

#### 2) CG 일러스트 생성 (레퍼런스 기반)

**API**: NanoBanana API (Multi-image reference)

**입력**:
- **레퍼런스 이미지**: 등장 캐릭터들의 `facePreview` 이미지
- **장면 설명**: `cg_scene_description`
- **조명**: `cg_lighting`
- **분위기**: `cg_mood`
- **카메라 각도**: `cg_camera_angle`

**프롬프트**:
```
"A high-quality full-screen illustration in detailed watercolor / painterly style.
Use the provided reference faces to preserve character identity.
Scene: {cg_scene_description}
Lighting: {cg_lighting}
Mood: {cg_mood}
Camera angle: {cg_camera_angle}
Resolution: 1920×1080."

Reference Images:
- 주인공 face_preview.png
- 엘런 face_preview.png
```

**저장 위치**: `Assets/Resources/Image/CG/{cg_id}.png`

---

#### 3) BGM 생성

**API**: ElevenLabs Sound Generation API

**프롬프트**:
```json
{
  "text": "mysterious calm ambient music with soft piano and strings",
  "duration_seconds": 60,
  "prompt_influence": 0.3
}
```

**저장 위치**: `Assets/Resources/Sound/BGM/{bgm_name}.mp3`

---

#### 4) SFX 생성

**API**: ElevenLabs Sound Generation API

**프롬프트**:
```json
{
  "text": "door opening sound, wooden door creaking",
  "duration_seconds": 3,
  "prompt_influence": 0.5
}
```

**저장 위치**: `Assets/Resources/Sound/SFX/{sfx_name}.mp3`

---

**진행률 표시**:
```
Cycle 1 & 2: 0% → 50%
Cycle 3:
  - 배경 1/3 완료 → 60%
  - 배경 2/3 완료 → 70%
  - CG 1/2 완료 → 80%
  - BGM 1/5 완료 → 85%
  - SFX 1/10 완료 → 90%
  - 모든 에셋 완료 → 100%
```

**예상 시간**:
- 배경 2-3개 × 10초 = 30초
- CG 1-2개 × 15초 = 30초
- BGM 3-5개 × 10초 = 40초
- SFX 5-10개 × 5초 = 40초
- **총 약 2-3분**

---

### 테스트 모드 (F5 AutoFill)

**조건**: `SetupWizardAutoFill` 컴포넌트가 활성화되어 있을 때

**스킵되는 작업**:
- ✅ Cycle 1 & 2는 정상 실행 (스탠딩 + 챕터1 JSON 생성)
- ❌ **Cycle 3 스킵** (배경/CG/BGM/SFX 생성 안 함)

**목적**: 빠른 프로토타입 테스트 (약 3분 → 30초로 단축)

---

## 🎮 Phase 3: 런타임 챕터 생성

### GameScene 로드 플로우

```
1. GameScene.Start()
   ↓
2. SaveDataManager에서 현재 SaveFile 로드
   ↓
3. GameStateSnapshot 초기화
   - coreValueScores: 모두 0
   - skillScores: 모두 0
   - characterAffections: 초기 호감도
   - chapterSummaries: 비어있음
   ↓
4. StartChapter(chapterId: 1)
   ↓
5. ChapterGenerationManager.GenerateOrLoadChapter()
   ┌──────────────────────────────┐
   │  캐시 확인                    │
   │  cacheKey = {ProjectGuid}_Ch1│
   └──────────────────────────────┘
   ↓                    ↓
캐시 있음               캐시 없음
   ↓                    ↓
LoadCached()        Generate()
                        ↓
                    Gemini API
                    (3 scenes × 10 lines)
                        ↓
                    AIDataConverter
                        ↓
                    CacheToDisk()
   ↓                    ↓
   └────────┬───────────┘
            ↓
6. DialogueUI.ShowCurrentLine()
   - 첫 대사 표시
   - 캐릭터 스탠딩 로드
   - 배경 로드
   - BGM 재생
```

---

### 챕터 생성 상세 플로우

**파일**: `ChapterGenerationManager.cs`

#### 1. 캐시 키 생성

```csharp
GenerateCacheKey(chapterId, state)
→ "{ProjectGuid}_Ch{ChapterId}"
→ 예: "5ec38237_Ch1"
```

**Chapter-Level Convergence**:
- 모든 플레이어가 같은 챕터 번호면 **같은 캐시 사용**
- Core Value 점수 무관 (스토리는 동일, 점수만 다름)

---

#### 2. 씬 단위 분할 생성 (3 Scenes)

**왜 분할?**
- Gemini API 출력 제한: 한 번에 10-15줄만 안정적으로 생성
- 긴 챕터 생성 시 JSON 불완전 응답 방지

**Scene 1 생성**:
```csharp
BuildScenePrompt(chapterId: 1, sceneNumber: 1, totalScenes: 3, state, previousScenes: "")
→ Gemini API 호출
→ JSON Response (10 lines)
→ AIDataConverter.FromAIJson(jsonResponse, chapterId: 1)
→ DialogueRecord[] (ID: 1000-1009)
```

**Scene 2 생성**:
```csharp
// previousScenes 컨텍스트 전달
previousScenesContext = "=== Scene 1 ===\n엘런: 여기가 정원인가요?\n주인공: 그런 것 같아.\n..."

BuildScenePrompt(chapterId: 1, sceneNumber: 2, totalScenes: 3, state, previousScenes: previousScenesContext)
→ Gemini API 호출
→ DialogueRecord[] (ID: 1010-1019)
```

**Scene 3 생성**:
```csharp
previousScenesContext += "\n=== Scene 2 ===\n..."
BuildScenePrompt(chapterId: 1, sceneNumber: 3, totalScenes: 3, state, previousScenes: previousScenesContext)
→ DialogueRecord[] (ID: 1020-1029)
```

**씬 연결**:
```csharp
// Scene 1 마지막 라인 (ID: 1009)
if (!record.Has("NextIndex1")) {
    record.Fields["NextIndex1"] = "1010";  // Scene 2 첫 라인
    record.Fields["Auto"] = "TRUE";
}

// Scene 2 마지막 라인 (ID: 1019)
if (!record.Has("NextIndex1")) {
    record.Fields["NextIndex1"] = "1020";  // Scene 3 첫 라인
    record.Fields["Auto"] = "TRUE";
}
```

---

#### 3. AI 프롬프트 주요 지시사항

**Chapter-Level Convergence 구조**:
```
# CRITICAL - Chapter-Level Convergence Structure
This chapter has a FIXED NARRATIVE ARC that all players experience:

1. Choices affect RELATIONSHIPS and VALUES, NOT the main plot
2. Branching → Convergence Pattern
   - After each choice, create 2-3 different dialogue responses
   - Then CONVERGE back to the main narrative within 1-2 lines
3. Next ID Management for Branching
   - Choice A → next_id: 1005
   - Choice B → next_id: 1007
   - Line 1006 (Choice A result) → next_id: 1009 (CONVERGENCE)
   - Line 1008 (Choice B result) → next_id: 1009 (CONVERGENCE)
```

**Skill Impact 지시**:
```
CRITICAL - Choice Impact Rules:
- NEVER use "value_impact" field - it is deprecated!
- ONLY use "skill_impact" to affect derived skills
- Core values are calculated automatically as the sum of their derived skills
- Available derived skills for this project: {coreValuesInfo}
- Example skill_impact format:
  "skill_impact": [
    { "skill_name": "[choose from the derived skills listed above]", "change": 5 to 15 }
  ]
- Each choice should affect 1-3 relevant derived skills
- Use skill names EXACTLY as listed in the Core Values section above
```

---

#### 4. 캐시 저장

**파일 경로**:
```
{Application.persistentDataPath}/Chapters/{ProjectGuid}/{ProjectGuid}_Ch1.json
```

**예시**:
```
/Users/yuli/Library/Application Support/YuliSpiel/IYAGI_AI/Chapters/5ec38237.../5ec38237_Ch1.json
```

**내용**:
```json
{
  "chapterId": 1,
  "records": [ /* DialogueRecord[] */ ],
  "stateSnapshot": { /* GameStateSnapshot */ },
  "timestamp": 1673520000
}
```

---

### 챕터 완료 및 다음 챕터 전환

**파일**: `GameController.cs`

#### 1. 챕터 완료 감지

```csharp
NextLine()
{
    // 마지막 라인 체크
    if (currentLineIndex >= currentChapterRecords.Count - 1) {
        OnChapterEnd();
    }
}
```

---

#### 2. 챕터 요약 생성

```csharp
OnChapterEnd()
→ StartCoroutine(GenerateChapterSummaryAndContinue())
    ↓
ShowLoadingPanel(true, "챕터 요약 생성 중...")
    ↓
ChapterGenerationManager.GenerateChapterSummary(chapterId, records)
    ↓
Gemini API 호출:
Prompt: "Summarize the following chapter in 2-3 sentences that capture:
         1. The main events that happened
         2. Key character interactions or developments
         3. Any important plot points or revelations"
    ↓
AI Response: "주인공은 환상정원에 도착하여 수호자 엘런을 만났다.
             정원의 비밀을 풀기 위해 탐험을 시작했으며,
             첫 번째 시련을 통과했다."
    ↓
currentState.chapterSummaries[1] = "주인공은 환상정원에..."
    ↓
ShowLoadingPanel(false)
    ↓
currentChapterId++  // 2
    ↓
StartChapter(2)
```

---

#### 3. 다음 챕터 시작 (요약 포함)

```csharp
StartChapter(chapterId: 2)
→ ShowLoadingPanel(true, "Chapter 2 생성 중...")
→ ChapterGenerationManager.GenerateOrLoadChapter(2, currentState)
    ↓
BuildScenePrompt(chapterId: 2, ...)
    ↓
Prompt includes:
# Current State
Current Chapter: 2
Core Values: 용기=15, 지혜=10, 우정=5
Skills: 검술=10, 판단력=5, ...
Character Affections: 엘런=15

Previous Chapters:
  Chapter 1: 주인공은 환상정원에 도착하여 수호자 엘런을 만났다.
             정원의 비밀을 풀기 위해 탐험을 시작했으며,
             첫 번째 시련을 통과했다.
    ↓
Gemini API가 Chapter 1 맥락을 이해하고 자연스럽게 연결되는 Chapter 2 생성
    ↓
DialogueUI.ShowCurrentLine()
```

---

### 선택지 처리 및 스킬 변화

**파일**: `GameController.cs`

#### OnChoiceSelected(choiceIndex)

```csharp
// 1. 선택지 텍스트 저장
string choiceText = currentLine.GetString($"Choice{choiceIndex + 1}");
currentState.previousChoices.Add(choiceText);

// 2. Skill Impact 처리
foreach (var value in projectData.coreValues) {
    foreach (var skill in value.derivedSkills) {
        string skillImpactKey = $"Choice{choiceIndex + 1}_SkillImpact_{skill}";
        if (currentLine.Has(skillImpactKey) && currentLine.TryGetInt(skillImpactKey, out int skillChange)) {
            currentState.skillScores[skill] += skillChange;
            Debug.Log($"[Skill] '{skill}' changed by {skillChange} (now: {currentState.skillScores[skill]})");
        }
    }
}

// 3. Core Values 재계산 (파생 스킬 합산)
RecalculateCoreValues();

// 4. Affection Impact 처리
foreach (var npc in projectData.npcs) {
    string affectKey = $"Choice{choiceIndex + 1}_AffectionImpact_{npc.characterName}";
    if (currentLine.Has(affectKey) && currentLine.TryGetInt(affectKey, out int change)) {
        currentState.characterAffections[npc.characterName] += change;
        Debug.Log($"{npc.characterName} affection changed by {change}");
    }
}

// 5. Next ID로 점프
string nextIdKey = $"Next{choiceIndex + 1}";
if (currentLine.TryGetInt(nextIdKey, out int nextId)) {
    for (int i = 0; i < currentChapterRecords.Count; i++) {
        if (currentChapterRecords[i].TryGetInt("ID", out int lineId) && lineId == nextId) {
            currentLineIndex = i;
            ShowCurrentLine();
            return;
        }
    }
}
```

---

### 엔딩 씬 생성

**파일**: `GameController.cs`

#### 마지막 챕터 완료 시

```csharp
OnChapterEnd()
→ GenerateChapterSummaryAndContinue()
    ↓
if (currentChapterId >= projectData.totalChapters) {
    // 게임 종료 - 엔딩 씬 생성
    GenerateAndShowEndingScene()
}
```

---

#### 엔딩 타입 결정

```csharp
DetermineEndingType()
{
    // EndingManager 초기화
    endingManager.Initialize(projectData, currentState);

    // 엔딩 결정
    string endingType = endingManager.DetermineEnding();
    // → "True Ending" or "용기 Ending" or "Normal Ending"

    return endingType;
}
```

---

#### 엔딩 씬 로드 (미리 작성된 엔딩)

```csharp
ShowLoadingPanel(true, "엔딩 씬 준비 중...")
    ↓
ChapterGenerationManager.GenerateEndingScene(endingType, currentState)
    ↓
EndingSceneDatabase에서 해당 엔딩 가져오기
    ↓
EndingSceneData.ToDialogueRecords()
    ↓
currentChapterRecords = endingRecords
currentChapterId = 999  // 엔딩 씬 ID
ShowCurrentLine()
    ↓
[엔딩 대사 표시]
    ↓
엔딩 씬 마지막 라인 (next_id: 0)
    ↓
ShowEndingPanel(endingType)
```

---

## 💾 데이터 저장 구조

### ScriptableObject 저장 위치

```
Assets/Resources/VNProjects/
├── {GameTitle}.asset                    # VNProjectData
├── {GameTitle}/
│   └── Characters/
│       ├── {PlayerName}.asset           # CharacterData
│       ├── {NPC1_Name}.asset
│       └── {NPC2_Name}.asset
```

### 리소스 저장 위치

```
Assets/Resources/
├── Generated/
│   └── Characters/
│       ├── {PlayerName}/
│       │   ├── face_preview.png         # 얼굴 프리뷰 (CG 레퍼런스)
│       │   ├── neutral_normal.png       # 스탠딩 5종
│       │   ├── happy_normal.png
│       │   ├── sad_normal.png
│       │   ├── angry_normal.png
│       │   ├── surprised_normal.png
│       │   └── [런타임 추가 생성 가능]
│       └── {NPC_Name}/
│           └── ...
├── Image/
│   ├── Background/
│   │   ├── EnchantedGarden_Day.png
│   │   ├── CastleHall_Night.png
│   │   └── ...
│   └── CG/
│       ├── Ch1_CG1.png
│       ├── Ch2_CG1.png
│       └── ...
└── Sound/
    ├── BGM/
    │   ├── Mysterious_Calm.mp3
    │   ├── Battle_Theme.mp3
    │   └── ...
    └── SFX/
        ├── DoorOpen.mp3
        ├── Footstep.mp3
        └── ...
```

### 런타임 캐시 저장 위치

```
{Application.persistentDataPath}/
├── Chapters/
│   └── {ProjectGuid}/
│       ├── {ProjectGuid}_Ch1.json
│       ├── {ProjectGuid}_Ch2.json
│       └── ...
└── SaveData/
    ├── ProjectSlots.json
    └── {SlotId}/
        ├── autosave.json
        ├── {SaveId1}.json
        └── {SaveId2}.json
```

**예시 (macOS)**:
```
/Users/yuli/Library/Application Support/YuliSpiel/IYAGI_AI/
├── Chapters/
│   └── 5ec38237-42f8-4186-b254-ca3f43545f97/
│       ├── 5ec38237-42f8-4186-b254-ca3f43545f97_Ch1.json
│       └── 5ec38237-42f8-4186-b254-ca3f43545f97_Ch2.json
└── SaveData/
    └── ...
```

---

## 🔧 트러블슈팅

### 1. 챕터 생성 시 JSON 파싱 실패

**증상**:
```
[AIDataConverter] JSON parsing error: Unexpected token }
```

**원인**: Gemini API가 불완전한 JSON 반환 (마지막 `]` 누락)

**해결**:
- `AIDataConverter.AttemptJSONRepair()` 자동 복구 시도
- 마지막 완성된 객체까지만 파싱, 불완전한 부분 버림

---

### 2. 선택지가 작동하지 않음 (next_id 오류)

**증상**: 선택지 클릭 시 다음 대사로 이동하지 않음

**원인**: AI가 생성한 `next_id`가 상대 인덱스 (7, 8)인데 절대 ID (1007, 1008)로 변환 안 됨

**해결**: ✅ 이미 수정됨
```csharp
// AIDataConverter.cs:120-123
int aiNextId = line.choices[c].next_id;
int actualNextId = (aiNextId == 0) ? 0 : (baseId + aiNextId);
record.Fields[$"Next{choiceNum}"] = actualNextId.ToString();
```

---

### 3. 스킬이 변화하지 않음

**증상**: 선택지 선택해도 스킬 점수 그대로

**원인**: AI가 생성한 `skill_name`과 프로젝트의 `derivedSkills` 이름 불일치

**해결**: ✅ 이미 수정됨
```csharp
// ChapterGenerationManager.cs:248-257
CRITICAL - Choice Impact Rules:
- Available derived skills for this project:{coreValuesInfo}
- Use skill names EXACTLY as listed in the Core Values section above
```

---

### 4. 챕터 간 스토리 일관성 부족

**증상**: Chapter 2가 Chapter 1과 전혀 무관한 내용

**원인**: AI가 이전 챕터 내용을 모름

**해결**: ✅ 이미 수정됨
```csharp
// GameStateSnapshot.chapterSummaries 추가
// ChapterGenerationManager.GenerateChapterSummary() 자동 호출
// AI 프롬프트에 이전 챕터 요약 포함
```

---

### 5. 챕터 생성 중 클릭 시 엔딩 팝업 뜸

**증상**: 로딩 중인데 화면 클릭하면 엔딩 패널이 나타남

**원인**: 챕터 생성 중에도 NextLine(), OnChoiceSelected() 호출 가능

**해결**: ✅ 이미 수정됨
```csharp
// GameController.cs
public bool isGeneratingChapter = false;

NextLine() {
    if (isGeneratingChapter) return;
    ...
}

OnChoiceSelected() {
    if (isGeneratingChapter) return;
    ...
}

StartChapter() {
    isGeneratingChapter = true;
    ShowLoadingPanel(true, "Chapter X 생성 중...");
    ...
    isGeneratingChapter = false;
    ShowLoadingPanel(false);
}
```

---

### 6. 캐시 삭제 방법

**위치**:
```
{Application.persistentDataPath}/Chapters/{ProjectGuid}/
```

**macOS**:
```bash
rm -rf "/Users/yuli/Library/Application Support/YuliSpiel/IYAGI_AI/Chapters/{ProjectGuid}/"
```

**Windows**:
```
%USERPROFILE%\AppData\LocalLow\YuliSpiel\IYAGI_AI\Chapters\{ProjectGuid}\
```

---

## 📊 전체 타임라인

| 단계 | 작업 | 예상 시간 | API 호출 |
|------|------|----------|---------|
| **Setup Wizard** | Step 1-5 입력 + 얼굴 프리뷰 | 5-10분 | Player + NPC 각 1회 |
| **Cycle 1** | 캐릭터 스탠딩 생성 (5종 × N명) | 2-3분 | 5 × N명 |
| **Cycle 2** | 챕터1 JSON 생성 (3 scenes) | 30초 | Gemini × 3 |
| **Cycle 3** | 챕터1 에셋 생성 (배경/CG/BGM/SFX) | 2-3분 | 10-20회 |
| **GameScene 로드** | 캐시 로드 또는 신규 생성 | 즉시 / 30초 | 0 / 3회 |
| **챕터 완료** | 요약 생성 | 10초 | Gemini × 1 |
| **다음 챕터** | 캐시 로드 또는 신규 생성 | 즉시 / 30초 | 0 / 3회 |

**총 소요 시간**: 약 10-15분 (프로젝트 생성 + 첫 플레이)

---

## 🎯 최종 결과물

### 생성되는 파일 개수 (예시: Player + NPC 2명, 3챕터 게임)

**ScriptableObjects**: 4개
- VNProjectData × 1
- CharacterData × 3 (Player + NPC 2명)

**이미지**:
- 얼굴 프리뷰: 3개
- 스탠딩 스프라이트: 15개 (5종 × 3명)
- 배경: 6-9개 (챕터당 2-3개)
- CG: 3-6개 (챕터당 1-2개)
- **총 약 30-35개**

**오디오**:
- BGM: 9-15개 (챕터당 3-5개)
- SFX: 15-30개 (챕터당 5-10개)
- **총 약 25-45개**

**캐시 JSON**: 3개 (챕터당 1개)

**SaveFile JSON**: 1-10개 (플레이어당)

---

## 📝 요약

1. **Setup Wizard (5-10분)**: 사용자가 게임 개요/가치/캐릭터 입력, AI가 자동 제안
2. **Cycle 1 & 2 (병렬, 3분)**: 모든 캐릭터 스탠딩 생성 + 챕터1 JSON 생성
3. **Cycle 3 (2-3분)**: 챕터1 에셋 (배경/CG/BGM/SFX) 병렬 생성
4. **GameScene**: 캐시된 챕터 로드 또는 새로 생성, 선택지에 따라 스킬/호감도 변화
5. **챕터 완료**: AI가 요약 생성 → 다음 챕터 맥락 제공
6. **엔딩**: 최종 Core Value 점수로 엔딩 결정, 미리 작성된 엔딩 씬 표시

**핵심 특징**:
- ✅ Chapter-Level Convergence: 모든 플레이어가 같은 스토리, 다른 스킬/호감도
- ✅ 병렬 에셋 생성: Fan-Out Barrier 패턴으로 빠른 생성
- ✅ 챕터 캐싱: 한 번 생성한 챕터는 재사용 (같은 챕터 번호면 동일)
- ✅ 챕터 요약: AI가 자동으로 맥락 생성, 일관된 스토리 전개
- ✅ UI 차단: 챕터 생성 중 클릭 방지, 로딩 팝업 표시

---

**Last Updated**: 2025-01-11
**Document Version**: 1.0
