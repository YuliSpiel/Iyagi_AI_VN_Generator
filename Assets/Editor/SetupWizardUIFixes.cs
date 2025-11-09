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
        EditorGUILayout.HelpBox(
            "각 Step 패널에 Loading Popup을 자동으로 생성하고\n" +
            "스크립트에 연결합니다.",
            MessageType.Info
        );

        GUILayout.Space(20);

        if (GUILayout.Button("Apply All Fixes"))
        {
            FixStep5ButtonPositions();
            AddLoadingPopupToStep("Step4Panel");
            AddLoadingPopupToStep("Step5Panel");
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
        GameObject step4Panel = GameObject.Find("Step4Panel");
        GameObject step5Panel = GameObject.Find("Step5Panel");

        if (step4Panel == null || step5Panel == null)
        {
            Debug.LogError("Could not find Step4Panel or Step5Panel in scene!");
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
        // 현재 씬에서 Step 패널 찾기
        GameObject stepPanel = GameObject.Find(stepPanelName);
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
