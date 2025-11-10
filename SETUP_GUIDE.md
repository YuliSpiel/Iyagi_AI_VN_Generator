# Iyagi AI VN Generator - Setup Guide

## 새로운 기능 (New Features)

### 1. Derived Skills System (파생 스킬 시스템)
- Core Values에서 파생된 세부 스킬 추적
- 선택지마다 Core Value + Derived Skill 동시 영향
- 좌측 상단 마우스 오버로 실시간 확인 가능

### 2. Skill Status UI (스킬 상태 UI)
- 화면 좌측 상단에 마우스를 올리면 현재 스킬 점수 표시
- Core Values: 금색 바
- Derived Skills: 하늘색 바
- 자동으로 GameScene에 생성됨

### 3. Background Removal (배경 제거)
- 캐릭터 스프라이트 생성 후 자동으로 배경 제거
- Python rembg 라이브러리 사용
- 투명 PNG로 저장

## 필수 설정 (Required Setup)

### Python rembg 설치
```bash
# GPU 버전 (권장)
pip install rembg[gpu] pillow

# CPU 버전
pip install rembg pillow
```

### Python 경로 확인
BackgroundRemover.cs에서 기본 경로는 "python3"입니다.
다른 경로를 사용하려면 수정하세요:

```csharp
// BackgroundRemover.cs 12번째 줄
private static string pythonPath = "python3"; // 또는 "/usr/local/bin/python3"
```

## 테스트 방법 (Testing)

### 1. Dummy Project로 테스트
1. Unity 메뉴: `Iyagi/Create Dummy Project`
2. GameScene 열기
3. Play 모드 실행
4. 좌측 상단에 마우스 올려서 스킬 UI 확인
5. 선택지 선택 시 스킬 점수 변화 확인

### 2. 실제 프로젝트 생성
1. Unity 메뉴: `Iyagi/Setup Wizard`
2. 프로젝트 정보 입력
3. Core Values와 Derived Skills 정의
4. 캐릭터 생성 (배경 제거 자동 실행)
5. AI로 스토리 생성

## 구조 설명 (System Architecture)

### Core Values & Derived Skills
```
Core Value: 우정 (Friendship)
  ├── Derived Skill: 공감 (Empathy)
  ├── Derived Skill: 협력 (Cooperation)
  └── Derived Skill: 신뢰 (Trust)
```

### 선택지 영향 (Choice Impact)
```csharp
// 선택지 1: "친구를 도와준다"
Choice1_ValueImpact_우정: +10        // Core Value 증가
Choice1_SkillImpact_공감: +5        // Derived Skill 증가
Choice1_SkillImpact_협력: +8        // Derived Skill 증가
```

### AI JSON 스키마
```json
{
  "choices": [
    {
      "text": "친구를 도와준다",
      "next_id": 1004,
      "value_impact": [
        {"value_name": "우정", "change": 10}
      ],
      "skill_impact": [
        {"skill_name": "공감", "change": 5},
        {"skill_name": "협력", "change": 8}
      ]
    }
  ]
}
```

## 파일 구조 (File Structure)

```
Assets/
├── Resources/
│   └── Generated/
│       ├── Characters/
│       │   └── [CharacterName]/
│       │       ├── neutral_normal.png  (배경 제거됨)
│       │       ├── happy_normal.png
│       │       └── ...
│       └── ProjectData/
│           └── project_data.json
├── Python/
│   └── remove_bg.py  (자동 생성됨)
└── Script/
    ├── AISystem/
    │   ├── BackgroundRemover.cs  (NEW)
    │   └── AIDataConverter.cs    (UPDATED)
    ├── Runtime/
    │   ├── GameStateSnapshot.cs  (UPDATED)
    │   ├── SkillStatusUI.cs      (NEW)
    │   └── GameController.cs     (UPDATED)
    └── SetupWizard/
        └── StandingSpriteGenerator.cs  (UPDATED)
```

## 주의사항 (Important Notes)

1. **rembg 설치 확인**: 첫 실행 전에 rembg가 설치되어 있는지 확인하세요
2. **프로세스 타임아웃**: 배경 제거는 최대 30초 걸릴 수 있습니다
3. **Editor Only**: 배경 제거는 Unity Editor에서만 실행됩니다 (빌드 시 불필요)
4. **원본 보존**: 배경 제거 실패 시 원본 이미지 사용

## 문제 해결 (Troubleshooting)

### rembg 설치 안 됨
```
[BackgroundRemover] Error: Required package not installed
```
→ `pip install rembg pillow` 실행

### Python 못 찾음
```
[BackgroundRemover] Failed to start Python process
```
→ BackgroundRemover.cs의 pythonPath 수정

### 배경 제거 실패
```
[BackgroundRemoval] Background removal failed, using original image
```
→ 원본 이미지가 사용됩니다 (정상 작동)

## 다음 단계 (Next Steps)

1. ✅ Core Values & Derived Skills 시스템
2. ✅ Skill Status UI (좌측 상단 호버)
3. ✅ Background Removal (rembg 통합)
4. 🔄 실제 프로젝트 생성 및 테스트
5. 📋 추가 기능 요청 시 구현

---

**작성일**: 2025-11-10
**버전**: 1.0.0
**문의**: GitHub Issues
