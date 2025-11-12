using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 우상단에 표시되는 디버그 정보 패널
    /// 현재 챕터, 대사 진행 상황 표시
    /// </summary>
    public class DebugInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Text chapterText;
        public TMP_Text lineProgressText;
        public TMP_Text totalLinesText;
        public Toggle visibilityToggle;
        public CanvasGroup canvasGroup;

        private GameController gameController;

        void Start()
        {
            // GameController 참조 가져오기
            gameController = FindObjectOfType<GameController>();

            if (gameController == null)
            {
                Debug.LogWarning("[DebugInfoPanel] GameController not found!");
            }

            // 토글 이벤트 연결
            if (visibilityToggle != null)
            {
                visibilityToggle.onValueChanged.AddListener(OnVisibilityToggled);
                visibilityToggle.isOn = true; // 기본적으로 표시
            }

            // 초기 업데이트
            UpdateDisplay();
        }

        void Update()
        {
            // 매 프레임 정보 업데이트
            UpdateDisplay();
        }

        /// <summary>
        /// 표시 정보 업데이트
        /// </summary>
        void UpdateDisplay()
        {
            if (gameController == null)
                return;

            // 챕터 정보
            if (chapterText != null)
            {
                chapterText.text = $"Chapter {gameController.currentChapterId}";
            }

            // 대사 진행 상황
            if (lineProgressText != null)
            {
                int currentLine = gameController.currentLineIndex + 1; // 0-based → 1-based
                lineProgressText.text = $"Line {currentLine}";
            }

            // 총 대사 수
            if (totalLinesText != null)
            {
                int totalLines = gameController.currentChapterRecords?.Count ?? 0;
                totalLinesText.text = $"/ {totalLines}";
            }
        }

        /// <summary>
        /// 패널 표시/숨김 토글
        /// </summary>
        void OnVisibilityToggled(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
        }

        /// <summary>
        /// 외부에서 패널 표시/숨김
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visibilityToggle != null)
            {
                visibilityToggle.isOn = visible;
            }
            else
            {
                OnVisibilityToggled(visible);
            }
        }
    }
}
