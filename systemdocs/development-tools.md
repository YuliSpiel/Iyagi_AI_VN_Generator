# 개발 도구 및 자동화

## F5 Auto-Fill (SetupWizardAutoFill)

- **기능**: Setup Wizard 각 단계를 F5 키로 자동 완성
- **테스트 시간**: ~30초 (API 비용 0원)
- **Stub 이미지**: 그라데이션 이미지 자동 생성

## GameScene 자동 설정 (GameSceneSetupHelper)

- **Unity 메뉴**: Iyagi > Setup Game Scene
- **생성 요소**: Canvas, DialogueUI, GameController 등 모두 자동 연결

## 테스트 모드 vs 프로덕션 모드

| 항목 | 테스트 모드 | 프로덕션 모드 |
|------|------------|--------------|
| 얼굴 이미지 | Stub 그라데이션 | Gemini API 생성 |
| 스탠딩 스프라이트 | TestResources 복사 | NanoBanana API 생성 |
| API 비용 | 0원 | 캐릭터당 ~$0.10 |

_상세 내용은 [CLAUDE_FULL_BACKUP.md](CLAUDE_FULL_BACKUP.md)의 "🛠️ Development Tools & Automation" 섹션을 참조하세요._
