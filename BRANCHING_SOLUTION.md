# AI VN의 분기 폭발 문제와 해결 전략 비교

## 문제 정의

AI 생성 비주얼노벨에서 진정한 동적 스토리를 만들려면 여러 상태를 동시에 고려해야 합니다:
- **Core Values** (가치관): 용기, 지혜, 우정 등 2-4개
- **NPC Affection** (호감도): 각 NPC마다 -100 ~ +100
- **Player Choices** (선택 히스토리): 누적된 선택의 효과

### 딜레마: 완전 고정 vs 완전 동적

```
시나리오 1 - 완전 고정 스토리:
모든 플레이어 → 같은 스토리
❌ LLM을 쓰는 의미 없음

시나리오 2 - 완전 동적 분기 (정량적 다중 기준):
Core Values 3개 × NPC 2명 = 6개 조합
챕터 5개 → 6^5 = 7,776개 경로
❌ API 비용 폭발, 캐시 불가, 품질 관리 불가능
```

---

## 해결 전략 비교: 3가지 접근법

### 방법 1: 완전 수렴 (Chapter-Level Convergence)

**핵심**: 선택지는 **관계/가치관**만 변경, **메인 플롯은 100% 고정**

```
Chapter 1 (모든 플레이어 동일)
    ↓
┌───────┴───────┐
선택 A         선택 B
용기 +10       지혜 +10
    ↓             ↓
[2-3줄 다른 반응]
    ↓             ↓
└───────┬───────┘
    ↓ (수렴)
Chapter 2 (다시 모두 동일)
```

**구현**:
```csharp
// 캐시 키: 상태 완전 무시
string cacheKey = $"{projectGuid}_Ch{chapterId}";
```

**장점**:
- ✅ API 비용 최소화 (챕터당 1개 버전만 생성)
- ✅ 캐시 재사용률 100%
- ✅ 품질 관리 용이

**단점**:
- ❌ **몰입감 파괴**: "Chapter 2에서 엘런을 배신했는데, Chapter 3에서 엘런이 친근하게 대함"
- ❌ 선택의 무게감 부족
- ❌ LLM이 상태를 반영하지 못함 (프롬프트에 전달해도 강제할 수 없음)

---

### 방법 2: 교차 분기 (Alternating Branching)

**핵심**: 챕터별로 **다른 기준**으로 분기 (짝수=Core Value, 홀수=NPC Affection)

```
Chapter 1: 공통 (1개)
Chapter 2: Core Value 분기 (A, B) → 2개
Chapter 3: NPC Affection 분기 (X, Y) → 4개 (AX, AY, BX, BY)
Chapter 4: Core Value 재분기 → 8개
Chapter 5: NPC Affection 재분기 → 16개
Chapter 6: Core Value 확정 → 32개
```

**구현**:
```csharp
// 짝수 챕터: Core Value 기준
if (chapterId % 2 == 0)
{
    string coreRoute = GetDominantCoreValue(state);     // "Courage"
    string coreBucket = GetCoreValueBucket(state);      // "LOW/MID/HIGH"
    return $"{projectId}_Ch{chapterId}_{coreRoute}_{coreBucket}";
}
// 홀수 챕터: NPC Affection 기준
else
{
    string loveRoute = GetDominantNPC(state);           // "Alice"
    string affBucket = GetAffectionBucket(state);       // "Alice_HIGH/BALANCED"
    return $"{projectId}_Ch{chapterId}_{coreRoute}_{loveRoute}_{affBucket}";
}
```

**장점**:
- ✅ 분기 복잡도 97% 감소 (7,776 → 32개)
- ✅ 각 챕터에 명확한 테마 (짝수=가치관, 홀수=관계)

**단점**:
- ❌ **정량적 비교**: 점수 49 vs 50의 차이가 큰 의미 없음
- ❌ 여전히 32개 루트 (관리 부담)
- ❌ 챕터별 기준이 달라 혼란스러움
- ❌ Bucket 양자화로 세밀한 상태 변화 반영 불가

---

### 방법 3: 주요 선택 분기 (Major-Choice-Driven Branching) ⭐ 최종 선택

**핵심**: **기본 스토리는 동일**, **주요 선택** 시에만 이벤트 기반 분기

```
Chapter 1: 공통 프롤로그 (1개)
    ↓
Chapter 2:
┌─────────────────┴─────────────────┐
기본 경로                     "betrayed_Ellen" 플래그
(대부분 플레이어)                (주요 선택)
    ↓                                 ↓
Chapter 3:                       Chapter 3 (배신 후):
엘런이 친밀                       엘런이 냉랭
    ↓                                 ↓
Chapter 4: 수렴 (선택 없으면)         ↓
    ↓ ←───────────────────────────────┘
Chapter 5: 공통 (또는 추가 분기)
```

**구현**:
```csharp
// 주요 선택 플래그만 캐시 키에 포함
string GetMajorFlagsForBranching(GameStateSnapshot state)
{
    var majorFlags = state.flags
        .Where(kvp => kvp.Value && IsMajorFlag(kvp.Key))
        .OrderBy(f => f)
        .ToList();

    return majorFlags.Count == 0 ? "none" : string.Join("_", majorFlags);
}

bool IsMajorFlag(string flag)
{
    string[] majorPrefixes = {
        "betrayed_", "saved_", "killed_", "romance_",
        "allied_", "rejected_", "revealed_", "sacrificed_"
    };
    return majorPrefixes.Any(prefix => flag.StartsWith(prefix));
}

// 캐시 키
string cacheKey = $"{projectId}_Ch{chapterId}_{majorFlags}";
// 예: "Project123_Ch3_betrayed_Ellen"
```

**장점**:
- ✅ **이벤트 기반**: 의미있는 선택만 분기 ("배신했다/안 했다")
- ✅ **몰입감 유지**: 주요 선택의 결과가 다음 챕터에 반영됨
- ✅ **분기 수 제한**: 챕터당 0-1개 주요 선택 → 최대 2^6 = 64개 루트 (실제로는 10-20개)
- ✅ **캐시 재사용**: 같은 플래그 조합이면 재사용
- ✅ **확장 용이**: 새로운 주요 선택 추가 가능

**단점**:
- ⚠️ 완전 수렴보다 API 비용 증가 (하지만 감당 가능)
- ⚠️ LLM에게 적절한 Major Choice 생성 지침 필요

---

## 최종 선택: Major-Choice-Driven Branching

| 비교 항목 | 완전 수렴 | 교차 분기 | **주요 선택 분기** |
|-----------|----------|----------|-----------------|
| **분기 수** | 1개/챕터 | 32개 (6챕터) | 10-20개 (6챕터) |
| **API 비용** | 최소 | 중간 | 중간 |
| **몰입감** | ❌ 파괴 | ⚠️ 보통 | ✅ 유지 |
| **선택 무게** | ❌ 없음 | ⚠️ 점수 기준 | ✅ 이벤트 기반 |
| **구현 복잡도** | 낮음 | 높음 | 중간 |
| **확장성** | 높음 | 낮음 | 높음 |

---

## 구현 예시

### LLM 프롬프트 (Major Choice 생성)

```
**CRITICAL - Major Choice Flag System**:
- Use SPARINGLY (0-1 per chapter) to avoid branching explosion
- Major Flags cause future chapters to branch

**When to use**:
✅ Betraying a key character ("betrayed_Ellen")
✅ Entering romance route ("romance_Alice")
✅ Major plot decision ("saved_village")
❌ Minor stat-affecting choices (no flag)

**Example - Major Choice**:
{
  "text": "Betray Ellen to save yourself",
  "skill_impact": [{"skill_name": "Survival", "change": 15}],
  "affection_impact": [{"character_name": "Ellen", "change": -30}],
  "flag_impact": [{"flag_name": "betrayed_Ellen", "value": true}]
}

**Example - Normal Choice** (no flag):
{
  "text": "Help Ellen with her task",
  "skill_impact": [{"skill_name": "Empathy", "change": 12}],
  "affection_impact": [{"character_name": "Ellen", "change": 10}]
}
```

### 플레이 시나리오

```
플레이어 A:
- Ch1: 프롤로그
- Ch2: 엘런과 협력 (일반 선택) → 플래그 없음
- Ch3: 기본 버전 (엘런 친밀) ← 캐시 재사용
- Ch4: 마을 구함 (Major Choice) → "saved_village" 플래그
- Ch5: 마을 구출 버전 ← 새로 생성
- Ending: Hero Ending

플레이어 B:
- Ch1: 프롤로그 ← 플레이어 A와 동일 (캐시)
- Ch2: 엘런 배신 (Major Choice) → "betrayed_Ellen" 플래그
- Ch3: 배신 후 버전 ← 새로 생성
- Ch4: 마을 무시 (일반 선택) → 플래그 없음
- Ch5: 배신 유지 버전 ← 새로 생성
- Ending: Betrayer Ending
```

---

## 결론

**Major-Choice-Driven Branching**은 다음을 달성합니다:

1. **몰입감**: 주요 선택이 스토리에 실제로 영향을 줌
2. **실용성**: 분기 수가 관리 가능한 수준 (10-20개)
3. **LLM 활용**: 이벤트별로 다른 대사/분위기 생성
4. **확장성**: 새로운 주요 선택 추가 시 선형 증가

이는 **"완벽한 동적 분기"** 대신 **"플레이어가 동적이라 느끼는 분기"**를 제공하는 실용적 해결책입니다.
