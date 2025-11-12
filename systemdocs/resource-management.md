# 리소스 관리

## 폴더 구조

```
Assets/Resources/
├── Generated/Characters/{CharName}/
│   ├── face_preview.png
│   └── {expression}_{pose}.png
├── Image/
│   ├── Background/
│   └── CG/
└── Sound/
    ├── BGM/
    └── SFX/
```

## 재사용 전략

- Setup Wizard: 초기 리소스 생성
- 런타임: 기존 리소스 목록에서 선택
- 필요 시: 새 Expression+Pose 조합 자동 생성

_상세 내용은 [CLAUDE_FULL_BACKUP.md](CLAUDE_FULL_BACKUP.md)의 "📁 전체 리소스 폴더 구조" 섹션을 참조하세요._
