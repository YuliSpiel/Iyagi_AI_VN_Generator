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

            // 입력이 있으면 Auto-Suggest 버튼 텍스트 변경
            bool hasInput = !string.IsNullOrEmpty(value1NameInput.text) ||
                           !string.IsNullOrEmpty(value2NameInput.text) ||
                           !string.IsNullOrEmpty(value3NameInput.text) ||
                           !string.IsNullOrEmpty(value4NameInput.text);

            if (autoSuggestButton != null)
            {
                var btnText = autoSuggestButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = hasInput ? "AI 풍부화" : "AI 자동 생성";
                }
            }
        }

        public void OnAutoSuggestClicked()
        {
            Debug.Log("Auto-Suggest button clicked!");
            if (autoSuggestButton != null)
                autoSuggestButton.interactable = false;
            StartCoroutine(AutoSuggestValues());
        }

        IEnumerator AutoSuggestValues()
        {
            Debug.Log("AutoSuggestValues coroutine started");
            Debug.Log($"Game Title: {wizardManager.projectData.gameTitle}");
            Debug.Log($"Game Premise: {wizardManager.projectData.gamePremise}");

            // 사용자 입력 확인
            string userValue1 = value1NameInput.text.Trim();
            string userValue2 = value2NameInput.text.Trim();
            string userValue3 = value3NameInput.text.Trim();
            string userValue4 = value4NameInput.text.Trim();
            string userSkills1 = value1SkillsInput.text.Trim();
            string userSkills2 = value2SkillsInput.text.Trim();
            string userSkills3 = value3SkillsInput.text.Trim();
            string userSkills4 = value4SkillsInput.text.Trim();

            string prompt;

            if (string.IsNullOrEmpty(userValue1) && string.IsNullOrEmpty(userValue2) &&
                string.IsNullOrEmpty(userValue3) && string.IsNullOrEmpty(userValue4))
            {
                // 완전 자동 생성
                prompt = $@"비주얼 노벨 게임을 위한 Core Value와 파생 스탯을 추천해줘.

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
            }
            else
            {
                // 입력 풍부화
                prompt = $@"사용자가 비주얼 노벨 게임의 Core Value 아이디어를 개떡같이 입력했어. 이걸 찰떡같이 이해하고 풍부하게 발전시켜줘.

게임 정보:
제목: {wizardManager.projectData.gameTitle}
줄거리: {wizardManager.projectData.gamePremise}
장르: {wizardManager.projectData.genre}
톤: {wizardManager.projectData.tone}

사용자 입력:
Core Value 1: {(string.IsNullOrEmpty(userValue1) ? "(없음)" : userValue1)}
  파생 스탯: {(string.IsNullOrEmpty(userSkills1) ? "(없음)" : userSkills1)}
Core Value 2: {(string.IsNullOrEmpty(userValue2) ? "(없음)" : userValue2)}
  파생 스탯: {(string.IsNullOrEmpty(userSkills2) ? "(없음)" : userSkills2)}
Core Value 3: {(string.IsNullOrEmpty(userValue3) ? "(없음)" : userValue3)}
  파생 스탯: {(string.IsNullOrEmpty(userSkills3) ? "(없음)" : userSkills3)}
Core Value 4: {(string.IsNullOrEmpty(userValue4) ? "(없음)" : userValue4)}
  파생 스탯: {(string.IsNullOrEmpty(userSkills4) ? "(없음)" : userSkills4)}

요구사항:
1. 사용자 의도를 정확히 파악하고 핵심은 유지
2. Core Value 이름이 비어있거나 어색하면 게임 세계관에 맞게 개선
3. 파생 스탯이 비어있으면 각 Core Value에 맞는 2~4개의 구체적인 능력 추천
4. 파생 스탯이 있으면 풍부하게 확장 (2~4개가 되도록)
5. 오타, 맞춤법 수정
6. 애매한 표현은 명확하게
7. 최소 2개, 최대 4개의 Core Value 출력
8. 한국어로 출력

JSON 형식으로만 출력:
{{
  ""coreValues"": [
    {{
      ""name"": ""다듬어진 Core Value 이름"",
      ""derivedSkills"": [""풍부해진 스탯1"", ""풍부해진 스탯2"", ""풍부해진 스탯3""]
    }},
    {{
      ""name"": ""다듬어진 Core Value 이름"",
      ""derivedSkills"": [""풍부해진 스탯1"", ""풍부해진 스탯2"", ""풍부해진 스탯3""]
    }}
  ]
}}";
            }

            bool completed = false;
            string result = null;

            Debug.Log("Calling Gemini API for Core Values...");
            yield return wizardManager.geminiClient.GenerateContent(
                prompt,
                (response) => {
                    Debug.Log($"API Response received: {response.Substring(0, Mathf.Min(200, response.Length))}...");
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
                Debug.Log("Parsing response...");
                ParseAutoSuggestResponse(result);
            }
            else
            {
                Debug.LogWarning("API returned empty result");
            }

            if (autoSuggestButton != null)
                autoSuggestButton.interactable = true;
            Debug.Log("AutoSuggestValues coroutine completed");
        }

        void ParseAutoSuggestResponse(string jsonResponse)
        {
            Debug.Log("=== ParseAutoSuggestResponse START ===");
            Debug.Log($"Full response length: {jsonResponse.Length}");

            try
            {
                int startIndex = jsonResponse.IndexOf('{');
                int endIndex = jsonResponse.LastIndexOf('}');

                if (startIndex == -1 || endIndex == -1)
                {
                    Debug.LogError("Invalid JSON format - no braces found");
                    Debug.LogError($"Response: {jsonResponse}");
                    return;
                }

                string json = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                Debug.Log($"Extracted JSON: {json}");

                var data = JsonUtility.FromJson<CoreValuesSuggestion>(json);

                if (data == null || data.coreValues == null || data.coreValues.Length == 0)
                {
                    Debug.LogError("Invalid response data - data is null or empty");
                    Debug.LogError($"Data: {data}, coreValues: {data?.coreValues}, Length: {data?.coreValues?.Length}");
                    return;
                }

                Debug.Log($"Parsed {data.coreValues.Length} core values");

                // Value 1
                Debug.Log($"value1NameInput is null? {value1NameInput == null}");
                Debug.Log($"value1SkillsInput is null? {value1SkillsInput == null}");

                if (data.coreValues.Length > 0 && value1NameInput != null && value1SkillsInput != null)
                {
                    var cv1 = data.coreValues[0];
                    if (cv1 != null && !string.IsNullOrEmpty(cv1.name))
                    {
                        Debug.Log($"Setting Value 1: {cv1.name}");
                        value1NameInput.text = cv1.name;
                        value1SkillsInput.text = cv1.derivedSkills != null ? string.Join(", ", cv1.derivedSkills) : "";
                        Debug.Log($"Value 1 set successfully! Name: {value1NameInput.text}");
                    }
                    else
                    {
                        Debug.LogWarning($"cv1 is null or name is empty. cv1 null? {cv1 == null}, name: {cv1?.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Cannot set Value 1. Count: {data.coreValues.Length}, nameInput null: {value1NameInput == null}, skillsInput null: {value1SkillsInput == null}");
                }

                // Value 2
                if (data.coreValues.Length > 1 && value2NameInput != null && value2SkillsInput != null)
                {
                    var cv2 = data.coreValues[1];
                    if (cv2 != null && !string.IsNullOrEmpty(cv2.name))
                    {
                        value2NameInput.text = cv2.name;
                        value2SkillsInput.text = cv2.derivedSkills != null ? string.Join(", ", cv2.derivedSkills) : "";
                    }
                }

                // Value 3
                if (data.coreValues.Length > 2 && value3NameInput != null && value3SkillsInput != null)
                {
                    var cv3 = data.coreValues[2];
                    if (cv3 != null && !string.IsNullOrEmpty(cv3.name))
                    {
                        value3NameInput.text = cv3.name;
                        value3SkillsInput.text = cv3.derivedSkills != null ? string.Join(", ", cv3.derivedSkills) : "";
                    }
                }

                // Value 4 (optional)
                if (data.coreValues.Length > 3 && value4NameInput != null && value4SkillsInput != null)
                {
                    var cv4 = data.coreValues[3];
                    if (cv4 != null && !string.IsNullOrEmpty(cv4.name))
                    {
                        value4NameInput.text = cv4.name;
                        value4SkillsInput.text = cv4.derivedSkills != null ? string.Join(", ", cv4.derivedSkills) : "";
                    }
                }

                ValidateInputs();
                Debug.Log("=== ParseAutoSuggestResponse SUCCESS ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse response: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
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
