# Design Changes

## 2025-11-09: Automatic Ending Generation System

### 변경 사항 (Changes)

**기존 시스템 (Old System)**:
- Step 3에서 사용자가 엔딩 이름을 수동으로 입력 (ending1, ending2, ending3)
- 최소 2개의 엔딩 필요
- 각 엔딩은 첫 번째, 두 번째, 세 번째 Core Value에 자동 매핑

**새로운 시스템 (New System)**:
- Step 3에서는 챕터 수만 선택
- 엔딩은 Step 5 (NPC 생성) 완료 후 자동으로 생성
- **엔딩 생성 공식**: `Core Values 수 × 공략가능 NPC 수 = 엔딩 수`

### 엔딩 생성 로직 (Ending Generation Logic)

각 엔딩은 다음으로 구성됩니다:

1. **엔딩 이름**: `{Core Value 이름} + {NPC 이름}`
   - 예: "정의 + Alice", "출세 + Bob"

2. **엔딩 설명**: `{Core Value 이름} 가치를 달성하고 {NPC 이름}와의 관계를 발전시킨 결말`

3. **요구 조건**:
   - `requiredValues`: 해당 Core Value ≥ 70
   - `requiredAffections`: 해당 NPC 호감도 ≥ 80

### 예시 (Example)

**게임 설정**:
- Core Values: 3개 (정의, 출세, 우정)
- 공략가능 NPC: 2명 (Alice, Bob)

**생성되는 엔딩**: 3 × 2 = 6개

| Ending ID | Ending Name | Required Value | Required Affection |
|-----------|-------------|----------------|-------------------|
| 1 | 정의 + Alice | 정의 ≥ 70 | Alice ≥ 80 |
| 2 | 정의 + Bob | 정의 ≥ 70 | Bob ≥ 80 |
| 3 | 출세 + Alice | 출세 ≥ 70 | Alice ≥ 80 |
| 4 | 출세 + Bob | 출세 ≥ 70 | Bob ≥ 80 |
| 5 | 우정 + Alice | 우정 ≥ 70 | Alice ≥ 80 |
| 6 | 우정 + Bob | 우정 ≥ 70 | Bob ≥ 80 |

### 구현 상세 (Implementation Details)

#### 1. Step3_StoryStructure.cs
- **변경 전**: 엔딩 이름 입력 필드 3개 (ending1NameInput, ending2NameInput, ending3NameInput)
- **변경 후**:
  - 입력 필드 제거
  - 챕터 수 선택만 유지
  - 안내 텍스트 추가: "엔딩은 Core Value와 공략 가능한 NPC 조합에 따라 자동 생성됩니다."

#### 2. SetupWizardManager.cs
- **새로운 메서드 추가**: `GenerateEndings()`
  - 공략 가능한 NPC 필터링 (`isRomanceable == true`)
  - Core Value × Romanceable NPC 중첩 루프
  - 각 조합마다 EndingCondition 생성
  - 디버그 로그로 생성 과정 추적

#### 3. Step5_NPCs.cs
- **변경**: `OnFinishClicked()` 메서드에 `wizardManager.GenerateEndings()` 호출 추가
- NPC 생성 완료 → 엔딩 자동 생성 → 다음 스텝

### UI 변경 필요 사항 (Required UI Changes)

**Step 3 Scene/Prefab**:
- ❌ 제거: `ending1NameInput`, `ending2NameInput`, `ending3NameInput` (TMP_InputField)
- ✅ 유지: `chapterCountDropdown` (TMP_Dropdown)
- ✅ 추가: `infoText` (TMP_Text) - 엔딩 자동 생성 안내

### 장점 (Benefits)

1. **사용자 경험 향상**: 엔딩 이름을 고민할 필요 없음
2. **일관성**: 모든 Core Value × NPC 조합을 자동으로 커버
3. **확장성**: Core Value나 NPC가 추가되면 엔딩도 자동으로 증가
4. **명확한 조건**: 각 엔딩의 달성 조건이 명확함

### 주의 사항 (Notes)

- 공략 가능한 NPC가 없으면 엔딩이 생성되지 않음 (경고 로그 출력)
- Core Value가 없으면 엔딩이 생성되지 않음 (경고 로그 출력)
- 최소 1개의 공략 가능한 NPC와 1개의 Core Value 필요
