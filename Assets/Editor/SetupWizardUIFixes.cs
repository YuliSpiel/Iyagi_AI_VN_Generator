using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Setup Wizard UI 자동 수정 도구
/// - Step5 NPC 생성 패널의 Prev/Next 버튼 위치를 Step4와 동일하게 조정
/// - Loading Popup 자동 생성
/// </summary>
public class SetupWizardUIFixes : EditorWindow
{
    [MenuItem("Iyagi/Fix SetupWizard UI Issues")]
    public static void ShowWindow()
    {
        GetWindow<SetupWizardUIFixes>("Setup Wizard UI Fixes");
    }

    void OnGUI()
    {
        GUILayout.Label("Setup Wizard UI Fixes", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Fix Step5 Prev/Next Button Positions"))
        {
            FixStep5ButtonPositions();
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Step4 Player 생성 패널의 Prev/Next 버튼 위치를 참고하여\n" +
            "Step5 NPC 생성 패널의 버튼 위치를 동일하게 조정합니다.",
            MessageType.Info
        );

        GUILayout.Space(20);

        if (GUILayout.Button("2. Add Loading Popup to Step4"))
        {
            AddLoadingPopupToStep("Step4Panel");
        }

        GUILayout.Space(5);

        if (GUILayout.Button("3. Add Loading Popup to Step5"))
        {
            AddLoadingPopupToStep("Step5Panel");
        }

        GUILayout.Space(5);

        if (GUILayout.Button("3-1. Remove All Loading Popups"))
        {
            RemoveLoadingPopup("Step4_Panel");
            RemoveLoadingPopup("Step5_Panel");
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "각 Step 패널에 Loading Popup을 자동으로 생성하고\n" +
            "스크립트에 연결합니다.",
            MessageType.Info
        );

        GUILayout.Space(20);

        if (GUILayout.Button("4. Add Romanceable Toggle to Step5"))
        {
            AddRomanceableToggleToStep5();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("4-1. Remove Romanceable Toggle (Reset)"))
        {
            RemoveRomanceableToggleFromStep5();
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Step5 NPC 생성 패널에 isRomanceable 토글을 추가합니다.\n" +
            "PersonalityInput 아래에 추가됩니다.\n" +
            "제대로 안 보이면 Remove 후 다시 Add 하세요.",
            MessageType.Info
        );

        GUILayout.Space(20);

        if (GUILayout.Button("Apply All Fixes"))
        {
            FixStep5ButtonPositions();
            AddLoadingPopupToStep("Step4Panel");
            AddLoadingPopupToStep("Step5Panel");
            AddRomanceableToggleToStep5();
            Debug.Log("✅ All Setup Wizard UI fixes applied!");
        }
    }

    static void FixStep5ButtonPositions()
    {
        // SetupWizardScene 열기
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "SetupWizardScene")
        {
            Debug.LogError("Please open SetupWizardScene first!");
            return;
        }

        // Step4와 Step5의 버튼 찾기
        GameObject step4Panel = GameObject.Find("Step4_Panel");
        GameObject step5Panel = GameObject.Find("Step5_Panel");

        if (step4Panel == null || step5Panel == null)
        {
            Debug.LogError("Could not find Step4_Panel or Step5_Panel in scene!");
            return;
        }

        // Step4의 Previous/Next 버튼 찾기
        Button step4PrevButton = FindButtonInChildren(step4Panel, "PreviousButton");
        Button step4NextButton = FindButtonInChildren(step4Panel, "NextButton");

        // Step5의 Previous/Next 버튼 찾기
        Button step5PrevButton = FindButtonInChildren(step5Panel, "PreviousButton");
        Button step5NextButton = FindButtonInChildren(step5Panel, "NextButton");

        if (step4PrevButton == null || step4NextButton == null)
        {
            Debug.LogError("Could not find Previous/Next buttons in Step4Panel!");
            return;
        }

        if (step5PrevButton == null || step5NextButton == null)
        {
            Debug.LogError("Could not find Previous/Next buttons in Step5Panel!");
            return;
        }

        // Step4 버튼의 RectTransform 정보 가져오기
        RectTransform step4PrevRect = step4PrevButton.GetComponent<RectTransform>();
        RectTransform step4NextRect = step4NextButton.GetComponent<RectTransform>();

        // Step5 버튼에 동일한 위치 적용
        RectTransform step5PrevRect = step5PrevButton.GetComponent<RectTransform>();
        RectTransform step5NextRect = step5NextButton.GetComponent<RectTransform>();

        Undo.RecordObject(step5PrevRect, "Fix Step5 Previous Button Position");
        Undo.RecordObject(step5NextRect, "Fix Step5 Next Button Position");

        step5PrevRect.anchorMin = step4PrevRect.anchorMin;
        step5PrevRect.anchorMax = step4PrevRect.anchorMax;
        step5PrevRect.anchoredPosition = step4PrevRect.anchoredPosition;
        step5PrevRect.sizeDelta = step4PrevRect.sizeDelta;

        step5NextRect.anchorMin = step4NextRect.anchorMin;
        step5NextRect.anchorMax = step4NextRect.anchorMax;
        step5NextRect.anchoredPosition = step4NextRect.anchoredPosition;
        step5NextRect.sizeDelta = step4NextRect.sizeDelta;

        EditorUtility.SetDirty(step5PrevButton.gameObject);
        EditorUtility.SetDirty(step5NextButton.gameObject);

        Debug.Log("✅ Step5 Prev/Next button positions fixed to match Step4!");
    }

    static void AddLoadingPopupToStep(string stepPanelName)
    {
        // 현재 씬에서 Step 패널 찾기 (언더스코어 버전도 시도)
        GameObject stepPanel = GameObject.Find(stepPanelName);
        if (stepPanel == null)
        {
            // 언더스코어 없는 이름으로 시도했다면 언더스코어 버전 시도
            stepPanel = GameObject.Find(stepPanelName.Replace("Panel", "_Panel"));
        }

        if (stepPanel == null)
        {
            Debug.LogError($"Could not find {stepPanelName} in scene!");
            return;
        }

        // 기존 LoadingPopup이 있는지 확인
        Transform existingPopup = stepPanel.transform.Find("LoadingPopup");
        if (existingPopup != null)
        {
            Debug.Log($"{stepPanelName} already has LoadingPopup. Skipping.");
            return;
        }

        // LoadingPopup 생성
        GameObject loadingPopup = new GameObject("LoadingPopup");
        loadingPopup.transform.SetParent(stepPanel.transform, false);

        // 최상위 레이어로 이동 (마지막 자식으로)
        loadingPopup.transform.SetAsLastSibling();

        // RectTransform 설정 (전체 화면 덮기)
        RectTransform popupRect = loadingPopup.AddComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;

        // 반투명 배경 (클릭 막기)
        Image background = loadingPopup.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);

        // CanvasGroup 추가 (클릭 막기)
        CanvasGroup canvasGroup = loadingPopup.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;

        // 로딩 텍스트 생성
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(loadingPopup.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(400, 100);

        TMPro.TMP_Text text = textObj.AddComponent<TMPro.TMP_Text>();
        text.text = "생성 중...";
        text.fontSize = 36;
        text.color = Color.white;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.font = Resources.Load<TMPro.TMP_FontAsset>("TextMesh Pro/Fonts/NotoSansKR");

        // 초기에는 비활성화
        loadingPopup.SetActive(false);

        // Step 스크립트에 연결
        if (stepPanelName == "Step4Panel")
        {
            var step4Script = stepPanel.GetComponent<IyagiAI.SetupWizard.Step4_PlayerCharacter>();
            if (step4Script != null)
            {
                Undo.RecordObject(step4Script, "Connect Loading Popup to Step4");
                step4Script.loadingPopup = loadingPopup;
                step4Script.loadingText = text;
                EditorUtility.SetDirty(step4Script);
            }
        }
        else if (stepPanelName == "Step5Panel")
        {
            var step5Script = stepPanel.GetComponent<IyagiAI.SetupWizard.Step5_NPCs>();
            if (step5Script != null)
            {
                Undo.RecordObject(step5Script, "Connect Loading Popup to Step5");
                step5Script.loadingPopup = loadingPopup;
                step5Script.loadingText = text;
                EditorUtility.SetDirty(step5Script);
            }
        }

        EditorUtility.SetDirty(loadingPopup);
        Debug.Log($"✅ Loading Popup added to {stepPanelName}!");
    }

    static void AddRomanceableToggleToStep5()
    {
        // SetupWizardScene 열기
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "SetupWizardScene")
        {
            Debug.LogError("Please open SetupWizardScene first!");
            return;
        }

        // Step5Panel 찾기
        GameObject step5Panel = GameObject.Find("Step5_Panel");
        if (step5Panel == null)
        {
            Debug.LogError("Could not find Step5_Panel in scene!");
            return;
        }

        // 기존 RomanceableToggle이 있는지 확인
        Transform existingToggle = step5Panel.transform.Find("RomanceableToggle");
        if (existingToggle != null)
        {
            Debug.Log("RomanceableToggle already exists in Step5Panel. Skipping.");
            return;
        }

        // PersonalityInput 찾기 (이 아래에 토글을 추가할 것)
        TMPro.TMP_InputField personalityInput = null;
        Transform personalityTransform = null;

        TMPro.TMP_InputField[] inputFields = step5Panel.GetComponentsInChildren<TMPro.TMP_InputField>(true);
        foreach (var field in inputFields)
        {
            if (field.gameObject.name.Contains("Personality"))
            {
                personalityInput = field;
                personalityTransform = field.transform;
                break;
            }
        }

        if (personalityInput == null)
        {
            Debug.LogError("Could not find PersonalityInput in Step5Panel!");
            return;
        }

        Debug.Log($"Found PersonalityInput: {personalityTransform.name}, Parent: {personalityTransform.parent.name}");

        // PersonalityInput의 부모 찾기 (InputsContainer 등)
        Transform container = personalityTransform.parent;

        // RomanceableToggle 생성
        GameObject toggleObj = new GameObject("RomanceableToggle");
        toggleObj.transform.SetParent(container, false);

        // PersonalityInput 다음으로 배치
        int personalityIndex = personalityTransform.GetSiblingIndex();
        toggleObj.transform.SetSiblingIndex(personalityIndex + 1);

        // RectTransform 설정 - 절대 위치 대신 LayoutElement 사용
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();

        // 고정 크기 설정
        toggleRect.anchorMin = new Vector2(0, 1);
        toggleRect.anchorMax = new Vector2(1, 1);
        toggleRect.pivot = new Vector2(0.5f, 1);
        toggleRect.anchoredPosition = Vector2.zero;
        toggleRect.sizeDelta = new Vector2(0, 40); // 높이 40

        // LayoutElement 추가 (부모가 VerticalLayoutGroup일 경우를 대비)
        var layoutElement = toggleObj.AddComponent<UnityEngine.UI.LayoutElement>();
        layoutElement.minHeight = 40;
        layoutElement.preferredHeight = 40;

        // Toggle 컴포넌트 추가
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = false;

        // Background (체크박스 배경)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.anchoredPosition = new Vector2(15, 0);
        bgRect.sizeDelta = new Vector2(20, 20);

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.white;

        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform, false);

        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.6f, 1f); // 파란색 체크

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(toggleObj.transform, false);

        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.anchoredPosition = new Vector2(20, 0);
        labelRect.offsetMin = new Vector2(40, 0);
        labelRect.offsetMax = new Vector2(0, 0);

        TMPro.TMP_Text labelText = label.AddComponent<TMPro.TMP_Text>();
        labelText.text = "연애 가능 NPC (Romanceable)";
        labelText.fontSize = 18;
        labelText.color = Color.white;
        labelText.alignment = TMPro.TextAlignmentOptions.Left;
        labelText.font = Resources.Load<TMPro.TMP_FontAsset>("TextMesh Pro/Fonts/NotoSansKR");

        // Toggle 컴포넌트 설정
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;

        // Step5_NPCs 스크립트에 연결
        var step5Script = step5Panel.GetComponent<IyagiAI.SetupWizard.Step5_NPCs>();
        if (step5Script != null)
        {
            Undo.RecordObject(step5Script, "Add Romanceable Toggle");
            step5Script.romanceableToggle = toggle;
            EditorUtility.SetDirty(step5Script);
        }

        EditorUtility.SetDirty(toggleObj);
        Debug.Log("✅ Romanceable Toggle added to Step5Panel!");
    }

    static void RemoveLoadingPopup(string stepPanelName)
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "SetupWizardScene")
        {
            Debug.LogError("Please open SetupWizardScene first!");
            return;
        }

        GameObject stepPanel = GameObject.Find(stepPanelName);
        if (stepPanel == null)
        {
            Debug.LogWarning($"Could not find {stepPanelName} in scene!");
            return;
        }

        // LoadingPopup 찾아서 삭제
        Transform[] allChildren = stepPanel.GetComponentsInChildren<Transform>(true);
        foreach (var child in allChildren)
        {
            if (child.name == "LoadingPopup")
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                Debug.Log($"✅ LoadingPopup removed from {stepPanelName}!");
                return;
            }
        }

        Debug.LogWarning($"LoadingPopup not found in {stepPanelName}.");
    }

    static void RemoveRomanceableToggleFromStep5()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "SetupWizardScene")
        {
            Debug.LogError("Please open SetupWizardScene first!");
            return;
        }

        GameObject step5Panel = GameObject.Find("Step5_Panel");
        if (step5Panel == null)
        {
            Debug.LogError("Could not find Step5_Panel in scene!");
            return;
        }

        // 기존 RomanceableToggle 찾아서 삭제
        Transform existingToggle = step5Panel.transform.Find("RomanceableToggle");
        if (existingToggle == null)
        {
            // 모든 자식에서 재귀적으로 찾기
            Transform[] allChildren = step5Panel.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                if (child.name == "RomanceableToggle")
                {
                    existingToggle = child;
                    break;
                }
            }
        }

        if (existingToggle != null)
        {
            Undo.DestroyObjectImmediate(existingToggle.gameObject);
            Debug.Log("✅ RomanceableToggle removed from Step5Panel!");
        }
        else
        {
            Debug.LogWarning("RomanceableToggle not found in Step5Panel.");
        }
    }

    static Button FindButtonInChildren(GameObject parent, string buttonName)
    {
        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            if (btn.gameObject.name.Contains(buttonName))
            {
                return btn;
            }
        }
        return null;
    }
}
