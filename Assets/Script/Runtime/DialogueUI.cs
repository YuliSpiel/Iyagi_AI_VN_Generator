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
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (autoButton != null)
                autoButton.onClick.AddListener(OnAutoToggled);
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
            if (logButton != null)
                logButton.onClick.AddListener(OnLogClicked);

            // Choice 버튼 초기화
            if (choiceButtons != null)
            {
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] != null)
                    {
                        int index = i; // 클로저 문제 방지
                        choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(index));
                    }
                }
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
            Debug.Log($"[DialogueUI] DisplayRecord called");
            Debug.Log($"[DialogueUI] Record has fields: {string.Join(", ", record.Fields.Keys)}");
            currentRecord = record;
            onChoiceSelected = choiceCallback;
            isTextComplete = false;

            // BGM 재생 (BGM 필드 사용 - AIDataConverter에서 매핑됨)
            string bgmName = record.GetString("BGM");
            if (!string.IsNullOrEmpty(bgmName))
            {
                PlayBGM(bgmName);
            }

            // SFX 재생 (SFX 필드 사용 - AIDataConverter에서 매핑됨)
            string sfxName = record.GetString("SFX");
            if (!string.IsNullOrEmpty(sfxName))
            {
                PlaySFX(sfxName);
            }

            // CG 표시
            if (record.HasCG())
            {
                DisplayCG(record.GetString("CG_ID"));
            }
            else
            {
                HideCG();
            }

            // 배경 변경 (BG 필드 사용 - AIDataConverter에서 매핑됨)
            string bgName = record.GetString("BG");
            Debug.Log($"[DialogueUI] BG field value: '{bgName}'");
            if (!string.IsNullOrEmpty(bgName))
            {
                ChangeBackground(bgName);
            }
            else
            {
                Debug.LogWarning("[DialogueUI] BG field is empty or null - background will not change");
            }

            // 캐릭터 표시
            DisplayCharacters(record);

            // 대화 표시 - NameTag와 ParsedLine_ENG 필드 사용
            string speaker = record.GetString("NameTag");
            string dialogue = record.GetString("ParsedLine_ENG");

            Debug.Log($"[DialogueUI] Speaker: '{speaker}', Dialogue: '{dialogue}'");

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
            Debug.Log($"[DialogueUI] Displaying {choiceCount} choices");

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < choiceCount)
                {
                    // Choice{i}_ENG, Choice{i}_KOR, Choice{i} 순으로 폴백
                    string choiceText = record.GetString($"Choice{i + 1}_ENG");
                    if (string.IsNullOrEmpty(choiceText))
                    {
                        choiceText = record.GetString($"Choice{i + 1}_KOR");
                    }
                    if (string.IsNullOrEmpty(choiceText))
                    {
                        choiceText = record.GetString($"Choice{i + 1}");
                    }

                    Debug.Log($"[DialogueUI] Choice {i + 1}: {choiceText}");
                    choiceTexts[i].text = choiceText;
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
            // CreateDummyProject와 AI 생성 데이터는 Char1Name, Char1Pos 형식 사용
            // 최대 2명의 캐릭터 지원 (Char1, Char2)

            // 모든 캐릭터 숨기기
            HideCharacter(leftCharacterGroup);
            HideCharacter(rightCharacterGroup);
            HideCharacter(centerCharacterGroup);

            // Char1 표시
            string char1Name = record.GetString("Char1Name");
            if (!string.IsNullOrEmpty(char1Name))
            {
                string char1Pos = record.GetString("Char1Pos");
                ShowCharacterByPosition(char1Name, char1Pos, record, "Char1");
            }

            // Char2 표시
            string char2Name = record.GetString("Char2Name");
            if (!string.IsNullOrEmpty(char2Name))
            {
                string char2Pos = record.GetString("Char2Pos");
                ShowCharacterByPosition(char2Name, char2Pos, record, "Char2");
            }
        }

        void ShowCharacterByPosition(string charName, string position, DialogueRecord record, string charPrefix)
        {
            Image targetImage = null;
            CanvasGroup targetGroup = null;

            // 위치에 따라 이미지 슬롯 선택
            if (position == "Left")
            {
                targetImage = leftCharacterImage;
                targetGroup = leftCharacterGroup;
            }
            else if (position == "Right")
            {
                targetImage = rightCharacterImage;
                targetGroup = rightCharacterGroup;
            }
            else if (position == "Center")
            {
                targetImage = centerCharacterImage;
                targetGroup = centerCharacterGroup;
            }

            if (targetImage != null && targetGroup != null)
            {
                ShowCharacter(targetImage, targetGroup, charName, record, charPrefix);
            }
        }

        void ShowCharacter(Image charImage, CanvasGroup charGroup, string charName, DialogueRecord record, string charPrefix)
        {
            // Expression과 Pose 가져오기 (Char1Look, Char2Look 형식)
            string lookKey = $"{charPrefix}Look";
            string lookValue = record.GetString(lookKey);

            // "happy_normal" 형식 파싱
            string expr = "neutral";
            string pose = "normal";

            if (!string.IsNullOrEmpty(lookValue))
            {
                string[] parts = lookValue.Split('_');
                if (parts.Length >= 1) expr = parts[0];
                if (parts.Length >= 2) pose = parts[1];
            }

            // Size 가져오기 (Char1Size, Char2Size)
            string sizeKey = $"{charPrefix}Size";
            string sizeValue = record.GetString(sizeKey);
            float targetScale = GetScaleForSize(sizeValue);

            // 스프라이트 로드 (TestResources에서 직접 로드, 생성 안 함!)
            string spritePath = $"TestResources/Standing/{charName}/{lookValue}";
            Debug.Log($"[DialogueUI] Loading character sprite: {spritePath}");

            Sprite characterSprite = Resources.Load<Sprite>(spritePath);

            if (characterSprite != null)
            {
                charImage.sprite = characterSprite;

                // 슬롯 크기에 맞춰 이미지 크기 설정 (바닥에 붙이고 영역 안에 맞춤)
                RectTransform imageRect = charImage.GetComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.sizeDelta = Vector2.zero; // 슬롯 전체 채우기
                imageRect.anchoredPosition = Vector2.zero;

                // ✅ 스케일 적용 (발화 중인 캐릭터 강조)
                charImage.transform.localScale = Vector3.one * targetScale;

                ShowCharacter(charGroup);
                Debug.Log($"[DialogueUI] ✅ Character sprite loaded: {spritePath}");
            }
            else
            {
                Debug.LogWarning($"[DialogueUI] ❌ Character sprite not found: {spritePath}");

                // 디버깅: 사용 가능한 스프라이트 목록 표시
                var allSprites = Resources.LoadAll<Sprite>($"TestResources/Standing/{charName}");
                Debug.Log($"[DialogueUI] Available sprites for {charName}: {allSprites.Length}");
                foreach (var s in allSprites)
                {
                    Debug.Log($"  - {s.name}");
                }
            }
        }

        /// <summary>
        /// Size 문자열을 스케일 값으로 변환
        /// </summary>
        float GetScaleForSize(string size)
        {
            switch (size)
            {
                case "Large":
                    return 1.2f; // 발화 중 (20% 확대)
                case "Medium":
                    return 1.0f; // 기본 크기
                case "Small":
                    return 0.8f; // 축소
                default:
                    return 1.0f; // 기본값
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
            if (string.IsNullOrEmpty(bgName))
                return;

            Sprite bgSprite = null;
            Debug.Log($"[DialogueUI] Attempting to load background: '{bgName}'");

            // 1. 전체 경로로 로드 시도
            bgSprite = Resources.Load<Sprite>(bgName);
            if (bgSprite != null)
            {
                Debug.Log($"[DialogueUI] ✅ Background loaded from full path: {bgName}");
            }

            // 2. Image/Background/ 경로로 시도
            if (bgSprite == null)
            {
                string path2 = $"Image/Background/{bgName}";
                Debug.Log($"[DialogueUI] Trying path: {path2}");
                bgSprite = Resources.Load<Sprite>(path2);
                if (bgSprite != null)
                {
                    Debug.Log($"[DialogueUI] ✅ Background loaded from: {path2}");
                }
            }

            // 3. 파일명만 추출하여 재시도
            if (bgSprite == null)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(bgName);
                string path3 = $"Image/Background/{fileName}";
                Debug.Log($"[DialogueUI] Trying path with extracted filename: {path3}");
                bgSprite = Resources.Load<Sprite>(path3);
                if (bgSprite != null)
                {
                    Debug.Log($"[DialogueUI] ✅ Background loaded from: {path3}");
                }
            }

            // 4. TestResources에서 직접 시도
            if (bgSprite == null)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(bgName);
                string path4 = $"TestResources/Background/{fileName}";
                Debug.Log($"[DialogueUI] Trying TestResources path: {path4}");
                bgSprite = Resources.Load<Sprite>(path4);
                if (bgSprite != null)
                {
                    Debug.Log($"[DialogueUI] ✅ Background loaded from: {path4}");
                }
            }

            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                Debug.Log($"[DialogueUI] ✅ Background successfully applied!");
            }
            else
            {
                Debug.LogError($"[DialogueUI] ❌ Background not found after all attempts: {bgName}");

                // 디버깅: Resources 폴더 내용 확인
                var allBackgrounds = Resources.LoadAll<Sprite>("TestResources/Background");
                Debug.Log($"[DialogueUI] Available backgrounds in TestResources/Background: {allBackgrounds.Length}");
                foreach (var bg in allBackgrounds)
                {
                    Debug.Log($"  - {bg.name}");
                }
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
                dialogueText.text = currentRecord.GetString("ParsedLine_ENG");
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

        // BGM/SFX 재생 헬퍼
        void PlayBGM(string bgmName)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBGM(bgmName);
            }
            else
            {
                Debug.LogWarning("[DialogueUI] SoundManager not found! Cannot play BGM.");
            }
        }

        void PlaySFX(string sfxName)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(sfxName);
            }
            else
            {
                Debug.LogWarning("[DialogueUI] SoundManager not found! Cannot play SFX.");
            }
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
