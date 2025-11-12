# API 통합

상세 내용은 [CLAUDE_FULL_BACKUP.md](CLAUDE_FULL_BACKUP.md#-api-통합-세부사항)의 "🤖 API 통합 세부사항" 섹션을 참조하세요.

## API 클라이언트

1. **GeminiClient** - Gemini 1.5 Flash API (텍스트 생성)
   - 챕터 JSON 생성
   - Rate Limit & Retry 시스템

2. **NanoBananaClient** - NanoBanana API (이미지 생성)
   - 캐릭터 스탠딩
   - 배경 이미지
   - CG 일러스트

3. **ElevenLabsClient** - ElevenLabs API (오디오 생성)
   - BGM 생성
   - SFX 생성

## API 키 관리

- **APIConfigData** (ScriptableObject)
- 저장 위치: `Assets/Resources/APIConfig.asset`
- `.gitignore`에 추가 권장

_TODO: 상세 내용을 CLAUDE_FULL_BACKUP.md에서 추출하여 이 문서로 옮기기_
