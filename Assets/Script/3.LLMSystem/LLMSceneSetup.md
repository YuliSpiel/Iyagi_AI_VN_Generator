# LLM 씬 설정 가이드

## 권장: 별도의 LLM 전용 씬 생성

기존 게임 흐름을 방해하지 않고 LLM 모드를 테스트하기 위해 새로운 씬을 만드는 것을 권장합니다.

---

## 단계별 설정 방법

### 1단계: 새 씬 생성

1. Unity에서 `File > New Scene`
2. 씬 이름: `LLMGameScene`
3. `Assets/Scenes/` 폴더에 저장

---

### 2단계: 필수 매니저 추가

기존 매니저들을 씬에 추가 (DontDestroyOnLoad 싱글턴이므로 중복 걱정 없음):

**Hierarchy에 추가**:
```
Scene Root
├── GameManager (Prefab 또는 컴포넌트)
├── DataManager (Prefab 또는 컴포넌트)
└── SoundManager (Prefab 또는 컴포넌트)
```

**또는** 이미 타이틀 씬에서 생성되었다면 생략 가능 (DontDestroyOnLoad)

---

### 3단계: Canvas 설정

#### 3-1. Main Canvas 생성

```
Hierarchy > 우클릭 > UI > Canvas
이름: LLMCanvas
```

**Canvas 설정**:
- Render Mode: `Screen Space - Overlay`
- Canvas Scaler 컴포넌트:
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution: `1920 x 1080` (프로젝트 기준에 맞게)
  - Match: `0.5`

#### 3-2. EventSystem 확인
Canvas 생성 시 자동으로 EventSystem이 생성됩니다. 없다면 추가:
```
Hierarchy > 우클릭 > UI > Event System
```

---

### 4단계: DialogueUI 구성

#### 4-1. DialogueUI GameObject 생성

```
LLMCanvas 하위에:
  └── DialogueUI (Empty GameObject)
```

**DialogueUI 컴포넌트 추가**:
- `Add Component > DialogueUI`

#### 4-2. UI 요소 생성

**배경 이미지**:
```
DialogueUI 하위:
  └── BGImage (Image)
      - Anchor: Stretch all
      - Width/Height: 0
      - Image: 기본 배경 스프라이트
```

**대화창 패널**:
```
DialogueUI 하위:
  └── DialoguePanel (Image)
      - Anchor: Bottom Stretch
      - Height: 300
      - Color: 반투명 검정 (0, 0, 0, 200)
```

**화자명 텍스트**:
```
DialoguePanel 하위:
  └── SpeakerText (TextMeshPro - Text)
      - Anchor: Top Left
      - Pos X: 50, Pos Y: -30
      - Font Size: 36
      - Color: White
```

**대사 텍스트**:
```
DialoguePanel 하위:
  └── DialogueText (TextMeshPro - Text)
      - Anchor: Top Stretch
      - Pos Y: -80
      - Width: -100 (left/right 여백)
      - Height: 180
      - Font Size: 28
      - Color: White
```

**선택지 버튼들**:
```
DialoguePanel 하위:
  └── ChoicesContainer (Vertical Layout Group)
      ├── Choice1 (Button + TMP Text)
      ├── Choice2 (Button + TMP Text)
      └── Choice3 (Button + TMP Text)
```

**스탠딩 이미지 슬롯**:
```
DialogueUI 하위:
  ├── StandingImg1 (Image)
  │   - Anchor: Bottom Center
  │   - Width: 600, Height: 1200
  │   - Preserve Aspect: true
  └── StandingImg2 (Image)
      - Anchor: Bottom Center
      - Width: 600, Height: 1200
      - Preserve Aspect: true
```

#### 4-3. DialogueUI 컴포넌트 연결

Inspector에서 모든 참조 연결:
- **dialogueText**: DialogueText
- **speakerText**: SpeakerText
- **choice1/2/3**: Choice 버튼들
- **StandingImg1/2**: 스탠딩 이미지들
- **BGImage**: 배경 이미지

**프리셋 설정**:
- Left Position: `(-500, -50)`
- Center Position: `(0, -50)`
- Right Position: `(500, -50)`
- Small/Medium/Large Scale: `0.95 / 1.0 / 1.05`

---

### 5단계: LLM 시스템 추가

#### 5-1. LLMGameSystem GameObject 생성

```
Hierarchy Root:
  └── LLMGameSystem (Empty GameObject)
```

**컴포넌트 3개 추가**:
1. `Add Component > LLMStoryGenerator`
2. `Add Component > DynamicDialogueBuilder`
3. `Add Component > LLMGameController`

#### 5-2. LLMStoryGenerator 설정

Inspector:
- **Provider**: `OpenAI` (또는 Anthropic)
- **API Key**: `sk-...` (나중에 입력 가능)
- **Model**: `gpt-4` (또는 `gpt-3.5-turbo`)
- **Temperature**: `0.8`
- **Max Tokens**: `1000`

**Available Resources**:
- Characters: `Hans`, `Heilner` (리스트에 추가)
- Backgrounds: `riverside`, `council`, `black` (리스트에 추가)
- Looks: `Normal_Normal` (리스트에 추가)

---

### 6단계: 입력 UI 패널 생성

#### 6-1. InputPanel 구성

```
LLMCanvas 하위:
  └── InputPanel (Image)
      - Anchor: Center
      - Width: 800, Height: 400
      - Color: (50, 50, 50, 230)
```

**제목 텍스트**:
```
InputPanel 하위:
  └── TitleText (TMP)
      - Text: "LLM Story Generator"
      - Anchor: Top Center
      - Font Size: 48
```

**프롬프트 입력창**:
```
InputPanel 하위:
  └── PromptInputField (TMP_InputField)
      - Anchor: Center
      - Width: 700, Height: 150
      - Placeholder: "Enter your story prompt..."
      - Line Type: Multi Line Submit
      - Font Size: 24
```

**생성 버튼**:
```
InputPanel 하위:
  └── GenerateButton (Button)
      - Anchor: Bottom Center
      - Width: 300, Height: 60
      - Text: "Generate Story"
      - Font Size: 32
```

**상태 텍스트**:
```
InputPanel 하위:
  └── StatusText (TMP)
      - Anchor: Bottom Stretch
      - Pos Y: 80
      - Text: "Ready to generate..."
      - Font Size: 20
      - Alignment: Center
```

---

### 7단계: LLMGameController 연결

LLMGameSystem의 LLMGameController 컴포넌트 Inspector:

**LLM Components**:
- **Story Generator**: LLMStoryGenerator (같은 GameObject)
- **Dialogue Builder**: DynamicDialogueBuilder (같은 GameObject)

**UI Components**:
- **Dialogue UI**: DialogueUI GameObject
- **Input Panel**: InputPanel GameObject
- **Prompt Input Field**: PromptInputField
- **Generate Button**: GenerateButton
- **Status Text**: StatusText

**Settings**:
- **Context History Count**: `5`

---

## 완성된 Hierarchy 구조

```
LLMGameScene
├── GameManager
├── DataManager
├── SoundManager
├── LLMGameSystem
│   ├── LLMStoryGenerator
│   ├── DynamicDialogueBuilder
│   └── LLMGameController
├── Main Camera
├── EventSystem
└── LLMCanvas
    ├── BGImage
    ├── DialogueUI
    │   ├── DialoguePanel
    │   │   ├── SpeakerText
    │   │   ├── DialogueText
    │   │   └── ChoicesContainer
    │   │       ├── Choice1
    │   │       ├── Choice2
    │   │       └── Choice3
    │   ├── StandingImg1
    │   └── StandingImg2
    └── InputPanel
        ├── TitleText
        ├── PromptInputField
        ├── GenerateButton
        └── StatusText
```

---

## 빠른 테스트 방법

### 1. API 키 설정
LLMStoryGenerator의 API Key 필드에 키 입력

### 2. 씬 실행
Play 버튼 클릭

### 3. 프롬프트 입력
```
Hans and Heilner meet at the riverside and talk about their dreams.
```

### 4. Generate 버튼 클릭
10~15초 대기

### 5. 스토리 재생
스페이스/엔터로 진행

---

## 타이틀 씬과 연결 (선택사항)

타이틀 씬에서 LLM 모드로 진입하려면:

### 타이틀 씬에 버튼 추가

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleButtons : MonoBehaviour
{
    public Button llmModeButton;

    void Start()
    {
        if (llmModeButton != null)
        {
            llmModeButton.onClick.AddListener(OnLLMModeClicked);
        }
    }

    void OnLLMModeClicked()
    {
        SceneManager.LoadScene("LLMGameScene");
    }
}
```

---

## 다른 옵션: 기존 게임 씬에 통합

기존 게임 씬을 사용하려면:

1. 게임 씬 열기
2. 기존 DialogueSystem GameObject 비활성화
3. LLMGameSystem + InputPanel 추가
4. 기존 DialogueUI 재사용
5. 토글 버튼으로 모드 전환

**장점**: 기존 UI 재사용
**단점**: 시스템 충돌 가능성

---

## 문제 해결

### DialogueUI가 작동하지 않음
- GameManager.Instance가 존재하는지 확인
- DialogueUI의 모든 참조가 연결되었는지 확인

### 캐릭터/배경이 표시되지 않음
- Resources/Image/Standing/ 경로 확인
- Resources/Image/BG/ 경로 확인
- 파일명 대소문자 확인

### API 호출 실패
- API 키 확인
- 인터넷 연결 확인
- Console 로그 확인

---

## 권장 사항

✅ **별도 씬 사용** - 가장 안전하고 테스트하기 쉬움
✅ **프리팹 생성** - 설정 완료 후 프리팹으로 저장하여 재사용
✅ **API 키 보안** - ScriptableObject나 환경변수 사용
