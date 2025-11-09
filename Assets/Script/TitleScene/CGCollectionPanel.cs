using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using IyagiAI.Runtime;

namespace IyagiAI.TitleScene
{
    /// <summary>
    /// CG 컬렉션 갤러리
    /// 해당 프로젝트에서 수집한 모든 CG 표시
    /// </summary>
    public class CGCollectionPanel : MonoBehaviour
    {
        [Header("References")]
        public TitleSceneManager sceneManager;

        [Header("UI Elements")]
        public TMP_Text projectNameText;
        public TMP_Text collectionProgressText; // "12 / 24 CGs Unlocked"
        public Transform cgGridContainer;        // Grid Layout Group
        public GameObject cgThumbnailPrefab;     // CG 썸네일 프리팹

        [Header("CG Viewer")]
        public GameObject cgViewerPanel;         // 전체 화면 CG 뷰어
        public Image cgFullImage;
        public TMP_Text cgDescriptionText;
        public Button closeCGViewerButton;

        [Header("Buttons")]
        public Button backButton;

        private string currentProjectGuid;
        private ProjectSlot currentProject;

        private void OnEnable()
        {
            backButton.onClick.AddListener(OnBackClicked);

            if (cgViewerPanel != null)
            {
                closeCGViewerButton.onClick.AddListener(CloseCGViewer);
                cgViewerPanel.SetActive(false);
            }
        }

        public void LoadCGCollection(string projectGuid)
        {
            currentProjectGuid = projectGuid;
            currentProject = SaveDataManager.Instance.GetProjectSlot(projectGuid);

            if (currentProject == null)
            {
                Debug.LogError($"Project not found: {projectGuid}");
                sceneManager.ShowProjectSelectPanel();
                return;
            }

            // 프로젝트 이름 표시
            projectNameText.text = currentProject.projectName;

            // CG 컬렉션 로드
            var cgCollection = SaveDataManager.Instance.GetCGCollection(projectGuid);

            // 진행도 표시
            int unlockedCount = 0;
            foreach (var cg in cgCollection)
            {
                if (cg.isUnlocked)
                    unlockedCount++;
            }
            collectionProgressText.text = $"{unlockedCount} / {cgCollection.Count} CGs Unlocked";

            // 기존 썸네일 삭제
            foreach (Transform child in cgGridContainer)
            {
                Destroy(child.gameObject);
            }

            // CG 썸네일 생성
            foreach (var cg in cgCollection)
            {
                CreateCGThumbnail(cg);
            }
        }

        private void CreateCGThumbnail(CGMetadata cg)
        {
            var thumbnail = Instantiate(cgThumbnailPrefab, cgGridContainer);
            var thumbnailUI = thumbnail.GetComponent<CGThumbnailItem>();

            if (thumbnailUI != null)
            {
                thumbnailUI.Setup(cg, this);
            }
        }

        public void ViewCG(CGMetadata cg)
        {
            if (!cg.isUnlocked)
            {
                Debug.Log("This CG is locked!");
                return;
            }

            // CG 뷰어 표시
            cgFullImage.sprite = Resources.Load<Sprite>(cg.resourcePath);
            cgDescriptionText.text = cg.description;
            cgViewerPanel.SetActive(true);
        }

        private void CloseCGViewer()
        {
            cgViewerPanel.SetActive(false);
        }

        private void OnBackClicked()
        {
            sceneManager.ShowProjectSelectPanel();
        }
    }

    /// <summary>
    /// CG 썸네일 UI 아이템
    /// </summary>
    public class CGThumbnailItem : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image thumbnailImage;
        public GameObject lockedOverlay; // 잠금 상태 표시
        public TMP_Text cgNumberText;

        [Header("Sprites")]
        public Sprite lockedSprite; // 잠긴 CG용 기본 스프라이트

        private Button button;
        private CGMetadata cgData;
        private CGCollectionPanel panel;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
        }

        public void Setup(CGMetadata cg, CGCollectionPanel collectionPanel)
        {
            cgData = cg;
            panel = collectionPanel;

            cgNumberText.text = $"CG {cg.cgId:D3}";

            if (cg.isUnlocked)
            {
                // 언락된 CG
                thumbnailImage.sprite = Resources.Load<Sprite>(cg.thumbnailPath);
                lockedOverlay.SetActive(false);
                button.onClick.AddListener(OnClicked);
            }
            else
            {
                // 잠긴 CG
                thumbnailImage.sprite = lockedSprite;
                lockedOverlay.SetActive(true);
                button.interactable = false;
            }
        }

        private void OnClicked()
        {
            panel.ViewCG(cgData);
        }
    }
}
