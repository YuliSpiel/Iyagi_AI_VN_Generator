using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 3: 스토리 구조 설정
    /// 챕터 수, 엔딩 조건 정의
    /// </summary>
    public class Step3_StoryStructure : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("UI Elements")]
        public TMP_Dropdown chapterCountDropdown;
        public TMP_InputField ending1NameInput;
        public TMP_InputField ending2NameInput;
        public TMP_InputField ending3NameInput; // 선택적

        [Header("Buttons")]
        public Button nextStepButton;

        void Start()
        {
            // 챕터 수 드롭다운 초기화 (한국어)
            chapterCountDropdown.ClearOptions();
            chapterCountDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "3개 챕터",
                "4개 챕터",
                "5개 챕터",
                "6개 챕터"
            });
            chapterCountDropdown.value = 1; // 기본값: 4개 챕터

            // 버튼 이벤트
            nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 입력 검증
            ending1NameInput.onValueChanged.AddListener((_) => ValidateInputs());
            ending2NameInput.onValueChanged.AddListener((_) => ValidateInputs());

            ValidateInputs();
        }

        void ValidateInputs()
        {
            // 최소 2개의 엔딩 필요
            bool isValid = !string.IsNullOrEmpty(ending1NameInput.text) &&
                           !string.IsNullOrEmpty(ending2NameInput.text);

            nextStepButton.interactable = isValid;
        }

        public void OnNextStepClicked()
        {
            // 챕터 수 저장
            wizardManager.projectData.totalChapters = chapterCountDropdown.value + 3; // 3~6

            // 엔딩 조건 생성 (간단한 버전 - 자동 생성)
            wizardManager.projectData.endings.Clear();

            if (!string.IsNullOrEmpty(ending1NameInput.text))
            {
                var ending1 = new EndingCondition
                {
                    endingId = 1,
                    endingName = ending1NameInput.text.Trim(),
                    endingDescription = "First ending path",
                    requiredValues = new System.Collections.Generic.Dictionary<string, int>(),
                    requiredAffections = new System.Collections.Generic.Dictionary<string, int>()
                };

                // 첫 번째 Core Value가 높아야 함 (예시)
                if (wizardManager.projectData.coreValues.Count > 0)
                {
                    ending1.requiredValues[wizardManager.projectData.coreValues[0].name] = 50;
                }

                wizardManager.projectData.endings.Add(ending1);
            }

            if (!string.IsNullOrEmpty(ending2NameInput.text))
            {
                var ending2 = new EndingCondition
                {
                    endingId = 2,
                    endingName = ending2NameInput.text.Trim(),
                    endingDescription = "Second ending path",
                    requiredValues = new System.Collections.Generic.Dictionary<string, int>(),
                    requiredAffections = new System.Collections.Generic.Dictionary<string, int>()
                };

                // 두 번째 Core Value가 높아야 함 (예시)
                if (wizardManager.projectData.coreValues.Count > 1)
                {
                    ending2.requiredValues[wizardManager.projectData.coreValues[1].name] = 50;
                }

                wizardManager.projectData.endings.Add(ending2);
            }

            if (!string.IsNullOrEmpty(ending3NameInput.text))
            {
                var ending3 = new EndingCondition
                {
                    endingId = 3,
                    endingName = ending3NameInput.text.Trim(),
                    endingDescription = "Third ending path",
                    requiredValues = new System.Collections.Generic.Dictionary<string, int>(),
                    requiredAffections = new System.Collections.Generic.Dictionary<string, int>()
                };

                // 세 번째 Core Value가 높아야 함 (예시)
                if (wizardManager.projectData.coreValues.Count > 2)
                {
                    ending3.requiredValues[wizardManager.projectData.coreValues[2].name] = 50;
                }

                wizardManager.projectData.endings.Add(ending3);
            }

            wizardManager.NextStep();
        }
    }
}
