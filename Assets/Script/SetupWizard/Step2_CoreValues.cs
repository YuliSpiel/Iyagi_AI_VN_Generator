using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 2: Core Values 설정
    /// Core Value와 파생 스탯 관리
    /// </summary>
    public class Step2_CoreValues : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("UI Elements")]
        public TMP_InputField value1NameInput;
        public TMP_InputField value1SkillsInput; // 쉼표로 구분
        public TMP_InputField value2NameInput;
        public TMP_InputField value2SkillsInput;
        public TMP_InputField value3NameInput;
        public TMP_InputField value3SkillsInput;
        public TMP_InputField value4NameInput; // 선택적
        public TMP_InputField value4SkillsInput;

        [Header("Buttons")]
        public Button autoSuggestButton;
        public Button nextStepButton;

        void Start()
        {
            // 버튼 이벤트
            if (autoSuggestButton != null)
                autoSuggestButton.onClick.AddListener(OnAutoSuggestClicked);
            if (nextStepButton != null)
                nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 입력 검증
            if (value1NameInput != null)
                value1NameInput.onValueChanged.AddListener((_) => ValidateInputs());
            if (value2NameInput != null)
                value2NameInput.onValueChanged.AddListener((_) => ValidateInputs());
            if (value3NameInput != null)
                value3NameInput.onValueChanged.AddListener((_) => ValidateInputs());

            ValidateInputs();
        }

        void ValidateInputs()
        {
            // 최소 2개의 Core Value 필요 (이름만 확인)
            bool isValid = value1NameInput != null && !string.IsNullOrEmpty(value1NameInput.text) &&
                           value2NameInput != null && !string.IsNullOrEmpty(value2NameInput.text);

            if (nextStepButton != null)
                nextStepButton.interactable = isValid;
        }

        public void OnAutoSuggestClicked()
        {
            autoSuggestButton.interactable = false;
            StartCoroutine(AutoSuggestValues());
        }

        IEnumerator AutoSuggestValues()
        {
            string prompt = $@"비주얼 노벨 게임을 위한 Core Value와 파생 스탯을 추천해줘.

게임 정보:
제목: {wizardManager.projectData.gameTitle}
줄거리: {wizardManager.projectData.gamePremise}
장르: {wizardManager.projectData.genre}
톤: {wizardManager.projectData.tone}

요구사항:
- 2~4개의 Core Value 추천
- 각 Core Value마다 2~4개의 파생 스탯(derived skills) 추천
- 한국어로 출력
- Core Value는 게임의 핵심 가치 (예: 정의, 출세, 우정)
- 파생 스탯은 구체적인 능력 (예: 자긍심, 공감능력, 판단력)

JSON 형식으로만 출력:
{{
  ""coreValues"": [
    {{
      ""name"": ""정의"",
      ""derivedSkills"": [""자긍심"", ""공감능력"", ""판단력""]
    }},
    {{
      ""name"": ""출세"",
      ""derivedSkills"": [""야망"", ""사교성"", ""카리스마""]
    }}
  ]
}}";

            bool completed = false;
            string result = null;

            yield return wizardManager.geminiClient.GenerateContent(
                prompt,
                (response) => {
                    result = response;
                    completed = true;
                },
                (error) => {
                    Debug.LogError($"Auto-suggest failed: {error}");
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (!string.IsNullOrEmpty(result))
            {
                ParseAutoSuggestResponse(result);
            }

            autoSuggestButton.interactable = true;
        }

        void ParseAutoSuggestResponse(string jsonResponse)
        {
            try
            {
                int startIndex = jsonResponse.IndexOf('{');
                int endIndex = jsonResponse.LastIndexOf('}');

                if (startIndex == -1 || endIndex == -1)
                {
                    Debug.LogError("Invalid JSON format");
                    return;
                }

                string json = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                var data = JsonUtility.FromJson<CoreValuesSuggestion>(json);

                if (data.coreValues != null && data.coreValues.Length > 0)
                {
                    // Value 1
                    if (data.coreValues.Length > 0)
                    {
                        value1NameInput.text = data.coreValues[0].name;
                        value1SkillsInput.text = string.Join(", ", data.coreValues[0].derivedSkills);
                    }

                    // Value 2
                    if (data.coreValues.Length > 1)
                    {
                        value2NameInput.text = data.coreValues[1].name;
                        value2SkillsInput.text = string.Join(", ", data.coreValues[1].derivedSkills);
                    }

                    // Value 3
                    if (data.coreValues.Length > 2)
                    {
                        value3NameInput.text = data.coreValues[2].name;
                        value3SkillsInput.text = string.Join(", ", data.coreValues[2].derivedSkills);
                    }

                    // Value 4 (optional)
                    if (data.coreValues.Length > 3)
                    {
                        value4NameInput.text = data.coreValues[3].name;
                        value4SkillsInput.text = string.Join(", ", data.coreValues[3].derivedSkills);
                    }
                }

                ValidateInputs();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse response: {e.Message}");
            }
        }

        public void OnNextStepClicked()
        {
            // Core Values 저장
            wizardManager.projectData.coreValues.Clear();

            // Value 1
            if (!string.IsNullOrEmpty(value1NameInput.text))
            {
                var value1 = new CoreValue
                {
                    name = value1NameInput.text.Trim(),
                    derivedSkills = ParseSkills(value1SkillsInput.text)
                };
                wizardManager.projectData.coreValues.Add(value1);
            }

            // Value 2
            if (!string.IsNullOrEmpty(value2NameInput.text))
            {
                var value2 = new CoreValue
                {
                    name = value2NameInput.text.Trim(),
                    derivedSkills = ParseSkills(value2SkillsInput.text)
                };
                wizardManager.projectData.coreValues.Add(value2);
            }

            // Value 3
            if (!string.IsNullOrEmpty(value3NameInput.text))
            {
                var value3 = new CoreValue
                {
                    name = value3NameInput.text.Trim(),
                    derivedSkills = ParseSkills(value3SkillsInput.text)
                };
                wizardManager.projectData.coreValues.Add(value3);
            }

            // Value 4 (optional)
            if (!string.IsNullOrEmpty(value4NameInput.text))
            {
                var value4 = new CoreValue
                {
                    name = value4NameInput.text.Trim(),
                    derivedSkills = ParseSkills(value4SkillsInput.text)
                };
                wizardManager.projectData.coreValues.Add(value4);
            }

            wizardManager.NextStep();
        }

        List<string> ParseSkills(string skillsText)
        {
            if (string.IsNullOrEmpty(skillsText))
                return new List<string>();

            return skillsText.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        // ===== JSON 스키마 =====

        [System.Serializable]
        private class CoreValuesSuggestion
        {
            public CoreValueSuggestion[] coreValues;
        }

        [System.Serializable]
        private class CoreValueSuggestion
        {
            public string name;
            public string[] derivedSkills;
        }
    }
}
