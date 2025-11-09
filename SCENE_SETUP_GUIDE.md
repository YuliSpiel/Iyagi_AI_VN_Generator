# Scene Setup Guide

Unity Scene 파일 구성 가이드입니다. Scene 파일은 코드로 생성할 수 없으므로 Unity Editor에서 수동으로 생성해야 합니다.

---

## 1. SetupWizardScene 구성

**경로**: `Assets/Scenes/SetupWizardScene.unity`

### Scene Hierarchy

```
SetupWizardScene
├── Canvas (UI Canvas)
│   ├── SetupWizardManager (빈 GameObject)
│   │   └── SetupWizardManager.cs
│   │   └── GeminiClient.cs
│   │   └── NanoBananaClient.cs
│   │   └── CharacterFaceGenerator.cs
│   │
│   ├── Step1_GameOverview (Panel)
│   │   ├── Title (TextMeshPro)
│   │   ├── GameTitleInput (TMP_InputField)
│   │   ├── PremiseInput (TMP_InputField - Multiline)
│   │   ├── GenreDropdown (TMP_Dropdown)
│   │   ├── ToneDropdown (TMP_Dropdown)
│   │   ├── PlaytimeDropdown (TMP_Dropdown)
│   │   ├── AutoFillButton (Button)
│   │   └── NextButton (Button)
│   │
│   ├── Step2_CoreValues (Panel)
│   │   ├── Title (TextMeshPro)
│   │   ├── Value1Input (TMP_InputField)
│   │   ├── Value2Input (TMP_InputField)
│   │   ├── Value3Input (TMP_InputField)
│   │   ├── Value4Input (TMP_InputField)
│   │   ├── AutoSuggestButton (Button)
│   │   └── NextButton (Button)
│   │
│   ├── Step3_StoryStructure (Panel)
│   │   ├── Title (TextMeshPro)
│   │   ├── ChapterCountDropdown (TMP_Dropdown)
│   │   ├── Ending1NameInput (TMP_InputField)
│   │   ├── Ending2NameInput (TMP_InputField)
│   │   ├── Ending3NameInput (TMP_InputField)
│   │   └── NextButton (Button)
│   │
│   ├── Step4_PlayerCharacter (Panel)
│   │   ├── Title (TextMeshPro)
│   │   ├── NameInput (TMP_InputField)
│   │   ├── AgeInput (TMP_InputField)
│   │   ├── GenderDropdown (TMP_Dropdown)
│   │   ├── POVDropdown (TMP_Dropdown)
│   │   ├── ArchetypeDropdown (TMP_Dropdown)
│   │   ├── AppearanceInput (TMP_InputField - Multiline)
│   │   ├── PersonalityInput (TMP_InputField - Multiline)
│   │   ├── FacePreviewPanel
│   │   │   ├── PreviewImage (Image)
│   │   │   ├── GenerateFaceButton (Button)
│   │   │   ├── PreviousButton (Button)
│   │   │   ├── NextButton (Button)
│   │   │   └── PreviewIndexText (TextMeshPro)
│   │   ├── ConfirmButton (Button)
│   │   └── NextStepButton (Button)
│   │
│   ├── Step5_NPCs (Panel)
│   │   ├── Title (TextMeshPro)
│   │   ├── NameInput (TMP_InputField)
│   │   ├── AgeInput (TMP_InputField)
│   │   ├── GenderDropdown (TMP_Dropdown)
│   │   ├── ArchetypeDropdown (TMP_Dropdown)
│   │   ├── RoleInput (TMP_InputField)
│   │   ├── AppearanceInput (TMP_InputField - Multiline)
│   │   ├── PersonalityInput (TMP_InputField - Multiline)
│   │   ├── RomanceableToggle (Toggle)
│   │   ├── FacePreviewPanel (동일 구조)
│   │   ├── ConfirmButton (Button)
│   │   ├── AddAnotherButton (Button)
│   │   └── FinishButton (Button)
│   │
│   └── Step6_Finalize (Panel)
│       ├── Title (TextMeshPro)
│       ├── SummaryScrollView (Scroll View)
│       │   └── SummaryText (TextMeshPro)
│       ├── BackButton (Button)
│       └── CreateProjectButton (Button)
│
└── EventSystem
```

### SetupWizardManager 연결

**SetupWizardManager GameObject**에서 다음 컴포넌트 연결:

1. **SetupWizardManager.cs**
   - `steps[]`: Step1~Step6 Panel GameObjects
   - `geminiClient`: GeminiClient 컴포넌트
   - `nanoBananaClient`: NanoBananaClient 컴포넌트
   - `projectData`: 런타임에 자동 생성됨

2. **Step1_GameOverview.cs**
   - `wizardManager`: SetupWizardManager
   - UI 요소들 (InputFields, Dropdowns, Buttons)

3. **Step2_CoreValues.cs** ~ **Step6_Finalize.cs**
   - 각각 동일한 방식으로 연결

### 초기 상태 설정

- Step1만 활성화, Step2~Step6은 비활성화
- SetupWizardManager가 단계 전환 관리

---

## 2. GameScene 구성

**경로**: `Assets/Scenes/GameScene.unity`

### Scene Hierarchy

```
GameScene
├── Canvas (UI Canvas)
│   ├── GameController (빈 GameObject)
│   │   └── GameController.cs
│   │   └── ChapterGenerationManager.cs
│   │   └── RuntimeSpriteManager.cs (Singleton)
│   │
│   ├── BackgroundLayer
│   │   └── BackgroundImage (Image - Full Screen)
│   │
│   ├── CharacterLayer
│   │   ├── LeftCharacter (Image + CanvasGroup)
│   │   ├── RightCharacter (Image + CanvasGroup)
│   │   └── CenterCharacter (Image + CanvasGroup)
│   │
│   ├── CGLayer
│   │   └── CGImage (Image + CanvasGroup - Full Screen)
│   │
│   ├── DialogueBoxLayer
│   │   └── DialogueBox (Panel)
│   │       ├── SpeakerNameText (TextMeshPro)
│   │       └── DialogueText (TextMeshPro)
│   │
│   ├── ChoicePanel (Panel)
│   │   ├── Choice1Button (Button + TextMeshPro)
│   │   ├── Choice2Button (Button + TextMeshPro)
│   │   ├── Choice3Button (Button + TextMeshPro)
│   │   └── Choice4Button (Button + TextMeshPro)
│   │
│   └── ControlPanel
│       ├── NextButton (Button)
│       ├── AutoButton (Button)
│       ├── SkipButton (Button)
│       ├── LogButton (Button)
│       ├── SaveButton (Button)
│       ├── LoadButton (Button)
│       └── SettingsButton (Button)
│
└── EventSystem
```

### GameController 연결

**GameController GameObject**:

1. **GameController.cs**
   - `projectData`: Resources/Projects/에서 로드 또는 Inspector에서 지정
   - `dialogueUI`: DialogueUI 컴포넌트
   - `chapterManager`: ChapterGenerationManager 컴포넌트
   - `spriteManager`: RuntimeSpriteManager 컴포넌트

2. **DialogueUI.cs** (Canvas에 직접 추가)
   - `leftCharacterImage`: LeftCharacter의 Image
   - `rightCharacterImage`: RightCharacter의 Image
   - `centerCharacterImage`: CenterCharacter의 Image
   - `leftCharacterGroup`: LeftCharacter의 CanvasGroup
   - `rightCharacterGroup`: RightCharacter의 CanvasGroup
   - `centerCharacterGroup`: CenterCharacter의 CanvasGroup
   - `speakerNameText`: DialogueBox의 SpeakerNameText
   - `dialogueText`: DialogueBox의 DialogueText
   - `dialogueBox`: DialogueBox의 Image
   - `dialogueBoxGroup`: DialogueBox의 CanvasGroup
   - `cgImage`: CGLayer의 CGImage
   - `cgGroup`: CGLayer의 CanvasGroup
   - `backgroundImage`: BackgroundLayer의 BackgroundImage
   - `choicePanel`: ChoicePanel GameObject
   - `choiceButtons[]`: Choice1~4 Buttons
   - `choiceTexts[]`: Choice1~4 TextMeshPro
   - `nextButton`: NextButton
   - `autoButton`: AutoButton
   - 기타 버튼들

### 레이어 순서 (Sorting Order)

1. BackgroundLayer: 0
2. CharacterLayer: 10
3. CGLayer: 20 (CG 표시 시 캐릭터 가림)
4. DialogueBoxLayer: 30
5. ChoicePanel: 40
6. ControlPanel: 50

---

## 3. 리소스 폴더 구조

프로젝트가 올바르게 작동하려면 다음 폴더 구조가 필요합니다:

```
Assets/
└── Resources/
    ├── APIConfig.asset (수동 생성 필요)
    ├── Projects/ (SetupWizard에서 자동 생성)
    ├── Generated/
    │   └── Characters/ (SetupWizard에서 자동 생성)
    │       └── {CharacterName}/
    │           ├── face_preview.png
    │           ├── neutral_normal.png
    │           ├── happy_normal.png
    │           └── ...
    └── Image/
        ├── Background/ (수동으로 배경 이미지 추가)
        │   ├── Classroom.png
        │   ├── Hallway.png
        │   └── ...
        └── CG/ (ChapterGenerationManager에서 자동 생성)
            ├── 1001_cg.png
            └── ...
```

### APIConfig.asset 생성 방법

1. Unity Editor에서 Project 창 열기
2. `Assets/Resources/` 폴더 우클릭
3. `Create > Iyagi > API Config` 선택
4. 파일명을 **정확히** `APIConfig`로 설정 (확장자 제외)
5. Inspector에서 API Keys 입력:
   - Gemini API Key
   - NanoBanana API Key
   - ElevenLabs API Key (옵션)

**중요**: `APIConfig.asset`는 `.gitignore`에 포함되어 있으므로 Git에 커밋되지 않습니다. 각 개발자가 로컬에서 직접 생성해야 합니다.

---

## 4. Scene 전환 설정

### Build Settings

1. `File > Build Settings` 열기
2. Scenes In Build에 다음 순서로 추가:
   - `SetupWizardScene` (Index 0)
   - `GameScene` (Index 1)

### Scene 전환 코드

**SetupWizardManager.cs**에 이미 구현됨:

```csharp
public void OnWizardComplete()
{
    Debug.Log("Setup complete! Loading game scene...");
    UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
}
```

**GameScene**에서 프로젝트 로드:

```csharp
// GameController.cs Start() 메서드에서
VNProjectData projectData = Resources.Load<VNProjectData>("Projects/{ProjectName}");
```

---

## 5. UI 스타일 가이드

### 권장 설정

**Canvas**:
- Canvas Scaler: Scale With Screen Size
- Reference Resolution: 1920x1080
- Match: 0.5 (Width/Height)

**TextMeshPro**:
- Font: Japanese-compatible font (예: Noto Sans CJK)
- Dialogue Text Size: 32-36
- Speaker Name Size: 40-44
- Choice Text Size: 28-32

**Buttons**:
- Normal Color: White (255, 255, 255)
- Highlighted Color: Light Gray (220, 220, 220)
- Pressed Color: Gray (180, 180, 180)
- Disabled Color: Dark Gray (120, 120, 120)

**Dialog Box**:
- Background Color: Semi-transparent Black (0, 0, 0, 180)
- Padding: 20-30px

**Character Images**:
- Preserve Aspect: True
- Raycast Target: False (성능 향상)

---

## 6. 테스트 체크리스트

### SetupWizardScene

- [ ] Step1 자동 완성 버튼 작동
- [ ] 모든 단계 전환 정상 작동
- [ ] 얼굴 프리뷰 생성 및 탐색
- [ ] 스탠딩 스프라이트 5종 생성
- [ ] NPC 추가 기능
- [ ] 최종 요약 표시
- [ ] 프로젝트 저장 (Resources/Projects/)

### GameScene

- [ ] 프로젝트 로드 성공
- [ ] 챕터 생성 및 표시
- [ ] 대화 타이핑 효과
- [ ] 캐릭터 스프라이트 표시
- [ ] 선택지 표시 및 선택
- [ ] Core Value 변화 추적
- [ ] CG 표시 및 해금
- [ ] 챕터 전환
- [ ] 엔딩 결정

---

## 7. 문제 해결

### API 키 오류

```
APIConfig is invalid!
```

**해결**: `Assets/Resources/APIConfig.asset` 생성 및 API 키 입력 확인

### 리소스 로드 실패

```
Resources.Load returned null
```

**해결**:
- 파일이 `Resources/` 폴더 내에 있는지 확인
- 경로에 확장자 제외 (예: `"APIConfig"` not `"APIConfig.asset"`)
- AssetDatabase.Refresh() 실행

### 스프라이트 생성 실패

```
Standing image generation failed
```

**해결**:
- API 키 유효성 확인
- 네트워크 연결 확인
- API 사용량 제한 확인

### Scene 전환 실패

```
Scene 'GameScene' couldn't be loaded
```

**해결**: Build Settings에 Scene 추가 확인

---

## 8. 다음 단계

Scene 구성 완료 후:

1. **Save/Load System 구현**
   - SaveDataManager.cs 작성
   - 세이브 슬롯 UI 추가

2. **CG Gallery 구현**
   - CGGalleryUI.cs 작성
   - 해금된 CG 표시

3. **Dialogue Log 구현**
   - DialogueLogUI.cs 작성
   - 이전 대화 내역 표시

4. **Settings UI 구현**
   - 텍스트 속도, 음량 조절
   - 언어 설정

5. **Title Screen 구현**
   - New Game / Continue / Gallery
   - Credits

---

이 가이드를 따라 Unity Editor에서 Scene을 구성하면 게임이 정상 작동합니다.
