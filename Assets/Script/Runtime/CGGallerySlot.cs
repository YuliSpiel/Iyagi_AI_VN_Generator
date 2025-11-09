using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// CG 갤러리 개별 슬롯
    /// 썸네일 표시 및 잠금/해금 상태 관리
    /// </summary>
    public class CGGallerySlot : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image thumbnailImage;
        public GameObject lockedOverlay; // 잠금 상태 오버레이
        public Image lockIcon;
        public TMP_Text cgNumberText; // "CG 01"
        public Button slotButton;

        [Header("Locked State")]
        public Sprite lockedSprite; // 잠긴 CG 대체 이미지 (물음표 등)
        public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private CGMetadata cgMetadata;
        private bool isUnlocked;
        private System.Action onClickCallback;

        public void Initialize(CGMetadata metadata, bool unlocked, System.Action onClick)
        {
            cgMetadata = metadata;
            isUnlocked = unlocked;
            onClickCallback = onClick;

            // 버튼 이벤트
            if (slotButton != null)
                slotButton.onClick.AddListener(OnSlotClicked);

            // 표시 업데이트
            UpdateDisplay();
        }

        /// <summary>
        /// 슬롯 표시 업데이트
        /// </summary>
        void UpdateDisplay()
        {
            if (isUnlocked)
            {
                ShowUnlockedState();
            }
            else
            {
                ShowLockedState();
            }

            // CG 번호 표시
            if (cgNumberText != null)
            {
                // cgId에서 번호 추출 (예: "1001_cg" -> "01")
                string numberStr = ExtractCGNumber(cgMetadata.cgId);
                cgNumberText.text = $"CG {numberStr}";
            }
        }

        /// <summary>
        /// 해금 상태 표시
        /// </summary>
        void ShowUnlockedState()
        {
            // 썸네일 로드
            Sprite thumbnail = LoadThumbnail();

            if (thumbnail != null && thumbnailImage != null)
            {
                thumbnailImage.sprite = thumbnail;
                thumbnailImage.color = Color.white;
            }

            // 잠금 오버레이 숨김
            if (lockedOverlay != null)
                lockedOverlay.SetActive(false);

            // 버튼 활성화
            if (slotButton != null)
                slotButton.interactable = true;
        }

        /// <summary>
        /// 잠금 상태 표시
        /// </summary>
        void ShowLockedState()
        {
            // 잠금 이미지 표시
            if (thumbnailImage != null)
            {
                if (lockedSprite != null)
                {
                    thumbnailImage.sprite = lockedSprite;
                }
                thumbnailImage.color = lockedColor;
            }

            // 잠금 오버레이 표시
            if (lockedOverlay != null)
                lockedOverlay.SetActive(true);

            // 버튼 비활성화
            if (slotButton != null)
                slotButton.interactable = false;
        }

        /// <summary>
        /// 썸네일 로드 (실제 CG 또는 별도 썸네일)
        /// </summary>
        Sprite LoadThumbnail()
        {
            // 1. 전용 썸네일이 있는지 확인
            Sprite thumbnail = Resources.Load<Sprite>($"Image/CG/Thumbnails/{cgMetadata.cgId}_thumb");

            if (thumbnail != null)
                return thumbnail;

            // 2. 없으면 원본 CG 사용
            return Resources.Load<Sprite>($"Image/CG/{cgMetadata.cgId}");
        }

        /// <summary>
        /// CG 번호 추출
        /// </summary>
        string ExtractCGNumber(string cgId)
        {
            // 예: "1001_cg" -> "01"
            // 예: "2015_cg" -> "15"

            if (string.IsNullOrEmpty(cgId))
                return "??";

            // cgId가 숫자로 시작하면 마지막 2자리 추출
            string numPart = "";
            foreach (char c in cgId)
            {
                if (char.IsDigit(c))
                    numPart += c;
                else
                    break;
            }

            if (numPart.Length >= 2)
            {
                return numPart.Substring(numPart.Length - 2);
            }
            else if (numPart.Length == 1)
            {
                return "0" + numPart;
            }

            return "??";
        }

        /// <summary>
        /// 슬롯 클릭 처리
        /// </summary>
        void OnSlotClicked()
        {
            if (isUnlocked)
            {
                onClickCallback?.Invoke();
            }
        }

        /// <summary>
        /// 해금 상태 변경 (외부에서 호출 가능)
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            UpdateDisplay();
        }
    }
}
