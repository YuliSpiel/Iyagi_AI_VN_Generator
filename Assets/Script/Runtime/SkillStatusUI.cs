using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 좌측 상단 스킬 상태 UI
    /// 마우스 오버 시 현재 스킬 점수 표시
    /// </summary>
    public class SkillStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform hoverArea; // 좌측 상단 호버 영역
        public GameObject skillPanel; // 스킬 리스트 패널
        public Transform skillListContainer; // 스킬 항목들이 들어갈 컨테이너
        public GameObject skillItemPrefab; // 스킬 항목 프리팹 (Runtime 생성)

        [Header("Settings")]
        public Vector2 hoverAreaSize = new Vector2(100, 100);

        private GameController gameController;
        private bool isHovering = false;
        private List<SkillItem> skillItems = new List<SkillItem>();

        void Start()
        {
            gameController = FindObjectOfType<GameController>();

            // EventTrigger 콜백 연결 (GameSceneSetupHelper에서 이미 EventTrigger는 생성됨)
            if (hoverArea != null)
            {
                var trigger = hoverArea.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger != null)
                {
                    // 기존 엔트리에 콜백 추가
                    foreach (var entry in trigger.triggers)
                    {
                        if (entry.eventID == UnityEngine.EventSystems.EventTriggerType.PointerEnter)
                        {
                            entry.callback.AddListener((data) => OnHoverEnter());
                        }
                        else if (entry.eventID == UnityEngine.EventSystems.EventTriggerType.PointerExit)
                        {
                            entry.callback.AddListener((data) => OnHoverExit());
                        }
                    }
                }
            }

            // 스킬 패널 초기 숨김
            if (skillPanel != null)
            {
                skillPanel.SetActive(false);
            }
        }

        void OnHoverEnter()
        {
            isHovering = true;
            ShowSkillPanel();
        }

        void OnHoverExit()
        {
            isHovering = false;
            HideSkillPanel();
        }

        void ShowSkillPanel()
        {
            if (skillPanel == null || gameController == null || gameController.currentState == null)
                return;

            // 스킬 데이터 업데이트
            UpdateSkillDisplay();

            skillPanel.SetActive(true);
        }

        void HideSkillPanel()
        {
            if (skillPanel != null)
            {
                skillPanel.SetActive(false);
            }
        }

        void UpdateSkillDisplay()
        {
            if (gameController == null)
            {
                Debug.LogWarning("[SkillStatusUI] gameController is null");
                return;
            }

            if (gameController.currentState == null)
            {
                Debug.LogWarning("[SkillStatusUI] gameController.currentState is null");
                return;
            }

            // 기존 항목 삭제
            foreach (Transform child in skillListContainer)
            {
                Destroy(child.gameObject);
            }
            skillItems.Clear();

            Debug.Log($"[SkillStatusUI] Core Values count: {gameController.currentState.coreValueScores.Count}");
            Debug.Log($"[SkillStatusUI] Skills count: {gameController.currentState.skillScores.Count}");

            // Core Values 표시
            foreach (var kv in gameController.currentState.coreValueScores)
            {
                Debug.Log($"[SkillStatusUI] Creating Core Value: {kv.Key} = {kv.Value}");
                CreateSkillItem($"[Core] {kv.Key}", kv.Value, 100, new Color(1f, 0.8f, 0.2f)); // 금색
            }

            // Derived Skills 표시
            foreach (var kv in gameController.currentState.skillScores)
            {
                Debug.Log($"[SkillStatusUI] Creating Skill: {kv.Key} = {kv.Value}");
                CreateSkillItem(kv.Key, kv.Value, 100, new Color(0.2f, 0.8f, 1f)); // 하늘색
            }
        }

        void CreateSkillItem(string skillName, int currentValue, int maxValue, Color barColor)
        {
            if (skillListContainer == null)
            {
                Debug.LogError("[SkillStatusUI] skillListContainer is null!");
                return;
            }

            GameObject itemObj = new GameObject($"Skill_{skillName}");
            itemObj.transform.SetParent(skillListContainer, false);

            var rectTransform = itemObj.AddComponent<RectTransform>();

            // LayoutElement 추가하여 높이 고정
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35;
            layoutElement.minHeight = 35;

            Debug.Log($"[SkillStatusUI] Created item '{skillName}' under '{skillListContainer.name}', child count: {skillListContainer.childCount}");

            // Background
            var bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            bg.raycastTarget = false; // 레이캐스트 차단 방지

            // Skill Name Text
            GameObject nameObj = new GameObject("SkillName");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(0.45f, 1);
            nameRect.offsetMin = new Vector2(5, 0);
            nameRect.offsetMax = new Vector2(-5, 0);

            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = skillName;
            nameText.fontSize = 13;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.verticalAlignment = VerticalAlignmentOptions.Middle;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            // NotoSansKR 폰트 로드
            var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts/NotoSansKR");
            if (koreanFont != null)
            {
                nameText.font = koreanFont;
                Debug.Log($"[SkillStatusUI] Loaded NotoSansKR font for '{skillName}'");
            }
            else
            {
                Debug.LogWarning($"[SkillStatusUI] Could not load NotoSansKR font for '{skillName}'");
            }

            Debug.Log($"[SkillStatusUI] Created text '{skillName}', fontSize: {nameText.fontSize}, color: {nameText.color}, font: {nameText.font?.name}");

            // Progress Bar Background
            GameObject barBgObj = new GameObject("BarBackground");
            barBgObj.transform.SetParent(itemObj.transform, false);
            var barBgRect = barBgObj.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.47f, 0.25f);
            barBgRect.anchorMax = new Vector2(0.82f, 0.75f);
            barBgRect.offsetMin = Vector2.zero;
            barBgRect.offsetMax = Vector2.zero;

            var barBg = barBgObj.AddComponent<Image>();
            barBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            barBg.raycastTarget = false; // 레이캐스트 차단 방지

            // Progress Bar Fill
            GameObject barFillObj = new GameObject("BarFill");
            barFillObj.transform.SetParent(barBgObj.transform, false);
            var barFillRect = barFillObj.AddComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = new Vector2((float)currentValue / maxValue, 1);
            barFillRect.offsetMin = Vector2.zero;
            barFillRect.offsetMax = Vector2.zero;

            var barFill = barFillObj.AddComponent<Image>();
            barFill.color = barColor;
            barFill.raycastTarget = false; // 레이캐스트 차단 방지

            // Value Text
            GameObject valueObj = new GameObject("ValueText");
            valueObj.transform.SetParent(itemObj.transform, false);
            var valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.84f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = new Vector2(0, 0);
            valueRect.offsetMax = new Vector2(-5, 0);

            var valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = currentValue.ToString();
            valueText.fontSize = 13;
            valueText.color = Color.white;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.verticalAlignment = VerticalAlignmentOptions.Middle;

            // NotoSansKR 폰트 로드
            if (koreanFont != null)
            {
                valueText.font = koreanFont;
            }
            else
            {
                Debug.LogWarning($"[SkillStatusUI] Could not load NotoSansKR font for value text");
            }

            var skillItem = itemObj.AddComponent<SkillItem>();
            skillItem.Setup(skillName, currentValue, maxValue, barFill);
            skillItems.Add(skillItem);
        }

        /// <summary>
        /// 스킬 값 업데이트 (외부에서 호출)
        /// </summary>
        public void RefreshSkillDisplay()
        {
            if (isHovering && skillPanel.activeSelf)
            {
                UpdateSkillDisplay();
            }
        }
    }

    /// <summary>
    /// 개별 스킬 항목
    /// </summary>
    public class SkillItem : MonoBehaviour
    {
        public string skillName;
        public int currentValue;
        public int maxValue;
        public Image fillBar;

        public void Setup(string name, int current, int max, Image fill)
        {
            skillName = name;
            currentValue = current;
            maxValue = max;
            fillBar = fill;
        }

        public void UpdateValue(int newValue)
        {
            currentValue = newValue;
            if (fillBar != null)
            {
                var rectTransform = fillBar.GetComponent<RectTransform>();
                rectTransform.anchorMax = new Vector2((float)currentValue / maxValue, 1);
            }
        }
    }
}
