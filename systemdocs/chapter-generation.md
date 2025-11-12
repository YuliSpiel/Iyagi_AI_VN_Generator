# 챕터 생성 시스템

## Scene-Based Generation (2025-01-11)

**문제**: Gemini API 응답이 잘림 (긴 JSON 생성 시)

**해결**: 챕터를 3개 씬으로 분할 생성
- 각 씬: 3-5개 대사만 생성
- 이전 씬 컨텍스트를 다음 씬에 전달
- 안정성 향상

## 캐싱 시스템

- **캐시 키**: `{projectGuid}_Ch{ChapterNum}_{ValueHash}`
- **현재 설정**: `enableCaching = false` (선택지별 다른 스토리 생성)

_상세 내용은 [CLAUDE_FULL_BACKUP.md](CLAUDE_FULL_BACKUP.md)를 참조하세요._
