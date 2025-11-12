# 세이브/로드 시스템

## 계층 구조

```
ProjectSlot (프로젝트별 슬롯)
└── SaveFile (저장 파일, 최대 10개)
    └── GameState (게임 진행 상태)
```

## 자동 저장 (2025-01-11 추가)

- **트리거**: 챕터 완료 시
- **저장 내용**:
  - currentChapter
  - gameState (skills, coreValues, affections)
  - lastPlayedDate

_상세 내용은 [CLAUDE_FULL_BACKUP.md](CLAUDE_FULL_BACKUP.md)의 "💾 세이브/로드 시스템" 섹션을 참조하세요._
