LLM 시스템 설정 가이드
====================

이 폴더에는 LLM API 설정 파일이 저장됩니다.

1. Unity 에디터에서 설정하기 (권장)
   - Unity 에디터를 엽니다
   - Project 창에서 Assets/Resources 폴더를 우클릭
   - Create > LLM > LLM Configuration 선택
   - 생성된 LLMConfig 파일을 선택
   - Inspector에서 API 키와 설정을 입력
   - apiKey: OpenAI는 "sk-..."로 시작, Anthropic은 "sk-ant-..."로 시작
   - model: 사용할 모델 (예: gpt-4o-mini, claude-3-sonnet-20240229)
   - provider: OpenAI 또는 Anthropic 선택
   - temperature: 0-2 사이 값 (창의성 조절, 기본값 0.8)
   - maxTokens: 100-4000 사이 값 (응답 길이, 기본값 1000)

2. 수동으로 설정하기 (고급 사용자)
   - LLMConfig.asset.example 파일을 복사
   - 파일 이름을 LLMConfig.asset으로 변경
   - 텍스트 에디터로 열어서 YOUR_API_KEY_HERE를 실제 API 키로 변경
   - YOUR_LLMCONFIG_SCRIPT_GUID는 Unity가 자동으로 설정합니다

주의사항:
- LLMConfig.asset 파일은 .gitignore에 추가되어 있어 Git에 커밋되지 않습니다
- API 키를 절대로 공개 저장소에 올리지 마세요
- 팀원들과 공유할 때는 API 키를 제외하고 공유하세요

문제 해결:
- "API Key is not set" 에러가 나면 LLMConfig.asset 파일이 있는지 확인하세요
- 파일이 있는데도 에러가 나면 Inspector에서 apiKey 필드가 비어있는지 확인하세요
- LLMStoryGenerator 컴포넌트에서 "Use Override" 옵션을 사용하면 config 파일 대신 Inspector 값을 사용할 수 있습니다
