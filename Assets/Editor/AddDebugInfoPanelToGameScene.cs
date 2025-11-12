using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;

namespace IyagiAI.Editor
{
    /// <summary>
    /// GameScene에 디버그 정보 패널을 자동으로 추가하는 에디터 툴
    /// </summary>
    public static class AddDebugInfoPanelToGameScene
    {
        [MenuItem("Iyagi/Add Debug Info Panel to GameScene")]
        public static void AddDebugInfoPanel()
        {
            // Canvas 찾기
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found in scene!");
                return;
            }

            // 기존 DebugInfoPanel 삭제
            var existingPanel = GameObject.Find("DebugInfoPanel");
            if (existingPanel != null)
            {
                GameObject.DestroyImmediate(existingPanel);
                Debug.Log("Removed existing DebugInfoPanel");
            }

            // ===== 1. DebugInfoPanel 생성 (우상단) =====
            GameObject debugPanel = new GameObject("DebugInfoPanel");
            debugPanel.transform.SetParent(canvas.transform, false);

            // RectTransform 설정 (우상단, 250x120)
            RectTransform panelRect = debugPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.anchoredPosition = new Vector2(-20, -20); // 우상단에서 약간 안쪽
            panelRect.sizeDelta = new Vector2(250, 120);

            // CanvasGroup 추가 (표시/숨김용)
            CanvasGroup canvasGroup = debugPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // 배경 이미지 (반투명 검정)
            Image panelBg = debugPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.7f);

            // Vertical Layout
            VerticalLayoutGroup layout = debugPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 15, 15);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // ===== 2. ChapterText 생성 =====
            GameObject chapterTextObj = new GameObject("ChapterText");
            chapterTextObj.transform.SetParent(debugPanel.transform, false);

            RectTransform chapterRect = chapterTextObj.AddComponent<RectTransform>();
            chapterRect.sizeDelta = new Vector2(0, 30);

            TextMeshProUGUI chapterText = chapterTextObj.AddComponent<TextMeshProUGUI>();
            chapterText.text = "Chapter 1";
            chapterText.fontSize = 20;
            chapterText.color = Color.white;
            chapterText.alignment = TextAlignmentOptions.Left;
            chapterText.font = LoadKoreanFont();

            // ===== 3. LineProgress 수평 레이아웃 =====
            GameObject lineProgressPanel = new GameObject("LineProgressPanel");
            lineProgressPanel.transform.SetParent(debugPanel.transform, false);

            RectTransform lineProgressRect = lineProgressPanel.AddComponent<RectTransform>();
            lineProgressRect.sizeDelta = new Vector2(0, 25);

            HorizontalLayoutGroup lineProgressLayout = lineProgressPanel.AddComponent<HorizontalLayoutGroup>();
            lineProgressLayout.childAlignment = TextAnchor.MiddleLeft;
            lineProgressLayout.spacing = 5;
            lineProgressLayout.childControlWidth = false;
            lineProgressLayout.childControlHeight = false;
            lineProgressLayout.childForceExpandWidth = false;
            lineProgressLayout.childForceExpandHeight = false;

            // LineProgressText (현재 라인)
            GameObject lineProgressTextObj = new GameObject("LineProgressText");
            lineProgressTextObj.transform.SetParent(lineProgressPanel.transform, false);

            RectTransform lineProgressTextRect = lineProgressTextObj.AddComponent<RectTransform>();
            lineProgressTextRect.sizeDelta = new Vector2(80, 25);

            TextMeshProUGUI lineProgressText = lineProgressTextObj.AddComponent<TextMeshProUGUI>();
            lineProgressText.text = "Line 5";
            lineProgressText.fontSize = 18;
            lineProgressText.color = Color.yellow;
            lineProgressText.alignment = TextAlignmentOptions.Left;
            lineProgressText.font = LoadKoreanFont();

            // TotalLinesText (총 라인 수)
            GameObject totalLinesTextObj = new GameObject("TotalLinesText");
            totalLinesTextObj.transform.SetParent(lineProgressPanel.transform, false);

            RectTransform totalLinesTextRect = totalLinesTextObj.AddComponent<RectTransform>();
            totalLinesTextRect.sizeDelta = new Vector2(60, 25);

            TextMeshProUGUI totalLinesText = totalLinesTextObj.AddComponent<TextMeshProUGUI>();
            totalLinesText.text = "/ 30";
            totalLinesText.fontSize = 18;
            totalLinesText.color = Color.gray;
            totalLinesText.alignment = TextAlignmentOptions.Left;
            totalLinesText.font = LoadKoreanFont();

            // ===== 4. VisibilityToggle 생성 =====
            GameObject toggleObj = new GameObject("VisibilityToggle");
            toggleObj.transform.SetParent(debugPanel.transform, false);

            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(0, 25);

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = true;

            // Toggle Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);

            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.sizeDelta = new Vector2(40, 0);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Checkmark
            GameObject checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(bgObj.transform, false);

            RectTransform checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.sizeDelta = Vector2.zero;

            Image checkmarkImage = checkmarkObj.AddComponent<Image>();
            checkmarkImage.color = Color.green;
            checkmarkImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkmarkImage;

            // Toggle Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);

            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(50, 0);
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Show Debug Info";
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.font = LoadKoreanFont();

            // ===== 5. DebugInfoPanel 컴포넌트 추가 및 연결 =====
            DebugInfoPanel debugInfoPanel = debugPanel.AddComponent<DebugInfoPanel>();
            debugInfoPanel.chapterText = chapterText;
            debugInfoPanel.lineProgressText = lineProgressText;
            debugInfoPanel.totalLinesText = totalLinesText;
            debugInfoPanel.visibilityToggle = toggle;
            debugInfoPanel.canvasGroup = canvasGroup;

            EditorUtility.SetDirty(debugPanel);
            Debug.Log("✅ Debug Info Panel added to GameScene!");
            Selection.activeGameObject = debugPanel;
        }

        private static TMP_FontAsset LoadKoreanFont()
        {
            // NotoSansKR 폰트 로드
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NotoSansKR.asset");
            if (font == null)
            {
                Debug.LogWarning("NotoSansKR font not found, using default");
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            return font;
        }
    }
}
