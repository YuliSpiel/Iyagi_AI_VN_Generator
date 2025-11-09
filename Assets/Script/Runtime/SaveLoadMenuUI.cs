using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// Save/Load 메뉴 UI
    /// 세이브 슬롯 목록 표시 및 관리
    /// </summary>
    public class SaveLoadMenuUI : MonoBehaviour
    {
        [Header("References")]
        public GameController gameController;
        public SaveDataManager saveManager;

        [Header("UI Elements")]
        public GameObject menuPanel;
        public Transform slotContainer; // 슬롯들이 배치될 컨테이너
        public GameObject slotPrefab; // SaveSlotUI Prefab

        [Header("Buttons")]
        public Button closeButton;
        public Button saveTabButton;
        public Button loadTabButton;

        [Header("Settings")]
        public int maxSlots = 10;

        private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();
        private bool isSaveMode = true; // true: Save, false: Load

        void Start()
        {
            // 버튼 이벤트
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseMenu);

            if (saveTabButton != null)
                saveTabButton.onClick.AddListener(() => SwitchMode(true));

            if (loadTabButton != null)
                loadTabButton.onClick.AddListener(() => SwitchMode(false));

            // 슬롯 생성
            CreateSlots();

            // 초기 상태
            menuPanel.SetActive(false);
        }

        /// <summary>
        /// 슬롯 UI 생성
        /// </summary>
        void CreateSlots()
        {
            if (slotPrefab == null || slotContainer == null)
            {
                Debug.LogError("SlotPrefab or SlotContainer is not assigned!");
                return;
            }

            // 기존 슬롯 정리
            foreach (var slot in slotUIs)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            slotUIs.Clear();

            // 새 슬롯 생성
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(i, saveManager, gameController);
                    slotUIs.Add(slotUI);
                }
            }
        }

        /// <summary>
        /// Save 모드로 메뉴 열기
        /// </summary>
        public void OpenSaveMenu()
        {
            SwitchMode(true);
            menuPanel.SetActive(true);
            RefreshAllSlots();
        }

        /// <summary>
        /// Load 모드로 메뉴 열기
        /// </summary>
        public void OpenLoadMenu()
        {
            SwitchMode(false);
            menuPanel.SetActive(true);
            RefreshAllSlots();
        }

        /// <summary>
        /// 메뉴 닫기
        /// </summary>
        public void CloseMenu()
        {
            menuPanel.SetActive(false);
        }

        /// <summary>
        /// Save/Load 모드 전환
        /// </summary>
        void SwitchMode(bool saveMode)
        {
            isSaveMode = saveMode;

            // 탭 버튼 색상 변경
            if (saveTabButton != null)
            {
                var colors = saveTabButton.colors;
                colors.normalColor = saveMode ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                saveTabButton.colors = colors;
            }

            if (loadTabButton != null)
            {
                var colors = loadTabButton.colors;
                colors.normalColor = saveMode ? new Color(0.7f, 0.7f, 0.7f) : Color.white;
                loadTabButton.colors = colors;
            }

            // 슬롯 UI 업데이트
            UpdateSlotButtonStates();
        }

        /// <summary>
        /// 슬롯 버튼 상태 업데이트
        /// </summary>
        void UpdateSlotButtonStates()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null)
                {
                    // Save 모드: Save 버튼 활성화, Load 버튼 숨김
                    // Load 모드: Load 버튼 활성화, Save 버튼 숨김
                    if (slotUI.saveButton != null)
                        slotUI.saveButton.gameObject.SetActive(isSaveMode);

                    if (slotUI.loadButton != null)
                        slotUI.loadButton.gameObject.SetActive(!isSaveMode);
                }
            }
        }

        /// <summary>
        /// 모든 슬롯 갱신
        /// </summary>
        void RefreshAllSlots()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null)
                {
                    slotUI.RefreshDisplay();
                }
            }
        }

        /// <summary>
        /// 외부에서 호출 가능한 빠른 저장
        /// </summary>
        public void QuickSave()
        {
            if (gameController != null && saveManager != null)
            {
                bool success = saveManager.QuickSave(gameController.currentState);
                if (success)
                {
                    Debug.Log("Quick Save successful");
                    // TODO: 저장 완료 알림 표시
                }
            }
        }

        /// <summary>
        /// 외부에서 호출 가능한 빠른 로드
        /// </summary>
        public void QuickLoad()
        {
            if (gameController != null && saveManager != null)
            {
                GameStateSnapshot state = saveManager.QuickLoad();
                if (state != null)
                {
                    gameController.LoadGame(state);
                    Debug.Log("Quick Load successful");
                }
                else
                {
                    Debug.LogWarning("No quick save found");
                }
            }
        }
    }
}
