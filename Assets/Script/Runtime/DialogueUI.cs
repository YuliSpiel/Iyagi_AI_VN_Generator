using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 대화 UI 컨트롤러
    /// DialogueRecord를 화면에 표시하고 선택지 처리
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("Character Display")]
        public Image leftCharacterImage;
        public Image rightCharacterImage;
        public Image centerCharacterImage;
        public CanvasGroup leftCharacterGroup;
        public CanvasGroup rightCharacterGroup;
        public CanvasGroup centerCharacterGroup;

        [Header("Dialogue Display")]
        public TMP_Text speakerNameText;
        public TMP_Text dialogueText;
        public Image dialogueBox;
        public CanvasGroup dialogueBoxGroup;

        [Header("CG Display")]
        public Image cgImage;
        public CanvasGroup cgGroup;

        [Header("Background")]
        public Image backgroundImage;

        [Header("Choice Display")]
        public GameObject choicePanel;
        public Button[] choiceButtons; // 최대 4개
        public TMP_Text[] choiceTexts;

        [Header("Control Buttons")]
        public Button nextButton;
        public Button autoButton;
        public Button skipButton;
        public Button logButton;
        public Button saveButton;
        public Button loadButton;
        public Button settingsButton;

        [Header("UI References")]
        public DialogueLogUI dialogueLogUI;

        [Header("Settings")]
        public float textSpeed = 0.05f;
        public float autoPlayDelay = 2f;

        private DialogueRecord currentRecord;
        private bool isTextComplete = false;
        private bool isAutoPlay = false;
        private Coroutine typewriterCoroutine;
        private System.Action<int> onChoiceSelected;

        void Start()
        {
            // 버튼 이벤트 연결
            nextButton.onClick.AddListener(OnNextClicked);
            autoButton.onClick.AddListener(OnAutoToggled);
            skipButton.onClick.AddListener(OnSkipClicked);

            if (logButton != null)
                logButton.onClick.AddListener(OnLogClicked);

            // Choice 버튼 초기화
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i; // 클로저 문제 방지
                choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(index));
            }

            // 초기 상태
            HideChoices();
            HideCG();
        }

        /// <summary>
        /// DialogueRecord 표시
        /// </summary>
        public void DisplayRecord(DialogueRecord record, System.Action<int> choiceCallback = null)
        {
            currentRecord = record;
            onChoiceSelected = choiceCallback;
            isTextComplete = false;

            // CG 표시
            if (record.HasCG())
            {
                DisplayCG(record.GetString("CG_ID"));
            }
            else
            {
                HideCG();
            }

            // 배경 변경
            string bgName = record.GetString("Background");
            if (!string.IsNullOrEmpty(bgName))
            {
                ChangeBackground(bgName);
            }

            // 캐릭터 표시
            DisplayCharacters(record);

            // 대화 표시
            string speaker = record.GetString("Speaker");
            string dialogue = record.GetString("Dialogue");

            speakerNameText.text = speaker;

            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = StartCoroutine(TypewriterEffect(dialogue));

            // 대화 로그에 추가
            if (dialogueLogUI != null && !string.IsNullOrEmpty(dialogue))
            {
                dialogueLogUI.AddDialogue(speaker, dialogue);
            }

            // 선택지 표시
            if (record.HasChoices())
            {
                StartCoroutine(ShowChoicesAfterText(record));
            }
            else
            {
                HideChoices();
            }
        }

        IEnumerator TypewriterEffect(string fullText)
        {
            dialogueText.text = "";
            isTextComplete = false;

            foreach (char c in fullText)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

            isTextComplete = true;

            // Auto Play 모드라면 자동 진행
            if (isAutoPlay && !currentRecord.HasChoices())
            {
                yield return new WaitForSeconds(autoPlayDelay);
                OnNextClicked();
            }
        }

        IEnumerator ShowChoicesAfterText(DialogueRecord record)
        {
            // 텍스트가 완료될 때까지 대기
            yield return new WaitUntil(() => isTextComplete);

            // 선택지 표시
            int choiceCount = record.GetChoiceCount();
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < choiceCount)
                {
                    string choiceKey = $"Choice{i + 1}";
                    choiceTexts[i].text = record.GetString(choiceKey);
                    choiceButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }

            choicePanel.SetActive(true);
            nextButton.gameObject.SetActive(false); // 선택지 있으면 Next 버튼 숨김
        }

        void DisplayCharacters(DialogueRecord record)
        {
            string leftChar = record.GetString("Left_Character");
            string rightChar = record.GetString("Right_Character");
            string centerChar = record.GetString("Center_Character");

            // Left Character
            if (!string.IsNullOrEmpty(leftChar))
            {
                ShowCharacter(leftCharacterImage, leftCharacterGroup, leftChar, record, "Left");
            }
            else
            {
                HideCharacter(leftCharacterGroup);
            }

            // Right Character
            if (!string.IsNullOrEmpty(rightChar))
            {
                ShowCharacter(rightCharacterImage, rightCharacterGroup, rightChar, record, "Right");
            }
            else
            {
                HideCharacter(rightCharacterGroup);
            }

            // Center Character
            if (!string.IsNullOrEmpty(centerChar))
            {
                ShowCharacter(centerCharacterImage, centerCharacterGroup, centerChar, record, "Center");
            }
            else
            {
                HideCharacter(centerCharacterGroup);
            }
        }

        void ShowCharacter(Image charImage, CanvasGroup charGroup, string charName, DialogueRecord record, string position)
        {
            // Expression과 Pose 가져오기
            string exprKey = $"{position}_Expression";
            string poseKey = $"{position}_Pose";

            string expr = record.GetString(exprKey);
            string pose = record.GetString(poseKey);

            if (string.IsNullOrEmpty(expr)) expr = "Neutral";
            if (string.IsNullOrEmpty(pose)) pose = "Normal";

            // 스프라이트 로드 (RuntimeSpriteManager 사용)
            var spriteManager = FindObjectOfType<RuntimeSpriteManager>();
            if (spriteManager != null)
            {
                Expression expression = ParseExpression(expr);
                Pose poseEnum = ParsePose(pose);

                StartCoroutine(spriteManager.GetOrGenerateSprite(
                    charName,
                    expression,
                    poseEnum,
                    (sprite) => {
                        if (sprite != null)
                        {
                            charImage.sprite = sprite;
                            charImage.SetNativeSize();
                            ShowCharacter(charGroup);
                        }
                    }
                ));
            }
        }

        void ShowCharacter(CanvasGroup group)
        {
            group.alpha = 1f;
            group.gameObject.SetActive(true);
        }

        void HideCharacter(CanvasGroup group)
        {
            group.alpha = 0f;
            // 완전히 비활성화하지 않고 투명하게만 처리 (레이아웃 유지)
        }

        void DisplayCG(string cgId)
        {
            if (string.IsNullOrEmpty(cgId)) return;

            // CG 로드
            Sprite cgSprite = Resources.Load<Sprite>($"Image/CG/{cgId}");
            if (cgSprite != null)
            {
                cgImage.sprite = cgSprite;
                cgGroup.alpha = 1f;
                cgGroup.gameObject.SetActive(true);

                // CG 표시 시 대화창 숨김 (옵션)
                dialogueBoxGroup.alpha = 0.7f; // 반투명
            }
        }

        void HideCG()
        {
            cgGroup.alpha = 0f;
            cgGroup.gameObject.SetActive(false);
            dialogueBoxGroup.alpha = 1f; // 완전 불투명
        }

        void ChangeBackground(string bgName)
        {
            Sprite bgSprite = Resources.Load<Sprite>($"Image/Background/{bgName}");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
            }
        }

        void HideChoices()
        {
            choicePanel.SetActive(false);
            nextButton.gameObject.SetActive(true);
        }

        public void OnNextClicked()
        {
            if (!isTextComplete)
            {
                // 텍스트 타이핑 중이면 즉시 완료
                if (typewriterCoroutine != null)
                {
                    StopCoroutine(typewriterCoroutine);
                }
                dialogueText.text = currentRecord.GetString("Dialogue");
                isTextComplete = true;
            }
            else
            {
                // 다음 대화로 진행 (GameController에 알림)
                onChoiceSelected?.Invoke(-1); // -1 = 일반 진행
            }
        }

        void OnChoiceClicked(int choiceIndex)
        {
            // 선택지 로그에 추가
            if (dialogueLogUI != null && choiceTexts != null && choiceIndex < choiceTexts.Length)
            {
                string choiceText = choiceTexts[choiceIndex].text;
                dialogueLogUI.AddChoice(choiceText);
            }

            HideChoices();
            onChoiceSelected?.Invoke(choiceIndex);
        }

        void OnLogClicked()
        {
            if (dialogueLogUI != null)
            {
                dialogueLogUI.OpenLog();
            }
        }

        void OnAutoToggled()
        {
            isAutoPlay = !isAutoPlay;
            // Auto 버튼 색상 변경 등 UI 피드백
            var colors = autoButton.colors;
            colors.normalColor = isAutoPlay ? Color.yellow : Color.white;
            autoButton.colors = colors;
        }

        void OnSkipClicked()
        {
            // Skip 기능 (챕터 끝까지 빠르게 진행)
            // GameController에서 구현 필요
            Debug.Log("Skip requested");
        }

        // Enum 파싱 헬퍼
        Expression ParseExpression(string expr)
        {
            if (System.Enum.TryParse<Expression>(expr, true, out Expression result))
                return result;
            return Expression.Neutral;
        }

        Pose ParsePose(string pose)
        {
            if (System.Enum.TryParse<Pose>(pose, true, out Pose result))
                return result;
            return Pose.Normal;
        }
    }
}
