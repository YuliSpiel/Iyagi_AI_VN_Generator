using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 2: Core Values 설정
    /// 게임의 핵심 가치들 정의 (예: Courage, Wisdom, Compassion)
    /// </summary>
    public class Step2_CoreValues : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("UI Elements")]
        public TMP_InputField value1Input;
        public TMP_InputField value2Input;
        public TMP_InputField value3Input;
        public TMP_InputField value4Input; // 선택적

        [Header("Buttons")]
        public Button autoSuggestButton;
        public Button nextStepButton;

        void Start()
        {
            // 버튼 이벤트
            autoSuggestButton.onClick.AddListener(OnAutoSuggestClicked);
            nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 입력 검증
            value1Input.onValueChanged.AddListener((_) => ValidateInputs());
            value2Input.onValueChanged.AddListener((_) => ValidateInputs());
            value3Input.onValueChanged.AddListener((_) => ValidateInputs());

            ValidateInputs();
        }

        void ValidateInputs()
        {
            // 최소 3개의 Core Value 필요
            bool isValid = !string.IsNullOrEmpty(value1Input.text) &&
                           !string.IsNullOrEmpty(value2Input.text) &&
                           !string.IsNullOrEmpty(value3Input.text);

            nextStepButton.interactable = isValid;
        }

        public void OnAutoSuggestClicked()
        {
            autoSuggestButton.interactable = false;
            StartCoroutine(AutoSuggestValues());
        }

        IEnumerator AutoSuggestValues()
        {
            string prompt = $@"Based on this visual novel concept:
Title: {wizardManager.projectData.gameTitle}
Premise: {wizardManager.projectData.gamePremise}
Genre: {wizardManager.projectData.genre}

Suggest 3-4 core values that players can develop through their choices.
Output JSON only:
{{
  ""values"": [""Value1"", ""Value2"", ""Value3"", ""Value4 (optional)""]
}}

Examples: Courage, Wisdom, Compassion, Justice, Creativity, Ambition, etc.";

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
                var data = JsonUtility.FromJson<CoreValuesData>(json);

                if (data.values != null && data.values.Length > 0)
                {
                    value1Input.text = data.values.Length > 0 ? data.values[0] : "";
                    value2Input.text = data.values.Length > 1 ? data.values[1] : "";
                    value3Input.text = data.values.Length > 2 ? data.values[2] : "";
                    value4Input.text = data.values.Length > 3 ? data.values[3] : "";
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

            if (!string.IsNullOrEmpty(value1Input.text))
                wizardManager.projectData.coreValues.Add(value1Input.text.Trim());

            if (!string.IsNullOrEmpty(value2Input.text))
                wizardManager.projectData.coreValues.Add(value2Input.text.Trim());

            if (!string.IsNullOrEmpty(value3Input.text))
                wizardManager.projectData.coreValues.Add(value3Input.text.Trim());

            if (!string.IsNullOrEmpty(value4Input.text))
                wizardManager.projectData.coreValues.Add(value4Input.text.Trim());

            wizardManager.NextStep();
        }

        [System.Serializable]
        private class CoreValuesData
        {
            public string[] values;
        }
    }
}
