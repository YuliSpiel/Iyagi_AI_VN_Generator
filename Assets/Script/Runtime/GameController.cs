using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IyagiAI.AISystem;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 게임 전체 플로우 제어
    /// 챕터 생성 → DialogueUI 표시 → 선택지 처리 → 상태 업데이트 → 다음 챕터
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Project")]
        public VNProjectData projectData;

        [Header("UI")]
        public DialogueUI dialogueUI;
        public GameObject endingPanel;
        public TMPro.TMP_Text endingTitleText;
        public TMPro.TMP_Text endingDescriptionText;
        public TMPro.TMP_Text endingStatsText;
        public TMPro.TMP_Text romanceText;

        [Header("Managers")]
        public ChapterGenerationManager chapterManager;
        public EndingManager endingManager;
        public RuntimeSpriteManager spriteManager;
        public SaveDataManager saveManager;

        [Header("Current State")]
        public GameStateSnapshot currentState;
        public int currentChapterId = 1;
        public List<DialogueRecord> currentChapterRecords;
        public int currentLineIndex = 0;

        void Start()
        {
            Debug.Log("=== [GameController] Start() 시작 ===");

            // SaveDataManager에서 현재 로드된 SaveFile 정보 가져오기
            string currentSaveFileId = PlayerPrefs.GetString("CurrentSaveFileId", "");
            Debug.Log($"[GameController] CurrentSaveFileId from PlayerPrefs: {currentSaveFileId}");

            if (string.IsNullOrEmpty(currentSaveFileId))
            {
                Debug.LogError("[GameController] No save file loaded! Returning to title...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                return;
            }

            // SaveFile에서 프로젝트 GUID 찾기
            SaveFile saveFile = null;
            var allProjects = SaveDataManager.Instance.GetAllProjectSlots();
            Debug.Log($"[GameController] Total project slots: {allProjects.Count}");

            foreach (var project in allProjects)
            {
                Debug.Log($"[GameController] Checking project: {project.projectName} (GUID: {project.projectGuid})");
                saveFile = project.saveFiles.FirstOrDefault(s => s.saveFileId == currentSaveFileId);
                if (saveFile != null)
                {
                    Debug.Log($"[GameController] Found SaveFile in project: {project.projectName}");
                    break;
                }
            }

            if (saveFile == null)
            {
                Debug.LogError($"[GameController] SaveFile not found: {currentSaveFileId}");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                return;
            }

            // 프로젝트 데이터 로드 (Resources에서, GUID로 검색)
            string projectGuid = saveFile.projectGuid;
            Debug.Log($"[GameController] Loading project with GUID: {projectGuid}");

            // 먼저 Projects 폴더에서 찾기
            var allProjectsInResources = Resources.LoadAll<VNProjectData>("Projects");
            Debug.Log($"[GameController] Found {allProjectsInResources.Length} projects in Resources/Projects");
            projectData = System.Array.Find(allProjectsInResources, p => p.projectGuid == projectGuid);

            // Projects에서 못 찾으면 VNProjects 폴더에서 찾기
            if (projectData == null)
            {
                var vnProjects = Resources.LoadAll<VNProjectData>("VNProjects");
                Debug.Log($"[GameController] Found {vnProjects.Length} projects in Resources/VNProjects");
                projectData = System.Array.Find(vnProjects, p => p.projectGuid == projectGuid);
            }

            if (projectData == null)
            {
                Debug.LogError($"[GameController] Failed to load project data: {projectGuid}");
                Debug.LogError($"[GameController] Searched in Resources/Projects and Resources/VNProjects");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                return;
            }

            Debug.Log($"[GameController] ✅ Loaded project: {projectData.gameTitle}");
            Debug.Log($"[GameController] Project GUID: {projectData.projectGuid}");
            Debug.Log($"[GameController] Player Character: {projectData.playerCharacter?.characterName ?? "null"}");
            Debug.Log($"[GameController] NPCs: {projectData.npcs?.Count ?? 0}");

            // API 설정 로드
            APIConfigData config = Resources.Load<APIConfigData>("APIConfig");

            if (config == null || !config.IsValid())
            {
                Debug.LogError("APIConfig is invalid!");
                return;
            }

            // ChapterGenerationManager 초기화
            if (chapterManager == null)
            {
                chapterManager = gameObject.AddComponent<ChapterGenerationManager>();
            }
            chapterManager.projectData = projectData;

            var geminiClient = gameObject.AddComponent<GeminiClient>();
            geminiClient.Initialize(config.geminiApiKey);
            chapterManager.geminiClient = geminiClient;

            var nanoBananaClient = gameObject.AddComponent<NanoBananaClient>();
            nanoBananaClient.Initialize(config.geminiApiKey); // Gemini API로 이미지도 생성
            chapterManager.nanoBananaClient = nanoBananaClient;

            // RuntimeSpriteManager 초기화
            if (RuntimeSpriteManager.Instance != null)
            {
                spriteManager = RuntimeSpriteManager.Instance;
                spriteManager.Initialize(projectData, nanoBananaClient);
            }

            // 게임 상태 초기화
            InitializeGameState();

            // 첫 번째 챕터 시작
            StartCoroutine(StartChapter(currentChapterId));
        }

        /// <summary>
        /// 게임 상태 초기화
        /// </summary>
        void InitializeGameState()
        {
            currentState = new GameStateSnapshot();
            currentState.currentChapter = 1;
            currentState.currentLineId = 1000; // 챕터 1의 시작 ID

            // Core Values 초기화
            if (projectData != null && projectData.coreValues != null)
            {
                foreach (var value in projectData.coreValues)
                {
                    if (value != null && !string.IsNullOrEmpty(value.name))
                    {
                        currentState.coreValueScores[value.name] = 0;

                        // Derived Skills 초기화
                        if (value.derivedSkills != null)
                        {
                            foreach (var skill in value.derivedSkills)
                            {
                                if (!string.IsNullOrEmpty(skill))
                                {
                                    currentState.skillScores[skill] = 0;
                                }
                            }
                        }
                    }
                }
            }

            // 캐릭터 호감도 초기화
            if (projectData != null && projectData.npcs != null)
            {
                foreach (var npc in projectData.npcs)
                {
                    if (npc != null && !string.IsNullOrEmpty(npc.characterName))
                    {
                        currentState.characterAffections[npc.characterName] = npc.initialAffection;
                    }
                }
            }

            Debug.Log($"Game state initialized: {currentState.coreValueScores.Count} core values, {currentState.skillScores.Count} skills, {currentState.characterAffections.Count} NPCs");
        }

        /// <summary>
        /// 챕터 시작
        /// </summary>
        public IEnumerator StartChapter(int chapterId)
        {
            Debug.Log($"Starting Chapter {chapterId}...");

            // 테스트 모드 체크: TestChapter1.json 파일이 있으면 직접 로드
            TextAsset testChapter = Resources.Load<TextAsset>("TestChapter1");
            if (testChapter != null)
            {
                Debug.Log("[GameController] Test mode detected - loading TestChapter1.json directly");
                currentChapterRecords = AIDataConverter.FromAIJson(testChapter.text, chapterId);

                if (currentChapterRecords != null && currentChapterRecords.Count > 0)
                {
                    currentChapterId = chapterId;
                    currentLineIndex = 0;

                    // DialogueUI에 첫 라인 표시
                    ShowCurrentLine();

                    Debug.Log($"[GameController] TestChapter1 loaded with {currentChapterRecords.Count} lines");
                }
                else
                {
                    Debug.LogError("[GameController] Failed to parse TestChapter1.json");
                }

                yield break;
            }

            // 일반 모드: ChapterGenerationManager로 생성/로드
            bool completed = false;

            yield return chapterManager.GenerateOrLoadChapter(
                chapterId,
                currentState,
                (records) => {
                    currentChapterRecords = records;
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (currentChapterRecords != null && currentChapterRecords.Count > 0)
            {
                currentChapterId = chapterId;
                currentLineIndex = 0;

                // DialogueUI에 첫 라인 표시
                ShowCurrentLine();

                Debug.Log($"Chapter {chapterId} loaded with {currentChapterRecords.Count} lines");
            }
            else
            {
                Debug.LogError($"Failed to load chapter {chapterId}");
            }
        }

        /// <summary>
        /// 현재 라인 표시
        /// </summary>
        void ShowCurrentLine()
        {
            Debug.Log($"[GameController] ShowCurrentLine called. Records null? {currentChapterRecords == null}, Index: {currentLineIndex}");

            if (currentChapterRecords == null || currentLineIndex >= currentChapterRecords.Count)
            {
                Debug.LogWarning($"[GameController] Cannot show line - records null or index out of range");
                return;
            }

            var currentLine = currentChapterRecords[currentLineIndex];
            string lineText = currentLine.GetString("ParsedLine_ENG");
            Debug.Log($"[GameController] Displaying line {currentLineIndex}: {lineText}");

            if (dialogueUI == null)
            {
                Debug.LogError("[GameController] DialogueUI is NULL! Cannot display dialogue.");
                return;
            }

            // DialogueUI에 표시 (선택지 콜백 포함)
            dialogueUI.DisplayRecord(currentLine, OnDialogueAction);

            // CG 해금 체크
            if (currentLine.HasCG())
            {
                string cgId = currentLine.GetString("CG_ID");
                UnlockCG(cgId);
            }

            // 상태 업데이트
            if (currentLine.TryGetInt("ID", out int lineId))
            {
                currentState.currentLineId = lineId;
            }
        }

        /// <summary>
        /// DialogueUI에서 호출되는 액션 처리
        /// </summary>
        void OnDialogueAction(int choiceIndex)
        {
            if (choiceIndex == -1)
            {
                // 일반 진행 (Next 버튼)
                NextLine();
            }
            else
            {
                // 선택지 선택
                OnChoiceSelected(choiceIndex);
            }
        }

        /// <summary>
        /// 다음 라인으로 진행
        /// </summary>
        public void NextLine()
        {
            if (currentChapterRecords == null || currentLineIndex >= currentChapterRecords.Count - 1)
            {
                // 챕터 끝
                OnChapterEnd();
                return;
            }

            currentLineIndex++;
            ShowCurrentLine();
        }

        /// <summary>
        /// 선택지 선택 처리
        /// </summary>
        public void OnChoiceSelected(int choiceIndex)
        {
            var currentLine = currentChapterRecords[currentLineIndex];

            // 선택지 텍스트
            string choiceText = currentLine.GetString($"Choice{choiceIndex + 1}");
            currentState.previousChoices.Add(choiceText);

            // Value Impact 처리
            foreach (var value in projectData.coreValues)
            {
                string impactKey = $"Choice{choiceIndex + 1}_ValueImpact_{value.name}";
                if (currentLine.Has(impactKey) && currentLine.TryGetInt(impactKey, out int change))
                {
                    currentState.coreValueScores[value.name] += change;
                    Debug.Log($"{value.name} changed by {change} (now: {currentState.coreValueScores[value.name]})");
                }
            }

            // Skill Impact 처리 (파생 스킬)
            foreach (var value in projectData.coreValues)
            {
                if (value.derivedSkills != null)
                {
                    foreach (var skill in value.derivedSkills)
                    {
                        string skillImpactKey = $"Choice{choiceIndex + 1}_SkillImpact_{skill}";
                        if (currentLine.Has(skillImpactKey) && currentLine.TryGetInt(skillImpactKey, out int skillChange))
                        {
                            if (currentState.skillScores.ContainsKey(skill))
                            {
                                currentState.skillScores[skill] += skillChange;
                                Debug.Log($"Skill '{skill}' changed by {skillChange} (now: {currentState.skillScores[skill]})");
                            }
                        }
                    }
                }
            }

            // Affection Impact 처리 (AIDataConverter가 AffectionImpact_ 키로 저장)
            foreach (var npc in projectData.npcs)
            {
                string affectKey = $"Choice{choiceIndex + 1}_AffectionImpact_{npc.characterName}";
                if (currentLine.Has(affectKey) && currentLine.TryGetInt(affectKey, out int change))
                {
                    if (currentState.characterAffections.ContainsKey(npc.characterName))
                    {
                        currentState.characterAffections[npc.characterName] += change;
                        Debug.Log($"{npc.characterName} affection changed by {change} (now: {currentState.characterAffections[npc.characterName]})");
                    }
                }
            }

            // Next ID로 이동
            string nextIdKey = $"Next{choiceIndex + 1}";
            if (currentLine.TryGetInt(nextIdKey, out int nextId))
            {
                // 같은 챕터 내에서 점프
                for (int i = 0; i < currentChapterRecords.Count; i++)
                {
                    if (currentChapterRecords[i].TryGetInt("ID", out int lineId) && lineId == nextId)
                    {
                        currentLineIndex = i;
                        ShowCurrentLine();
                        return;
                    }
                }
            }

            // 찾지 못하면 다음 라인으로
            NextLine();
        }

        /// <summary>
        /// 챕터 종료 처리
        /// </summary>
        void OnChapterEnd()
        {
            Debug.Log($"Chapter {currentChapterId} completed");

            // 챕터 종료 시 자동 저장
            AutoSave();

            // 다음 챕터가 있으면 시작
            if (currentChapterId < projectData.totalChapters)
            {
                StartCoroutine(StartChapter(currentChapterId + 1));
            }
            else
            {
                // 게임 종료 - 엔딩 결정
                DetermineEnding();
            }
        }

        /// <summary>
        /// 엔딩 결정 및 표시
        /// </summary>
        void DetermineEnding()
        {
            Debug.Log("Game completed! Determining ending...");

            // EndingManager 초기화 (없으면 생성)
            if (endingManager == null)
            {
                endingManager = gameObject.AddComponent<EndingManager>();
                endingManager.projectData = projectData;
            }

            // 엔딩 판정
            EndingResult result = endingManager.DetermineEnding(currentState);

            // 엔딩 화면 표시
            DisplayEnding(result);
        }

        /// <summary>
        /// 엔딩 화면 표시
        /// </summary>
        void DisplayEnding(EndingResult result)
        {
            if (endingPanel == null)
            {
                Debug.LogError("[GameController] endingPanel is null! Cannot display ending.");
                return;
            }

            // 패널 활성화
            endingPanel.SetActive(true);

            // 제목 및 설명 표시
            if (endingTitleText != null)
                endingTitleText.text = result.endingTitle;

            if (endingDescriptionText != null)
                endingDescriptionText.text = result.endingDescription;

            // 최종 통계 표시
            if (endingStatsText != null)
            {
                string statsText = "[Final Stats]\n\n";
                statsText += "Core Values:\n";
                foreach (var kvp in currentState.coreValueScores)
                {
                    statsText += $"  {kvp.Key}: {kvp.Value}\n";
                }
                statsText += "\nAffection:\n";
                foreach (var kvp in currentState.characterAffections)
                {
                    statsText += $"  {kvp.Key}: {kvp.Value}\n";
                }
                endingStatsText.text = statsText;
            }

            // Romance Achievements 표시 - 간단한 텍스트로 변경
            if (romanceText != null)
            {
                if (result.romanceCharacters.Count > 0)
                {
                    string romanceNames = string.Join(", ", result.romanceCharacters);
                    romanceText.text = $"{romanceNames}과(와) 함께하게 됩니다.";
                    romanceText.gameObject.SetActive(true);
                }
                else
                {
                    romanceText.gameObject.SetActive(false);
                }
            }

            // 페이드인 효과 (CanvasGroup 사용)
            var canvasGroup = endingPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInEnding(canvasGroup));
            }

            Debug.Log($"[GameController] Ending displayed: {result.endingTitle}");
        }

        /// <summary>
        /// 엔딩 패널 페이드인
        /// </summary>
        IEnumerator FadeInEnding(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// CG 해금
        /// </summary>
        public void UnlockCG(string cgId)
        {
            if (!currentState.unlockedCGs.Contains(cgId))
            {
                currentState.unlockedCGs.Add(cgId);
                Debug.Log($"CG unlocked: {cgId}");
            }
        }

        /// <summary>
        /// 게임 저장
        /// </summary>
        public void SaveGame(int slotIndex, string saveName = null)
        {
            if (saveManager != null && currentState != null)
            {
                bool success = saveManager.SaveGame(currentState, slotIndex, saveName);
                if (success)
                {
                    Debug.Log($"Game saved to slot {slotIndex}: {saveName}");
                }
            }
        }

        /// <summary>
        /// 게임 로드
        /// </summary>
        public void LoadGame(GameStateSnapshot savedState)
        {
            if (savedState == null)
            {
                Debug.LogError("Cannot load: savedState is null");
                return;
            }

            currentState = savedState;
            StartCoroutine(StartChapter(savedState.currentChapter));
        }

        /// <summary>
        /// 자동 저장 (챕터 시작/종료 시)
        /// </summary>
        void AutoSave()
        {
            if (saveManager != null && currentState != null)
            {
                saveManager.AutoSave(currentState);
                Debug.Log("Auto save completed");
            }
        }
    }
}
