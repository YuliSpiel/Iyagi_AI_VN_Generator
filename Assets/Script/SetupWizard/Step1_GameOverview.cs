using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;
using System.Collections;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 1: 게임 개요 입력
    /// 제목, 줄거리, 장르, 톤, 플레이타임 설정
    /// </summary>
    public class Step1_GameOverview : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("UI Elements")]
        public TMP_InputField titleInput;
        public TMP_InputField premiseInput;
        public TMP_Dropdown genreDropdown;
        public TMP_Dropdown toneDropdown;
        public TMP_Dropdown playtimeDropdown;

        [Header("Buttons")]
        public Button autoFillButton;
        public Button nextStepButton;

        void Start()
        {
            // 드롭다운 초기화
            InitializeDropdowns();

            // 버튼 이벤트 연결
            autoFillButton.onClick.AddListener(OnAutoFillClicked);
            nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 다음 버튼은 필수 필드 입력 시에만 활성화
            titleInput.onValueChanged.AddListener((_) => ValidateInputs());
            premiseInput.onValueChanged.AddListener((_) => ValidateInputs());

            ValidateInputs();
        }

        /// <summary>
        /// 드롭다운 옵션 초기화
        /// </summary>
        void InitializeDropdowns()
        {
            // Genre 드롭다운 (한국어)
            genreDropdown.ClearOptions();
            genreDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "판타지",
                "SF",
                "미스터리",
                "로맨스",
                "호러",
                "어드벤처",
                "일상"
            });

            // Tone 드롭다운 (한국어)
            toneDropdown.ClearOptions();
            toneDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "밝고 경쾌함",
                "진지함",
                "어두움",
                "코미디",
                "드라마틱"
            });

            // Playtime 드롭다운 (한국어)
            playtimeDropdown.ClearOptions();
            playtimeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "30분",
                "1시간",
                "2시간",
                "3시간 이상"
            });
        }

        /// <summary>
        /// 입력 검증
        /// </summary>
        void ValidateInputs()
        {
            bool isValid = !string.IsNullOrEmpty(titleInput.text) &&
                           !string.IsNullOrEmpty(premiseInput.text);

            nextStepButton.interactable = isValid;

            // 입력이 있으면 Auto-Fill 버튼 텍스트 변경
            if (autoFillButton != null)
            {
                var btnText = autoFillButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = isValid ? "AI 풍부화" : "AI 자동 생성";
                }
            }
        }

        /// <summary>
        /// Auto Fill 버튼 클릭 (사용자 입력 풍부화 또는 자동 생성)
        /// </summary>
        public void OnAutoFillClicked()
        {
            autoFillButton.interactable = false;
            StartCoroutine(AutoFillWithAI());
        }

        IEnumerator AutoFillWithAI()
        {
            // 사용자 입력 확인
            string userTitle = titleInput.text.Trim();
            string userPremise = premiseInput.text.Trim();

            string prompt;

            if (string.IsNullOrEmpty(userTitle) && string.IsNullOrEmpty(userPremise))
            {
                // 아무것도 입력 안 했으면 완전 새로 생성
                prompt = @"비주얼 노벨 게임 컨셉을 생성해줘. 창의적이고 매력적인 내용으로 만들어줘.

JSON 형식으로만 출력:
{
  ""title"": ""게임 제목 (짧고 임팩트 있게)"",
  ""premise"": ""스토리 줄거리 (2-3문장, 흥미롭게)"",
  ""genre"": ""Fantasy/SciFi/Mystery/Romance/Horror/Adventure/Slice_of_Life 중 하나"",
  ""tone"": ""Lighthearted/Serious/Dark/Comedic/Dramatic 중 하나""
}";
            }
            else
            {
                // 입력이 있으면 풍부화
                prompt = $@"사용자가 비주얼 노벨 게임 아이디어를 개떡같이 입력했어. 이걸 찰떡같이 이해하고 풍부하게 발전시켜줘.

사용자 입력:
제목: {(string.IsNullOrEmpty(userTitle) ? "(없음)" : userTitle)}
줄거리: {(string.IsNullOrEmpty(userPremise) ? "(없음)" : userPremise)}

요구사항:
1. 사용자 의도를 정확히 파악하고 핵심은 유지
2. 제목은 더 매력적이고 기억에 남게 다듬기 (단, 원래 의도는 살릴 것)
3. 줄거리는 2-3문장으로 풍부하고 구체적으로 확장
4. 입력 내용을 기반으로 적절한 장르와 톤 추천
5. 오타, 맞춤법 수정
6. 애매한 표현은 명확하게

JSON 형식으로만 출력:
{{
  ""title"": ""다듬어진 제목"",
  ""premise"": ""풍부해진 줄거리 (2-3문장)"",
  ""genre"": ""Fantasy/SciFi/Mystery/Romance/Horror/Adventure/Slice_of_Life 중 하나"",
  ""tone"": ""Lighthearted/Serious/Dark/Comedic/Dramatic 중 하나""
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

        /// <summary>
        /// AI 응답 파싱
        /// </summary>
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
                var data = JsonUtility.FromJson<AutoFillData>(json);

                titleInput.text = data.title;
                premiseInput.text = data.premise;

                // Genre 설정
                if (System.Enum.TryParse<Genre>(data.genre.Replace(" ", "_"), out Genre g))
                {
                    genreDropdown.value = (int)g;
                }

                // Tone 설정
                if (System.Enum.TryParse<Tone>(data.tone, out Tone t))
                {
                    toneDropdown.value = (int)t;
                }

                ValidateInputs();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse auto-fill response: {e.Message}");
            }
        }

        /// <summary>
        /// 다음 단계로
        /// </summary>
        public void OnNextStepClicked()
        {
            // 데이터 저장
            wizardManager.projectData.gameTitle = titleInput.text;
            wizardManager.projectData.gamePremise = premiseInput.text;
            wizardManager.projectData.genre = (Genre)genreDropdown.value;
            wizardManager.projectData.tone = (Tone)toneDropdown.value;
            wizardManager.projectData.playtime = (PlaytimeEstimate)playtimeDropdown.value;

            wizardManager.NextStep();
        }

        // ===== JSON 스키마 =====

        [System.Serializable]
        private class AutoFillData
        {
            public string title;
            public string premise;
            public string genre;
            public string tone;
        }
    }
}
