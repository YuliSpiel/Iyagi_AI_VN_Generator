using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using IyagiAI.Runtime;

namespace IyagiAI.TitleScene
{
    /// <summary>
    /// 저장 파일 선택 화면
    /// 선택한 프로젝트의 모든 저장 파일 + 새 저장 파일 생성
    /// </summary>
    public class SaveFileSelectPanel : MonoBehaviour
    {
        [Header("References")]
        public TitleSceneManager sceneManager;

        [Header("UI Elements")]
        public TMP_Text projectNameText;
        public Transform saveFileListContainer; // ScrollView Content
        public GameObject saveFileItemPrefab;   // 저장 파일 아이템 프리팹

        [Header("Buttons")]
        public Button newSaveButton;
        public Button backButton;

        [Header("New Save Popup")]
        public GameObject newSavePopup;
        public TMP_Dropdown startChapterDropdown;
        public Button confirmNewSaveButton;
        public Button cancelNewSaveButton;

        private string currentProjectGuid;
        private ProjectSlot currentProject;

        private void OnEnable()
        {
            backButton.onClick.AddListener(OnBackClicked);
            newSaveButton.onClick.AddListener(OnNewSaveClicked);

            if (newSavePopup != null)
            {
                confirmNewSaveButton.onClick.AddListener(OnConfirmNewSave);
                cancelNewSaveButton.onClick.AddListener(OnCancelNewSave);
                newSavePopup.SetActive(false);
            }
        }

        public void LoadSaveFiles(string projectGuid)
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

            // 기존 저장 파일 목록 삭제
            foreach (Transform child in saveFileListContainer)
            {
                Destroy(child.gameObject);
            }

            // 저장 파일 아이템 생성
            if (currentProject.saveFiles.Count == 0)
            {
                CreateEmptyMessage();
            }
            else
            {
                foreach (var saveFile in currentProject.saveFiles)
                {
                    CreateSaveFileItem(saveFile);
                }
            }
        }

        private void CreateSaveFileItem(SaveFile saveFile)
        {
            var item = Instantiate(saveFileItemPrefab, saveFileListContainer);
            var itemUI = item.GetComponent<SaveFileItem>();

            if (itemUI != null)
            {
                itemUI.Setup(saveFile, sceneManager);
            }
        }

        private void CreateEmptyMessage()
        {
            var messageObj = new GameObject("EmptyMessage");
            messageObj.transform.SetParent(saveFileListContainer, false);

            var text = messageObj.AddComponent<TMP_Text>();
            text.text = "저장 파일이 없습니다.\n'New Save'로 새 저장 파일을 만들어보세요!";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.gray;

            var rectTransform = messageObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 100);
        }

        private void OnNewSaveClicked()
        {
            // 챕터 선택 팝업 표시
            SetupChapterDropdown();
            newSavePopup.SetActive(true);
        }

        private void SetupChapterDropdown()
        {
            startChapterDropdown.ClearOptions();

            var options = new List<string>();
            for (int i = 1; i <= currentProject.totalChapters; i++)
            {
                options.Add($"Chapter {i}");
            }

            startChapterDropdown.AddOptions(options);
            startChapterDropdown.value = 0; // Chapter 1 기본값
        }

        private void OnConfirmNewSave()
        {
            int startChapter = startChapterDropdown.value + 1; // 0-based to 1-based

            // 새 저장 파일 생성
            var newSaveFile = SaveDataManager.Instance.CreateNewSaveFile(
                currentProjectGuid,
                startChapter
            );

            newSavePopup.SetActive(false);

            // 게임 시작
            sceneManager.LoadGame(newSaveFile.saveFileId);
        }

        private void OnCancelNewSave()
        {
            newSavePopup.SetActive(false);
        }

        private void OnBackClicked()
        {
            sceneManager.ShowProjectSelectPanel();
        }
    }

    /// <summary>
    /// 저장 파일 UI 아이템
    /// </summary>
    public class SaveFileItem : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_Text saveSlotNameText;
        public TMP_Text chapterProgressText;
        public TMP_Text lastPlayedText;
        public TMP_Text playtimeText;

        [Header("Buttons")]
        public Button loadButton;
        public Button deleteButton;

        private SaveFile saveFile;
        private TitleSceneManager sceneManager;

        public void Setup(SaveFile file, TitleSceneManager manager)
        {
            saveFile = file;
            sceneManager = manager;

            // UI 정보 표시
            saveSlotNameText.text = $"Save {saveFile.slotNumber}";
            chapterProgressText.text = $"Chapter {saveFile.currentChapter}/{saveFile.totalChapters}";
            lastPlayedText.text = $"최근 플레이: {saveFile.lastPlayedDate:yyyy-MM-dd HH:mm}";

            int hours = saveFile.totalPlaytimeSeconds / 3600;
            int minutes = (saveFile.totalPlaytimeSeconds % 3600) / 60;
            playtimeText.text = $"플레이 시간: {hours}h {minutes}m";

            // 버튼 이벤트
            loadButton.onClick.AddListener(OnLoadClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        private void OnLoadClicked()
        {
            sceneManager.LoadGame(saveFile.saveFileId);
        }

        private void OnDeleteClicked()
        {
            if (ConfirmDelete())
            {
                SaveDataManager.Instance.DeleteSaveFile(saveFile.saveFileId);
                Destroy(gameObject);
            }
        }

        private bool ConfirmDelete()
        {
            // TODO: 제대로 된 다이얼로그 구현
            return UnityEditor.EditorUtility.DisplayDialog(
                "저장 파일 삭제",
                $"Save {saveFile.slotNumber}를 정말 삭제하시겠습니까?",
                "삭제",
                "취소"
            );
        }
    }
}
