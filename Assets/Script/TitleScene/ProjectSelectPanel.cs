using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using IyagiAI.Runtime;

namespace IyagiAI.TitleScene
{
    /// <summary>
    /// 프로젝트 선택 화면
    /// Setup Wizard로 만든 모든 VN 프로젝트 목록 표시
    /// </summary>
    public class ProjectSelectPanel : MonoBehaviour
    {
        [Header("References")]
        public TitleSceneManager sceneManager;

        [Header("UI Elements")]
        public Transform projectListContainer; // ScrollView Content
        public TMP_FontAsset koreanFont; // NotoSansKR 폰트 (인스펙터에서 설정)

        [Header("Buttons")]
        public Button backButton;

        private GameObject projectItemPrefab; // 런타임에 자동 생성

        private void OnEnable()
        {
            Debug.Log("[ProjectSelectPanel] OnEnable called");

            if (projectListContainer == null)
            {
                Debug.LogError("[ProjectSelectPanel] projectListContainer is null!");
                return;
            }

            // 프리팹이 없으면 런타임에 생성
            if (projectItemPrefab == null)
            {
                Debug.Log("[ProjectSelectPanel] Creating project item prefab programmatically");
                projectItemPrefab = CreateProjectItemPrefab();
            }

            LoadProjectList();

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
            else
            {
                Debug.LogWarning("[ProjectSelectPanel] backButton is null");
            }
        }

        private void LoadProjectList()
        {
            Debug.Log("[ProjectSelectPanel] LoadProjectList called");

            // 기존 항목 삭제
            foreach (Transform child in projectListContainer)
            {
                Destroy(child.gameObject);
            }

            // 모든 프로젝트 슬롯 로드
            var projectSlots = SaveDataManager.Instance.GetAllProjectSlots();
            Debug.Log($"[ProjectSelectPanel] Loaded {projectSlots.Count} project slots");

            if (projectSlots.Count == 0)
            {
                // 프로젝트가 없으면 안내 메시지
                Debug.LogWarning("[ProjectSelectPanel] No projects found, showing empty message");
                CreateEmptyMessage();
                return;
            }

            // 프로젝트 아이템 생성
            foreach (var slot in projectSlots)
            {
                Debug.Log($"[ProjectSelectPanel] Creating item for project: {slot.projectName}");
                CreateProjectItem(slot);
            }
        }

        private void CreateProjectItem(ProjectSlot slot)
        {
            if (projectItemPrefab == null)
            {
                Debug.LogError("[ProjectSelectPanel] Cannot create item - projectItemPrefab is null");
                return;
            }

            if (projectListContainer == null)
            {
                Debug.LogError("[ProjectSelectPanel] Cannot create item - projectListContainer is null");
                return;
            }

            var item = Instantiate(projectItemPrefab, projectListContainer);
            Debug.Log($"[ProjectSelectPanel] Instantiated project item: {item.name}");

            var itemUI = item.GetComponent<ProjectSlotItem>();

            if (itemUI != null)
            {
                Debug.Log($"[ProjectSelectPanel] Setting up ProjectSlotItem for: {slot.projectName}");
                itemUI.Setup(slot, sceneManager);
            }
            else
            {
                Debug.LogError($"[ProjectSelectPanel] ProjectSlotItem component not found on prefab!");
            }
        }

        private void CreateEmptyMessage()
        {
            var messageObj = new GameObject("EmptyMessage");
            messageObj.transform.SetParent(projectListContainer, false);

            var text = messageObj.AddComponent<TextMeshProUGUI>();
            text.text = "생성된 프로젝트가 없습니다.\n'New Game'으로 새 프로젝트를 만들어보세요!";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.gray;

            // koreanFont 사용
            if (koreanFont != null) text.font = koreanFont;

            var rectTransform = messageObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100);
        }

        private void OnBackClicked()
        {
            sceneManager.ShowTitlePanel();
        }

        /// <summary>
        /// 런타임에 ProjectItem 프리팹을 프로그래밍 방식으로 생성
        /// </summary>
        private GameObject CreateProjectItemPrefab()
        {
            // koreanFont 사용 (인스펙터에서 설정된 폰트)
            TMP_FontAsset font = koreanFont;
            if (font == null)
            {
                Debug.LogWarning("[ProjectSelectPanel] Korean font not assigned in inspector");
            }

            // 1. 루트 GameObject 생성
            GameObject projectItem = new GameObject("ProjectItemPrefab");
            var rectTransform = projectItem.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800, 120);

            // ProjectSlotItem 스크립트 추가
            var slotItem = projectItem.AddComponent<ProjectSlotItem>();

            // 2. Background Image 추가
            var bgImage = projectItem.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 3. Project Name Text
            GameObject nameObj = new GameObject("ProjectNameText");
            nameObj.transform.SetParent(projectItem.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "프로젝트 이름";
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            if (font != null) nameText.font = font;
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.6f);
            nameRect.anchorMax = new Vector2(0.6f, 1);
            nameRect.offsetMin = new Vector2(20, 10);
            nameRect.offsetMax = new Vector2(-10, -10);

            // 4. Last Played Text
            GameObject lastPlayedObj = new GameObject("LastPlayedText");
            lastPlayedObj.transform.SetParent(projectItem.transform, false);
            var lastPlayedText = lastPlayedObj.AddComponent<TextMeshProUGUI>();
            lastPlayedText.text = "최근 플레이: 2025-01-10";
            lastPlayedText.fontSize = 14;
            lastPlayedText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            if (font != null) lastPlayedText.font = font;
            var lastPlayedRect = lastPlayedObj.GetComponent<RectTransform>();
            lastPlayedRect.anchorMin = new Vector2(0, 0.3f);
            lastPlayedRect.anchorMax = new Vector2(0.6f, 0.6f);
            lastPlayedRect.offsetMin = new Vector2(20, 0);
            lastPlayedRect.offsetMax = new Vector2(-10, 0);

            // 5. Save Count Text
            GameObject saveCountObj = new GameObject("SaveCountText");
            saveCountObj.transform.SetParent(projectItem.transform, false);
            var saveCountText = saveCountObj.AddComponent<TextMeshProUGUI>();
            saveCountText.text = "1개의 저장 파일";
            saveCountText.fontSize = 14;
            saveCountText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            if (font != null) saveCountText.font = font;
            var saveCountRect = saveCountObj.GetComponent<RectTransform>();
            saveCountRect.anchorMin = new Vector2(0, 0);
            saveCountRect.anchorMax = new Vector2(0.3f, 0.3f);
            saveCountRect.offsetMin = new Vector2(20, 10);
            saveCountRect.offsetMax = new Vector2(0, 0);

            // 6. Chapter Info Text
            GameObject chapterInfoObj = new GameObject("ChapterInfoText");
            chapterInfoObj.transform.SetParent(projectItem.transform, false);
            var chapterInfoText = chapterInfoObj.AddComponent<TextMeshProUGUI>();
            chapterInfoText.text = "3개 챕터";
            chapterInfoText.fontSize = 14;
            chapterInfoText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            if (font != null) chapterInfoText.font = font;
            var chapterInfoRect = chapterInfoObj.GetComponent<RectTransform>();
            chapterInfoRect.anchorMin = new Vector2(0.3f, 0);
            chapterInfoRect.anchorMax = new Vector2(0.6f, 0.3f);
            chapterInfoRect.offsetMin = new Vector2(10, 10);
            chapterInfoRect.offsetMax = new Vector2(0, 0);

            // 7. Select Button
            GameObject selectBtnObj = new GameObject("SelectButton");
            selectBtnObj.transform.SetParent(projectItem.transform, false);
            var selectBtn = selectBtnObj.AddComponent<Button>();
            var selectBtnImage = selectBtnObj.AddComponent<Image>();
            selectBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 1);
            var selectBtnRect = selectBtnObj.GetComponent<RectTransform>();
            selectBtnRect.anchorMin = new Vector2(0.62f, 0.5f);
            selectBtnRect.anchorMax = new Vector2(0.78f, 0.9f);
            selectBtnRect.offsetMin = Vector2.zero;
            selectBtnRect.offsetMax = Vector2.zero;

            GameObject selectBtnTextObj = new GameObject("Text");
            selectBtnTextObj.transform.SetParent(selectBtnObj.transform, false);
            var selectBtnText = selectBtnTextObj.AddComponent<TextMeshProUGUI>();
            selectBtnText.text = "선택";
            selectBtnText.fontSize = 18;
            selectBtnText.alignment = TextAlignmentOptions.Center;
            selectBtnText.color = Color.white;
            if (font != null) selectBtnText.font = font;
            var selectBtnTextRect = selectBtnTextObj.GetComponent<RectTransform>();
            selectBtnTextRect.anchorMin = Vector2.zero;
            selectBtnTextRect.anchorMax = Vector2.one;
            selectBtnTextRect.offsetMin = Vector2.zero;
            selectBtnTextRect.offsetMax = Vector2.zero;

            // 8. CG Gallery Button
            GameObject cgBtnObj = new GameObject("CGGalleryButton");
            cgBtnObj.transform.SetParent(projectItem.transform, false);
            var cgBtn = cgBtnObj.AddComponent<Button>();
            var cgBtnImage = cgBtnObj.AddComponent<Image>();
            cgBtnImage.color = new Color(0.2f, 0.4f, 0.6f, 1);
            var cgBtnRect = cgBtnObj.GetComponent<RectTransform>();
            cgBtnRect.anchorMin = new Vector2(0.8f, 0.5f);
            cgBtnRect.anchorMax = new Vector2(0.96f, 0.9f);
            cgBtnRect.offsetMin = Vector2.zero;
            cgBtnRect.offsetMax = Vector2.zero;

            GameObject cgBtnTextObj = new GameObject("Text");
            cgBtnTextObj.transform.SetParent(cgBtnObj.transform, false);
            var cgBtnText = cgBtnTextObj.AddComponent<TextMeshProUGUI>();
            cgBtnText.text = "CG";
            cgBtnText.fontSize = 18;
            cgBtnText.alignment = TextAlignmentOptions.Center;
            cgBtnText.color = Color.white;
            if (font != null) cgBtnText.font = font;
            var cgBtnTextRect = cgBtnTextObj.GetComponent<RectTransform>();
            cgBtnTextRect.anchorMin = Vector2.zero;
            cgBtnTextRect.anchorMax = Vector2.one;
            cgBtnTextRect.offsetMin = Vector2.zero;
            cgBtnTextRect.offsetMax = Vector2.zero;

            // 9. Delete Button
            GameObject delBtnObj = new GameObject("DeleteButton");
            delBtnObj.transform.SetParent(projectItem.transform, false);
            var delBtn = delBtnObj.AddComponent<Button>();
            var delBtnImage = delBtnObj.AddComponent<Image>();
            delBtnImage.color = new Color(0.6f, 0.2f, 0.2f, 1);
            var delBtnRect = delBtnObj.GetComponent<RectTransform>();
            delBtnRect.anchorMin = new Vector2(0.62f, 0.1f);
            delBtnRect.anchorMax = new Vector2(0.96f, 0.4f);
            delBtnRect.offsetMin = Vector2.zero;
            delBtnRect.offsetMax = Vector2.zero;

            GameObject delBtnTextObj = new GameObject("Text");
            delBtnTextObj.transform.SetParent(delBtnObj.transform, false);
            var delBtnText = delBtnTextObj.AddComponent<TextMeshProUGUI>();
            delBtnText.text = "삭제";
            delBtnText.fontSize = 18;
            delBtnText.alignment = TextAlignmentOptions.Center;
            delBtnText.color = Color.white;
            if (font != null) delBtnText.font = font;
            var delBtnTextRect = delBtnTextObj.GetComponent<RectTransform>();
            delBtnTextRect.anchorMin = Vector2.zero;
            delBtnTextRect.anchorMax = Vector2.one;
            delBtnTextRect.offsetMin = Vector2.zero;
            delBtnTextRect.offsetMax = Vector2.zero;

            // 10. 스크립트에 참조 연결
            slotItem.projectNameText = nameText;
            slotItem.lastPlayedText = lastPlayedText;
            slotItem.saveCountText = saveCountText;
            slotItem.chapterInfoText = chapterInfoText;
            slotItem.selectButton = selectBtn;
            slotItem.cgGalleryButton = cgBtn;
            slotItem.deleteButton = delBtn;

            Debug.Log("[ProjectSelectPanel] Created project item prefab with all components");

            return projectItem;
        }
    }

    /// <summary>
    /// 프로젝트 슬롯 UI 아이템
    /// </summary>
    public class ProjectSlotItem : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_Text projectNameText;
        public TMP_Text lastPlayedText;
        public TMP_Text saveCountText;
        public TMP_Text chapterInfoText;

        [Header("Buttons")]
        public Button selectButton;
        public Button cgGalleryButton;
        public Button deleteButton;

        private ProjectSlot slot;
        private TitleSceneManager sceneManager;

        public void Setup(ProjectSlot projectSlot, TitleSceneManager manager)
        {
            slot = projectSlot;
            sceneManager = manager;

            // UI 정보 표시
            projectNameText.text = slot.projectName;
            lastPlayedText.text = $"최근 플레이: {slot.lastPlayedDate:yyyy-MM-dd HH:mm}";
            saveCountText.text = $"{slot.saveFiles.Count}개의 저장 파일";
            chapterInfoText.text = $"{slot.totalChapters}개 챕터";

            // 버튼 이벤트
            selectButton.onClick.AddListener(OnSelectClicked);
            cgGalleryButton.onClick.AddListener(OnCGGalleryClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        private void OnSelectClicked()
        {
            sceneManager.ShowSaveFileSelectPanel(slot.projectGuid);
        }

        private void OnCGGalleryClicked()
        {
            sceneManager.ShowCGCollectionPanel(slot.projectGuid);
        }

        private void OnDeleteClicked()
        {
            // 확인 다이얼로그 (TODO: 나중에 구현)
            if (ConfirmDelete())
            {
                SaveDataManager.Instance.DeleteProjectSlot(slot.projectGuid);
                Destroy(gameObject);
            }
        }

        private bool ConfirmDelete()
        {
            // TODO: 제대로 된 다이얼로그 구현
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.DisplayDialog(
                "프로젝트 삭제",
                $"'{slot.projectName}' 프로젝트를 정말 삭제하시겠습니까?\n모든 저장 파일과 CG가 삭제됩니다.",
                "삭제",
                "취소"
            );
#else
            // 런타임에서는 일단 true 반환 (나중에 UI 다이얼로그로 교체 필요)
            Debug.LogWarning($"Deleting project: {slot.projectName} (no confirmation dialog in build)");
            return true;
#endif
        }
    }
}
