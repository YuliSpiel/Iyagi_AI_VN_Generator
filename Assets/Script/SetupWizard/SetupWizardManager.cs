using UnityEngine;
using IyagiAI.Runtime;
using IyagiAI.AISystem;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard 전체 플로우 관리
    /// STEP 1 → STEP 6까지 순차적으로 진행
    /// </summary>
    public class SetupWizardManager : MonoBehaviour
    {
        [Header("Project Data")]
        public VNProjectData projectData;

        [Header("API Clients")]
        public GeminiClient geminiClient;
        public NanoBananaClient nanoBananaClient;

        [Header("Steps (UI Panels)")]
        public GameObject[] stepPanels; // Step1 ~ Step6 UI 패널

        private int currentStep = 0;

        void Start()
        {
            // API 설정 로드
            APIConfigData config = Resources.Load<APIConfigData>("APIConfig");

            if (config == null)
            {
                Debug.LogError("APIConfig.asset not found! Please create it in Resources folder.");
                Debug.LogError("Create: Assets/Resources/APIConfig.asset");
                return;
            }

            if (!config.IsValid())
            {
                Debug.LogError("APIConfig is invalid. Please set API keys.");
                return;
            }

            // 새 프로젝트 데이터 생성
            projectData = ScriptableObject.CreateInstance<VNProjectData>();
            projectData.projectGuid = System.Guid.NewGuid().ToString();
            projectData.createdTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // API 클라이언트 초기화
            geminiClient = gameObject.AddComponent<GeminiClient>();
            geminiClient.Initialize(config.geminiApiKey);

            nanoBananaClient = gameObject.AddComponent<NanoBananaClient>();
            nanoBananaClient.Initialize(config.nanoBananaApiKey);

            // 첫 번째 스텝 표시
            ShowStep(0);
        }

        /// <summary>
        /// 특정 스텝 표시
        /// </summary>
        public void ShowStep(int stepIndex)
        {
            // 모든 패널 비활성화
            for (int i = 0; i < stepPanels.Length; i++)
            {
                if (stepPanels[i] != null)
                    stepPanels[i].SetActive(false);
            }

            // 현재 스텝 활성화
            if (stepIndex >= 0 && stepIndex < stepPanels.Length && stepPanels[stepIndex] != null)
            {
                stepPanels[stepIndex].SetActive(true);
                currentStep = stepIndex;
                Debug.Log($"Setup Wizard: Step {stepIndex + 1}/{stepPanels.Length}");
            }
        }

        /// <summary>
        /// 다음 스텝으로 이동
        /// </summary>
        public void NextStep()
        {
            if (currentStep < stepPanels.Length - 1)
            {
                ShowStep(currentStep + 1);
            }
            else
            {
                Debug.Log("Setup Wizard completed!");
                OnWizardComplete();
            }
        }

        /// <summary>
        /// 이전 스텝으로 이동
        /// </summary>
        public void PreviousStep()
        {
            if (currentStep > 0)
            {
                ShowStep(currentStep - 1);
            }
        }

        /// <summary>
        /// 위자드 완료 시 호출
        /// </summary>
        public void OnWizardComplete()
        {
#if UNITY_EDITOR
            // 프로젝트 데이터를 ScriptableObject로 저장
            string projectName = projectData.gameTitle.Replace(" ", "_");
            string savePath = $"Assets/VNProjects/{projectName}.asset";

            // 폴더 생성
            if (!System.IO.Directory.Exists("Assets/VNProjects"))
            {
                System.IO.Directory.CreateDirectory("Assets/VNProjects");
            }

            UnityEditor.AssetDatabase.CreateAsset(projectData, savePath);

            // 캐릭터 데이터도 저장
            string charFolder = $"Assets/VNProjects/Characters";
            if (!System.IO.Directory.Exists(charFolder))
            {
                System.IO.Directory.CreateDirectory(charFolder);
            }

            if (projectData.playerCharacter != null)
            {
                string charPath = $"{charFolder}/{projectData.playerCharacter.characterName}.asset";
                UnityEditor.AssetDatabase.CreateAsset(projectData.playerCharacter, charPath);
            }

            foreach (var npc in projectData.npcs)
            {
                string charPath = $"{charFolder}/{npc.characterName}.asset";
                UnityEditor.AssetDatabase.CreateAsset(npc, charPath);
            }

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"VN Project saved: {savePath}");
#endif

            // TODO: 게임 Scene으로 전환
            // UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
