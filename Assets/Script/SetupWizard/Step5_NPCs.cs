using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;
using System.Collections;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 5: NPC 생성
    /// Step4와 동일한 프로세스, 여러 NPC 추가 가능
    /// </summary>
    public class Step5_NPCs : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;
        public CharacterFaceGenerator faceGenerator;

        [Header("UI Elements")]
        public TMP_InputField nameInput;
        public TMP_InputField ageInput;
        public TMP_Dropdown genderDropdown;
        public TMP_Dropdown archetypeDropdown;
        public TMP_InputField roleInput; // NPC 전용 (예: "Friend", "Rival", "Mentor")
        public TMP_InputField appearanceInput;
        public TMP_InputField personalityInput;
        public Toggle romanceableToggle;

        [Header("Face Preview UI")]
        public Image previewImage;
        public Button generateFaceButton;
        public Button previousButton;
        public Button nextButton;
        public TMP_Text previewIndexText;

        [Header("Buttons")]
        public Button autoFillButton;
        public Button confirmButton;
        public Button addAnotherButton;
        public Button finishButton;

        [Header("Loading Popup")]
        public GameObject loadingPopup;
        public TMP_Text loadingText;

        private int npcCount = 0;

        void Start()
        {
            // faceGenerator 연결 및 히스토리 초기화
            if (faceGenerator != null)
            {
                faceGenerator.previewImage = previewImage;
                faceGenerator.ClearHistory(); // Step5 시작 시 히스토리 초기화
            }

            // 로딩 팝업 초기화
            if (loadingPopup != null)
                loadingPopup.SetActive(false);

            // 드롭다운 초기화
            InitializeDropdowns();

            // 버튼 이벤트
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
            if (addAnotherButton != null)
                addAnotherButton.onClick.AddListener(OnAddAnotherClicked);
            if (finishButton != null)
                finishButton.onClick.AddListener(OnFinishClicked);

            // 입력 검증
            if (nameInput != null)
                nameInput.onValueChanged.AddListener((_) => ValidateInputs());
            if (appearanceInput != null)
                appearanceInput.onValueChanged.AddListener((_) => ValidateInputs());

            // 초기 상태
            if (confirmButton != null)
                confirmButton.interactable = false;
            if (addAnotherButton != null)
                addAnotherButton.interactable = false;
            if (finishButton != null)
                finishButton.interactable = true; // NPC는 선택적
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
            string userRole = roleInput.text.Trim();
            string userAppearance = appearanceInput.text.Trim();
            string userPersonality = personalityInput.text.Trim();

            // 게임 컨텍스트 수집
            string coreValuesText = string.Join(", ",
                System.Linq.Enumerable.Select(wizardManager.projectData.coreValues, v => v.name));

            // 플레이어 캐릭터 정보
            string playerName = wizardManager.projectData.playerCharacter != null
                ? wizardManager.projectData.playerCharacter.characterName
                : "주인공";

            // 기존 NPC 이름들
            string existingNPCs = wizardManager.projectData.npcs.Count > 0
                ? string.Join(", ", System.Linq.Enumerable.Select(wizardManager.projectData.npcs, n => n.characterName))
                : "(없음)";

            string prompt;

            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userAppearance) && string.IsNullOrEmpty(userPersonality))
            {
                // 완전 자동 생성
                prompt = $@"비주얼 노벨 게임의 NPC 캐릭터를 생성해줘.

게임 정보:
제목: {wizardManager.projectData.gameTitle}
줄거리: {wizardManager.projectData.gamePremise}
장르: {wizardManager.projectData.genre}
톤: {wizardManager.projectData.tone}
핵심 가치: {coreValuesText}

기존 캐릭터:
주인공: {playerName}
기존 NPC: {existingNPCs}

요구사항:
1. 기존 캐릭터들과 차별화되는 개성 있는 NPC 생성
2. 이름은 짧고 기억하기 쉽게
3. 나이는 15~40세 사이
4. 역할(role)은 주인공과의 관계 (예: 친구, 라이벌, 멘토, 연인, 적 등)
5. 외모는 구체적이고 상세하게 (얼굴 특징, 머리 스타일, 체형, 패션 등)
6. 성격은 2-3가지 주요 특징으로
7. 성별은 기존 캐릭터들과 다양성 있게
8. 아키타입은 게임 줄거리에 맞게 선택

JSON 형식으로만 출력:
{{
  ""name"": ""캐릭터 이름"",
  ""age"": 18,
  ""gender"": ""Male/Female/NonBinary 중 하나"",
  ""role"": ""주인공과의 관계 (예: 친구, 라이벌 등)"",
  ""appearance"": ""외모 상세 설명 (얼굴, 머리, 체형, 옷차림 등)"",
  ""personality"": ""성격 설명"",
  ""archetype"": ""Hero/Strategist/Innocent/Rebel/Mentor/Trickster 중 하나"",
  ""romanceable"": true
}}";
            }
            else
            {
                // 입력 풍부화
                prompt = $@"사용자가 비주얼 노벨 게임의 NPC 캐릭터 아이디어를 개떡같이 입력했어. 이걸 찰떡같이 이해하고 풍부하게 발전시켜줘.

게임 정보:
제목: {wizardManager.projectData.gameTitle}
줄거리: {wizardManager.projectData.gamePremise}
장르: {wizardManager.projectData.genre}
톤: {wizardManager.projectData.tone}
핵심 가치: {coreValuesText}

기존 캐릭터:
주인공: {playerName}
기존 NPC: {existingNPCs}

사용자 입력:
이름: {(string.IsNullOrEmpty(userName) ? "(없음)" : userName)}
역할: {(string.IsNullOrEmpty(userRole) ? "(없음)" : userRole)}
외모: {(string.IsNullOrEmpty(userAppearance) ? "(없음)" : userAppearance)}
성격: {(string.IsNullOrEmpty(userPersonality) ? "(없음)" : userPersonality)}

요구사항:
1. 사용자 의도를 정확히 파악하고 핵심은 유지
2. 이름이 비어있거나 어색하면 게임 세계관에 맞게 개선
3. 역할(role)이 비어있으면 주인공과의 관계 자동 추천
4. 외모는 매우 구체적이고 상세하게 확장 (얼굴 특징, 머리 스타일, 체형, 패션 등)
5. 성격은 2-3가지 주요 특징으로 풍부하게
6. 나이는 게임 톤에 맞게 자동 추천
7. 성별과 아키타입도 입력과 게임 내용에 맞게 추천
8. 연애 가능 여부도 자동 추천
9. 오타, 맞춤법 수정
10. 애매한 표현은 명확하게

JSON 형식으로만 출력:
{{
  ""name"": ""다듬어진 이름"",
  ""age"": 18,
  ""gender"": ""Male/Female/NonBinary 중 하나"",
  ""role"": ""주인공과의 관계"",
  ""appearance"": ""풍부해진 외모 설명"",
  ""personality"": ""풍부해진 성격 설명"",
  ""archetype"": ""Hero/Strategist/Innocent/Rebel/Mentor/Trickster 중 하나"",
  ""romanceable"": true
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
                var data = JsonUtility.FromJson<NPCAutoFillData>(json);

                if (data == null)
                {
                    Debug.LogError("Failed to parse JSON data");
                    return;
                }

                if (nameInput != null && !string.IsNullOrEmpty(data.name))
                    nameInput.text = data.name;
                if (ageInput != null)
                    ageInput.text = data.age.ToString();
                if (roleInput != null && !string.IsNullOrEmpty(data.role))
                    roleInput.text = data.role;
                if (appearanceInput != null && !string.IsNullOrEmpty(data.appearance))
                    appearanceInput.text = data.appearance;
                if (personalityInput != null && !string.IsNullOrEmpty(data.personality))
                    personalityInput.text = data.personality;
                if (romanceableToggle != null)
                    romanceableToggle.isOn = data.romanceable;

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
                    // 로딩 팝업 숨기기
                    if (loadingPopup != null)
                        loadingPopup.SetActive(false);

                    generateFaceButton.interactable = true;
                    confirmButton.interactable = true;

                    // 버튼 텍스트를 "Generate More"로 변경
                    var btnText = generateFaceButton.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        btnText.text = "Generate More";
                    }

                    UpdatePreviewNavigation();
                },
                () => {
                    // 로딩 팝업 표시
                    if (loadingPopup != null)
                    {
                        loadingPopup.SetActive(true);
                        if (loadingText != null)
                            loadingText.text = "얼굴 이미지 생성 중...";
                    }
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
            // NPC 데이터 생성
            CharacterData npc = ScriptableObject.CreateInstance<CharacterData>();
            npc.characterName = nameInput.text;
            npc.age = int.Parse(ageInput.text);
            npc.gender = (Gender)genderDropdown.value;
            npc.role = roleInput.text;
            npc.appearanceDescription = appearanceInput.text;
            npc.personality = personalityInput.text;
            npc.archetype = (Archetype)archetypeDropdown.value;
            npc.confirmedSeed = faceGenerator.GetCurrentSeed();
            npc.facePreview = faceGenerator.GetCurrentPreview();
            npc.resourcePath = $"Generated/Characters/{npc.characterName}";
            npc.isRomanceable = romanceableToggle.isOn;
            npc.initialAffection = 0;

            wizardManager.projectData.npcs.Add(npc);
            npcCount++;

#if UNITY_EDITOR
            // 얼굴 프리뷰 저장
            SaveFacePreview(npc, previewImage.sprite.texture);
#endif

            // 테스트 모드 확인 (AutoFill 컴포넌트 존재 여부로 판단)
            var autoFill = wizardManager.GetComponent<SetupWizardAutoFill>();
            bool isTestMode = autoFill != null && autoFill.enableAutoFill;

            if (isTestMode)
            {
                // 테스트 모드: 스탠딩 이미지 생성 스킵
                Debug.Log("[Test Mode] Skipping standing sprite generation for NPC");
                addAnotherButton.interactable = true;
            }
            else
            {
                // 일반 모드: 스탠딩 5종 자동 생성
                StartCoroutine(GenerateStandingSprites(npc));
            }
        }

#if UNITY_EDITOR
        private void SaveFacePreview(CharacterData character, Texture2D texture)
        {
            string dir = $"Assets/Resources/Generated/Characters/{character.characterName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/face_preview.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"NPC face preview saved: {path}");
        }
#endif

        IEnumerator GenerateStandingSprites(CharacterData character)
        {
            var generator = gameObject.AddComponent<StandingSpriteGenerator>();

            // NPC는 추가 캐릭터 (스타일 통일)
            bool isFirst = false;

            bool completed = false;
            yield return generator.GenerateStandingSet(
                character,
                wizardManager.nanoBananaClient,
                isFirst,
                () => completed = true
            );

            yield return new WaitUntil(() => completed);

            Debug.Log($"NPC standing sprites generated: {character.characterName}");

            // 다음 NPC 추가 가능
            addAnotherButton.interactable = true;
        }

        public void OnAddAnotherClicked()
        {
            // 폼 초기화
            ClearForm();
            faceGenerator.ClearHistory();
            addAnotherButton.interactable = false;
        }

        public void OnFinishClicked()
        {
            Debug.Log($"Total NPCs created: {npcCount}");

            // NPC 생성 완료 후 엔딩 자동 생성
            wizardManager.GenerateEndings();

            wizardManager.NextStep();
        }

        void ClearForm()
        {
            nameInput.text = "";
            ageInput.text = "";
            roleInput.text = "";
            appearanceInput.text = "";
            personalityInput.text = "";
            romanceableToggle.isOn = false;
            genderDropdown.value = 0;
            archetypeDropdown.value = 0;

            // 버튼 텍스트 초기화
            if (autoFillButton != null)
            {
                var autoFillBtnText = autoFillButton.GetComponentInChildren<TMP_Text>();
                if (autoFillBtnText != null)
                {
                    autoFillBtnText.text = "AI 자동 생성";
                }
            }

            if (generateFaceButton != null)
            {
                var generateFaceBtnText = generateFaceButton.GetComponentInChildren<TMP_Text>();
                if (generateFaceBtnText != null)
                {
                    generateFaceBtnText.text = "Generate Face";
                }
            }

            // 프리뷰 네비게이션 UI 초기화
            UpdatePreviewNavigation();
        }

        // ===== JSON 스키마 =====

        [System.Serializable]
        private class NPCAutoFillData
        {
            public string name;
            public int age;
            public string gender;
            public string role;
            public string appearance;
            public string personality;
            public string archetype;
            public bool romanceable;
        }
    }
}
