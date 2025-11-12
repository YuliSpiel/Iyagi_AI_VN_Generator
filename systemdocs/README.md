# System Documentation Index

이 폴더에는 Iyagi AI VN Generator의 시스템 설계 및 구현 상세 문서가 포함되어 있습니다.

## 📚 문서 목록

### 핵심 시스템

1. **[데이터 구조](data-structures.md)**
   - VNProjectData, CharacterData 구조
   - GameStateSnapshot, SaveFile 구조
   - DialogueRecord 포맷

2. **[챕터 생성 시스템](chapter-generation.md)**
   - Scene-based generation 플로우
   - AI 프롬프트 엔지니어링
   - 캐싱 시스템

3. **[세이브/로드 시스템](save-load-system.md)**
   - 프로젝트 슬롯 구조
   - SaveFile 관리
   - 자동 저장 로직

### API 통합

4. **[API 통합](api-integration.md)**
   - Gemini API (텍스트 생성)
   - NanoBanana API (이미지 생성)
   - ElevenLabs API (오디오 생성)
   - Rate Limit & Retry 시스템

### 리소스 생성

5. **[이미지 생성 파이프라인](image-generation.md)**
   - 캐릭터 얼굴 프리뷰
   - 스탠딩 스프라이트 (Expression + Pose)
   - CG 일러스트 (레퍼런스 기반)
   - 배경 이미지

6. **[리소스 관리](resource-management.md)**
   - 폴더 구조
   - Resources.Load 경로
   - 재사용 전략

### 개발 지원

7. **[개발 도구 및 자동화](development-tools.md)**
   - F5 Auto-Fill (SetupWizardAutoFill)
   - GameScene 자동 설정
   - 테스트 모드 vs 프로덕션 모드

8. **[구현 히스토리](implementation-history.md)**
   - 주요 변경사항 기록
   - 아키텍처 진화 과정
   - 문제 해결 사례

---

**Last Updated**: 2025-01-11
