using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// LLM 씬을 빠르게 설정하기 위한 Unity Editor 메뉴
/// GameObject > Iyagi VN Toolkit 메뉴에서 접근 가능
/// </summary>
public class LLMSetupMenus
{
    [MenuItem("GameObject/Iyagi VN Toolkit/Create LLM Game System", false, 10)]
    static void CreateLLMGameSystem()
    {
        // LLMGameSystem GameObject 생성
        GameObject llmSystem = new GameObject("LLMGameSystem");

        // 컴포넌트 추가
        llmSystem.AddComponent<LLMStoryGenerator>();
        llmSystem.AddComponent<DynamicDialogueBuilder>();
        llmSystem.AddComponent<LLMGameController>();

        // 기본 설정
        var generator = llmSystem.GetComponent<LLMStoryGenerator>();
        if (generator != null)
        {
            generator.model = "gpt-4";
            generator.temperature = 0.8f;
            generator.maxTokens = 1000;

            // 기본 리소스 설정
            generator.availableCharacters.Add("Hans");
            generator.availableCharacters.Add("Heilner");

            generator.availableBackgrounds.Add("riverside");
            generator.availableBackgrounds.Add("council");
            generator.availableBackgrounds.Add("black");

            generator.availableLooks.Add("Normal_Normal");
        }

        Selection.activeGameObject = llmSystem;
        Debug.Log("[LLM Setup] LLMGameSystem created! Don't forget to set your API Key in the LLMStoryGenerator component.");
    }

    [MenuItem("GameObject/Iyagi VN Toolkit/Create Input Panel UI", false, 11)]
    static void CreateInputPanelUI()
    {
        // Canvas 찾기 또는 생성
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem 생성
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // InputPanel 생성
        GameObject inputPanel = new GameObject("InputPanel");
        inputPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = inputPanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        RectTransform panelRect = inputPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(800, 400);
        panelRect.anchoredPosition = Vector2.zero;

        // 제목
        GameObject title = new GameObject("TitleText");
        title.transform.SetParent(inputPanel.transform, false);

        TMP_Text titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "LLM Story Generator";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(700, 80);
        titleRect.anchoredPosition = new Vector2(0, -50);

        // 프롬프트 입력창
        GameObject inputField = new GameObject("PromptInputField");
        inputField.transform.SetParent(inputPanel.transform, false);

        Image inputBg = inputField.AddComponent<Image>();
        inputBg.color = new Color(1f, 1f, 1f, 0.1f);

        TMP_InputField inputComponent = inputField.AddComponent<TMP_InputField>();
        inputComponent.lineType = TMP_InputField.LineType.MultiLineSubmit;

        RectTransform inputRect = inputField.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(700, 150);
        inputRect.anchoredPosition = new Vector2(0, 10);

        // 텍스트 에리어
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputField.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = Vector2.zero;
        textAreaRect.offsetMin = new Vector2(10, 10);
        textAreaRect.offsetMax = new Vector2(-10, -10);
        textArea.AddComponent<RectMask2D>();

        GameObject inputText = new GameObject("Text");
        inputText.transform.SetParent(textArea.transform, false);
        TMP_Text inputTextComponent = inputText.AddComponent<TextMeshProUGUI>();
        inputTextComponent.fontSize = 24;
        inputTextComponent.color = Color.white;
        RectTransform inputTextRect = inputText.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.sizeDelta = Vector2.zero;

        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        TMP_Text placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Enter your story prompt...";
        placeholderText.fontSize = 24;
        placeholderText.color = new Color(1f, 1f, 1f, 0.5f);
        placeholderText.fontStyle = FontStyles.Italic;
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;

        inputComponent.textComponent = inputTextComponent;
        inputComponent.placeholder = placeholderText;

        // 생성 버튼
        GameObject button = new GameObject("GenerateButton");
        button.transform.SetParent(inputPanel.transform, false);

        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.8f, 1f);

        Button buttonComponent = button.AddComponent<Button>();

        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.sizeDelta = new Vector2(300, 60);
        buttonRect.anchoredPosition = new Vector2(0, 100);

        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(button.transform, false);
        TMP_Text buttonTextComponent = buttonText.AddComponent<TextMeshProUGUI>();
        buttonTextComponent.text = "Generate Story";
        buttonTextComponent.fontSize = 32;
        buttonTextComponent.alignment = TextAlignmentOptions.Center;
        buttonTextComponent.color = Color.white;
        RectTransform buttonTextRect = buttonText.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        // 상태 텍스트
        GameObject status = new GameObject("StatusText");
        status.transform.SetParent(inputPanel.transform, false);

        TMP_Text statusText = status.AddComponent<TextMeshProUGUI>();
        statusText.text = "Ready to generate...";
        statusText.fontSize = 20;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = new Color(1f, 1f, 0.5f, 1f);

        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0f);
        statusRect.anchorMax = new Vector2(0.5f, 0f);
        statusRect.sizeDelta = new Vector2(700, 40);
        statusRect.anchoredPosition = new Vector2(0, 40);

        Selection.activeGameObject = inputPanel;
        Debug.Log("[LLM Setup] Input Panel UI created! Connect it to LLMGameController.");
    }

    [MenuItem("GameObject/Iyagi VN Toolkit/Find Existing DialogueUI", false, 12)]
    static void FindExistingDialogueUI()
    {
        DialogueUI dialogueUI = Object.FindObjectOfType<DialogueUI>();

        if (dialogueUI == null)
        {
            EditorUtility.DisplayDialog("DialogueUI Not Found",
                "No DialogueUI component found in the scene.\n\n" +
                "Please open a game scene that has DialogueUI, or create a new one manually.",
                "OK");
            return;
        }

        // DialogueUI 선택
        Selection.activeGameObject = dialogueUI.gameObject;

        EditorUtility.DisplayDialog("DialogueUI Found",
            $"Found DialogueUI on '{dialogueUI.gameObject.name}'.\n\n" +
            "Make sure to connect this to LLMGameController's 'Dialogue UI' field.\n\n" +
            "Required components:\n" +
            "- dialogueText\n" +
            "- speakerText\n" +
            "- choice1/2/3 buttons\n" +
            "- StandingImg1/2\n" +
            "- BGImage",
            "OK");
    }

    [MenuItem("GameObject/Iyagi VN Toolkit/Auto-Connect LLM Components", false, 13)]
    static void AutoConnectComponents()
    {
        LLMGameController controller = Object.FindObjectOfType<LLMGameController>();

        if (controller == null)
        {
            EditorUtility.DisplayDialog("LLMGameController Not Found",
                "No LLMGameController found in the scene.\n\n" +
                "Please create LLM Game System first using:\n" +
                "GameObject > Iyagi VN Toolkit > Create LLM Game System",
                "OK");
            return;
        }

        bool changed = false;

        // LLM 컴포넌트 자동 연결
        if (controller.storyGenerator == null)
        {
            controller.storyGenerator = controller.GetComponent<LLMStoryGenerator>();
            if (controller.storyGenerator != null) changed = true;
        }

        if (controller.dialogueBuilder == null)
        {
            controller.dialogueBuilder = controller.GetComponent<DynamicDialogueBuilder>();
            if (controller.dialogueBuilder != null) changed = true;
        }

        // DialogueUI 찾기
        if (controller.dialogueUI == null)
        {
            controller.dialogueUI = Object.FindObjectOfType<DialogueUI>();
            if (controller.dialogueUI != null) changed = true;
        }

        // Input Panel 자동 찾기
        if (controller.inputPanel == null)
        {
            GameObject inputPanel = GameObject.Find("InputPanel");
            if (inputPanel != null)
            {
                controller.inputPanel = inputPanel;
                changed = true;

                // 하위 컴포넌트도 자동 연결
                if (controller.promptInputField == null)
                {
                    controller.promptInputField = inputPanel.GetComponentInChildren<TMP_InputField>();
                    if (controller.promptInputField != null) changed = true;
                }

                Transform buttonTransform = inputPanel.transform.Find("GenerateButton");
                if (buttonTransform != null && controller.generateButton == null)
                {
                    controller.generateButton = buttonTransform.GetComponent<Button>();
                    if (controller.generateButton != null) changed = true;
                }

                Transform statusTransform = inputPanel.transform.Find("StatusText");
                if (statusTransform != null && controller.statusText == null)
                {
                    controller.statusText = statusTransform.GetComponent<TMP_Text>();
                    if (controller.statusText != null) changed = true;
                }
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(controller);
            Debug.Log("[LLM Setup] Auto-connected components to LLMGameController.");

            Selection.activeGameObject = controller.gameObject;

            EditorUtility.DisplayDialog("Auto-Connect Complete",
                "Successfully connected available components to LLMGameController.\n\n" +
                "Please check the Inspector and manually connect any missing references.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Changes",
                "All components are already connected or could not be found automatically.\n\n" +
                "Please connect them manually in the Inspector.",
                "OK");
        }
    }
}
