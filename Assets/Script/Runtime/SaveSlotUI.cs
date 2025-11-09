using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 세이브 슬롯 UI 컴포넌트
    /// Save/Load 메뉴에서 개별 슬롯 표시
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_Text slotIndexText;
        public TMP_Text saveNameText;
        public TMP_Text chapterText;
        public TMP_Text dateText;
        public Image thumbnailImage; // 옵션: 스크린샷
        public GameObject emptySlotPanel;
        public GameObject filledSlotPanel;

        [Header("Buttons")]
        public Button saveButton;
        public Button loadButton;
        public Button deleteButton;

        [Header("Data")]
        public int slotIndex;
        public SaveInfo saveInfo;

        private SaveDataManager saveManager;
        private GameController gameController;

        public void Initialize(int index, SaveDataManager manager, GameController controller)
        {
            slotIndex = index;
            saveManager = manager;
            gameController = controller;

            // 버튼 이벤트
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);

            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);

            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteClicked);

            // 초기 표시
            RefreshDisplay();
        }

        /// <summary>
        /// 슬롯 표시 갱신
        /// </summary>
        public void RefreshDisplay()
        {
            saveInfo = saveManager.GetSaveInfo(slotIndex);

            if (saveInfo != null)
            {
                // 세이브 존재
                ShowFilledSlot();
            }
            else
            {
                // 빈 슬롯
                ShowEmptySlot();
            }
        }

        void ShowFilledSlot()
        {
            if (emptySlotPanel != null)
                emptySlotPanel.SetActive(false);

            if (filledSlotPanel != null)
                filledSlotPanel.SetActive(true);

            // 정보 표시
            if (slotIndexText != null)
                slotIndexText.text = $"Slot {slotIndex + 1}";

            if (saveNameText != null)
                saveNameText.text = saveInfo.saveName;

            if (chapterText != null)
                chapterText.text = saveInfo.GetChapterText();

            if (dateText != null)
                dateText.text = saveInfo.GetFormattedDate();

            // 버튼 활성화
            if (saveButton != null)
                saveButton.interactable = true;

            if (loadButton != null)
                loadButton.interactable = true;

            if (deleteButton != null)
                deleteButton.interactable = true;
        }

        void ShowEmptySlot()
        {
            if (emptySlotPanel != null)
                emptySlotPanel.SetActive(true);

            if (filledSlotPanel != null)
                filledSlotPanel.SetActive(false);

            // 슬롯 번호만 표시
            if (slotIndexText != null)
                slotIndexText.text = $"Slot {slotIndex + 1}";

            // 버튼 비활성화
            if (saveButton != null)
                saveButton.interactable = true; // 저장은 가능

            if (loadButton != null)
                loadButton.interactable = false; // 로드 불가

            if (deleteButton != null)
                deleteButton.interactable = false; // 삭제 불가
        }

        void OnSaveClicked()
        {
            if (gameController == null || gameController.currentState == null)
            {
                Debug.LogWarning("Cannot save: No game state available");
                return;
            }

            // 덮어쓰기 확인 (이미 세이브가 있는 경우)
            if (saveInfo != null)
            {
                // TODO: 확인 다이얼로그 표시
                Debug.Log($"Overwriting save slot {slotIndex}");
            }

            // 세이브 이름 생성
            string saveName = $"Chapter {gameController.currentState.currentChapter} - {System.DateTime.Now:MMdd HHmm}";

            // 저장 실행
            bool success = saveManager.SaveGame(gameController.currentState, slotIndex, saveName);

            if (success)
            {
                Debug.Log($"Game saved to slot {slotIndex}");
                RefreshDisplay();
            }
        }

        void OnLoadClicked()
        {
            if (saveInfo == null)
            {
                Debug.LogWarning("Cannot load: No save data");
                return;
            }

            // 로드 실행
            GameStateSnapshot loadedState = saveManager.LoadGame(slotIndex);

            if (loadedState != null && gameController != null)
            {
                Debug.Log($"Game loaded from slot {slotIndex}");
                gameController.LoadGame(loadedState);

                // Save/Load 메뉴 닫기
                CloseSaveLoadMenu();
            }
        }

        void OnDeleteClicked()
        {
            if (saveInfo == null)
            {
                Debug.LogWarning("Cannot delete: No save data");
                return;
            }

            // TODO: 확인 다이얼로그 표시
            Debug.Log($"Deleting save slot {slotIndex}");

            bool success = saveManager.DeleteSave(slotIndex);

            if (success)
            {
                RefreshDisplay();
            }
        }

        void CloseSaveLoadMenu()
        {
            // 부모 메뉴 닫기
            var menu = GetComponentInParent<SaveLoadMenuUI>();
            if (menu != null)
            {
                menu.CloseMenu();
            }
        }
    }
}
