using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using IyagiAI.Runtime;
using UnityEngine.EventSystems;

/// <summary>
/// GameScene 자동 설정 헬퍼
/// 필요한 UI 요소들을 자동으로 생성하고 연결
/// </summary>
public class GameSceneSetupHelper : EditorWindow
{
    private static TMP_FontAsset notoSansKRFont;

    [MenuItem("Iyagi/Setup Game Scene")]
    public static void SetupGameScene()
    {
        // NotoSansKR 폰트 로드
        LoadFont();

        // 새 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // EventSystem 생성
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cameraObj.AddComponent<AudioListener>();

        // Canvas 생성
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // GameController 생성
        GameObject gameControllerObj = new GameObject("GameController");
        GameController gameController = gameControllerObj.AddComponent<GameController>();

        // DialogueUI 패널 생성
        GameObject dialoguePanel = CreateDialogueUI(canvasObj.transform);
        DialogueUI dialogueUI = dialoguePanel.GetComponent<DialogueUI>();

        // GameController에 DialogueUI 연결
        SerializedObject serializedController = new SerializedObject(gameController);
        serializedController.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
        serializedController.ApplyModifiedProperties();

        // SaveDataManager (DontDestroyOnLoad)
        if (FindObjectOfType<SaveDataManager>() == null)
        {
            GameObject saveManager = new GameObject("SaveDataManager");
            saveManager.AddComponent<SaveDataManager>();
        }

        // RuntimeSpriteManager (DontDestroyOnLoad)
        if (FindObjectOfType<RuntimeSpriteManager>() == null)
        {
            GameObject spriteManager = new GameObject("RuntimeSpriteManager");
            spriteManager.AddComponent<RuntimeSpriteManager>();
        }

        // 씬 저장
        string scenePath = "Assets/Scenes/GameScene.unity";
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        }

        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log("✅ GameScene created successfully!");
        Debug.Log($"Scene saved at: {scenePath}");
    }

    static GameObject CreateDialogueUI(Transform parent)
    {
        // DialogueUI 패널
        GameObject panel = new GameObject("DialogueUI");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        DialogueUI dialogueUI = panel.AddComponent<DialogueUI>();

        // 배경 이미지 (전체 화면)
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(panel.transform, false);
        RectTransform backgroundRect = backgroundObj.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundObj.transform.SetAsFirstSibling(); // 맨 뒤로
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = Color.black; // 기본 검은색

        // 캐릭터 이미지 슬롯 (Left, Right, Center)
        GameObject leftCharObj = CreateCharacterSlot("LeftCharacter", panel.transform, new Vector2(0.2f, 0.5f));
        Image leftCharImage = leftCharObj.GetComponent<Image>();
        CanvasGroup leftCharGroup = leftCharObj.GetComponent<CanvasGroup>();

        GameObject rightCharObj = CreateCharacterSlot("RightCharacter", panel.transform, new Vector2(0.8f, 0.5f));
        Image rightCharImage = rightCharObj.GetComponent<Image>();
        CanvasGroup rightCharGroup = rightCharObj.GetComponent<CanvasGroup>();

        GameObject centerCharObj = CreateCharacterSlot("CenterCharacter", panel.transform, new Vector2(0.5f, 0.5f));
        Image centerCharImage = centerCharObj.GetComponent<Image>();
        CanvasGroup centerCharGroup = centerCharObj.GetComponent<CanvasGroup>();

        // 대화창 배경 이미지 (하단)
        GameObject dialogueBoxObj = new GameObject("DialogueBox");
        dialogueBoxObj.transform.SetParent(panel.transform, false);
        RectTransform dialogueBoxRect = dialogueBoxObj.AddComponent<RectTransform>();
        dialogueBoxRect.anchorMin = new Vector2(0, 0);
        dialogueBoxRect.anchorMax = new Vector2(1, 0.3f);
        dialogueBoxRect.offsetMin = Vector2.zero;
        dialogueBoxRect.offsetMax = Vector2.zero;
        Image dialogueBoxImage = dialogueBoxObj.AddComponent<Image>();
        dialogueBoxImage.color = new Color(0, 0, 0, 0.8f);
        CanvasGroup dialogueBoxGroup = dialogueBoxObj.AddComponent<CanvasGroup>();

        // 스피커 이름
        GameObject speakerObj = CreateText("SpeakerName", panel.transform);
        RectTransform speakerRect = speakerObj.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0.05f, 0.25f);
        speakerRect.anchorMax = new Vector2(0.3f, 0.28f);
        speakerRect.offsetMin = Vector2.zero;
        speakerRect.offsetMax = Vector2.zero;
        TMP_Text speakerText = speakerObj.GetComponent<TMP_Text>();
        speakerText.fontSize = 28;
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.alignment = TextAlignmentOptions.Left;

        // 대사 텍스트
        GameObject dialogueTextObj = CreateText("DialogueText", panel.transform);
        RectTransform dialogueRect = dialogueTextObj.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0.05f, 0.05f);
        dialogueRect.anchorMax = new Vector2(0.95f, 0.24f);
        dialogueRect.offsetMin = Vector2.zero;
        dialogueRect.offsetMax = Vector2.zero;
        TMP_Text dialogueText = dialogueTextObj.GetComponent<TMP_Text>();
        dialogueText.fontSize = 24;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;

        // Next 버튼
        GameObject nextBtn = CreateButton("NextButton", panel.transform, "▼");
        RectTransform nextRect = nextBtn.GetComponent<RectTransform>();
        nextRect.anchorMin = new Vector2(0.92f, 0.02f);
        nextRect.anchorMax = new Vector2(0.98f, 0.06f);
        nextRect.offsetMin = Vector2.zero;
        nextRect.offsetMax = Vector2.zero;

        // Auto 버튼
        GameObject autoBtn = CreateButton("AutoButton", panel.transform, "AUTO");
        RectTransform autoRect = autoBtn.GetComponent<RectTransform>();
        autoRect.anchorMin = new Vector2(0.85f, 0.02f);
        autoRect.anchorMax = new Vector2(0.91f, 0.06f);
        autoRect.offsetMin = Vector2.zero;
        autoRect.offsetMax = Vector2.zero;

        // Skip 버튼 (선택적)
        GameObject skipBtn = CreateButton("SkipButton", panel.transform, "SKIP");
        RectTransform skipRect = skipBtn.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0.78f, 0.02f);
        skipRect.anchorMax = new Vector2(0.84f, 0.06f);
        skipRect.offsetMin = Vector2.zero;
        skipRect.offsetMax = Vector2.zero;

        // Log 버튼 (선택적)
        GameObject logBtn = CreateButton("LogButton", panel.transform, "LOG");
        RectTransform logRect = logBtn.GetComponent<RectTransform>();
        logRect.anchorMin = new Vector2(0.02f, 0.02f);
        logRect.anchorMax = new Vector2(0.08f, 0.06f);
        logRect.offsetMin = Vector2.zero;
        logRect.offsetMax = Vector2.zero;

        // Choice Panel (선택지 패널)
        GameObject choicePanel = new GameObject("ChoicePanel");
        choicePanel.transform.SetParent(panel.transform, false);
        RectTransform choicePanelRect = choicePanel.AddComponent<RectTransform>();
        choicePanelRect.anchorMin = Vector2.zero;
        choicePanelRect.anchorMax = Vector2.one;
        choicePanelRect.offsetMin = Vector2.zero;
        choicePanelRect.offsetMax = Vector2.zero;
        choicePanel.SetActive(false); // 초기에는 숨김

        // Choice 버튼들 (4개)
        Button[] choiceButtons = new Button[4];
        TMP_Text[] choiceTexts = new TMP_Text[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject choiceBtn = CreateButton($"ChoiceButton{i + 1}", choicePanel.transform, $"선택지 {i + 1}");
            RectTransform choiceRect = choiceBtn.GetComponent<RectTransform>();
            float yPos = 0.5f + (1.5f - i) * 0.1f;
            choiceRect.anchorMin = new Vector2(0.2f, yPos - 0.05f);
            choiceRect.anchorMax = new Vector2(0.8f, yPos);
            choiceRect.offsetMin = Vector2.zero;
            choiceRect.offsetMax = Vector2.zero;
            choiceButtons[i] = choiceBtn.GetComponent<Button>();
            choiceTexts[i] = choiceBtn.GetComponentInChildren<TMP_Text>();
        }

        // CG 이미지
        GameObject cgObj = new GameObject("CGImage");
        cgObj.transform.SetParent(panel.transform, false);
        RectTransform cgRect = cgObj.AddComponent<RectTransform>();
        cgRect.anchorMin = Vector2.zero;
        cgRect.anchorMax = Vector2.one;
        cgRect.offsetMin = Vector2.zero;
        cgRect.offsetMax = Vector2.zero;
        cgObj.transform.SetSiblingIndex(1); // 배경 다음, 캐릭터 앞
        Image cgImage = cgObj.AddComponent<Image>();
        cgImage.color = Color.white;
        CanvasGroup cgGroup = cgObj.AddComponent<CanvasGroup>();
        cgGroup.alpha = 0f;
        cgObj.SetActive(false);

        // DialogueUI 필드 연결
        SerializedObject serializedUI = new SerializedObject(dialogueUI);

        // Character Display
        serializedUI.FindProperty("leftCharacterImage").objectReferenceValue = leftCharImage;
        serializedUI.FindProperty("rightCharacterImage").objectReferenceValue = rightCharImage;
        serializedUI.FindProperty("centerCharacterImage").objectReferenceValue = centerCharImage;
        serializedUI.FindProperty("leftCharacterGroup").objectReferenceValue = leftCharGroup;
        serializedUI.FindProperty("rightCharacterGroup").objectReferenceValue = rightCharGroup;
        serializedUI.FindProperty("centerCharacterGroup").objectReferenceValue = centerCharGroup;

        // Dialogue Display
        serializedUI.FindProperty("speakerNameText").objectReferenceValue = speakerText;
        serializedUI.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        serializedUI.FindProperty("dialogueBox").objectReferenceValue = dialogueBoxImage;
        serializedUI.FindProperty("dialogueBoxGroup").objectReferenceValue = dialogueBoxGroup;

        // CG Display
        serializedUI.FindProperty("cgImage").objectReferenceValue = cgImage;
        serializedUI.FindProperty("cgGroup").objectReferenceValue = cgGroup;

        // Background
        serializedUI.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;

        // Choice Display
        serializedUI.FindProperty("choicePanel").objectReferenceValue = choicePanel;
        SerializedProperty choiceButtonsProp = serializedUI.FindProperty("choiceButtons");
        choiceButtonsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            choiceButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtons[i];
        }
        SerializedProperty choiceTextsProp = serializedUI.FindProperty("choiceTexts");
        choiceTextsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            choiceTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceTexts[i];
        }

        // Control Buttons
        serializedUI.FindProperty("nextButton").objectReferenceValue = nextBtn.GetComponent<Button>();
        serializedUI.FindProperty("autoButton").objectReferenceValue = autoBtn.GetComponent<Button>();
        serializedUI.FindProperty("skipButton").objectReferenceValue = skipBtn.GetComponent<Button>();
        serializedUI.FindProperty("logButton").objectReferenceValue = logBtn.GetComponent<Button>();

        serializedUI.ApplyModifiedProperties();

        EditorUtility.SetDirty(dialogueUI);

        return panel;
    }

    static GameObject CreateCharacterSlot(string name, Transform parent, Vector2 anchorPosition)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorPosition;
        rt.anchorMax = anchorPosition;
        rt.sizeDelta = new Vector2(400, 800); // 캐릭터 스프라이트 크기
        rt.pivot = new Vector2(0.5f, 0f); // 하단 중심

        Image image = obj.AddComponent<Image>();
        image.color = Color.white;
        image.preserveAspect = true;

        CanvasGroup group = obj.AddComponent<CanvasGroup>();
        group.alpha = 0f; // 초기에는 숨김

        return obj;
    }

    static GameObject CreateText(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.fontSize = 24;

        // NotoSansKR 폰트 할당
        if (notoSansKRFont != null)
        {
            text.font = notoSansKRFont;
        }

        return obj;
    }

    static GameObject CreateButton(string name, Transform parent, string labelText)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = labelText;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 20;

        // NotoSansKR 폰트 할당
        if (notoSansKRFont != null)
        {
            text.font = notoSansKRFont;
        }

        return buttonObj;
    }

    static void LoadFont()
    {
        string fontPath = "Assets/TextMesh Pro/Fonts/NotoSansKR";
        notoSansKRFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath + ".asset");

        if (notoSansKRFont == null)
        {
            Debug.LogWarning($"Font asset not found at {fontPath}.asset");
        }
        else
        {
            Debug.Log("✅ NotoSansKR font loaded successfully");
        }
    }
}
