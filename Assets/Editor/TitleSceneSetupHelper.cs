using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using IyagiAI.TitleScene;

public class TitleSceneSetupHelper : EditorWindow
{
    private static TMP_FontAsset notoSansKRFont;

    [MenuItem("Iyagi/Setup Title Scene")]
    public static void SetupTitleScene()
    {
        // NotoSansKR 폰트 로드
        LoadFont();

        // 새 씬 생성 (에디터 모드)
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Canvas 생성
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem 생성
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // TitleSceneManager 생성
        GameObject managerObj = new GameObject("TitleSceneManager");
        TitleSceneManager manager = managerObj.AddComponent<TitleSceneManager>();

        // 패널들 생성
        GameObject titlePanel = CreateTitlePanel(canvas.transform, manager);
        GameObject projectSelectPanel = CreateProjectSelectPanel(canvas.transform, manager);
        GameObject saveFileSelectPanel = CreateSaveFileSelectPanel(canvas.transform, manager);
        GameObject cgCollectionPanel = CreateCGCollectionPanel(canvas.transform, manager);

        // Manager에 패널 연결
        manager.titlePanel = titlePanel;
        manager.projectSelectPanel = projectSelectPanel;
        manager.saveFileSelectPanel = saveFileSelectPanel;
        manager.cgCollectionPanel = cgCollectionPanel;

        // 씬 저장
        string scenePath = "Assets/Scenes/TitleScene.unity";
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        }

        EditorSceneManager.SaveScene(scene, scenePath);

        Debug.Log("✅ TitleScene created successfully!");
        Debug.Log($"Scene saved at: {scenePath}");
    }

    // ===== TitlePanel =====
    static GameObject CreateTitlePanel(Transform parent, TitleSceneManager manager)
    {
        GameObject panel = CreatePanel("TitlePanel", parent, true);
        TitlePanel titlePanel = panel.AddComponent<TitlePanel>();
        titlePanel.sceneManager = manager;

        // 타이틀 텍스트
        GameObject titleTextObj = CreateTextMeshPro("TitleText", panel.transform);
        titlePanel.titleText = titleTextObj.GetComponent<TMP_Text>();
        titlePanel.titleText.text = "Iyagi AI VN Generator";
        titlePanel.titleText.fontSize = 72;
        titlePanel.titleText.alignment = TextAlignmentOptions.Center;
        SetPosition(titleTextObj.GetComponent<RectTransform>(), 0.1f, 0.6f, 0.9f, 0.9f);

        // Continue 버튼
        titlePanel.continueButton = CreateButton("ContinueButton", panel.transform, "Continue").GetComponent<Button>();
        SetPosition(titlePanel.continueButton.GetComponent<RectTransform>(), 0.3f, 0.45f, 0.7f, 0.55f);

        // Load Game 버튼
        titlePanel.loadGameButton = CreateButton("LoadGameButton", panel.transform, "Load Game").GetComponent<Button>();
        SetPosition(titlePanel.loadGameButton.GetComponent<RectTransform>(), 0.3f, 0.35f, 0.7f, 0.45f);

        // New Game 버튼
        titlePanel.newGameButton = CreateButton("NewGameButton", panel.transform, "New Game").GetComponent<Button>();
        SetPosition(titlePanel.newGameButton.GetComponent<RectTransform>(), 0.3f, 0.25f, 0.7f, 0.35f);

        return panel;
    }

    // ===== ProjectSelectPanel =====
    static GameObject CreateProjectSelectPanel(Transform parent, TitleSceneManager manager)
    {
        GameObject panel = CreatePanel("ProjectSelectPanel", parent, false);
        ProjectSelectPanel projectSelectPanel = panel.AddComponent<ProjectSelectPanel>();
        projectSelectPanel.sceneManager = manager;

        // 타이틀
        GameObject titleTextObj = CreateTextMeshPro("Title", panel.transform);
        TMP_Text titleText = titleTextObj.GetComponent<TMP_Text>();
        titleText.text = "Select Project";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        SetPosition(titleTextObj.GetComponent<RectTransform>(), 0.1f, 0.85f, 0.9f, 0.95f);

        // ScrollView 생성
        GameObject scrollView = CreateScrollView("ProjectScrollView", panel.transform, 0.1f, 0.15f, 0.9f, 0.8f);
        Transform content = scrollView.transform.Find("Viewport/Content");
        projectSelectPanel.projectListContainer = content;

        // GridLayoutGroup 설정
        GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(350, 150);
        grid.spacing = new Vector2(20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        // Back 버튼
        projectSelectPanel.backButton = CreateButton("BackButton", panel.transform, "Back").GetComponent<Button>();
        SetPosition(projectSelectPanel.backButton.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.25f, 0.12f);

        // ProjectItem Prefab 생성 (나중에 사용)
        GameObject prefab = CreateProjectItemPrefab();
        projectSelectPanel.projectItemPrefab = prefab;

        return panel;
    }

    // ===== SaveFileSelectPanel =====
    static GameObject CreateSaveFileSelectPanel(Transform parent, TitleSceneManager manager)
    {
        GameObject panel = CreatePanel("SaveFileSelectPanel", parent, false);
        SaveFileSelectPanel saveFilePanel = panel.AddComponent<SaveFileSelectPanel>();
        saveFilePanel.sceneManager = manager;

        // 프로젝트 이름
        GameObject projectNameObj = CreateTextMeshPro("ProjectName", panel.transform);
        saveFilePanel.projectNameText = projectNameObj.GetComponent<TMP_Text>();
        saveFilePanel.projectNameText.fontSize = 48;
        saveFilePanel.projectNameText.alignment = TextAlignmentOptions.Center;
        SetPosition(projectNameObj.GetComponent<RectTransform>(), 0.1f, 0.85f, 0.9f, 0.95f);

        // ScrollView
        GameObject scrollView = CreateScrollView("SaveFileScrollView", panel.transform, 0.1f, 0.2f, 0.9f, 0.8f);
        Transform content = scrollView.transform.Find("Viewport/Content");
        saveFilePanel.saveFileListContainer = content;

        VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // New Save 버튼
        saveFilePanel.newSaveButton = CreateButton("NewSaveButton", panel.transform, "New Save").GetComponent<Button>();
        SetPosition(saveFilePanel.newSaveButton.GetComponent<RectTransform>(), 0.55f, 0.05f, 0.75f, 0.15f);

        // Back 버튼
        saveFilePanel.backButton = CreateButton("BackButton", panel.transform, "Back").GetComponent<Button>();
        SetPosition(saveFilePanel.backButton.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.25f, 0.15f);

        // New Save Popup 생성
        GameObject popup = CreateNewSavePopup(panel.transform, saveFilePanel);
        saveFilePanel.newSavePopup = popup;

        // SaveFileItem Prefab 생성
        GameObject prefab = CreateSaveFileItemPrefab();
        saveFilePanel.saveFileItemPrefab = prefab;

        return panel;
    }

    // ===== CGCollectionPanel =====
    static GameObject CreateCGCollectionPanel(Transform parent, TitleSceneManager manager)
    {
        GameObject panel = CreatePanel("CGCollectionPanel", parent, false);
        CGCollectionPanel cgPanel = panel.AddComponent<CGCollectionPanel>();
        cgPanel.sceneManager = manager;

        // 프로젝트 이름
        GameObject projectNameObj = CreateTextMeshPro("ProjectName", panel.transform);
        cgPanel.projectNameText = projectNameObj.GetComponent<TMP_Text>();
        cgPanel.projectNameText.fontSize = 48;
        cgPanel.projectNameText.alignment = TextAlignmentOptions.Center;
        SetPosition(projectNameObj.GetComponent<RectTransform>(), 0.1f, 0.88f, 0.9f, 0.95f);

        // 진행도 텍스트
        GameObject progressObj = CreateTextMeshPro("Progress", panel.transform);
        cgPanel.collectionProgressText = progressObj.GetComponent<TMP_Text>();
        cgPanel.collectionProgressText.fontSize = 24;
        cgPanel.collectionProgressText.alignment = TextAlignmentOptions.Center;
        SetPosition(progressObj.GetComponent<RectTransform>(), 0.1f, 0.82f, 0.9f, 0.88f);

        // ScrollView
        GameObject scrollView = CreateScrollView("CGScrollView", panel.transform, 0.05f, 0.15f, 0.95f, 0.8f);
        Transform content = scrollView.transform.Find("Viewport/Content");
        cgPanel.cgGridContainer = content;

        GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 200);
        grid.spacing = new Vector2(15, 15);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // Back 버튼
        cgPanel.backButton = CreateButton("BackButton", panel.transform, "Back").GetComponent<Button>();
        SetPosition(cgPanel.backButton.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.25f, 0.12f);

        // CG Viewer Panel
        GameObject viewer = CreateCGViewerPanel(panel.transform, cgPanel);
        cgPanel.cgViewerPanel = viewer;

        // CG Thumbnail Prefab
        GameObject prefab = CreateCGThumbnailPrefab();
        cgPanel.cgThumbnailPrefab = prefab;

        return panel;
    }

    // ===== Helper Methods =====

    static GameObject CreatePanel(string name, Transform parent, bool active)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        panel.SetActive(active);

        return panel;
    }

    static GameObject CreateTextMeshPro(string name, Transform parent)
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

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.3f, 0.5f, 1f);

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
        text.fontSize = 28;

        // NotoSansKR 폰트 할당
        if (notoSansKRFont != null)
        {
            text.font = notoSansKRFont;
        }

        return buttonObj;
    }

    static GameObject CreateScrollView(string name, Transform parent, float left, float bottom, float right, float top)
    {
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent, false);

        RectTransform rt = scrollView.AddComponent<RectTransform>();
        SetPosition(rt, left, bottom, right, top);

        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        Image scrollImage = scrollView.AddComponent<Image>();
        scrollImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);

        RectTransform viewportRt = viewport.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 1000);

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRt;
        scrollRect.viewport = viewportRt;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        return scrollView;
    }

    static GameObject CreateNewSavePopup(Transform parent, SaveFileSelectPanel savePanel)
    {
        GameObject popup = CreatePanel("NewSavePopup", parent, false);
        popup.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // 팝업 컨테이너
        GameObject container = new GameObject("Container");
        container.transform.SetParent(popup.transform, false);

        RectTransform containerRt = container.AddComponent<RectTransform>();
        SetPosition(containerRt, 0.25f, 0.3f, 0.75f, 0.7f);

        Image containerImage = container.AddComponent<Image>();
        containerImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 타이틀
        GameObject titleObj = CreateTextMeshPro("Title", container.transform);
        TMP_Text titleText = titleObj.GetComponent<TMP_Text>();
        titleText.text = "Start from which chapter?";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        SetPosition(titleObj.GetComponent<RectTransform>(), 0.1f, 0.7f, 0.9f, 0.9f);

        // Dropdown
        GameObject dropdownObj = CreateDropdown("ChapterDropdown", container.transform, 0.2f, 0.45f, 0.8f, 0.65f);
        savePanel.startChapterDropdown = dropdownObj.GetComponent<TMP_Dropdown>();

        // Confirm 버튼
        savePanel.confirmNewSaveButton = CreateButton("ConfirmButton", container.transform, "Start").GetComponent<Button>();
        SetPosition(savePanel.confirmNewSaveButton.GetComponent<RectTransform>(), 0.55f, 0.15f, 0.85f, 0.35f);

        // Cancel 버튼
        savePanel.cancelNewSaveButton = CreateButton("CancelButton", container.transform, "Cancel").GetComponent<Button>();
        SetPosition(savePanel.cancelNewSaveButton.GetComponent<RectTransform>(), 0.15f, 0.15f, 0.45f, 0.35f);

        return popup;
    }

    static GameObject CreateCGViewerPanel(Transform parent, CGCollectionPanel cgPanel)
    {
        GameObject viewer = CreatePanel("CGViewerPanel", parent, false);
        viewer.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);

        // CG Image
        GameObject imageObj = new GameObject("CGImage");
        imageObj.transform.SetParent(viewer.transform, false);

        RectTransform imageRt = imageObj.AddComponent<RectTransform>();
        SetPosition(imageRt, 0.1f, 0.15f, 0.9f, 0.85f);

        cgPanel.cgFullImage = imageObj.AddComponent<Image>();
        cgPanel.cgFullImage.preserveAspect = true;

        // Description
        GameObject descObj = CreateTextMeshPro("Description", viewer.transform);
        cgPanel.cgDescriptionText = descObj.GetComponent<TMP_Text>();
        cgPanel.cgDescriptionText.fontSize = 24;
        cgPanel.cgDescriptionText.alignment = TextAlignmentOptions.Center;
        SetPosition(descObj.GetComponent<RectTransform>(), 0.1f, 0.05f, 0.9f, 0.12f);

        // Close 버튼
        cgPanel.closeCGViewerButton = CreateButton("CloseButton", viewer.transform, "Close").GetComponent<Button>();
        SetPosition(cgPanel.closeCGViewerButton.GetComponent<RectTransform>(), 0.4f, 0.88f, 0.6f, 0.95f);

        return viewer;
    }

    static GameObject CreateDropdown(string name, Transform parent, float left, float bottom, float right, float top)
    {
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);

        RectTransform rt = dropdownObj.AddComponent<RectTransform>();
        SetPosition(rt, left, bottom, right, top);

        Image image = dropdownObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

        // Template (간단하게)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);

        RectTransform templateRt = template.AddComponent<RectTransform>();
        templateRt.anchorMin = new Vector2(0, 0);
        templateRt.anchorMax = new Vector2(1, 0);
        templateRt.pivot = new Vector2(0.5f, 1);
        templateRt.sizeDelta = new Vector2(0, 150);

        template.SetActive(false);

        return dropdownObj;
    }

    static GameObject CreateProjectItemPrefab()
    {
        // TODO: Prefab 생성 로직
        return new GameObject("ProjectItemPrefab");
    }

    static GameObject CreateSaveFileItemPrefab()
    {
        // TODO: Prefab 생성 로직
        return new GameObject("SaveFileItemPrefab");
    }

    static GameObject CreateCGThumbnailPrefab()
    {
        // TODO: Prefab 생성 로직
        return new GameObject("CGThumbnailPrefab");
    }

    static void SetPosition(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = new Vector2(left, bottom);
        rt.anchorMax = new Vector2(right, top);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void LoadFont()
    {
        string fontPath = "Assets/TextMesh Pro/Fonts/NotoSansKR";
        notoSansKRFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath + ".asset");

        if (notoSansKRFont == null)
        {
            Debug.LogError($"❌ Font asset not found at {fontPath}.asset");
            return;
        }

        // 폰트를 Dynamic 모드로 설정
        if (notoSansKRFont.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            notoSansKRFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(notoSansKRFont);
            AssetDatabase.SaveAssets();
            Debug.Log("✅ Set font atlas to Dynamic mode for Korean characters");
        }

        Debug.Log("✅ NotoSansKR font loaded successfully");
    }
}
