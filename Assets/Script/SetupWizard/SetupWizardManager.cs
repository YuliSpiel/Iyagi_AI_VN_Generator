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
        public ElevenLabsClient elevenLabsClient;

        [Header("Managers")]
        public ChapterGenerationManager chapterManager;

        [Header("Steps (UI Panels)")]
        public GameObject[] stepPanels; // Step1 ~ Step6 UI 패널

        public int currentStep { get; private set; } = 0;

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

            // 새 프로젝트 데이터 생성 (메모리에만)
            projectData = ScriptableObject.CreateInstance<VNProjectData>();
            projectData.projectGuid = System.Guid.NewGuid().ToString();
            projectData.createdTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // API 클라이언트 초기화
            geminiClient = gameObject.AddComponent<GeminiClient>();
            geminiClient.Initialize(config.geminiApiKey);

            nanoBananaClient = gameObject.AddComponent<NanoBananaClient>();
            nanoBananaClient.Initialize(config.geminiApiKey); // Gemini API로 이미지도 생성

            elevenLabsClient = gameObject.AddComponent<ElevenLabsClient>();
            if (!string.IsNullOrEmpty(config.elevenLabsApiKey))
            {
                elevenLabsClient.Initialize(config.elevenLabsApiKey);
            }
            else
            {
                Debug.LogWarning("ElevenLabs API key not set. Audio generation will be skipped.");
            }

            // ChapterGenerationManager 초기화
            chapterManager = gameObject.AddComponent<ChapterGenerationManager>();
            chapterManager.projectData = projectData;
            chapterManager.geminiClient = geminiClient;
            chapterManager.nanoBananaClient = nanoBananaClient;

            // 첫 번째 스텝 표시
            ShowStep(0);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Step 1에서 타이틀 입력 후 에셋 파일 생성
        /// </summary>
        public void CreateProjectAsset(string gameTitle)
        {
            projectData.gameTitle = gameTitle;

            // 프로젝트 파일 생성
            string projectName = gameTitle.Replace(" ", "_");
            string savePath = $"Assets/Resources/Projects/{projectName}.asset";

            // 폴더 생성
            if (!System.IO.Directory.Exists("Assets/Resources/Projects"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources/Projects");
            }

            // 기존 에셋 삭제
            if (UnityEditor.AssetDatabase.LoadAssetAtPath<VNProjectData>(savePath) != null)
            {
                UnityEditor.AssetDatabase.DeleteAsset(savePath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"Deleted existing project: {savePath}");
            }

            // 에셋 파일 생성
            UnityEditor.AssetDatabase.CreateAsset(projectData, savePath);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"Created project asset: {savePath}");
        }

        /// <summary>
        /// 각 스텝에서 변경사항을 에셋에 저장
        /// </summary>
        public void SaveProjectAsset()
        {
            if (projectData != null)
            {
                UnityEditor.EditorUtility.SetDirty(projectData);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
#endif

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
        /// 위자드 완료 시 호출 - 병렬 에셋 생성 포함 (Cycle 1-3)
        /// </summary>
        public void OnWizardComplete()
        {
#if UNITY_EDITOR
            // 캐릭터 데이터를 서브 에셋으로 저장
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(projectData);

            if (projectData.playerCharacter != null)
            {
                projectData.playerCharacter.name = projectData.playerCharacter.characterName;
                UnityEditor.AssetDatabase.AddObjectToAsset(projectData.playerCharacter, assetPath);
            }

            foreach (var npc in projectData.npcs)
            {
                npc.name = npc.characterName;
                UnityEditor.AssetDatabase.AddObjectToAsset(npc, assetPath);
            }

            // 최종 저장
            UnityEditor.EditorUtility.SetDirty(projectData);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"VN Project completed: {assetPath}");
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

                // ✅ [2025-01-10] 병렬 에셋 생성 시작
                StartCoroutine(RunParallelAssetGeneration());
            }
            else
            {
                Debug.LogError("Failed to create initial save file!");
            }
        }

        /// <summary>
        /// 병렬 에셋 생성 실행 (Cycle 1-3)
        /// </summary>
        private System.Collections.IEnumerator RunParallelAssetGeneration()
        {
            Debug.Log("=== [ParallelAssetGeneration] 시작 ===");

            // ParallelAssetGenerator 초기화
            var generator = gameObject.AddComponent<ParallelAssetGenerator>();
            generator.projectData = projectData;
            generator.nanoBananaClient = nanoBananaClient;
            generator.geminiClient = geminiClient;
            generator.elevenLabsClient = elevenLabsClient;
            generator.chapterManager = chapterManager;

            string chapter1JSON = null;

            // Cycle 1 & 2 병렬 실행 (0% → 50%)
            yield return generator.RunCycle1And2Parallel(
                (progress) => {
                    Debug.Log($"[Progress] {(progress * 100):F0}%");
                    // TODO: Step6 UI에 진행률 표시
                },
                (json) => {
                    chapter1JSON = json;
                    Debug.Log("[Barrier] Cycle 1 & 2 완료 (50%)");
                },
                () => {
                    Debug.Log("[Barrier] 챕터1 JSON 준비 완료");
                }
            );

            // 테스트 모드 확인
            var autoFill = GetComponent<SetupWizardAutoFill>();
            bool isTestMode = autoFill != null && autoFill.enableAutoFill;

            if (isTestMode)
            {
                Debug.Log("[Test Mode] Cycle 3 스킵 - 에셋 생성 없이 GameScene 로드");
            }
            else
            {
                // Cycle 3: 에셋 생성 (50% → 100%)
                yield return generator.RunCycle3(
                    chapter1JSON,
                    (progress) => {
                        Debug.Log($"[Progress] {(progress * 100):F0}%");
                        // TODO: Step6 UI에 진행률 표시
                    },
                    () => {
                        Debug.Log("[Final Barrier] Cycle 3 완료 (100%)");
                    }
                );
            }

            Debug.Log("=== [ParallelAssetGeneration] 완료 ===");

            // GameScene 로드
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
