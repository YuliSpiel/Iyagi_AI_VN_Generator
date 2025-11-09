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
            nanoBananaClient.Initialize(config.geminiApiKey); // Gemini API로 이미지도 생성

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
        /// Core Value × 공략가능 NPC 조합으로 엔딩 자동 생성
        /// Step 5 (NPC 생성) 완료 후 호출
        /// </summary>
        public void GenerateEndings()
        {
            projectData.endings.Clear();

            // 공략 가능한 NPC 필터링
            var romanceableNPCs = projectData.npcs.FindAll(npc => npc.isRomanceable);

            if (romanceableNPCs.Count == 0)
            {
                Debug.LogWarning("No romanceable NPCs found. Endings will not be generated.");
                return;
            }

            if (projectData.coreValues.Count == 0)
            {
                Debug.LogWarning("No core values found. Endings will not be generated.");
                return;
            }

            Debug.Log($"=== Generating Endings: {projectData.coreValues.Count} Core Values × {romanceableNPCs.Count} Romanceable NPCs ===");

            int endingId = 1;
            foreach (var coreValue in projectData.coreValues)
            {
                foreach (var npc in romanceableNPCs)
                {
                    var ending = new EndingCondition
                    {
                        endingId = endingId,
                        endingName = $"{coreValue.name} + {npc.characterName}",
                        endingDescription = $"{coreValue.name} 가치를 달성하고 {npc.characterName}와의 관계를 발전시킨 결말",
                        requiredValues = new System.Collections.Generic.Dictionary<string, int>(),
                        requiredAffections = new System.Collections.Generic.Dictionary<string, int>()
                    };

                    // Core Value 요구치 설정 (해당 가치가 70 이상)
                    ending.requiredValues[coreValue.name] = 70;

                    // NPC 호감도 요구치 설정 (해당 NPC가 80 이상)
                    ending.requiredAffections[npc.characterName] = 80;

                    projectData.endings.Add(ending);
                    Debug.Log($"[Ending {endingId}] Created: {ending.endingName}");

                    endingId++;
                }
            }

            Debug.Log($"=== Total {projectData.endings.Count} endings generated ===");
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

            // 기존 에셋이 있으면 삭제
            if (System.IO.File.Exists(savePath))
            {
                UnityEditor.AssetDatabase.DeleteAsset(savePath);
                Debug.Log($"Deleted existing project at {savePath}");
            }

            UnityEditor.AssetDatabase.CreateAsset(projectData, savePath);

            // 캐릭터 데이터를 서브 에셋으로 저장
            if (projectData.playerCharacter != null)
            {
                projectData.playerCharacter.name = projectData.playerCharacter.characterName;
                UnityEditor.AssetDatabase.AddObjectToAsset(projectData.playerCharacter, projectData);
            }

            foreach (var npc in projectData.npcs)
            {
                npc.name = npc.characterName;
                UnityEditor.AssetDatabase.AddObjectToAsset(npc, projectData);
            }

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"VN Project saved: {savePath}");
#endif

            // SaveDataManager에 프로젝트 슬롯 생성
            var existingSlot = SaveDataManager.Instance.GetProjectSlot(projectData.projectGuid);
            if (existingSlot == null)
            {
                SaveDataManager.Instance.CreateProjectSlot(projectData);
                Debug.Log($"Created project slot for: {projectData.gameTitle}");
            }

            // 첫 저장 파일 생성 (Chapter 1 시작)
            var newSaveFile = SaveDataManager.Instance.CreateNewSaveFile(projectData.projectGuid, 1);
            if (newSaveFile != null)
            {
                Debug.Log($"Created initial save file: {newSaveFile.saveFileId}");

                // 저장 파일 로드 (PlayerPrefs에 저장)
                SaveDataManager.Instance.LoadSaveFile(newSaveFile.saveFileId);

                // 게임 씬으로 전환
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError("Failed to create initial save file!");
            }
        }
    }
}
