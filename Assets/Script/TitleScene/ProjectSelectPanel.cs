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
        public GameObject projectItemPrefab;   // 프로젝트 아이템 프리팹

        [Header("Buttons")]
        public Button backButton;

        private void OnEnable()
        {
            Debug.Log("[ProjectSelectPanel] OnEnable called");

            if (projectListContainer == null)
            {
                Debug.LogError("[ProjectSelectPanel] projectListContainer is null!");
                return;
            }

            if (projectItemPrefab == null)
            {
                Debug.LogError("[ProjectSelectPanel] projectItemPrefab is null!");
                return;
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

            var text = messageObj.AddComponent<TMP_Text>();
            text.text = "생성된 프로젝트가 없습니다.\n'New Game'으로 새 프로젝트를 만들어보세요!";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.gray;

            var rectTransform = messageObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100);
        }

        private void OnBackClicked()
        {
            sceneManager.ShowTitlePanel();
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
