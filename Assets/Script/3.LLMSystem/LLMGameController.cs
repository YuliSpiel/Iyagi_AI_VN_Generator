using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LLM 기반 실시간 스토리 생성 및 렌더링을 관리하는 컨트롤러
/// 기존 DialogueSystem과 유사하지만 동적으로 스토리를 생성합니다.
/// </summary>
public class LLMGameController : MonoBehaviour
{
    [Header("LLM Components")]
    public LLMStoryGenerator storyGenerator;
    public DynamicDialogueBuilder dialogueBuilder;

    [Header("UI Components")]
    public DialogueUI dialogueUI;

    [Header("Input UI")]
    public GameObject inputPanel;
    public TMP_InputField promptInputField;
    public Button generateButton;
    public TMP_Text statusText;

    [Header("Settings")]
    [Tooltip("스토리 생성 시 이전 컨텍스트를 포함할 대사 개수")]
    public int contextHistoryCount = 5;

    // 상태 관리
    private List<DialogueRecord> currentStory = new List<DialogueRecord>();
    private int currentIndex = 0;
    private bool isGenerating = false;
    private bool isPlaying = false;

    // 컨텍스트 히스토리
    private List<string> contextHistory = new List<string>();

    private void Start()
    {
        // 버튼 이벤트 연결
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
        }

        // 초기 상태
        ShowInputPanel();
        UpdateStatusText("Enter a story prompt to begin...");

        // DialogueUI 초기화
        if (dialogueUI != null)
        {
            dialogueUI.HideChoices();
        }
    }

    private void Update()
    {
        // 스토리 재생 중일 때만 입력 처리
        if (!isPlaying || isGenerating) return;

        // 스페이스/엔터로 다음 대사
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            // 타이핑 중이면 스킵
            if (dialogueUI != null && dialogueUI.IsTyping)
            {
                dialogueUI.SkipTyping();
                return;
            }

            // 선택지가 있으면 진행 금지
            if (dialogueUI != null && dialogueUI.HasActiveChoices())
            {
                return;
            }

            // 다음 대사
            ProceedNext();
        }

        // 숫자키로 선택지 선택
        if (dialogueUI != null && dialogueUI.HasActiveChoices())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnChoiceSelected(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) OnChoiceSelected(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) OnChoiceSelected(2);
        }
    }

    // ────────────────────── 입력 UI ──────────────────────

    private void ShowInputPanel()
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(true);
        }
        isPlaying = false;
    }

    private void HideInputPanel()
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(false);
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[LLMGameController] {message}");
    }

    // ────────────────────── 스토리 생성 ──────────────────────

    private void OnGenerateButtonClicked()
    {
        if (isGenerating) return;

        string prompt = promptInputField != null ? promptInputField.text : "";

        if (string.IsNullOrEmpty(prompt))
        {
            UpdateStatusText("Please enter a story prompt!");
            return;
        }

        StartCoroutine(GenerateAndPlayStory(prompt));
    }

    private IEnumerator GenerateAndPlayStory(string userPrompt)
    {
        isGenerating = true;
        UpdateStatusText("Generating story with LLM...");

        if (generateButton != null)
        {
            generateButton.interactable = false;
        }

        // 이전 컨텍스트 구성
        string context = BuildContext();

        bool success = false;
        string storyJson = null;

        // LLM에 스토리 생성 요청
        storyGenerator.GenerateStory(
            userPrompt,
            context,
            onSuccess: (json) =>
            {
                storyJson = json;
                success = true;
            },
            onError: (error) =>
            {
                UpdateStatusText($"Error: {error}");
                success = false;
            }
        );

        // 응답 대기
        float timeout = 30f;
        float elapsed = 0f;
        while (!success && storyJson == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (success && !string.IsNullOrEmpty(storyJson))
        {
            // JSON을 DialogueRecord로 변환
            List<DialogueRecord> newRecords = dialogueBuilder.BuildFromJson(storyJson);

            if (newRecords != null && newRecords.Count > 0)
            {
                currentStory = newRecords;
                currentIndex = 0;

                // 컨텍스트에 추가
                AddToContext(userPrompt, newRecords);

                // 스토리 재생 시작
                UpdateStatusText($"Story generated! {newRecords.Count} dialogue entries created.");
                HideInputPanel();
                isPlaying = true;
                ShowCurrentDialogue();
            }
            else
            {
                UpdateStatusText("Failed to build dialogue from LLM response.");
            }
        }
        else
        {
            UpdateStatusText("Story generation timed out or failed.");
        }

        isGenerating = false;

        if (generateButton != null)
        {
            generateButton.interactable = true;
        }
    }

    private string BuildContext()
    {
        if (contextHistory.Count == 0) return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Previous story segments:");

        int start = Mathf.Max(0, contextHistory.Count - contextHistoryCount);
        for (int i = start; i < contextHistory.Count; i++)
        {
            sb.AppendLine(contextHistory[i]);
        }

        return sb.ToString();
    }

    private void AddToContext(string prompt, List<DialogueRecord> records)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"User: {prompt}");
        sb.AppendLine("Generated dialogues:");

        foreach (var record in records)
        {
            string speaker = record.Get("NameTag");
            string line = record.Get("Line_ENG");
            if (!string.IsNullOrEmpty(speaker) && !string.IsNullOrEmpty(line))
            {
                sb.AppendLine($"{speaker}: {line}");
            }
        }

        contextHistory.Add(sb.ToString());
    }

    // ────────────────────── 스토리 재생 ──────────────────────

    private void ShowCurrentDialogue()
    {
        if (currentIndex < 0 || currentIndex >= currentStory.Count)
        {
            OnStoryEnd();
            return;
        }

        DialogueRecord record = currentStory[currentIndex];

        if (dialogueUI != null)
        {
            // 선택지 숨김
            dialogueUI.HideChoices();

            // 화자/본문
            string speaker = record.GetSpeakerByLang();
            string text = record.GetTextByLang(GameManager.Instance.CurrentLanguage);

            dialogueUI.ShowLine(speaker, text);

            // 스탠딩, 배경
            dialogueUI.ApplyStanding(record);
            dialogueUI.ApplyBackground(record);

            // 선택지가 있는지 확인
            if (HasChoices(record))
            {
                StartCoroutine(ShowChoicesWhenTypingDone(record));
            }
        }
    }

    private IEnumerator ShowChoicesWhenTypingDone(DialogueRecord record)
    {
        // 타이핑이 끝날 때까지 대기
        while (dialogueUI != null && dialogueUI.IsTyping)
        {
            yield return null;
        }

        // 선택지 표시
        if (dialogueUI != null)
        {
            ShowChoices(record);
        }
    }

    private bool HasChoices(DialogueRecord record)
    {
        var lang = GameManager.Instance.CurrentLanguage;

        for (int i = 1; i <= 3; i++)
        {
            string choiceText = record.GetChoiceText(i, lang);
            if (!string.IsNullOrEmpty(choiceText))
            {
                return true;
            }
        }

        return false;
    }

    private void ShowChoices(DialogueRecord record)
    {
        var lang = GameManager.Instance.CurrentLanguage;

        string choice1Text = record.GetChoiceText(1, lang);
        string choice2Text = record.GetChoiceText(2, lang);
        string choice3Text = record.GetChoiceText(3, lang);

        if (dialogueUI != null)
        {
            dialogueUI.SetupChoices(
                choice1Text,
                choice2Text,
                choice3Text,
                () => OnChoiceSelected(0),
                () => OnChoiceSelected(1),
                () => OnChoiceSelected(2)
            );
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"[LLMGameController] Choice {choiceIndex + 1} selected");

        // 선택지 숨김
        if (dialogueUI != null)
        {
            dialogueUI.HideChoices();
        }

        // 선택지 선택 후 다음 대사로 진행
        // 현재는 단순히 다음 대사로 이동하지만,
        // 향후 선택지에 따라 새로운 스토리를 생성할 수 있습니다.
        ProceedNext();
    }

    private void ProceedNext()
    {
        currentIndex++;

        if (currentIndex < currentStory.Count)
        {
            ShowCurrentDialogue();
        }
        else
        {
            OnStoryEnd();
        }
    }

    private void OnStoryEnd()
    {
        UpdateStatusText("Story segment ended. Enter a new prompt to continue...");
        ShowInputPanel();
        isPlaying = false;

        if (dialogueUI != null)
        {
            dialogueUI.HideChoices();
        }

        // 입력 필드 초기화
        if (promptInputField != null)
        {
            promptInputField.text = "";
        }
    }

    // ────────────────────── 공개 메서드 ──────────────────────

    /// <summary>
    /// 외부에서 직접 프롬프트로 스토리 생성
    /// </summary>
    public void GenerateStoryFromPrompt(string prompt)
    {
        if (promptInputField != null)
        {
            promptInputField.text = prompt;
        }
        OnGenerateButtonClicked();
    }

    /// <summary>
    /// 컨텍스트 히스토리 초기화
    /// </summary>
    public void ClearContext()
    {
        contextHistory.Clear();
        UpdateStatusText("Context cleared. Starting fresh...");
    }

    /// <summary>
    /// 현재 재생중인 스토리 초기화
    /// </summary>
    public void ResetStory()
    {
        currentStory.Clear();
        currentIndex = 0;
        isPlaying = false;
        ShowInputPanel();
    }
}
