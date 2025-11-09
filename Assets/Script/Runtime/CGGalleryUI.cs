using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// CG 갤러리 UI
    /// 해금된 CG 목록 표시 및 감상
    /// </summary>
    public class CGGalleryUI : MonoBehaviour
    {
        [Header("References")]
        public VNProjectData projectData;
        public GameStateSnapshot currentGameState;

        [Header("UI Elements")]
        public GameObject galleryPanel;
        public Transform gridContainer; // CG 썸네일 그리드
        public GameObject cgSlotPrefab; // CGGallerySlot Prefab

        [Header("Viewer")]
        public GameObject cgViewerPanel; // 전체 화면 CG 뷰어
        public Image fullCGImage;
        public Button closeViewerButton;
        public Button previousCGButton;
        public Button nextCGButton;

        [Header("Info")]
        public TMPro.TMP_Text cgTitleText;
        public TMPro.TMP_Text cgDescriptionText;
        public TMPro.TMP_Text progressText; // "5 / 20"

        [Header("Buttons")]
        public Button closeGalleryButton;

        private List<CGGallerySlot> cgSlots = new List<CGGallerySlot>();
        private int currentViewingIndex = -1;
        private List<CGMetadata> unlockedCGs = new List<CGMetadata>();

        void Start()
        {
            // 버튼 이벤트
            if (closeGalleryButton != null)
                closeGalleryButton.onClick.AddListener(CloseGallery);

            if (closeViewerButton != null)
                closeViewerButton.onClick.AddListener(CloseViewer);

            if (previousCGButton != null)
                previousCGButton.onClick.AddListener(ShowPreviousCG);

            if (nextCGButton != null)
                nextCGButton.onClick.AddListener(ShowNextCG);

            // 초기 상태
            galleryPanel.SetActive(false);
            cgViewerPanel.SetActive(false);
        }

        /// <summary>
        /// 갤러리 열기
        /// </summary>
        public void OpenGallery(GameStateSnapshot gameState)
        {
            currentGameState = gameState;

            // 해금된 CG 목록 가져오기
            UpdateUnlockedCGs();

            // CG 슬롯 생성
            CreateCGSlots();

            // 진행률 표시
            UpdateProgressText();

            galleryPanel.SetActive(true);
        }

        /// <summary>
        /// 갤러리 닫기
        /// </summary>
        public void CloseGallery()
        {
            galleryPanel.SetActive(false);
        }

        /// <summary>
        /// 해금된 CG 목록 업데이트
        /// </summary>
        void UpdateUnlockedCGs()
        {
            unlockedCGs.Clear();

            if (projectData == null || projectData.allCGs == null)
                return;

            foreach (var cg in projectData.allCGs)
            {
                // 해금 여부 확인
                if (currentGameState != null &&
                    currentGameState.unlockedCGs.Contains(cg.cgId))
                {
                    unlockedCGs.Add(cg);
                }
            }
        }

        /// <summary>
        /// CG 슬롯 생성
        /// </summary>
        void CreateCGSlots()
        {
            if (cgSlotPrefab == null || gridContainer == null)
            {
                Debug.LogError("CGSlotPrefab or GridContainer is not assigned!");
                return;
            }

            // 기존 슬롯 정리
            foreach (var slot in cgSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            cgSlots.Clear();

            // 모든 CG에 대해 슬롯 생성
            for (int i = 0; i < projectData.allCGs.Count; i++)
            {
                CGMetadata cgMeta = projectData.allCGs[i];
                GameObject slotObj = Instantiate(cgSlotPrefab, gridContainer);
                CGGallerySlot slot = slotObj.GetComponent<CGGallerySlot>();

                if (slot != null)
                {
                    bool isUnlocked = currentGameState.unlockedCGs.Contains(cgMeta.cgId);
                    int index = i;
                    slot.Initialize(cgMeta, isUnlocked, () => OnCGSlotClicked(index));
                    cgSlots.Add(slot);
                }
            }
        }

        /// <summary>
        /// CG 슬롯 클릭 처리
        /// </summary>
        void OnCGSlotClicked(int cgIndex)
        {
            if (cgIndex < 0 || cgIndex >= projectData.allCGs.Count)
                return;

            CGMetadata cgMeta = projectData.allCGs[cgIndex];

            // 해금 확인
            if (!currentGameState.unlockedCGs.Contains(cgMeta.cgId))
            {
                Debug.Log("CG is locked");
                return;
            }

            // 뷰어로 표시
            currentViewingIndex = cgIndex;
            ShowCGInViewer(cgMeta);
        }

        /// <summary>
        /// CG 뷰어에 표시
        /// </summary>
        void ShowCGInViewer(CGMetadata cgMeta)
        {
            // CG 이미지 로드
            Sprite cgSprite = Resources.Load<Sprite>($"Image/CG/{cgMeta.cgId}");

            if (cgSprite != null)
            {
                fullCGImage.sprite = cgSprite;
                fullCGImage.SetNativeSize();

                // 정보 표시
                if (cgTitleText != null)
                    cgTitleText.text = cgMeta.sceneDescription ?? $"CG {cgMeta.cgId}";

                if (cgDescriptionText != null)
                {
                    string charNames = string.Join(", ", cgMeta.characterNames);
                    cgDescriptionText.text = $"Characters: {charNames}";
                }

                // 네비게이션 버튼 상태
                UpdateViewerNavigation();

                cgViewerPanel.SetActive(true);
            }
            else
            {
                Debug.LogError($"Failed to load CG: {cgMeta.cgId}");
            }
        }

        /// <summary>
        /// 뷰어 네비게이션 업데이트
        /// </summary>
        void UpdateViewerNavigation()
        {
            // 이전 CG 버튼
            if (previousCGButton != null)
            {
                int prevIndex = FindPreviousUnlockedCG(currentViewingIndex);
                previousCGButton.interactable = (prevIndex >= 0);
            }

            // 다음 CG 버튼
            if (nextCGButton != null)
            {
                int nextIndex = FindNextUnlockedCG(currentViewingIndex);
                nextCGButton.interactable = (nextIndex >= 0);
            }
        }

        /// <summary>
        /// 이전 해금된 CG 찾기
        /// </summary>
        int FindPreviousUnlockedCG(int currentIndex)
        {
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                if (currentGameState.unlockedCGs.Contains(projectData.allCGs[i].cgId))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 다음 해금된 CG 찾기
        /// </summary>
        int FindNextUnlockedCG(int currentIndex)
        {
            for (int i = currentIndex + 1; i < projectData.allCGs.Count; i++)
            {
                if (currentGameState.unlockedCGs.Contains(projectData.allCGs[i].cgId))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 이전 CG 표시
        /// </summary>
        void ShowPreviousCG()
        {
            int prevIndex = FindPreviousUnlockedCG(currentViewingIndex);
            if (prevIndex >= 0)
            {
                currentViewingIndex = prevIndex;
                ShowCGInViewer(projectData.allCGs[prevIndex]);
            }
        }

        /// <summary>
        /// 다음 CG 표시
        /// </summary>
        void ShowNextCG()
        {
            int nextIndex = FindNextUnlockedCG(currentViewingIndex);
            if (nextIndex >= 0)
            {
                currentViewingIndex = nextIndex;
                ShowCGInViewer(projectData.allCGs[nextIndex]);
            }
        }

        /// <summary>
        /// 뷰어 닫기
        /// </summary>
        void CloseViewer()
        {
            cgViewerPanel.SetActive(false);
            currentViewingIndex = -1;
        }

        /// <summary>
        /// 진행률 텍스트 업데이트
        /// </summary>
        void UpdateProgressText()
        {
            if (progressText != null && projectData != null)
            {
                int unlocked = currentGameState.unlockedCGs.Count;
                int total = projectData.allCGs.Count;
                progressText.text = $"{unlocked} / {total} CGs Unlocked";
            }
        }
    }
}
