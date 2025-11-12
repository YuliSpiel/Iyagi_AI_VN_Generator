using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;

namespace IyagiAI.Editor
{
    /// <summary>
    /// GameScene에 로딩 팝업 UI를 자동으로 추가하는 에디터 툴
    /// </summary>
    public static class AddLoadingPanelToGameScene
    {
        [MenuItem("Iyagi/Add Loading Panel to GameScene")]
        public static void AddLoadingPanel()
        {
            // Canvas 찾기
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found in scene!");
                return;
            }

            // 기존 LoadingPanel 삭제
            var existingLoadingPanel = GameObject.Find("LoadingPanel");
            if (existingLoadingPanel != null)
            {
                GameObject.DestroyImmediate(existingLoadingPanel);
                Debug.Log("Removed existing LoadingPanel");
            }

            // 기존 UIBlocker 삭제
            var existingBlocker = GameObject.Find("UIBlocker");
            if (existingBlocker != null)
            {
                GameObject.DestroyImmediate(existingBlocker);
                Debug.Log("Removed existing UIBlocker");
            }

            // ===== 1. UIBlocker 생성 (전체 화면 차단) =====
            GameObject uiBlocker = new GameObject("UIBlocker");
            uiBlocker.transform.SetParent(canvas.transform, false);

            // RectTransform 설정 (전체 화면)
            RectTransform blockerRect = uiBlocker.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;

            // CanvasGroup 추가 (초기 상태: 비활성)
            CanvasGroup blockerGroup = uiBlocker.AddComponent<CanvasGroup>();
            blockerGroup.alpha = 0f;
            blockerGroup.interactable = false;
            blockerGroup.blocksRaycasts = false;

            // 반투명 검정 배경
            Image blockerImage = uiBlocker.AddComponent<Image>();
            blockerImage.color = new Color(0, 0, 0, 0.5f);

            // ===== 2. LoadingPanel 생성 (중앙 팝업) =====
            GameObject loadingPanel = new GameObject("LoadingPanel");
            loadingPanel.transform.SetParent(canvas.transform, false);
            loadingPanel.SetActive(false); // 초기 상태: 숨김

            // RectTransform 설정 (중앙, 400x200)
            RectTransform panelRect = loadingPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(400, 200);

            // 배경 이미지 (검정 반투명)
            Image panelBg = loadingPanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);

            // Vertical Layout
            VerticalLayoutGroup layout = loadingPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // ===== 3. LoadingText 생성 =====
            GameObject loadingTextObj = new GameObject("LoadingText");
            loadingTextObj.transform.SetParent(loadingPanel.transform, false);

            RectTransform textRect = loadingTextObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0, 60);

            TextMeshProUGUI loadingText = loadingTextObj.AddComponent<TextMeshProUGUI>();
            loadingText.text = "Loading...";
            loadingText.fontSize = 24;
            loadingText.color = Color.white;
            loadingText.alignment = TextAlignmentOptions.Center;
            loadingText.font = LoadKoreanFont();

            // ===== 4. Spinner (회전 애니메이션) 생성 =====
            GameObject spinnerObj = new GameObject("Spinner");
            spinnerObj.transform.SetParent(loadingPanel.transform, false);

            RectTransform spinnerRect = spinnerObj.AddComponent<RectTransform>();
            spinnerRect.sizeDelta = new Vector2(60, 60);

            Image spinnerImage = spinnerObj.AddComponent<Image>();
            spinnerImage.color = Color.white;
            // 간단한 원 모양 (Unity 기본 Sprite 사용)
            spinnerImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            spinnerImage.type = Image.Type.Filled;
            spinnerImage.fillMethod = Image.FillMethod.Radial360;
            spinnerImage.fillOrigin = (int)Image.Origin360.Top;

            // 회전 애니메이션 추가
            var spinner = spinnerObj.AddComponent<SimpleSpinner>();

            // ===== 5. GameController에 연결 =====
            GameController gameController = GameObject.FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.loadingPanel = loadingPanel;
                gameController.loadingText = loadingText;
                gameController.uiBlockerGroup = blockerGroup;

                EditorUtility.SetDirty(gameController);
                Debug.Log("✅ Loading panel connected to GameController!");
            }
            else
            {
                Debug.LogWarning("GameController not found. Please connect manually.");
            }

            Debug.Log("✅ Loading Panel added to GameScene!");
            EditorUtility.SetDirty(canvas.gameObject);
            Selection.activeGameObject = loadingPanel;
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
