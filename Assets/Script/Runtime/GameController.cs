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

        [Header("Managers")]
        public ChapterGenerationManager chapterManager;
        public RuntimeSpriteManager spriteManager;
        public SaveDataManager saveManager;

        [Header("Current State")]
        public GameStateSnapshot currentState;
        public int currentChapterId = 1;
        public List<DialogueRecord> currentChapterRecords;
        public int currentLineIndex = 0;

        void Start()
        {
            // SaveDataManager에서 현재 로드된 SaveFile 정보 가져오기
            string currentSaveFileId = PlayerPrefs.GetString("CurrentSaveFileId", "");

            if (string.IsNullOrEmpty(currentSaveFileId))
            {
                Debug.LogError("No save file loaded! Returning to title...");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                return;
            }

            // SaveFile에서 프로젝트 GUID 찾기
            SaveFile saveFile = null;
            foreach (var project in SaveDataManager.Instance.GetAllProjectSlots())
            {
                saveFile = project.saveFiles.FirstOrDefault(s => s.saveFileId == currentSaveFileId);
                if (saveFile != null)
                    break;
            }

            if (saveFile == null)
            {
                Debug.LogError($"SaveFile not found: {currentSaveFileId}");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                return;
            }

            // 프로젝트 데이터 로드 (Resources에서)
            string projectGuid = saveFile.projectGuid;
            var allProjects = Resources.LoadAll<VNProjectData>("Projects");
            projectData = System.Array.Find(allProjects, p => p.projectGuid == projectGuid);

            if (projectData == null)
            {
                Debug.LogError($"Failed to load project data: {projectGuid}");
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                return;
            }

            Debug.Log($"Loaded project: {projectData.gameTitle}");

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

            Debug.Log($"Game state initialized: {currentState.coreValueScores.Count} core values, {currentState.characterAffections.Count} NPCs");
        }

        /// <summary>
        /// 챕터 시작
        /// </summary>
        public IEnumerator StartChapter(int chapterId)
        {
            Debug.Log($"Starting Chapter {chapterId}...");

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
            if (currentChapterRecords == null || currentLineIndex >= currentChapterRecords.Count)
            {
                return;
            }

            var currentLine = currentChapterRecords[currentLineIndex];

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

            // Affection Impact 처리
            foreach (var npc in projectData.npcs)
            {
                string affectKey = $"Choice{choiceIndex + 1}_Affection_{npc.characterName}";
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
        /// 엔딩 결정
        /// </summary>
        void DetermineEnding()
        {
            Debug.Log("Game completed! Determining ending...");

            // 엔딩 조건 체크
            foreach (var ending in projectData.endings)
            {
                bool meetsRequirements = true;

                // Core Value 요구치 확인
                if (ending.requiredValues != null)
                {
                    foreach (var req in ending.requiredValues)
                    {
                        if (!currentState.coreValueScores.ContainsKey(req.Key) ||
                            currentState.coreValueScores[req.Key] < req.Value)
                        {
                            meetsRequirements = false;
                            break;
                        }
                    }
                }

                // 캐릭터 호감도 요구치 확인
                if (meetsRequirements && ending.requiredAffections != null)
                {
                    foreach (var req in ending.requiredAffections)
                    {
                        if (!currentState.characterAffections.ContainsKey(req.Key) ||
                            currentState.characterAffections[req.Key] < req.Value)
                        {
                            meetsRequirements = false;
                            break;
                        }
                    }
                }

                if (meetsRequirements)
                {
                    Debug.Log($"Ending achieved: {ending.endingName}");
                    currentState.unlockedEndings.Add(ending.endingId);

                    // TODO: 엔딩 화면 표시
                    // ShowEnding(ending);
                    return;
                }
            }

            Debug.Log("No specific ending met - showing default ending");
            // TODO: 기본 엔딩 표시
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
