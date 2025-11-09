using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;

namespace IyagiAI.TitleScene
{
    /// <summary>
    /// 타이틀 메인 화면
    /// Continue / Load Game / New Game
    /// </summary>
    public class TitlePanel : MonoBehaviour
    {
        [Header("References")]
        public TitleSceneManager sceneManager;

        [Header("Buttons")]
        public Button continueButton;
        public Button loadGameButton;
        public Button newGameButton;

        [Header("UI Text")]
        public TMP_Text titleText;

        private void Start()
        {
            // 게임 타이틀 설정
            if (titleText != null)
            {
                titleText.text = "Iyagi AI VN Generator";
            }

            // Continue 버튼: 가장 최근 플레이한 저장 파일이 있으면 활성화
            var lastSaveFile = SaveDataManager.Instance.GetLastPlayedSaveFile();
            continueButton.interactable = lastSaveFile != null;

            // 버튼 이벤트 연결
            continueButton.onClick.AddListener(OnContinueClicked);
            loadGameButton.onClick.AddListener(OnLoadGameClicked);
            newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        private void OnContinueClicked()
        {
            var lastSaveFile = SaveDataManager.Instance.GetLastPlayedSaveFile();
            if (lastSaveFile != null)
            {
                sceneManager.LoadGame(lastSaveFile.saveFileId);
            }
        }

        private void OnLoadGameClicked()
        {
            sceneManager.ShowProjectSelectPanel();
        }

        private void OnNewGameClicked()
        {
            sceneManager.OpenSetupWizard();
        }
    }
}
