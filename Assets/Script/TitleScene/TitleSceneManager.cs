using UnityEngine;
using IyagiAI.Runtime;

namespace IyagiAI.TitleScene
{
    /// <summary>
    /// TitleScene의 패널 전환을 관리
    /// </summary>
    public class TitleSceneManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject titlePanel;
        public GameObject projectSelectPanel;
        public GameObject saveFileSelectPanel;
        public GameObject cgCollectionPanel;

        [Header("Current State")]
        public string selectedProjectGuid; // 현재 선택된 프로젝트

        private void Start()
        {
            ShowTitlePanel();
        }

        /// <summary>
        /// 타이틀 패널 표시 (메인 화면)
        /// </summary>
        public void ShowTitlePanel()
        {
            HideAllPanels();
            titlePanel.SetActive(true);
            selectedProjectGuid = null;
        }

        /// <summary>
        /// 프로젝트 선택 패널 표시
        /// </summary>
        public void ShowProjectSelectPanel()
        {
            HideAllPanels();
            projectSelectPanel.SetActive(true);
        }

        /// <summary>
        /// 저장 파일 선택 패널 표시
        /// </summary>
        public void ShowSaveFileSelectPanel(string projectGuid)
        {
            selectedProjectGuid = projectGuid;
            HideAllPanels();
            saveFileSelectPanel.SetActive(true);

            // SaveFileSelectPanel에 프로젝트 정보 전달
            var panel = saveFileSelectPanel.GetComponent<SaveFileSelectPanel>();
            if (panel != null)
            {
                panel.LoadSaveFiles(projectGuid);
            }
        }

        /// <summary>
        /// CG 컬렉션 패널 표시
        /// </summary>
        public void ShowCGCollectionPanel(string projectGuid)
        {
            selectedProjectGuid = projectGuid;
            HideAllPanels();
            cgCollectionPanel.SetActive(true);

            // CGCollectionPanel에 프로젝트 정보 전달
            var panel = cgCollectionPanel.GetComponent<CGCollectionPanel>();
            if (panel != null)
            {
                panel.LoadCGCollection(projectGuid);
            }
        }

        /// <summary>
        /// Setup Wizard 씬으로 전환
        /// </summary>
        public void OpenSetupWizard()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SetupWizardScene");
        }

        /// <summary>
        /// 게임 씬으로 전환 (저장 파일 로드)
        /// </summary>
        public void LoadGame(string saveFileId)
        {
            // SaveDataManager에 로드할 세이브 파일 정보 저장
            SaveDataManager.Instance.LoadSaveFile(saveFileId);

            // 게임 씬으로 전환
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }

        private void HideAllPanels()
        {
            titlePanel.SetActive(false);
            projectSelectPanel.SetActive(false);
            saveFileSelectPanel.SetActive(false);
            cgCollectionPanel.SetActive(false);
        }
    }
}
