# Iyagi AI VN Generator - 사용자 가이드

> AI로 비주얼노벨을 자동 생성하는 Unity 툴

## 📖 문서 목록

- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Unity 프로젝트 설치 및 초기 설정
- **[SCENE_SETUP_GUIDE.md](SCENE_SETUP_GUIDE.md)** - TitleScene, GameScene 수동 설정 가이드

## 🚀 빠른 시작

### 1. API 키 설정

Unity Editor에서 `Iyagi > Create API Config`를 실행하거나, 직접 생성:

```
Assets/Resources/APIConfig.asset
- geminiApiKey: "YOUR_GEMINI_KEY"
- nanoBananaApiKey: "YOUR_NANOBANANA_KEY" (선택)
- elevenLabsApiKey: "YOUR_ELEVENLABS_KEY" (선택)
```

### 2. Setup Wizard 실행

- Unity Editor > Play Mode
- SetupWizardScene 로드
- **테스트 모드**: 각 단계에서 F5 키를 눌러 자동 완성
- **일반 모드**: 직접 정보 입력

### 3. 프로젝트 생성

Step 6에서 "Create Project" 버튼 클릭:
- Cycle 1: 캐릭터 스탠딩 생성
- Cycle 2: 챕터 1 시나리오 생성
- Cycle 3: 배경/BGM 생성 (테스트 모드에서는 스킵)

### 4. 게임 플레이

TitleScene에서:
1. 프로젝트 선택
2. SaveFile 선택 (또는 새 게임 시작)
3. 게임 플레이

---

## 💡 팁

- **F5 Auto-Fill**: 테스트용으로 30초 만에 프로젝트 생성 (API 비용 없음)
- **GameScene 자동 설정**: Unity Editor > Iyagi > Setup Game Scene
- **프로젝트 삭제**: Unity Editor > Iyagi > Cleanup > Delete All Generated Projects

---

자세한 내용은 각 가이드 문서를 참조하세요.
