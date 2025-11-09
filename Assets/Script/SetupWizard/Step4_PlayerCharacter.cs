using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;
using System.Collections;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 4: 플레이어 캐릭터 생성
    /// 얼굴 프리뷰 생성 + 스탠딩 5종 자동 생성
    /// </summary>
    public class Step4_PlayerCharacter : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;
        public CharacterFaceGenerator faceGenerator;

        [Header("UI Elements")]
        public TMP_InputField nameInput;
        public TMP_InputField ageInput;
        public TMP_Dropdown genderDropdown;
        public TMP_Dropdown povDropdown;
        public TMP_Dropdown archetypeDropdown;
        public TMP_InputField appearanceInput;
        public TMP_InputField personalityInput;

        [Header("Face Preview UI")]
        public Image previewImage; // 얼굴 프리뷰 표시
        public Button generateFaceButton;
        public Button previousButton;
        public Button nextButton;
        public TMP_Text previewIndexText; // "1 / 5"

        [Header("Buttons")]
        public Button autoFillButton;
        public Button confirmButton;
        public Button nextStepButton;

        void Start()
        {
            // faceGenerator 연결 및 히스토리 초기화
            if (faceGenerator != null)
            {
                faceGenerator.previewImage = previewImage;
                faceGenerator.ClearHistory(); // Step4 시작 시 히스토리 초기화
            }

            // 드롭다운 초기화
            InitializeDropdowns();

            // 버튼 이벤트 연결
            if (autoFillButton != null)
                autoFillButton.onClick.AddListener(OnAutoFillClicked);
            if (generateFaceButton != null)
                generateFaceButton.onClick.AddListener(OnGenerateFaceClicked);
            if (previousButton != null)
                previousButton.onClick.AddListener(OnPreviousClicked);
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
            if (nextStepButton != null)
                nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 입력 검증
            if (nameInput != null)
                nameInput.onValueChanged.AddListener((_) => ValidateInputs());
            if (appearanceInput != null)
                appearanceInput.onValueChanged.AddListener((_) => ValidateInputs());

            // 초기 상태
            if (nextStepButton != null)
                nextStepButton.interactable = false;
            if (confirmButton != null)
                confirmButton.interactable = false;
            ValidateInputs();
            UpdatePreviewNavigation();
        }

        void InitializeDropdowns()
        {
            // Gender (한국어)
            genderDropdown.ClearOptions();
            genderDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "남성",
                "여성",
                "논바이너리"
            });

            // POV (한국어)
            povDropdown.ClearOptions();
            povDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "1인칭",
                "2인칭",
                "3인칭"
            });

            // Archetype (한국어)
            archetypeDropdown.ClearOptions();
            archetypeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "영웅",
                "전략가",
                "순수한 자",
                "반항아",
                "멘토",
                "트릭스터"
            });
        }

        void ValidateInputs()
        {
            // 입력이 있으면 Auto-Fill 버튼 텍스트 변경
            bool hasInput = !string.IsNullOrEmpty(nameInput.text) ||
                           !string.IsNullOrEmpty(appearanceInput.text) ||
                           !string.IsNullOrEmpty(personalityInput.text);

            if (autoFillButton != null)
            {
                var btnText = autoFillButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = hasInput ? "AI 풍부화" : "AI 자동 생성";
                }
            }
        }

        public void OnAutoFillClicked()
        {
            autoFillButton.interactable = false;
            StartCoroutine(AutoFillWithAI());
        }

        IEnumerator AutoFillWithAI()
        {
            // 사용자 입력 확인
            string userName = nameInput.text.Trim();
            string userAppearance = appearanceInput.text.Trim();
            string userPersonality = personalityInput.text.Trim();

            // 게임 컨텍스트 수집
            string coreValuesText = string.Join(", ",
                System.Linq.Enumerable.Select(wizardManager.projectData.coreValues, v => v.name));

            string prompt;

            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userAppearance) && string.IsNullOrEmpty(userPersonality))
            {
                // 완전 자동 생성
                prompt = $@"비주얼 노벨 게임의 플레이어 캐릭터를 생성해줘.

게임 정보:
제목: {wizardManager.projectData.gameTitle}
줄거리: {wizardManager.projectData.gamePremise}
장르: {wizardManager.projectData.genre}
톤: {wizardManager.projectData.tone}
핵심 가치: {coreValuesText}

요구사항:
1. 게임의 세계관과 톤에 맞는 주인공 생성
2. 이름은 짧고 기억하기 쉽게 (성+이름 또는 이름만)
3. 나이는 15~25세 사이
4. 외모는 구체적이고 상세하게 (얼굴 특징, 머리 스타일, 체형, 패션 등)
5. 성격은 2-3가지 주요 특징으로
6. 성별은 게임 톤에 맞게 선택
7. 아키타입은 게임 줄거리에 맞게 선택

JSON 형식으로만 출력:
{{
  ""name"": ""캐릭터 이름"",
  ""age"": 18,
  ""gender"": ""Male/Female/NonBinary 중 하나"",
  ""appearance"": ""외모 상세 설명 (얼굴, 머리, 체형, 옷차림 등)"",
  ""personality"": ""성격 설명"",
  ""archetype"": ""Hero/Strategist/Innocent/Rebel/Mentor/Trickster 중 하나""
}}";
            }
            else
            {
                // 입력 풍부화
                prompt = $@"사용자가 비주얼 노벨 게임의 플레이어 캐릭터 아이디어를 개떡같이 입력했어. 이걸 찰떡같이 이해하고 풍부하게 발전시켜줘.

게임 정보:
제목: {wizardManager.projectData.gameTitle}
줄거리: {wizardManager.projectData.gamePremise}
장르: {wizardManager.projectData.genre}
톤: {wizardManager.projectData.tone}
핵심 가치: {coreValuesText}

사용자 입력:
이름: {(string.IsNullOrEmpty(userName) ? "(없음)" : userName)}
외모: {(string.IsNullOrEmpty(userAppearance) ? "(없음)" : userAppearance)}
성격: {(string.IsNullOrEmpty(userPersonality) ? "(없음)" : userPersonality)}

요구사항:
1. 사용자 의도를 정확히 파악하고 핵심은 유지
2. 이름이 비어있거나 어색하면 게임 세계관에 맞게 개선
3. 외모는 매우 구체적이고 상세하게 확장 (얼굴 특징, 머리 스타일, 체형, 패션 등)
4. 성격은 2-3가지 주요 특징으로 풍부하게
5. 나이는 게임 톤에 맞게 자동 추천
6. 성별과 아키타입도 입력과 게임 내용에 맞게 추천
7. 오타, 맞춤법 수정
8. 애매한 표현은 명확하게

JSON 형식으로만 출력:
{{
  ""name"": ""다듬어진 이름"",
  ""age"": 18,
  ""gender"": ""Male/Female/NonBinary 중 하나"",
  ""appearance"": ""풍부해진 외모 설명"",
  ""personality"": ""풍부해진 성격 설명"",
  ""archetype"": ""Hero/Strategist/Innocent/Rebel/Mentor/Trickster 중 하나""
}}";
            }

            bool completed = false;
            string result = null;

            yield return wizardManager.geminiClient.GenerateContent(
                prompt,
                (response) => {
                    result = response;
                    completed = true;
                },
                (error) => {
                    Debug.LogError($"Auto-fill failed: {error}");
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (!string.IsNullOrEmpty(result))
            {
                ParseAutoFillResponse(result);
            }

            autoFillButton.interactable = true;
        }

        void ParseAutoFillResponse(string jsonResponse)
        {
            try
            {
                // JSON 추출
                int startIndex = jsonResponse.IndexOf('{');
                int endIndex = jsonResponse.LastIndexOf('}');

                if (startIndex == -1 || endIndex == -1)
                {
                    Debug.LogError("Invalid JSON format");
                    return;
                }

                string json = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                var data = JsonUtility.FromJson<CharacterAutoFillData>(json);

                if (data == null)
                {
                    Debug.LogError("Failed to parse JSON data");
                    return;
                }

                if (nameInput != null && !string.IsNullOrEmpty(data.name))
                    nameInput.text = data.name;
                if (ageInput != null)
                    ageInput.text = data.age.ToString();
                if (appearanceInput != null && !string.IsNullOrEmpty(data.appearance))
                    appearanceInput.text = data.appearance;
                if (personalityInput != null && !string.IsNullOrEmpty(data.personality))
                    personalityInput.text = data.personality;

                // Gender 설정
                if (genderDropdown != null && !string.IsNullOrEmpty(data.gender))
                {
                    if (System.Enum.TryParse<Gender>(data.gender, out Gender g))
                    {
                        genderDropdown.value = (int)g;
                    }
                }

                // Archetype 설정
                if (archetypeDropdown != null && !string.IsNullOrEmpty(data.archetype))
                {
                    if (System.Enum.TryParse<Archetype>(data.archetype, out Archetype a))
                    {
                        archetypeDropdown.value = (int)a;
                    }
                }

                ValidateInputs();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse auto-fill response: {e.Message}");
            }
        }

        public void OnGenerateFaceClicked()
        {
            if (string.IsNullOrEmpty(appearanceInput.text))
            {
                Debug.LogWarning("Please enter appearance description first");
                return;
            }

            generateFaceButton.interactable = false;
            StartCoroutine(GenerateFacePreview());
        }

        IEnumerator GenerateFacePreview()
        {
            yield return faceGenerator.GenerateNewFace(
                appearanceInput.text,
                wizardManager.nanoBananaClient,
                () => {
                    generateFaceButton.interactable = true;
                    confirmButton.interactable = true;

                    // 버튼 텍스트를 "Generate More"로 변경
                    var btnText = generateFaceButton.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        btnText.text = "Generate More";
                    }

                    UpdatePreviewNavigation();
                }
            );
        }

        public void OnPreviousClicked()
        {
            faceGenerator.ShowPrevious();
            UpdatePreviewNavigation();
        }

        public void OnNextClicked()
        {
            faceGenerator.ShowNext();
            UpdatePreviewNavigation();
        }

        void UpdatePreviewNavigation()
        {
            int current = faceGenerator.currentIndex + 1;
            int total = faceGenerator.previewHistory.Count;

            previewIndexText.text = total > 0 ? $"{current} / {total}" : "No previews";

            previousButton.interactable = faceGenerator.currentIndex > 0;
            nextButton.interactable = faceGenerator.currentIndex < total - 1;
            confirmButton.interactable = total > 0;
        }

        public void OnConfirmClicked()
        {
            // 캐릭터 데이터 생성
            CharacterData character = ScriptableObject.CreateInstance<CharacterData>();
            character.characterName = nameInput.text;
            character.age = int.Parse(ageInput.text);
            character.gender = (Gender)genderDropdown.value;
            character.pov = (POV)povDropdown.value;
            character.appearanceDescription = appearanceInput.text;
            character.personality = personalityInput.text;
            character.archetype = (Archetype)archetypeDropdown.value;
            character.confirmedSeed = faceGenerator.GetCurrentSeed();
            character.facePreview = faceGenerator.GetCurrentPreview();
            character.resourcePath = $"Generated/Characters/{character.characterName}"; // Resources.Load 경로 설정

            wizardManager.projectData.playerCharacter = character;

#if UNITY_EDITOR
            // 얼굴 프리뷰 저장
            SaveFacePreview(character, previewImage.sprite.texture);
#endif

            // 스탠딩 5종 자동 생성
            StartCoroutine(GenerateStandingSprites(character));
        }

#if UNITY_EDITOR
        private void SaveFacePreview(CharacterData character, Texture2D texture)
        {
            // 캐릭터별 폴더 생성
            string dir = $"Assets/Resources/Generated/Characters/{character.characterName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // 얼굴 프리뷰 저장
            string path = $"{dir}/face_preview.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Face preview saved: {path}");
        }
#endif

        IEnumerator GenerateStandingSprites(CharacterData character)
        {
            var generator = gameObject.AddComponent<StandingSpriteGenerator>();

            // 플레이어 캐릭터는 첫 번째 캐릭터 (스타일 기준)
            bool isFirst = (wizardManager.projectData.npcs.Count == 0);

            bool completed = false;
            yield return generator.GenerateStandingSet(
                character,
                wizardManager.nanoBananaClient,
                isFirst, // 첫 캐릭터 여부 전달
                () => completed = true
            );

            yield return new WaitUntil(() => completed);

            Debug.Log($"Standing sprites generated for {character.characterName}");
            nextStepButton.interactable = true;
        }

        public void OnNextStepClicked()
        {
            wizardManager.NextStep();
        }

        // ===== JSON 스키마 =====

        [System.Serializable]
        private class CharacterAutoFillData
        {
            public string name;
            public int age;
            public string gender;
            public string appearance;
            public string personality;
            public string archetype;
        }
    }
}
