# LLM 기반 실시간 비주얼 노벨 시스템

이 시스템은 LLM(Large Language Model)을 활용하여 실시간으로 스토리를 생성하고, 기존 CSV 기반 비주얼 노벨 렌더링 시스템과 통합합니다.

## 주요 컴포넌트

### 1. LLMStoryGenerator
- **역할**: OpenAI API 또는 Anthropic API를 호출하여 스토리 생성
- **기능**:
  - LLM에게 프롬프트 전송
  - 사용 가능한 리소스(캐릭터, 배경) 정보 제공
  - JSON 형식의 스토리 응답 수신

### 2. DynamicDialogueBuilder
- **역할**: LLM이 생성한 JSON을 DialogueRecord로 변환
- **기능**:
  - JSON 파싱
  - 기존 CSV 시스템과 호환되는 DialogueRecord 생성
  - ID 자동 할당 (10000번대 사용)

### 3. LLMGameController
- **역할**: 전체 LLM 게임 흐름 관리
- **기능**:
  - 사용자 프롬프트 입력 처리
  - 스토리 생성 요청 및 대기
  - 생성된 스토리를 DialogueUI로 렌더링
  - 컨텍스트 히스토리 관리 (연속된 스토리 생성)

## 설치 및 설정

### 1. Unity 씬 설정

1. 새로운 GameObject 생성: `LLMGameSystem`
2. 다음 컴포넌트들을 추가:
   - `LLMStoryGenerator`
   - `DynamicDialogueBuilder`
   - `LLMGameController`

### 2. LLMStoryGenerator 설정

Inspector에서 다음 항목을 설정:

- **Provider**: OpenAI 또는 Anthropic 선택
- **API Key**:
  - OpenAI: `sk-...`로 시작하는 키
  - Anthropic: `sk-ant-...`로 시작하는 키
- **Model**:
  - OpenAI: `gpt-4`, `gpt-3.5-turbo`
  - Anthropic: `claude-3-sonnet-20240229`
- **Temperature**: 0.7~1.0 권장 (창의성 조절)
- **Max Tokens**: 1000~2000 권장

**Available Resources** (현재 프로젝트 기준):
- Characters: `Hans`, `Heilner`
- Backgrounds: `riverside`, `council`, `black`
- Looks: `Normal_Normal`

### 3. LLMGameController 설정

Inspector에서 다음을 연결:

- **Story Generator**: LLMStoryGenerator 컴포넌트
- **Dialogue Builder**: DynamicDialogueBuilder 컴포넌트
- **Dialogue UI**: 기존 DialogueUI 컴포넌트
- **Input Panel**: 프롬프트 입력 UI 패널
- **Prompt Input Field**: TMP_InputField
- **Generate Button**: 생성 버튼
- **Status Text**: 상태 표시 텍스트

### 4. UI 설정

Input Panel에 다음 요소를 포함:

```
InputPanel (GameObject)
├── PromptInputField (TMP_InputField)
│   └── Placeholder: "Enter your story prompt..."
├── GenerateButton (Button)
│   └── Text: "Generate Story"
└── StatusText (TMP_Text)
    └── "Ready to generate..."
```

## 사용 방법

### 기본 사용

1. 게임 실행
2. 프롬프트 입력창에 원하는 스토리 방향 입력
3. "Generate Story" 버튼 클릭
4. LLM이 스토리를 생성할 때까지 대기 (5~15초)
5. 생성된 스토리가 비주얼 노벨 형식으로 자동 재생
6. 스페이스/엔터로 다음 대사 진행
7. 스토리 끝나면 새로운 프롬프트 입력 가능

### 프롬프트 예제

#### 예제 1: 기본 대화
```
Hans and Heilner meet by the riverside and discuss their dreams.
(한스와 하일너가 강가에서 만나 각자의 꿈에 대해 이야기한다.)
```

#### 예제 2: 갈등 상황
```
Hans is worried about the upcoming exam, but Heilner encourages him to follow his heart.
(한스는 다가오는 시험이 걱정되지만, 하일너는 그에게 마음을 따르라고 격려한다.)
```

#### 예제 3: 선택지 포함
```
Hans must decide whether to study hard or pursue his artistic passion. Give him two choices.
(한스는 열심히 공부할지 예술적 열정을 추구할지 결정해야 한다. 두 가지 선택지를 제공하라.)
```

#### 예제 4: 특정 배경 지정
```
At the council room, the principal announces an important decision that affects both Hans and Heilner.
(회의실에서 교장이 한스와 하일너 모두에게 영향을 미치는 중요한 결정을 발표한다.)
```

### 연속 스토리 생성

시스템은 자동으로 이전 스토리의 컨텍스트를 유지합니다:

1. 첫 번째 프롬프트: `Hans meets Heilner at the riverside.`
2. 스토리 재생 후
3. 두 번째 프롬프트: `They decide to skip class and go on an adventure.`
   - 이전 대화 내용이 자동으로 컨텍스트에 포함됨

컨텍스트를 초기화하려면 `LLMGameController.ClearContext()` 호출

## LLM 응답 형식

LLM은 다음과 같은 JSON 배열을 반환해야 합니다:

```json
[
  {
    "char1Name": "Hans",
    "char1Look": "Normal_Normal",
    "char1Pos": "Center",
    "char1Size": "Medium",
    "bg": "riverside",
    "nameTag": "Hans",
    "lineEng": "It's a beautiful day, isn't it?",
    "lineKr": "정말 아름다운 날이야, 그렇지 않아?"
  },
  {
    "char1Name": "Hans",
    "char1Look": "Normal_Normal",
    "char1Pos": "Left",
    "char1Size": "Large",
    "char2Name": "Heilner",
    "char2Look": "Normal_Normal",
    "char2Pos": "Right",
    "char2Size": "Medium",
    "bg": "riverside",
    "nameTag": "Heilner",
    "lineEng": "Indeed. Days like this make me want to write poetry.",
    "lineKr": "그래. 이런 날은 시를 쓰고 싶게 만들어."
  },
  {
    "char1Name": "Hans",
    "char1Pos": "Center",
    "nameTag": "Hans",
    "lineEng": "What should I do?",
    "lineKr": "나는 뭘 해야 할까?",
    "choices": [
      {
        "textEng": "Study hard for the exam",
        "textKr": "시험 공부를 열심히 한다"
      },
      {
        "textEng": "Follow my artistic passion",
        "textKr": "예술적 열정을 따른다"
      }
    ]
  }
]
```

### 필드 설명

| 필드 | 필수 | 설명 | 기본값 |
|------|------|------|--------|
| char1Name | No | 캐릭터 1 이름 | - |
| char1Look | No | 캐릭터 1 표정/포즈 | Normal_Normal |
| char1Pos | No | 캐릭터 1 위치 (Left/Center/Right) | Center |
| char1Size | No | 캐릭터 1 크기 (Small/Medium/Large) | Medium |
| char2Name | No | 캐릭터 2 이름 | - |
| char2Look | No | 캐릭터 2 표정/포즈 | Normal_Normal |
| char2Pos | No | 캐릭터 2 위치 | Right |
| char2Size | No | 캐릭터 2 크기 | Medium |
| bg | No | 배경 이름 | - |
| nameTag | Yes | 화자명 | - |
| lineEng | Yes | 영어 대사 | - |
| lineKr | Yes | 한국어 대사 | - |
| choices | No | 선택지 배열 (최대 3개) | - |

## 고급 기능

### 프로그래밍 방식 스토리 생성

```csharp
// LLMGameController 참조
LLMGameController controller = FindObjectOfType<LLMGameController>();

// 프롬프트로 스토리 생성
controller.GenerateStoryFromPrompt("Hans and Heilner go on an adventure.");

// 컨텍스트 초기화
controller.ClearContext();

// 스토리 리셋
controller.ResetStory();
```

### 커스텀 리소스 추가

새로운 캐릭터나 배경을 추가한 경우:

1. LLMStoryGenerator의 Inspector에서:
   - Available Characters에 캐릭터 이름 추가
   - Available Backgrounds에 배경 이름 추가
   - Available Looks에 표정/포즈 이름 추가

2. 리소스 폴더에 실제 이미지 추가:
   - `Resources/Image/Standing/{캐릭터명}_{표정}.asset`
   - `Resources/Image/BG/{배경명}.png`

### API 키 보안

**중요**: API 키를 코드에 직접 하드코딩하지 마세요!

권장 방법:

1. **에디터 전용 ScriptableObject**:
```csharp
[CreateAssetMenu(fileName = "APIConfig", menuName = "LLM/API Config")]
public class APIConfig : ScriptableObject
{
    public string apiKey;
}
```

2. **환경 변수 사용**:
```csharp
string apiKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
```

3. **빌드 시 제외**: API 키 설정 파일을 `.gitignore`에 추가

## 트러블슈팅

### API 호출 실패
- API 키가 올바른지 확인
- 인터넷 연결 확인
- API 사용량 제한 확인 (OpenAI Rate Limit)

### 잘못된 JSON 응답
- Model 설정 확인 (gpt-4 권장)
- Temperature 낮추기 (0.7 이하)
- System Prompt 수정

### 캐릭터/배경이 표시되지 않음
- 리소스 이름 대소문자 확인
- Resources 폴더 경로 확인
- Available Resources 설정 확인

### 한국어 대사가 제대로 생성되지 않음
- System Prompt에 "Generate Korean translations" 추가
- 또는 후처리로 번역 API 사용

## 성능 최적화

- **Max Tokens**: 필요한 만큼만 사용 (비용 절감)
- **Context History Count**: 5~10개 권장 (너무 많으면 느려짐)
- **Temperature**: 0.7~0.8 (빠른 응답과 품질의 균형)

## 향후 개선 사항

- [ ] 선택지에 따른 분기 스토리 생성
- [ ] 다중 언어 지원 확장 (일본어 등)
- [ ] 스토리 저장/불러오기 기능
- [ ] 캐릭터 감정 표현 자동 선택
- [ ] 배경음악/효과음 자동 선택
- [ ] 로컬 LLM 지원 (Ollama 등)

## 라이선스 및 주의사항

- OpenAI API 사용 시 [OpenAI 이용약관](https://openai.com/policies/terms-of-use) 준수
- Anthropic API 사용 시 [Anthropic 이용약관](https://www.anthropic.com/legal/commercial-terms) 준수
- API 비용 발생에 유의
- 생성된 콘텐츠의 저작권 및 책임 확인

## 참고 자료

- [OpenAI API Documentation](https://platform.openai.com/docs/api-reference)
- [Anthropic API Documentation](https://docs.anthropic.com/claude/reference/getting-started-with-the-api)
- Unity WebRequest: [Unity Manual](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html)
