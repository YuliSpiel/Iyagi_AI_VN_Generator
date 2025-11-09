using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 3: 스토리 구조 설정
    /// 챕터 수만 선택 (엔딩은 Step 5 이후 자동 생성)
    /// </summary>
    public class Step3_StoryStructure : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("UI Elements")]
        public TMP_Dropdown chapterCountDropdown;
        public TMP_Text infoText; // 엔딩 자동 생성 안내 텍스트

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

            // 안내 텍스트 설정
            if (infoText != null)
            {
                infoText.text = "엔딩은 Core Value와 공략 가능한 NPC 조합에 따라 자동 생성됩니다.\n" +
                               "(코어밸류 수 × 공략가능 NPC 수 = 엔딩 수)";
            }

            // 버튼 이벤트
            nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 항상 진행 가능
            nextStepButton.interactable = true;
        }

        public void OnNextStepClicked()
        {
            // 챕터 수만 저장
            wizardManager.projectData.totalChapters = chapterCountDropdown.value + 3; // 3~6

            // 엔딩은 Step 5 이후에 GenerateEndings()로 생성됨
            wizardManager.projectData.endings.Clear();

            wizardManager.NextStep();
        }
    }
}
