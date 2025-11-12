# 구현 히스토리

## 최신 변경사항

### Core Value System (2025-01-11)
- **변경 전**: 선택지 → Core Value 직접 수정
- **변경 후**: 선택지 → Derived Skills → Core Value = Skills 합계

### SaveFile Auto-Update (2025-01-11)
- `SaveDataManager.UpdateSaveFile()` 추가
- `GameController.AutoSave()`에서 SaveFile 자동 갱신

### Scene-Based Chapter Generation (2025-01-11)
- JSON 잘림 문제 해결
- 챕터당 3개 씬 분할 생성

### Parallel Asset Generation (2025-01-10)
- Fan-Out Barrier 구조 도입
- Cycle 1-3 병렬 처리

_상세 내용은 [CLAUDE_FULL_BACKUP.md](CLAUDE_FULL_BACKUP.md)를 참조하세요._
