using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.AISystem;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 엔딩 시스템 테스트용 컨트롤러
    /// TestChapter1.json을 로드하여 1챕터만 플레이 → 엔딩 판정
    /// </summary>
    public class EndingTestController : MonoBehaviour
    {
        [Header("Test Project")]
        public VNProjectData testProject; // Drag: EndingTestProject.asset

        [Header("UI")]
        public TMP_Text dialogueText;
        public TMP_Text speakerText;
        public GameObject choicePanel;
        public Button[] choiceButtons;
        public TMP_Text[] choiceTexts;

        [Header("Ending Display")]
        public GameObject endingPanel;
        public TMP_Text endingTitleText;
        public TMP_Text endingDescriptionText;
        public TMP_Text endingStatsText;
        public TMP_Text romanceText;

        [Header("Debug")]
        public TMP_Text debugText;

        private GameStateSnapshot currentState;
        private List<DialogueRecord> currentChapter;
        private int currentLineIndex = 0;

        void Start()
        {
            if (testProject == null)
            {
                Debug.LogError("[EndingTestController] testProject is null! Assign EndingTestProject.asset");
                return;
            }

            InitializeGameState();
            LoadTestChapter();
        }

        void InitializeGameState()
        {
            currentState = new GameStateSnapshot();
            currentState.currentChapter = 1;
            currentState.currentLineId = 1000;

            // Core Values 초기화
            currentState.coreValueScores = new Dictionary<string, int>();
            foreach (var coreValue in testProject.coreValues)
            {
                currentState.coreValueScores[coreValue.name] = 0;
            }

            // Affection 초기화
            currentState.characterAffections = new Dictionary<string, int>();
            foreach (var npc in testProject.npcs)
            {
                currentState.characterAffections[npc.characterName] = npc.initialAffection;
            }

            currentState.previousChoices = new List<string>();

            Debug.Log("[EndingTestController] Game state initialized");
            UpdateDebugText();
        }

        void LoadTestChapter()
        {
            // TestChapter1.json 로드
            TextAsset jsonFile = Resources.Load<TextAsset>("TestChapter1");

            if (jsonFile == null)
            {
                Debug.LogError("[EndingTestController] TestChapter1.json not found! Run 'Iyagi > Create Ending Test Project' first.");
                return;
            }

            // JSON → DialogueRecord 변환
            currentChapter = AIDataConverter.FromAIJson(jsonFile.text, 1);

            if (currentChapter == null || currentChapter.Count == 0)
            {
                Debug.LogError("[EndingTestController] Failed to parse TestChapter1.json");
                return;
            }

            Debug.Log($"[EndingTestController] Loaded {currentChapter.Count} dialogue lines");

            // 첫 라인 표시
            DisplayCurrentLine();
        }

        void DisplayCurrentLine()
        {
            if (currentLineIndex >= currentChapter.Count)
            {
                // 챕터 종료 → 엔딩 판정
                EndChapter();
                return;
            }

            DialogueRecord record = currentChapter[currentLineIndex];

            // 대사 표시
            speakerText.text = record.Get("NameTag");
            dialogueText.text = record.Get("ParsedLine_ENG");

            // 선택지 체크
            string choice1 = record.Get("Choice1_ENG");

            if (!string.IsNullOrEmpty(choice1))
            {
                // 선택지 표시
                DisplayChoices(record);
            }
            else
            {
                // 선택지 없음 → 다음 라인으로
                choicePanel.SetActive(false);
            }

            Debug.Log($"[Line {currentLineIndex}] {record.Get("NameTag")}: {record.Get("ParsedLine_ENG")}");
        }

        void DisplayChoices(DialogueRecord record)
        {
            choicePanel.SetActive(true);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int choiceNum = i + 1;
                string choiceText = record.Get($"Choice{choiceNum}_ENG");

                if (!string.IsNullOrEmpty(choiceText))
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceTexts[i].text = choiceText;

                    int capturedIndex = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(capturedIndex, record));
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        void OnChoiceClicked(int choiceIndex, DialogueRecord record)
        {
            int choiceNum = choiceIndex + 1;
            string choiceText = record.Get($"Choice{choiceNum}_ENG");

            Debug.Log($"[Choice Selected] {choiceText}");

            // 1. Core Value 업데이트
            foreach (var kvp in currentState.coreValueScores.Keys.ToArray())
            {
                string valueKey = $"Choice{choiceNum}_ValueImpact_{kvp}";
                string valueStr = record.Get(valueKey);

                if (!string.IsNullOrEmpty(valueStr))
                {
                    int change = int.Parse(valueStr);
                    currentState.coreValueScores[kvp] += change;

                    Debug.Log($"  → {kvp}: {currentState.coreValueScores[kvp] - change} + {change} = {currentState.coreValueScores[kvp]}");
                }
            }

            // 2. Affection 업데이트
            foreach (var npc in testProject.npcs)
            {
                string affectionKey = $"Choice{choiceNum}_AffectionImpact_{npc.characterName}";
                string affectionStr = record.Get(affectionKey);

                if (!string.IsNullOrEmpty(affectionStr))
                {
                    int change = int.Parse(affectionStr);
                    currentState.characterAffections[npc.characterName] += change;

                    Debug.Log($"  → {npc.characterName} Affection: {currentState.characterAffections[npc.characterName] - change} + {change} = {currentState.characterAffections[npc.characterName]}");
                }
            }

            // 3. 선택 기록
            currentState.previousChoices.Add(choiceText);

            // 4. 디버그 텍스트 업데이트
            UpdateDebugText();

            // 5. 다음 라인으로
            currentLineIndex++;
            DisplayCurrentLine();
        }

        public void OnNextButtonClicked()
        {
            currentLineIndex++;
            DisplayCurrentLine();
        }

        void EndChapter()
        {
            Debug.Log("[EndingTestController] Chapter finished! Determining ending...");

            // EndingManager로 엔딩 판정
            EndingManager endingManager = gameObject.AddComponent<EndingManager>();
            endingManager.projectData = testProject;

            EndingResult result = endingManager.DetermineEnding(currentState);

            // 엔딩 표시
            DisplayEnding(result);
        }

        void DisplayEnding(EndingResult result)
        {
            endingPanel.SetActive(true);

            // 메인 엔딩
            endingTitleText.text = result.endingTitle;
            endingDescriptionText.text = result.endingDescription;

            // 최종 스탯 표시
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

            // Romance Achievements - 간단한 텍스트로 표시
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

            Debug.Log($"[Ending] {result.endingType} - {result.endingTitle}");
            Debug.Log($"[Romance Achievements] {string.Join(", ", result.romanceCharacters)}");
        }

        void UpdateDebugText()
        {
            string debug = "[Current State]\n\n";
            debug += "Core Values:\n";
            foreach (var kvp in currentState.coreValueScores)
            {
                debug += $"  {kvp.Key}: {kvp.Value}\n";
            }
            debug += "\nAffection:\n";
            foreach (var kvp in currentState.characterAffections)
            {
                debug += $"  {kvp.Key}: {kvp.Value}\n";
            }

            debugText.text = debug;
        }
    }
}
