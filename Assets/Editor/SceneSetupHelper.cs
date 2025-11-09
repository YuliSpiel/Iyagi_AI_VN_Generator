using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using IyagiAI.Runtime;
using IyagiAI.SetupWizard;
using IyagiAI.AISystem;

/// <summary>
/// Scene 자동 구성 Helper
/// GameScene과 SetupWizardScene의 기본 UI 구조를 자동 생성
/// </summary>
public class SceneSetupHelper : EditorWindow
{
        // NotoSansKR 폰트 경로
        private static TMP_FontAsset notoSansKR;

        [MenuItem("Iyagi/Setup GameScene")]
        public static void SetupGameScene()
        {
            LoadFont();
            SetupGameSceneInternal();
        }

        [MenuItem("Iyagi/Setup SetupWizardScene")]
        public static void SetupWizardScene()
        {
            LoadFont();
            SetupWizardSceneInternal();
        }

        /// <summary>
        /// NotoSansKR 폰트 로드
        /// </summary>
        static void LoadFont()
        {
            if (notoSansKR == null)
            {
                notoSansKR = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NotoSansKR.asset");

                if (notoSansKR == null)
                {
                    Debug.LogWarning("NotoSansKR.asset not found at 'Assets/TextMesh Pro/Fonts/NotoSansKR.asset'");
                }
                else
                {
                    Debug.Log("NotoSansKR font loaded successfully!");
                }
            }
        }

        static void SetupGameSceneInternal()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // EventSystem 찾기 또는 생성
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // GameController 생성
            GameObject controllerObj = new GameObject("GameController");
            controllerObj.transform.SetParent(canvas.transform);
            GameController controller = controllerObj.AddComponent<GameController>();

            // Background Layer
            GameObject bgLayer = CreateUIObject("BackgroundLayer", canvas.transform);
            Image bgImage = bgLayer.AddComponent<Image>();
            bgImage.color = Color.black;
            SetFullScreen(bgLayer.GetComponent<RectTransform>());

            // Character Layer
            GameObject charLayer = CreateUIObject("CharacterLayer", canvas.transform);

            GameObject leftChar = CreateUIObject("LeftCharacter", charLayer.transform);
            Image leftImage = leftChar.AddComponent<Image>();
            CanvasGroup leftGroup = leftChar.AddComponent<CanvasGroup>();
            SetCharacterPosition(leftChar.GetComponent<RectTransform>(), -400, 0, 400, 800);

            GameObject rightChar = CreateUIObject("RightCharacter", charLayer.transform);
            Image rightImage = rightChar.AddComponent<Image>();
            CanvasGroup rightGroup = rightChar.AddComponent<CanvasGroup>();
            SetCharacterPosition(rightChar.GetComponent<RectTransform>(), 400, 0, 400, 800);

            GameObject centerChar = CreateUIObject("CenterCharacter", charLayer.transform);
            Image centerImage = centerChar.AddComponent<Image>();
            CanvasGroup centerGroup = centerChar.AddComponent<CanvasGroup>();
            SetCharacterPosition(centerChar.GetComponent<RectTransform>(), 0, 0, 400, 800);

            // CG Layer
            GameObject cgLayer = CreateUIObject("CGLayer", canvas.transform);
            CanvasGroup cgGroup = cgLayer.AddComponent<CanvasGroup>();
            Image cgImage = cgLayer.AddComponent<Image>();
            SetFullScreen(cgLayer.GetComponent<RectTransform>());
            cgLayer.SetActive(false);

            // Dialogue Box Layer
            GameObject dialogueBoxObj = CreateUIObject("DialogueBox", canvas.transform);
            Image dialogueBox = dialogueBoxObj.AddComponent<Image>();
            dialogueBox.color = new Color(0, 0, 0, 0.7f);
            CanvasGroup dialogueBoxGroup = dialogueBoxObj.AddComponent<CanvasGroup>();
            RectTransform dialogueRect = dialogueBoxObj.GetComponent<RectTransform>();
            dialogueRect.anchorMin = new Vector2(0.05f, 0.05f);
            dialogueRect.anchorMax = new Vector2(0.95f, 0.35f);
            dialogueRect.offsetMin = Vector2.zero;
            dialogueRect.offsetMax = Vector2.zero;

            GameObject speakerNameObj = CreateTextMeshPro("SpeakerNameText", dialogueBoxObj.transform);
            TMP_Text speakerText = speakerNameObj.GetComponent<TMP_Text>();
            speakerText.fontSize = 40;
            speakerText.fontStyle = FontStyles.Bold;
            RectTransform speakerRect = speakerNameObj.GetComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0.05f, 0.7f);
            speakerRect.anchorMax = new Vector2(0.5f, 0.95f);
            speakerRect.offsetMin = Vector2.zero;
            speakerRect.offsetMax = Vector2.zero;

            GameObject dialogueTextObj = CreateTextMeshPro("DialogueText", dialogueBoxObj.transform);
            TMP_Text dialogueText = dialogueTextObj.GetComponent<TMP_Text>();
            dialogueText.fontSize = 32;
            RectTransform dialogueTextRect = dialogueTextObj.GetComponent<RectTransform>();
            dialogueTextRect.anchorMin = new Vector2(0.05f, 0.1f);
            dialogueTextRect.anchorMax = new Vector2(0.95f, 0.65f);
            dialogueTextRect.offsetMin = Vector2.zero;
            dialogueTextRect.offsetMax = Vector2.zero;

            // Choice Panel
            GameObject choicePanel = CreateUIObject("ChoicePanel", canvas.transform);
            RectTransform choicePanelRect = choicePanel.GetComponent<RectTransform>();
            choicePanelRect.anchorMin = new Vector2(0.3f, 0.4f);
            choicePanelRect.anchorMax = new Vector2(0.7f, 0.6f);
            choicePanelRect.offsetMin = Vector2.zero;
            choicePanelRect.offsetMax = Vector2.zero;

            Button[] choiceButtons = new Button[4];
            TMP_Text[] choiceTexts = new TMP_Text[4];

            for (int i = 0; i < 4; i++)
            {
                GameObject buttonObj = CreateButton($"Choice{i + 1}Button", choicePanel.transform);
                choiceButtons[i] = buttonObj.GetComponent<Button>();
                choiceTexts[i] = buttonObj.GetComponentInChildren<TMP_Text>();

                RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
                float yPos = 0.75f - (i * 0.22f);
                btnRect.anchorMin = new Vector2(0.1f, yPos - 0.15f);
                btnRect.anchorMax = new Vector2(0.9f, yPos);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;
            }

            // Control Panel
            GameObject controlPanel = CreateUIObject("ControlPanel", canvas.transform);
            RectTransform controlRect = controlPanel.GetComponent<RectTransform>();
            controlRect.anchorMin = new Vector2(0.85f, 0.85f);
            controlRect.anchorMax = new Vector2(0.98f, 0.98f);
            controlRect.offsetMin = Vector2.zero;
            controlRect.offsetMax = Vector2.zero;

            Button nextButton = CreateButton("NextButton", controlPanel.transform).GetComponent<Button>();
            Button autoButton = CreateButton("AutoButton", controlPanel.transform).GetComponent<Button>();
            Button logButton = CreateButton("LogButton", controlPanel.transform).GetComponent<Button>();

            // DialogueUI 컴포넌트 추가 및 연결
            DialogueUI dialogueUI = canvas.gameObject.AddComponent<DialogueUI>();
            dialogueUI.leftCharacterImage = leftImage;
            dialogueUI.rightCharacterImage = rightImage;
            dialogueUI.centerCharacterImage = centerImage;
            dialogueUI.leftCharacterGroup = leftGroup;
            dialogueUI.rightCharacterGroup = rightGroup;
            dialogueUI.centerCharacterGroup = centerGroup;
            dialogueUI.speakerNameText = speakerText;
            dialogueUI.dialogueText = dialogueText;
            dialogueUI.dialogueBox = dialogueBox;
            dialogueUI.dialogueBoxGroup = dialogueBoxGroup;
            dialogueUI.cgImage = cgImage;
            dialogueUI.cgGroup = cgGroup;
            dialogueUI.backgroundImage = bgImage;
            dialogueUI.choicePanel = choicePanel;
            dialogueUI.choiceButtons = choiceButtons;
            dialogueUI.choiceTexts = choiceTexts;
            dialogueUI.nextButton = nextButton;
            dialogueUI.autoButton = autoButton;
            dialogueUI.logButton = logButton;

            // GameController 연결
            controller.dialogueUI = dialogueUI;

            Debug.Log("GameScene setup complete!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchoredPosition = Vector2.zero;
            return obj;
        }

        static GameObject CreateButton(string name, Transform parent)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = name.Replace("Button", "");
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;

            // NotoSansKR 폰트 배정
            if (notoSansKR != null)
            {
                text.font = notoSansKR;
            }

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;

            return btnObj;
        }

        static GameObject CreateTextMeshPro(string name, Transform parent)
        {
            GameObject textObj = CreateUIObject(name, parent);
            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;

            // NotoSansKR 폰트 배정
            if (notoSansKR != null)
            {
                text.font = notoSansKR;
            }

            return textObj;
        }

        static void SetFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void SetCharacterPosition(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        static void SetupWizardSceneInternal()
        {
            // Canvas 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // SetupWizardManager
            GameObject managerObj = new GameObject("SetupWizardManager");
            managerObj.transform.SetParent(canvas.transform);
            SetupWizardManager wizardManager = managerObj.AddComponent<SetupWizardManager>();

            // API Clients
            GeminiClient geminiClient = managerObj.AddComponent<GeminiClient>();
            NanoBananaClient nanoBananaClient = managerObj.AddComponent<NanoBananaClient>();
            CharacterFaceGenerator faceGenerator = managerObj.AddComponent<CharacterFaceGenerator>();

            // Step Panels (6개)
            GameObject[] stepPanels = new GameObject[6];

            for (int i = 0; i < 6; i++)
            {
                GameObject stepPanel = CreateUIObject($"Step{i + 1}_Panel", canvas.transform);
                Image panelBg = stepPanel.AddComponent<Image>();
                panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                SetFullScreen(stepPanel.GetComponent<RectTransform>());

                stepPanels[i] = stepPanel;

                // Step1 이외는 비활성화
                if (i > 0) stepPanel.SetActive(false);
            }

            // Step1: Game Overview
            SetupStep1(stepPanels[0], wizardManager);

            // Step2: Core Values
            SetupStep2(stepPanels[1], wizardManager);

            // Step3: Story Structure
            SetupStep3(stepPanels[2], wizardManager);

            // Step4: Player Character
            SetupStep4(stepPanels[3], wizardManager, faceGenerator);

            // Step5: NPCs
            SetupStep5(stepPanels[4], wizardManager, faceGenerator);

            // Step6: Finalize
            SetupStep6(stepPanels[5], wizardManager);

            // WizardManager 연결
            wizardManager.stepPanels = stepPanels;

            Debug.Log("SetupWizardScene setup complete!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        static void SetupStep1(GameObject panel, SetupWizardManager manager)
        {
            Step1_GameOverview step1 = panel.AddComponent<Step1_GameOverview>();
            step1.wizardManager = manager;

            // Title
            GameObject title = CreateTextMeshPro("Title", panel.transform);
            title.GetComponent<TMP_Text>().text = "Step 1: Game Overview";
            title.GetComponent<TMP_Text>().fontSize = 48;
            SetPosition(title.GetComponent<RectTransform>(), 0.1f, 0.85f, 0.9f, 0.95f);

            // Input Fields
            step1.titleInput = CreateInputField("GameTitleInput", panel.transform, "Game Title", 0.1f, 0.75f, 0.9f, 0.82f);
            step1.premiseInput = CreateInputField("PremiseInput", panel.transform, "Game Premise", 0.1f, 0.55f, 0.9f, 0.72f);

            // Dropdowns
            step1.genreDropdown = CreateDropdown("GenreDropdown", panel.transform, 0.1f, 0.45f, 0.45f, 0.52f);
            step1.toneDropdown = CreateDropdown("ToneDropdown", panel.transform, 0.55f, 0.45f, 0.9f, 0.52f);
            step1.playtimeDropdown = CreateDropdown("PlaytimeDropdown", panel.transform, 0.1f, 0.35f, 0.45f, 0.42f);

            // Buttons
            step1.autoFillButton = CreateButton("AutoFillButton", panel.transform).GetComponent<Button>();
            SetPosition(step1.autoFillButton.GetComponent<RectTransform>(), 0.55f, 0.35f, 0.9f, 0.42f);

            step1.nextStepButton = CreateButton("NextStepButton", panel.transform).GetComponent<Button>();
            SetPosition(step1.nextStepButton.GetComponent<RectTransform>(), 0.7f, 0.05f, 0.9f, 0.12f);
        }

        static void SetupStep2(GameObject panel, SetupWizardManager manager)
        {
            Step2_CoreValues step2 = panel.AddComponent<Step2_CoreValues>();
            step2.wizardManager = manager;

            GameObject title = CreateTextMeshPro("Title", panel.transform);
            title.GetComponent<TMP_Text>().text = "Step 2: Core Values + Derived Skills";
            title.GetComponent<TMP_Text>().fontSize = 48;
            SetPosition(title.GetComponent<RectTransform>(), 0.1f, 0.9f, 0.9f, 0.98f);

            // Core Value 1 (Required)
            step2.value1NameInput = CreateInputField("Value1NameInput", panel.transform, "Core Value 1 이름", 0.05f, 0.75f, 0.35f, 0.82f);
            step2.value1SkillsInput = CreateInputField("Value1SkillsInput", panel.transform, "파생 스탯 (쉼표로 구분)", 0.4f, 0.75f, 0.95f, 0.82f);

            // Core Value 2 (Required)
            step2.value2NameInput = CreateInputField("Value2NameInput", panel.transform, "Core Value 2 이름", 0.05f, 0.65f, 0.35f, 0.72f);
            step2.value2SkillsInput = CreateInputField("Value2SkillsInput", panel.transform, "파생 스탯 (쉼표로 구분)", 0.4f, 0.65f, 0.95f, 0.72f);

            // Core Value 3 (Optional)
            step2.value3NameInput = CreateInputField("Value3NameInput", panel.transform, "Core Value 3 이름 (선택)", 0.05f, 0.55f, 0.35f, 0.62f);
            step2.value3SkillsInput = CreateInputField("Value3SkillsInput", panel.transform, "파생 스탯 (쉼표로 구분)", 0.4f, 0.55f, 0.95f, 0.62f);

            // Core Value 4 (Optional)
            step2.value4NameInput = CreateInputField("Value4NameInput", panel.transform, "Core Value 4 이름 (선택)", 0.05f, 0.45f, 0.35f, 0.52f);
            step2.value4SkillsInput = CreateInputField("Value4SkillsInput", panel.transform, "파생 스탯 (쉼표로 구분)", 0.4f, 0.45f, 0.95f, 0.52f);

            step2.autoSuggestButton = CreateButton("AutoSuggestButton", panel.transform).GetComponent<Button>();
            SetPosition(step2.autoSuggestButton.GetComponent<RectTransform>(), 0.3f, 0.3f, 0.7f, 0.37f);

            step2.nextStepButton = CreateButton("NextStepButton", panel.transform).GetComponent<Button>();
            SetPosition(step2.nextStepButton.GetComponent<RectTransform>(), 0.7f, 0.05f, 0.9f, 0.12f);
        }

        static void SetupStep3(GameObject panel, SetupWizardManager manager)
        {
            Step3_StoryStructure step3 = panel.AddComponent<Step3_StoryStructure>();
            step3.wizardManager = manager;

            GameObject title = CreateTextMeshPro("Title", panel.transform);
            title.GetComponent<TMP_Text>().text = "Step 3: Story Structure";
            title.GetComponent<TMP_Text>().fontSize = 48;
            SetPosition(title.GetComponent<RectTransform>(), 0.1f, 0.85f, 0.9f, 0.95f);

            step3.chapterCountDropdown = CreateDropdown("ChapterCountDropdown", panel.transform, 0.1f, 0.7f, 0.5f, 0.77f);

            // 안내 텍스트 추가
            GameObject infoTextObj = CreateTextMeshPro("InfoText", panel.transform);
            step3.infoText = infoTextObj.GetComponent<TMP_Text>();
            step3.infoText.fontSize = 24;
            step3.infoText.alignment = TextAlignmentOptions.Center;
            SetPosition(infoTextObj.GetComponent<RectTransform>(), 0.1f, 0.4f, 0.9f, 0.6f);

            step3.nextStepButton = CreateButton("NextStepButton", panel.transform).GetComponent<Button>();
            SetPosition(step3.nextStepButton.GetComponent<RectTransform>(), 0.7f, 0.05f, 0.9f, 0.12f);
        }

        static void SetupStep4(GameObject panel, SetupWizardManager manager, CharacterFaceGenerator faceGen)
        {
            Step4_PlayerCharacter step4 = panel.AddComponent<Step4_PlayerCharacter>();
            step4.wizardManager = manager;
            step4.faceGenerator = faceGen;

            GameObject title = CreateTextMeshPro("Title", panel.transform);
            title.GetComponent<TMP_Text>().text = "Step 4: Player Character";
            title.GetComponent<TMP_Text>().fontSize = 48;
            SetPosition(title.GetComponent<RectTransform>(), 0.1f, 0.9f, 0.9f, 0.98f);

            // Left side: Character Info
            step4.nameInput = CreateInputField("NameInput", panel.transform, "Name", 0.05f, 0.78f, 0.45f, 0.85f);
            step4.ageInput = CreateInputField("AgeInput", panel.transform, "Age", 0.05f, 0.68f, 0.25f, 0.75f);
            step4.genderDropdown = CreateDropdown("GenderDropdown", panel.transform, 0.3f, 0.68f, 0.45f, 0.75f);
            step4.povDropdown = CreateDropdown("POVDropdown", panel.transform, 0.05f, 0.58f, 0.25f, 0.65f);
            step4.archetypeDropdown = CreateDropdown("ArchetypeDropdown", panel.transform, 0.3f, 0.58f, 0.45f, 0.65f);
            step4.appearanceInput = CreateInputField("AppearanceInput", panel.transform, "Appearance", 0.05f, 0.38f, 0.45f, 0.55f);
            step4.personalityInput = CreateInputField("PersonalityInput", panel.transform, "Personality", 0.05f, 0.18f, 0.45f, 0.35f);

            step4.autoFillButton = CreateButton("AutoFillButton", panel.transform).GetComponent<Button>();
            SetPosition(step4.autoFillButton.GetComponent<RectTransform>(), 0.05f, 0.08f, 0.45f, 0.15f);

            // Right side: Face Preview
            GameObject previewPanel = CreateUIObject("FacePreviewPanel", panel.transform);
            Image previewBg = previewPanel.AddComponent<Image>();
            previewBg.color = new Color(0.2f, 0.2f, 0.2f);
            SetPosition(previewPanel.GetComponent<RectTransform>(), 0.55f, 0.25f, 0.95f, 0.82f);

            GameObject previewImg = CreateUIObject("PreviewImage", previewPanel.transform);
            step4.previewImage = previewImg.AddComponent<Image>();
            SetPosition(previewImg.GetComponent<RectTransform>(), 0.1f, 0.3f, 0.9f, 0.9f);

            step4.generateFaceButton = CreateButton("GenerateFaceButton", previewPanel.transform).GetComponent<Button>();
            SetPosition(step4.generateFaceButton.GetComponent<RectTransform>(), 0.1f, 0.15f, 0.9f, 0.25f);

            GameObject navPanel = CreateUIObject("NavigationPanel", previewPanel.transform);
            SetPosition(navPanel.GetComponent<RectTransform>(), 0.1f, 0.05f, 0.9f, 0.12f);

            step4.previousButton = CreateButton("PreviousButton", navPanel.transform).GetComponent<Button>();
            SetPosition(step4.previousButton.GetComponent<RectTransform>(), 0f, 0f, 0.3f, 1f);

            GameObject indexText = CreateTextMeshPro("PreviewIndexText", navPanel.transform);
            step4.previewIndexText = indexText.GetComponent<TMP_Text>();
            step4.previewIndexText.alignment = TextAlignmentOptions.Center;
            SetPosition(indexText.GetComponent<RectTransform>(), 0.35f, 0f, 0.65f, 1f);

            step4.nextButton = CreateButton("NextButton", navPanel.transform).GetComponent<Button>();
            SetPosition(step4.nextButton.GetComponent<RectTransform>(), 0.7f, 0f, 1f, 1f);

            // Bottom Buttons
            step4.confirmButton = CreateButton("ConfirmButton", panel.transform).GetComponent<Button>();
            SetPosition(step4.confirmButton.GetComponent<RectTransform>(), 0.35f, 0.05f, 0.55f, 0.12f);

            step4.nextStepButton = CreateButton("NextStepButton", panel.transform).GetComponent<Button>();
            SetPosition(step4.nextStepButton.GetComponent<RectTransform>(), 0.7f, 0.05f, 0.9f, 0.12f);
        }

        static void SetupStep5(GameObject panel, SetupWizardManager manager, CharacterFaceGenerator faceGen)
        {
            Step5_NPCs step5 = panel.AddComponent<Step5_NPCs>();
            step5.wizardManager = manager;
            step5.faceGenerator = faceGen;

            GameObject title = CreateTextMeshPro("Title", panel.transform);
            title.GetComponent<TMP_Text>().text = "Step 5: NPCs (Optional)";
            title.GetComponent<TMP_Text>().fontSize = 48;
            SetPosition(title.GetComponent<RectTransform>(), 0.1f, 0.9f, 0.9f, 0.98f);

            // Similar layout to Step4 but with NPC-specific fields
            step5.nameInput = CreateInputField("NameInput", panel.transform, "NPC Name", 0.05f, 0.78f, 0.45f, 0.85f);
            step5.ageInput = CreateInputField("AgeInput", panel.transform, "Age", 0.05f, 0.68f, 0.25f, 0.75f);
            step5.genderDropdown = CreateDropdown("GenderDropdown", panel.transform, 0.3f, 0.68f, 0.45f, 0.75f);
            step5.archetypeDropdown = CreateDropdown("ArchetypeDropdown", panel.transform, 0.05f, 0.58f, 0.25f, 0.65f);
            step5.roleInput = CreateInputField("RoleInput", panel.transform, "Role (Friend/Rival/etc)", 0.3f, 0.58f, 0.45f, 0.65f);
            step5.appearanceInput = CreateInputField("AppearanceInput", panel.transform, "Appearance", 0.05f, 0.38f, 0.45f, 0.55f);
            step5.personalityInput = CreateInputField("PersonalityInput", panel.transform, "Personality", 0.05f, 0.18f, 0.45f, 0.35f);

            GameObject romanceToggleObj = CreateUIObject("RomanceableToggle", panel.transform);
            step5.romanceableToggle = romanceToggleObj.AddComponent<Toggle>();
            SetPosition(romanceToggleObj.GetComponent<RectTransform>(), 0.05f, 0.08f, 0.2f, 0.15f);

            step5.autoFillButton = CreateButton("AutoFillButton", panel.transform).GetComponent<Button>();
            SetPosition(step5.autoFillButton.GetComponent<RectTransform>(), 0.25f, 0.08f, 0.45f, 0.15f);

            // Face Preview (same as Step4)
            GameObject previewPanel = CreateUIObject("FacePreviewPanel", panel.transform);
            Image previewBg = previewPanel.AddComponent<Image>();
            previewBg.color = new Color(0.2f, 0.2f, 0.2f);
            SetPosition(previewPanel.GetComponent<RectTransform>(), 0.55f, 0.25f, 0.95f, 0.82f);

            GameObject previewImg = CreateUIObject("PreviewImage", previewPanel.transform);
            step5.previewImage = previewImg.AddComponent<Image>();
            SetPosition(previewImg.GetComponent<RectTransform>(), 0.1f, 0.3f, 0.9f, 0.9f);

            step5.generateFaceButton = CreateButton("GenerateFaceButton", previewPanel.transform).GetComponent<Button>();
            SetPosition(step5.generateFaceButton.GetComponent<RectTransform>(), 0.1f, 0.15f, 0.9f, 0.25f);

            GameObject navPanel = CreateUIObject("NavigationPanel", previewPanel.transform);
            SetPosition(navPanel.GetComponent<RectTransform>(), 0.1f, 0.05f, 0.9f, 0.12f);

            step5.previousButton = CreateButton("PreviousButton", navPanel.transform).GetComponent<Button>();
            step5.nextButton = CreateButton("NextButton", navPanel.transform).GetComponent<Button>();
            GameObject indexText = CreateTextMeshPro("PreviewIndexText", navPanel.transform);
            step5.previewIndexText = indexText.GetComponent<TMP_Text>();

            // Bottom Buttons
            step5.confirmButton = CreateButton("ConfirmButton", panel.transform).GetComponent<Button>();
            SetPosition(step5.confirmButton.GetComponent<RectTransform>(), 0.2f, 0.05f, 0.4f, 0.12f);

            step5.addAnotherButton = CreateButton("AddAnotherButton", panel.transform).GetComponent<Button>();
            SetPosition(step5.addAnotherButton.GetComponent<RectTransform>(), 0.45f, 0.05f, 0.65f, 0.12f);

            step5.finishButton = CreateButton("FinishButton", panel.transform).GetComponent<Button>();
            SetPosition(step5.finishButton.GetComponent<RectTransform>(), 0.7f, 0.05f, 0.9f, 0.12f);
        }

        static void SetupStep6(GameObject panel, SetupWizardManager manager)
        {
            Step6_Finalize step6 = panel.AddComponent<Step6_Finalize>();
            step6.wizardManager = manager;

            GameObject title = CreateTextMeshPro("Title", panel.transform);
            title.GetComponent<TMP_Text>().text = "Step 6: Final Confirmation";
            title.GetComponent<TMP_Text>().fontSize = 48;
            SetPosition(title.GetComponent<RectTransform>(), 0.1f, 0.9f, 0.9f, 0.98f);

            // Summary ScrollView
            GameObject scrollView = new GameObject("SummaryScrollView");
            scrollView.transform.SetParent(panel.transform);
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            step6.summaryScrollRect = scrollRect;
            SetPosition(scrollView.GetComponent<RectTransform>(), 0.1f, 0.2f, 0.9f, 0.85f);

            GameObject viewport = CreateUIObject("Viewport", scrollView.transform);
            Image viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = new Color(0.15f, 0.15f, 0.15f);
            viewport.AddComponent<Mask>().showMaskGraphic = true;
            SetFullScreen(viewport.GetComponent<RectTransform>());

            GameObject content = CreateUIObject("Content", viewport.transform);
            GameObject summaryTextObj = CreateTextMeshPro("SummaryText", content.transform);
            step6.summaryText = summaryTextObj.GetComponent<TMP_Text>();
            step6.summaryText.alignment = TextAlignmentOptions.TopLeft;

            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Buttons
            step6.backButton = CreateButton("BackButton", panel.transform).GetComponent<Button>();
            SetPosition(step6.backButton.GetComponent<RectTransform>(), 0.1f, 0.05f, 0.3f, 0.12f);

            step6.createProjectButton = CreateButton("CreateProjectButton", panel.transform).GetComponent<Button>();
            SetPosition(step6.createProjectButton.GetComponent<RectTransform>(), 0.7f, 0.05f, 0.9f, 0.12f);
        }

        static TMP_InputField CreateInputField(string name, Transform parent, string placeholder, float xMin, float yMin, float xMax, float yMax)
        {
            GameObject inputObj = CreateUIObject(name, parent);
            Image bg = inputObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f);

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

            GameObject textArea = CreateUIObject("Text Area", inputObj.transform);
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 6);
            textAreaRect.offsetMax = new Vector2(-10, -6);

            GameObject textObj = CreateTextMeshPro("Text", textArea.transform);
            TMP_Text text = textObj.GetComponent<TMP_Text>();
            text.color = Color.white;
            SetFullScreen(textObj.GetComponent<RectTransform>());

            GameObject placeholderObj = CreateTextMeshPro("Placeholder", textArea.transform);
            TMP_Text placeholderText = placeholderObj.GetComponent<TMP_Text>();
            placeholderText.text = placeholder;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderText.fontStyle = FontStyles.Italic;
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());

            inputField.textViewport = textAreaRect;
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;

            SetPosition(inputObj.GetComponent<RectTransform>(), xMin, yMin, xMax, yMax);
            return inputField;
        }

        static TMP_Dropdown CreateDropdown(string name, Transform parent, float xMin, float yMin, float xMax, float yMax)
        {
            GameObject dropdownObj = CreateUIObject(name, parent);
            Image bg = dropdownObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // Label
            GameObject label = CreateTextMeshPro("Label", dropdownObj.transform);
            dropdown.captionText = label.GetComponent<TMP_Text>();
            SetPosition(label.GetComponent<RectTransform>(), 0.1f, 0f, 0.9f, 1f);

            // Arrow
            GameObject arrow = CreateUIObject("Arrow", dropdownObj.transform);
            Image arrowImg = arrow.AddComponent<Image>();
            arrowImg.color = Color.white;
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-15, 0);

            // Template
            GameObject template = CreateUIObject("Template", dropdownObj.transform);
            template.SetActive(false);
            Image templateBg = template.AddComponent<Image>();
            templateBg.color = new Color(0.25f, 0.25f, 0.25f);
            RectTransform templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0, 150);
            templateRect.anchoredPosition = new Vector2(0, 0);

            // Viewport
            GameObject viewport = CreateUIObject("Viewport", template.transform);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            Image viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = new Color(0.25f, 0.25f, 0.25f);
            SetFullScreen(viewport.GetComponent<RectTransform>());

            // Content
            GameObject content = CreateUIObject("Content", viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 28);

            // Item
            GameObject item = CreateUIObject("Item", content.transform);
            Toggle itemToggle = item.AddComponent<Toggle>();
            Image itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.3f, 0.3f, 0.3f);
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 20);

            // Item Background
            GameObject itemBackground = CreateUIObject("Item Background", item.transform);
            Image itemBgImg = itemBackground.AddComponent<Image>();
            itemBgImg.color = new Color(0.4f, 0.4f, 0.4f);
            SetFullScreen(itemBackground.GetComponent<RectTransform>());

            // Item Checkmark
            GameObject itemCheckmark = CreateUIObject("Item Checkmark", item.transform);
            Image checkImg = itemCheckmark.AddComponent<Image>();
            checkImg.color = Color.white;
            RectTransform checkRect = itemCheckmark.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0, 0.5f);
            checkRect.anchorMax = new Vector2(0, 0.5f);
            checkRect.sizeDelta = new Vector2(20, 20);
            checkRect.anchoredPosition = new Vector2(10, 0);

            // Item Label
            GameObject itemLabel = CreateTextMeshPro("Item Label", item.transform);
            TMP_Text itemText = itemLabel.GetComponent<TMP_Text>();
            itemText.alignment = TextAlignmentOptions.Left;
            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(20, 1);
            itemLabelRect.offsetMax = new Vector2(-5, -2);

            itemToggle.targetGraphic = itemBgImg;
            itemToggle.graphic = checkImg;
            itemToggle.isOn = true;

            // ScrollRect
            ScrollRect scrollRect = template.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Set dropdown properties
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = label.GetComponent<TMP_Text>();
            dropdown.itemText = itemText;

            SetPosition(dropdownObj.GetComponent<RectTransform>(), xMin, yMin, xMax, yMax);
            return dropdown;
        }

        static void SetPosition(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
}
