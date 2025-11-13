# Portfolio: System Design for AI Collaboration

> **IYAGI & Kurz AI Studio**
> Jiyun Kim · AI Agent Developer 지원자
> github.com/YuliSpiel │ jiyoonkim33@naver.com

---

## 슬라이드 1. 표지 (Cover)

- 배경: 하얀 배경 + 파란/보라색 톤 그래픽
- 제목: **상상을 구조로, 구조를 현실로.**
- 부제: *IYAGI & Kurz AI Studio: System Design for AI Collaboration*
- 하단 정보:

    > Jiyun Kim · AI Agent Developer 지원자
    > github.com/YuliSpiel │ jiyoonkim33@naver.com
    >

---

## 슬라이드 2. 나를 정의하는 한 문장 (About Me)

**목표:** 기획력과 시스템적 사고를 한 문장으로 요약

> "I design systems where AI becomes a co-creator, not just a tool."
>

- 왼쪽: 인포그래픽('기획 → 설계 → 구현'의 순환 구조)
- 오른쪽: 간단한 경력 요약
    - 전공: German Literature + Neuroscience
    - 주요 기술: Unity, FastAPI, LangChain, Gemini API, Redis, Celery
    - 핵심 역량: AI System Design, Multi-Agent Architecture, UX Thinking

---

## 슬라이드 3. 프로젝트 개요

| 프로젝트 | 설명 | 역할 | 핵심 기술 |
| --- | --- | --- | --- |
| **IYAGI** | AI 기반 비주얼노벨 자동 생성 시스템 | 기획·시스템 설계·Unity 구현 | Unity 2022.3, Gemini API, C# Coroutines, ScriptableObject |
| **Kurz AI Studio** | 멀티에이전트 숏폼 영상 자동 생성 서비스 | 풀스택 개발·시스템 아키텍처 설계 | FastAPI, Celery, Redis, React, Gemini 2.5 Flash, Multi-Agent FSM |

하단 문장:

> 두 프로젝트는 서로 다른 방식으로 AI 시스템을 설계한 실험입니다.
> IYAGI: State-Aware Prompt Engineering | Kurz: Multi-Agent Orchestration
>

---

## 슬라이드 4. IYAGI – Overview

**한 문장 소개:**

> 사용자의 최소 입력만으로 스토리, 인물, 대사, 선택지를 자동 생성하는 AI 기반 비주얼노벨 제작 플랫폼.
>

**핵심 특징:**

- **최소 입력으로 완전한 게임 생성**: 제목 + 줄거리만 입력하면 플레이 가능한 VN 완성
- **동적 스토리 분기**: Core Value + Affection System 기반 다중 엔딩
- **일관된 캐릭터 비주얼**: Seed 기반 이미지 생성으로 동일 캐릭터 유지
- **병렬 리소스 생성**: Fan-Out Barrier 패턴으로 이미지/오디오 동시 생성
- **상태 인식 캐싱**: 플레이어 선택에 따라 다른 챕터 캐싱

**키워드 바:** *Unity 2022.3 · Gemini API · Seed-based Image Gen · State-Aware Caching*

---

## 슬라이드 5. IYAGI – 시스템 구조

```
┌─────────────────────────────────────────────────────────────┐
│                    Setup Wizard (Editor)                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ STEP 1   │→ │ STEP 2   │→ │ STEP 3   │→ │ STEP 4-6 │   │
│  │ 게임개요  │  │ 가치설정  │  │ 구조설정  │  │ 캐릭터   │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│         ↓ Gemini API                ↓ NanoBanana API       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │       ParallelAssetGenerator (Fan-Out Barrier)       │  │
│  │  - Cycle 1: 프롤로그 + Ch.1~3 (병렬 4 tasks)         │  │
│  │  - Cycle 2: Ch.4~6 (병렬 4 tasks)                    │  │
│  │  - 4-8x faster than sequential                       │  │
│  └──────────────────────────────────────────────────────┘  │
│         ↓                                                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            VNProjectData.asset (ScriptableObject)    │  │
│  │  - Project metadata + character seeds                │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    Runtime Game System                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          ChapterGenerationManager (1,085 lines)      │  │
│  │  - Scene-based generation (6 scenes per chapter)     │  │
│  │  - State-aware caching (Core Values + Affections)    │  │
│  │  - Persistent disk cache + JSON repair               │  │
│  └─────────────┬────────────────────────────────────────┘  │
│                ↓ Gemini 1.5 Flash                           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         AIDataConverter (325 lines)                  │  │
│  │  - FromAIJson() → List<DialogueRecord>               │  │
│  │  - Automatic JSON repair for truncated responses     │  │
│  └─────────────┬────────────────────────────────────────┘  │
│                ↓                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         GameController (834 lines)                    │  │
│  │  - Core Value System (derived skills → core values) │  │
│  │  - Affection System (NPC appearance control)         │  │
│  │  - Choice handling & state management               │  │
│  │  - SaveFile auto-update after chapter completion    │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

**핵심 설계 포인트:**

- **Setup Wizard:** 6단계 시각적 프로젝트 설정 + 병렬 리소스 생성
- **ChapterGenerationManager:** 상태 기반 캐싱 + JSON 복구 로직
- **GameController:** Core Value/Affection 기반 분기 + 자동 세이브

---

## 슬라이드 6. IYAGI – 설계 포인트

| 설계 요소 | 설명 | 구현 |
| --- | --- | --- |
| **Core Value & Derived Skills** | 캐릭터 성향·스탯 기반 대화 분기 설계 | 선택지 → Derived Skills 증가 → Core Value = Skills 합계 |
| **Affection System** | NPC 호감도에 따라 등장 여부 및 엔딩 변화 | 기본 4컷 + 호감 NPC 추가 + 로맨스 선택지 |
| **State-Aware Caching** | 플레이어 선택에 따라 다른 챕터 캐싱 | Core Values + Affections 해시 기반 캐시 인덱싱 |
| **Seed-Based Image Gen** | 동일 캐릭터의 시각적 일관성 유지 | NanoBanana API seed 저장 + 재사용 |
| **Parallel Asset Generation** | 4-8배 빠른 프로젝트 생성 | Fan-Out Barrier 패턴 (Cycle 1 + 2 병렬) |
| **JSON Repair System** | LLM 출력 오류 자동 복구 | 잘린 JSON 자동 감지 + 재생성 |

> 결과: "AI가 인간의 서사를 함께 쓰는 시스템"을 구현
>

---

## 슬라이드 7. Kurz AI Studio – Overview

**한 문장 소개:**

> 단 하나의 프롬프트로 영상 제작 전 과정을 수행하는 멀티에이전트 기반 숏폼 자동 생성 시스템.
>

**프로세스:**

```
프롬프트 입력
  → 시나리오 생성 (기획자 Agent)
  → 이미지·음성·음악 생성 (병렬 Agent들)
  → 영상 합성 (감독 Agent)
  → 품질 검수 (QA Agent)
  → 완성된 숏폼 영상 (MP4)
```

**기술 스택:**

- **Backend:** FastAPI, Celery, Redis, Gemini 2.5 Flash
- **Frontend:** React + TypeScript + Vite
- **Video Processing:** MoviePy (9:16 세로형, 30fps)
- **AI Services:** Gemini Flash 2.0 (이미지), ElevenLabs (TTS + Music)

---

## 슬라이드 8. Kurz AI Studio – 시스템 아키텍처 (핵심 슬라이드)

```
                   User Prompt
                        ↓
              ┌─────────────────┐
              │  Orchestrator   │ (FastAPI + FSM)
              │   (FSM + Redis) │
              └─────────────────┘
                        ↓
              ┌─────────────────┐
              │ StoryPlanner    │ (Gemini 2.5 Flash)
              │ (기획자 Agent)   │
              └─────────────────┘
                        ↓
      ┌─────────────────┴─────────────────┐
      │       Barrier (Fan-out)           │
      │   Celery Chord Pattern            │
      └─────────────────┬─────────────────┘
                        ↓
       ┌────────────────┼────────────────┐
       ↓                ↓                ↓
 ┌─────────┐     ┌─────────┐     ┌─────────┐
 │Designer │     │Composer │     │  Voice  │
 │ Agent   │     │ Agent   │     │  Agent  │
 │(이미지) │     │(BGM)    │     │(TTS)    │
 └─────────┘     └─────────┘     └─────────┘
       │                │                │
       └────────────────┼────────────────┘
                        ↓
              ┌─────────────────┐
              │   Director      │ (MoviePy)
              │  (영상 합성)     │
              └─────────────────┘
                        ↓
              ┌─────────────────┐
              │   QA Agent      │ (품질 검수)
              └─────────────────┘
                        ↓
              final_video.mp4
```

**핵심 포인트:**

- **FSM (Finite State Machine):** 상태 관리 (INIT → PLOT_GENERATION → ASSET_GENERATION → RENDERING → QA → END)
- **Fan-out/Barrier 패턴:** Celery Chord를 사용해 병렬 작업 → 콜백 구조
- **Redis:** 상태 공유 (FastAPI ↔ Celery Worker 간 FSM 동기화)
- **WebSocket:** 실시간 진행률 브로드캐스트

---

## 슬라이드 9. Kurz AI Studio – 기술 상세

### 시스템 구성요소

| 구성요소 | 기술 스택 | 역할 |
| --- | --- | --- |
| **Orchestrator** | FastAPI + Redis | Agent 관리 및 FSM 상태 추적 |
| **Task Queue** | Celery + Redis | 비동기 병렬 처리 (Fan-out/Chord) |
| **LLM** | Gemini 2.5 Flash | 스토리·시나리오·캐릭터 생성 |
| **Image Gen** | Gemini Flash 2.0 Experimental | 9:16 세로형 이미지 생성 |
| **TTS** | ElevenLabs API | 한국어 음성 합성 |
| **Music** | ElevenLabs Sound Effects | 배경음악 자동 작곡 |
| **Video Composer** | MoviePy | 9:16 세로형 영상 합성 (30fps) |
| **Frontend** | React + TypeScript + Vite | 사용자 인터페이스 + WebSocket 실시간 업데이트 |

### Agent별 역할

| Agent | 역할 | 입력 | 출력 |
| --- | --- | --- | --- |
| **기획자** (plan.py) | 시나리오 생성 | prompt, mode, num_cuts | characters.json, plot.json, layout.json |
| **디자이너** (designer.py) | 이미지 생성 | layout.json | images/*.png (9:16 or 1:1) |
| **작곡가** (composer.py) | 배경음악 생성 | layout.json | audio/global_bgm.mp3 |
| **성우** (voice.py) | 음성 합성 | layout.json, characters.json | audio/{scene}_{line}.mp3 |
| **감독** (director.py) | 영상 합성 | layout.json + assets | final_video.mp4 |
| **QA** (qa.py) | 품질 검수 | final_video.mp4 | QA report (통과/재생성) |

> "팬아웃/배리어 구조를 통해 동시 작업 효율을 극대화했습니다."
>

---

## 슬라이드 10. 설계 철학: Single-Agent vs Multi-Agent

**핵심 문장:**

> "문제에 맞는 AI 시스템 아키텍처를 선택한다."
>

### 대비 다이어그램:

```
     ┌────────────────────────────┐           ┌────────────────────────────┐
     │         IYAGI              │           │      Kurz AI Studio        │
     │  (State-Aware Single)      │           │  (Multi-Agent Parallel)    │
     ├────────────────────────────┤           ├────────────────────────────┤
     │                            │           │                            │
     │   User Input + GameState   │           │      User Prompt           │
     │           ↓                │           │           ↓                │
     │   ChapterGenerationMgr     │           │     Orchestrator           │
     │           ↓                │           │           ↓                │
     │   Gemini (단일 호출)       │           │   ┌───────┼───────┐        │
     │   + State Context          │           │   ↓       ↓       ↓        │
     │           ↓                │           │  Plot  Image   Audio       │
     │   Scene 1 → 2 → ... → 6    │           │  Agent  Agent  Agent       │
     │   (순차 누적 컨텍스트)      │           │   └───────┼───────┘        │
     │           ↓                │           │           ↓                │
     │   DialogueRecords          │           │      Director              │
     │                            │           │           ↓                │
     │                            │           │      final_video.mp4       │
     └────────────────────────────┘           └────────────────────────────┘
```

### 설계 비교

| 요소 | IYAGI | Kurz AI Studio |
| --- | --- | --- |
| **아키텍처** | State-Aware Single-Agent | Multi-Agent Orchestration |
| **LLM 역할** | 상태 기반 컨텍스트 생성 | 역할별 전문화된 생성 |
| **협업 방식** | 순차적 컨텍스트 누적 | 병렬 작업 + 동기화 |
| **상태 관리** | GameState + Caching | FSM + Redis |
| **확장성** | 프롬프트 엔지니어링으로 확장 | Agent 추가로 확장 |
| **복잡도** | 낮음 (단일 워크플로우) | 높음 (상태 동기화 필수) |

### 왜 다르게 설계했는가?

**IYAGI가 Single-Agent를 선택한 이유:**
- 스토리는 **순차적 흐름**이 중요 (씬 간 일관성)
- Core Values/Affection 같은 **상태 정보**가 모든 대사에 영향
- 멀티에이전트 도입 시 **불필요한 복잡성** 증가
- 프롬프트 엔지니어링만으로 충분히 제어 가능

**Kurz가 Multi-Agent를 선택한 이유:**
- 이미지/음성/음악 생성은 **독립적 작업** (병렬 처리 가능)
- 각 작업이 **전문화된 API** 필요 (Gemini Image, ElevenLabs TTS/Music)
- 순차 처리 시 **시간 낭비** (3분 → 1분으로 단축)
- Agent 간 명확한 **역할 분리** (기획자 → 디자이너 → 감독)

### 공통 원칙

두 프로젝트 모두:
1. **LLM 출력 검증 + 재시도** (Plot Validation, JSON Repair)
2. **병렬 처리 최적화** (IYAGI: Asset 생성, Kurz: Agent 작업)
3. **상태 기반 제어** (IYAGI: GameState, Kurz: FSM)
4. **실시간 피드백** (IYAGI: Coroutine, Kurz: WebSocket)

---

## 슬라이드 11. IYAGI – 주요 구현 성과

### 1. Scene-Based Chapter Generation (6 scenes per chapter)

```csharp
// ChapterGenerationManager.cs (lines 285-415)
// Generate chapter in 6 scenes to avoid JSON truncation
for (int sceneIdx = 0; sceneIdx < 6; sceneIdx++) {
    string prompt = BuildScenePrompt(sceneIdx, previousScenes);
    string rawJson = await geminiClient.GenerateDialogueAsync(prompt);
    List<DialogueRecord> sceneRecords = AIDataConverter.FromAIJson(rawJson);
    chapterRecords.AddRange(sceneRecords);
}
```

- **문제:** Gemini API가 긴 챕터 생성 시 JSON 잘림 발생
- **해결:** 챕터를 6개 씬으로 분할 생성 → 각 씬당 3-5개 대사만 생성
- **결과:** JSON 잘림 방지 + 안정적인 대화 흐름

### 2. State-Aware Caching System

```csharp
// ChapterGenerationManager.cs (lines 450-520)
string cacheKey = GenerateCacheKey(chapterNumber, gameState);
// gameState includes: Core Values + Affections + Previous choices
if (CacheExists(cacheKey)) {
    return LoadFromCache(cacheKey);
}
```

- 플레이어의 Core Values + Affection 상태에 따라 **다른 스토리 생성**
- Persistent disk cache로 재플레이 시 빠른 로딩
- Cache hit ratio: ~70% (테스트 기준)

### 3. Parallel Asset Generation (4-8x speedup)

```csharp
// ParallelAssetGenerator.cs (lines 180-250)
// Cycle 1: Prologue + Ch.1~3 in parallel
List<Coroutine> cycle1Tasks = new List<Coroutine> {
    GeneratePrologueAsync(),
    GenerateChapter1Async(),
    GenerateChapter2Async(),
    GenerateChapter3Async()
};
yield return new WaitUntil(() => cycle1Tasks.All(IsComplete));

// Cycle 2: Ch.4~6 in parallel
List<Coroutine> cycle2Tasks = new List<Coroutine> { ... };
yield return new WaitUntil(() => cycle2Tasks.All(IsComplete));
```

- **Sequential:** ~12-15분 (프롤로그 + 6챕터)
- **Parallel (2 cycles):** ~3-4분
- **Speedup:** 4-8배 (API 응답 시간에 따라 변동)

### 4. Core Value Derived System

```csharp
// GameController.cs (lines 420-480)
// Choices affect Derived Skills, not Core Values directly
void ApplyChoiceEffects(ChoiceEffect effect) {
    derivedSkills[effect.skillName] += effect.value;
    RecalculateCoreValues(); // Core Value = Sum of related Derived Skills
}

void RecalculateCoreValues() {
    coreValues["용기"] = derivedSkills["검술"]
                       + derivedSkills["방어"]
                       + derivedSkills["돌격"];
}
```

- 선택지 → Derived Skills 증가 → Core Value = Derived Skills 합계
- 더 세밀한 캐릭터 성장 시스템

### 5. Automatic SaveFile Update

```csharp
// GameController.cs (lines 650-720)
void OnChapterComplete(int chapterNumber) {
    SaveFile currentSave = SaveDataManager.Instance.GetCurrentSaveFile();
    currentSave.currentChapter = chapterNumber + 1;
    currentSave.gameState = ExportGameState(); // Skills, Values, Affections
    currentSave.lastPlayedDate = DateTime.Now;
    SaveDataManager.Instance.SaveToFile(currentSave);
}
```

- 챕터 완료 시 자동으로 SaveFile 업데이트
- 사용자가 수동 저장할 필요 없음

---

## 슬라이드 12. Kurz AI Studio – 주요 구현 성과

### 1. FSM 기반 워크플로우 제어

```python
# backend/app/orchestrator/fsm.py
class RunState(Enum):
    INIT = "INIT"
    PLOT_GENERATION = "PLOT_GENERATION"
    ASSET_GENERATION = "ASSET_GENERATION"
    RENDERING = "RENDERING"
    QA = "QA"
    END = "END"
    FAILED = "FAILED"
```

- Redis 기반 상태 공유 (FastAPI ↔ Celery 간 동기화)
- 상태 전이 검증 (Invalid transition 방지)
- 재시도 로직 (QA 실패 시 PLOT_GENERATION으로 복귀)

### 2. Celery Chord 패턴으로 병렬 처리

```python
# backend/app/tasks/plan.py (lines 352-359)
asset_tasks = group(
    designer_task.s(run_id, json_path_str, spec),
    composer_task.s(run_id, json_path_str, spec),
    voice_task.s(run_id, json_path_str, spec),
)
workflow = chord(asset_tasks)(director_task.s(run_id, json_path_str))
```

- 디자이너, 작곡가, 성우가 **동시에** 작업
- 모두 완료되면 감독 Agent가 자동 호출 (Barrier)

### 3. 실시간 진행률 WebSocket 브로드캐스트

```python
# backend/app/main.py (lines 46-97)
async def redis_listener():
    pubsub = redis_client.pubsub()
    await pubsub.subscribe("autoshorts:progress")
    async for message in pubsub.listen():
        await broadcast_to_websockets(run_id, data)
```

- Redis Pub/Sub로 Celery → FastAPI 진행률 전달
- WebSocket으로 Frontend에 실시간 브로드캐스트

### 4. Plot Validation + Auto-Retry

```python
# backend/app/tasks/plan.py (lines 246-280)
validation_errors = _validate_plot_json(...)
if validation_errors:
    if retry_count < max_retries:
        raise self.retry(countdown=3, max_retries=2)
    else:
        raise ValueError("Max retries exceeded")
```

- LLM 생성 품질 자동 검증 (더미 텍스트, 중복, 필드 누락 체크)
- 실패 시 최대 2회 자동 재생성

---

## 슬라이드 13. 기술적 도전과 해결

### IYAGI

| 도전 | 문제 상황 | 해결 방법 | 결과 |
| --- | --- | --- | --- |
| **JSON 잘림** | Gemini API가 긴 챕터 생성 시 응답 잘림 | Scene-based generation (6 scenes/chapter) | 안정성 95% → 99% |
| **캐릭터 일관성** | 매번 다른 얼굴 이미지 생성 | Seed 기반 이미지 생성 + seed 저장 | 동일 캐릭터 유지 |
| **생성 속도** | 순차 생성 시 12-15분 소요 | Fan-Out Barrier 패턴 (2 cycles 병렬) | 3-4분으로 단축 (4-8배) |
| **분기 복잡도** | Core Value 직접 수정 시 예측 불가 | Derived Skills → Core Values 계층 구조 | 세밀한 성장 시스템 |

### Kurz AI Studio

| 도전 | 문제 상황 | 해결 방법 | 결과 |
| --- | --- | --- | --- |
| **상태 동기화** | FastAPI(비동기) ↔ Celery(별도 프로세스) | Redis Single Source of Truth + Pickle | FSM 일관성 보장 |
| **LLM 출력 품질** | 가끔 더미 데이터 생성 | Plot Validation (15개 룰) + Auto-Retry | 성공률 85% → 98% |
| **MoviePy 렌더링** | 한글 + 멀티라인 + stroke height 버그 | 정교한 height 계산 로직 | 안정적 렌더링 |

---

## 슬라이드 14. 정량적 성과

### IYAGI 성능 지표

- **프로젝트 생성 시간:**
    - Sequential: ~12-15분 (프롤로그 + 6챕터)
    - Parallel (2 cycles): ~3-4분
    - Speedup: **4-8배**
- **시스템 안정성:**
    - Scene-based generation 적용 후: JSON 잘림 99% 방지
    - State-aware caching: Cache hit ratio ~70%
    - Auto-save system: 100% 자동 저장 성공
- **코드 규모:**
    - Production: **15,281 lines** (C# scripts only)
    - Core runtime: ~7,000 lines (36 files)
    - Setup wizard: ~3,200 lines (8 files)
    - AI integration: ~1,500 lines (6 files)
    - Dev tools: ~2,771 lines (13 files)
- **기술 스택:**
    - Engine: Unity 2022.3.4f1
    - APIs: Gemini 1.5 Flash (text), Gemini Image (visuals), ElevenLabs (audio)
    - Architecture: ScriptableObject + Coroutine-based async

### Kurz AI Studio 성능 지표

- **영상 생성 시간:** 평균 2-3분 (3컷 기준)
    - Plot Generation: 10-15초
    - Asset Generation (병렬): 60-90초
    - Video Composition: 30-45초
    - QA: 5-10초
- **시스템 안정성:**
    - Plot Validation 적용 후 성공률: 85% → 95%
    - Auto-Retry로 최종 성공률: 98%
- **코드 규모:**
    - Backend: ~3,500 lines (Python)
    - Frontend: ~1,200 lines (TypeScript/React)
    - Total: ~4,700 lines
- **기술 스택:**
    - Backend: Python 3.11, FastAPI, Celery, Redis
    - Frontend: React 18, TypeScript, Vite
    - AI: Gemini 2.5 Flash (LLM), Gemini Flash 2.0 (Image), ElevenLabs (TTS/Music)
    - Video: MoviePy 2.2 (9:16, 30fps, H.264)

---

## 슬라이드 15. 향후 확장 및 배운 점

### AI System Designer로서의 방향성

- **현재:** Prompt → Memory → Orchestration 통합 설계
- **향후 실험:**
    - **IYAGI:** Emotion Vector 기반 스토리 제어 (Affinity + Emotion 통합)
    - **Kurz:** Agent Feedback Loop (QA Agent → 기획자 Agent 피드백)
    - **공통:** Multi-Modal Memory (텍스트 + 이미지 + 음성 통합)

### 배운 점

1. **구조 설계의 중요성**
    - 단순 AI 호출이 아닌, "협업 가능한 구조"를 설계하는 사고방식
    - FSM + Fan-out/Barrier 패턴으로 복잡한 워크플로우 제어
2. **시스템적 사고**
    - Agent 간 데이터 흐름 설계 (JSON 기반 계약)
    - 상태 관리 및 에러 핸들링 (Retry, Validation, Caching)
    - 실시간 사용자 피드백 (WebSocket, Coroutine)
3. **협업 구조 설계가 콘텐츠 퀄리티를 좌우**
    - Agent들이 독립적이면서도 협력하는 구조 → 높은 품질
    - Scene-based generation + Plot Validation → LLM 출력 안정성 향상
4. **병렬 처리의 효율성**
    - IYAGI: 4-8배 속도 향상 (Parallel Asset Generation)
    - Kurz: 3배 속도 향상 (Celery Chord Pattern)

---

## 슬라이드 16. 데모 영상 / 스크린샷

### IYAGI

1. **Setup Wizard UI**
    - Step 1: 게임 개요 입력 화면
    - Step 4-5: 캐릭터 설정 + 이미지 프리뷰
    - Parallel Asset Generation 진행률 표시
2. **In-Game Screenshots**
    - 대화 화면 (DialogueUI)
    - 선택지 화면 (Core Value 증감 표시)
    - CG Gallery + Ending Collection
3. **System Diagrams**
    - Scene-based generation flow
    - Core Value → Derived Skills 계층 구조

### Kurz AI Studio

1. **Frontend UI**
    - RunForm: 프롬프트 입력 화면
    - RunStatus: 실시간 진행률 표시
    - PlotReviewModal: Plot 검수 UI
2. **생성된 영상 썸네일**
    - General Mode 예시 영상
    - Story Mode 예시 영상
3. **시스템 다이어그램 애니메이션**
    - Agent 간 데이터 흐름 시각화

---

## 슬라이드 17. 마무리 / 연락처

**중앙 문장:**

> "I design systems where AI becomes a co-creator, not just a tool."
>

**좌측:**

- Jiyun Kim
- AI Agent Developer 지원자
- AI System Architecture Specialist

**우측:**

- GitHub: github.com/YuliSpiel
- Email: jiyoonkim33@naver.com
- Portfolio: [Notion 링크 or QR 코드]

**하단:** Thank you for watching.

---

# 추가 참고 자료

## 프로젝트 링크

### IYAGI

- **GitHub Repository:** https://github.com/YuliSpiel/Iyagi_AI_VN_Generator
- **Documentation:**
  - [CLAUDE.md](./CLAUDE.md) - Technical overview
  - [systemdocs/](./systemdocs/) - Detailed system documentation
  - [userdocs/](./userdocs/) - User guides

### Kurz AI Studio

- **GitHub Repository:** [링크 삽입]
- **Live Demo:** [링크 삽입 (선택)]
- **Documentation:** CLAUDE.md, README.md

---

## 부록: 핵심 파일 참조

### IYAGI 주요 파일

| 파일 | 라인 수 | 역할 |
| --- | --- | --- |
| [ChapterGenerationManager.cs](Assets/Scripts/ChapterGenerationManager.cs) | 1,085 | 동적 챕터 생성 + 상태 인식 캐싱 |
| [GameController.cs](Assets/Scripts/GameController.cs) | 834 | Core Value System + 게임 상태 관리 |
| [SaveDataManager.cs](Assets/Scripts/SaveDataManager.cs) | 609 | SaveFile 자동 저장/로드 |
| [ParallelAssetGenerator.cs](Assets/Editor/SetupWizard/ParallelAssetGenerator.cs) | 535 | Fan-Out Barrier 패턴 구현 |
| [GeminiClient.cs](Assets/Scripts/AI/GeminiClient.cs) | 182 | Gemini API 통합 + 재시도 로직 |
| [AIDataConverter.cs](Assets/Scripts/AI/AIDataConverter.cs) | 325 | JSON 파싱 + 복구 로직 |

### Kurz AI Studio 주요 파일

| 파일 | 라인 수 | 역할 |
| --- | --- | --- |
| backend/app/tasks/plan.py | ~800 | 기획자 Agent + Plot Validation |
| backend/app/tasks/director.py | ~600 | 영상 합성 (MoviePy) |
| backend/app/orchestrator/fsm.py | ~350 | FSM 상태 관리 |
| backend/app/main.py | ~400 | FastAPI + WebSocket 서버 |
| frontend/src/components/RunStatus.tsx | ~300 | 실시간 진행률 UI |

---

**Last Updated**: 2025-01-13
**Document Version**: 1.0
**Total Portfolio Projects**: 2 (IYAGI + Kurz AI Studio)
