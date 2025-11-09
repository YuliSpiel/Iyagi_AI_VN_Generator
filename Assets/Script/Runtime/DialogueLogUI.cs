using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 대화 기록 UI
    /// 이전 대화 내용 표시 및 스크롤
    /// </summary>
    public class DialogueLogUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject logPanel;
        public ScrollRect scrollRect;
        public Transform logContent; // 로그 엔트리들이 추가될 컨테이너
        public GameObject logEntryPrefab; // DialogueLogEntry Prefab

        [Header("Buttons")]
        public Button closeButton;
        public Button clearButton;

        [Header("Settings")]
        public int maxLogEntries = 100; // 최대 로그 수 (메모리 절약)

        private List<DialogueLogEntry> logEntries = new List<DialogueLogEntry>();
        private List<LogData> logHistory = new List<LogData>();

        void Start()
        {
            // 버튼 이벤트
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseLog);

            if (clearButton != null)
                clearButton.onClick.AddListener(ClearLog);

            // 초기 상태
            logPanel.SetActive(false);
        }

        /// <summary>
        /// 대화 로그 열기
        /// </summary>
        public void OpenLog()
        {
            logPanel.SetActive(true);

            // 스크롤을 맨 아래로 (최신 대화)
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// 대화 로그 닫기
        /// </summary>
        public void CloseLog()
        {
            logPanel.SetActive(false);
        }

        /// <summary>
        /// 대화 기록 추가
        /// </summary>
        public void AddDialogue(string speaker, string dialogue, string characterName = null)
        {
            // 로그 데이터 생성
            LogData logData = new LogData
            {
                speaker = speaker,
                dialogue = dialogue,
                characterName = characterName,
                timestamp = System.DateTime.Now
            };

            logHistory.Add(logData);

            // 최대 개수 초과 시 오래된 것 제거
            if (logHistory.Count > maxLogEntries)
            {
                logHistory.RemoveAt(0);

                // UI 엔트리도 제거
                if (logEntries.Count > 0)
                {
                    var oldEntry = logEntries[0];
                    logEntries.RemoveAt(0);
                    if (oldEntry != null)
                        Destroy(oldEntry.gameObject);
                }
            }

            // UI 엔트리 생성
            CreateLogEntry(logData);
        }

        /// <summary>
        /// 선택지 기록 추가
        /// </summary>
        public void AddChoice(string choiceText)
        {
            LogData logData = new LogData
            {
                speaker = "[Choice]",
                dialogue = $"▶ {choiceText}",
                isChoice = true,
                timestamp = System.DateTime.Now
            };

            logHistory.Add(logData);

            if (logHistory.Count > maxLogEntries)
            {
                logHistory.RemoveAt(0);

                if (logEntries.Count > 0)
                {
                    var oldEntry = logEntries[0];
                    logEntries.RemoveAt(0);
                    if (oldEntry != null)
                        Destroy(oldEntry.gameObject);
                }
            }

            CreateLogEntry(logData);
        }

        /// <summary>
        /// 로그 엔트리 UI 생성
        /// </summary>
        void CreateLogEntry(LogData logData)
        {
            if (logEntryPrefab == null || logContent == null)
                return;

            GameObject entryObj = Instantiate(logEntryPrefab, logContent);
            DialogueLogEntry entry = entryObj.GetComponent<DialogueLogEntry>();

            if (entry != null)
            {
                entry.Initialize(logData);
                logEntries.Add(entry);
            }
        }

        /// <summary>
        /// 로그 전체 삭제
        /// </summary>
        void ClearLog()
        {
            // 확인 다이얼로그 (TODO)
            Debug.Log("Clearing dialogue log...");

            // 기록 삭제
            logHistory.Clear();

            // UI 엔트리 삭제
            foreach (var entry in logEntries)
            {
                if (entry != null)
                    Destroy(entry.gameObject);
            }
            logEntries.Clear();
        }

        /// <summary>
        /// 현재 챕터의 로그만 표시 (옵션)
        /// </summary>
        public void FilterByChapter(int chapterId)
        {
            // TODO: 챕터별 필터링 구현
            Debug.Log($"Filtering log by chapter {chapterId}");
        }
    }

    /// <summary>
    /// 로그 데이터
    /// </summary>
    [System.Serializable]
    public class LogData
    {
        public string speaker;
        public string dialogue;
        public string characterName;
        public bool isChoice;
        public System.DateTime timestamp;
    }

    /// <summary>
    /// 대화 로그 엔트리 UI
    /// </summary>
    public class DialogueLogEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_Text speakerText;
        public TMP_Text dialogueText;
        public Image characterIcon; // 옵션: 캐릭터 아이콘
        public GameObject choiceMarker; // 선택지 표시용

        private LogData logData;

        public void Initialize(LogData data)
        {
            logData = data;

            // Speaker 표시
            if (speakerText != null)
            {
                speakerText.text = data.speaker;

                // 선택지는 다른 색상
                if (data.isChoice)
                {
                    speakerText.color = new Color(1f, 0.8f, 0.2f); // 황금색
                }
            }

            // Dialogue 표시
            if (dialogueText != null)
            {
                dialogueText.text = data.dialogue;

                if (data.isChoice)
                {
                    dialogueText.fontStyle = FontStyles.Bold;
                }
            }

            // 선택지 마커
            if (choiceMarker != null)
            {
                choiceMarker.SetActive(data.isChoice);
            }

            // 캐릭터 아이콘 (옵션)
            if (characterIcon != null && !string.IsNullOrEmpty(data.characterName))
            {
                LoadCharacterIcon(data.characterName);
            }
        }

        void LoadCharacterIcon(string characterName)
        {
            // 캐릭터 얼굴 프리뷰 로드
            Sprite icon = Resources.Load<Sprite>($"Generated/Characters/{characterName}/face_preview");

            if (icon != null)
            {
                characterIcon.sprite = icon;
                characterIcon.gameObject.SetActive(true);
            }
            else
            {
                characterIcon.gameObject.SetActive(false);
            }
        }
    }
}
