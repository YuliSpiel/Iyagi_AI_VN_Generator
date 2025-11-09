using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;
using System.Collections;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 5: NPC 생성
    /// Step4와 동일한 프로세스, 여러 NPC 추가 가능
    /// </summary>
    public class Step5_NPCs : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;
        public CharacterFaceGenerator faceGenerator;

        [Header("UI Elements")]
        public TMP_InputField nameInput;
        public TMP_InputField ageInput;
        public TMP_Dropdown genderDropdown;
        public TMP_Dropdown archetypeDropdown;
        public TMP_InputField roleInput; // NPC 전용 (예: "Friend", "Rival", "Mentor")
        public TMP_InputField appearanceInput;
        public TMP_InputField personalityInput;
        public Toggle romanceableToggle;

        [Header("Face Preview UI")]
        public Image previewImage;
        public Button generateFaceButton;
        public Button previousButton;
        public Button nextButton;
        public TMP_Text previewIndexText;

        [Header("Buttons")]
        public Button confirmButton;
        public Button addAnotherButton;
        public Button finishButton;

        private int npcCount = 0;

        void Start()
        {
            // faceGenerator 연결
            if (faceGenerator != null)
            {
                faceGenerator.previewImage = previewImage;
            }

            // 드롭다운 초기화
            InitializeDropdowns();

            // 버튼 이벤트
            generateFaceButton.onClick.AddListener(OnGenerateFaceClicked);
            previousButton.onClick.AddListener(OnPreviousClicked);
            nextButton.onClick.AddListener(OnNextClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);
            addAnotherButton.onClick.AddListener(OnAddAnotherClicked);
            finishButton.onClick.AddListener(OnFinishClicked);

            // 초기 상태
            confirmButton.interactable = false;
            addAnotherButton.interactable = false;
            finishButton.interactable = true; // NPC는 선택적

            UpdatePreviewNavigation();
        }

        void InitializeDropdowns()
        {
            // Gender
            genderDropdown.ClearOptions();
            var genderOptions = new System.Collections.Generic.List<string>();
            foreach (Gender g in System.Enum.GetValues(typeof(Gender)))
            {
                genderOptions.Add(g.ToString());
            }
            genderDropdown.AddOptions(genderOptions);

            // Archetype
            archetypeDropdown.ClearOptions();
            var archetypeOptions = new System.Collections.Generic.List<string>();
            foreach (Archetype a in System.Enum.GetValues(typeof(Archetype)))
            {
                archetypeOptions.Add(a.ToString());
            }
            archetypeDropdown.AddOptions(archetypeOptions);
        }

        public void OnGenerateFaceClicked()
        {
            if (string.IsNullOrEmpty(appearanceInput.text))
            {
                Debug.LogWarning("Please enter appearance description first");
                return;
            }

            generateFaceButton.interactable = false;
            StartCoroutine(GenerateFacePreview());
        }

        IEnumerator GenerateFacePreview()
        {
            yield return faceGenerator.GenerateNewFace(
                appearanceInput.text,
                wizardManager.nanoBananaClient,
                () => {
                    generateFaceButton.interactable = true;
                    confirmButton.interactable = true;
                    UpdatePreviewNavigation();
                }
            );
        }

        public void OnPreviousClicked()
        {
            faceGenerator.ShowPrevious();
            UpdatePreviewNavigation();
        }

        public void OnNextClicked()
        {
            faceGenerator.ShowNext();
            UpdatePreviewNavigation();
        }

        void UpdatePreviewNavigation()
        {
            int current = faceGenerator.currentIndex + 1;
            int total = faceGenerator.previewHistory.Count;

            previewIndexText.text = total > 0 ? $"{current} / {total}" : "No previews";

            previousButton.interactable = faceGenerator.currentIndex > 0;
            nextButton.interactable = faceGenerator.currentIndex < total - 1;
            confirmButton.interactable = total > 0;
        }

        public void OnConfirmClicked()
        {
            // NPC 데이터 생성
            CharacterData npc = ScriptableObject.CreateInstance<CharacterData>();
            npc.characterName = nameInput.text;
            npc.age = int.Parse(ageInput.text);
            npc.gender = (Gender)genderDropdown.value;
            npc.role = roleInput.text;
            npc.appearanceDescription = appearanceInput.text;
            npc.personality = personalityInput.text;
            npc.archetype = (Archetype)archetypeDropdown.value;
            npc.confirmedSeed = faceGenerator.GetCurrentSeed();
            npc.facePreview = faceGenerator.GetCurrentPreview();
            npc.resourcePath = $"Generated/Characters/{npc.characterName}";
            npc.isRomanceable = romanceableToggle.isOn;
            npc.initialAffection = 0;

            wizardManager.projectData.npcs.Add(npc);
            npcCount++;

#if UNITY_EDITOR
            // 얼굴 프리뷰 저장
            SaveFacePreview(npc, previewImage.sprite.texture);
#endif

            // 스탠딩 5종 자동 생성
            StartCoroutine(GenerateStandingSprites(npc));
        }

#if UNITY_EDITOR
        private void SaveFacePreview(CharacterData character, Texture2D texture)
        {
            string dir = $"Assets/Resources/Generated/Characters/{character.characterName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/face_preview.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"NPC face preview saved: {path}");
        }
#endif

        IEnumerator GenerateStandingSprites(CharacterData character)
        {
            var generator = gameObject.AddComponent<StandingSpriteGenerator>();

            // NPC는 추가 캐릭터 (스타일 통일)
            bool isFirst = false;

            bool completed = false;
            yield return generator.GenerateStandingSet(
                character,
                wizardManager.nanoBananaClient,
                isFirst,
                () => completed = true
            );

            yield return new WaitUntil(() => completed);

            Debug.Log($"NPC standing sprites generated: {character.characterName}");

            // 다음 NPC 추가 가능
            addAnotherButton.interactable = true;
        }

        public void OnAddAnotherClicked()
        {
            // 폼 초기화
            ClearForm();
            faceGenerator.ClearHistory();
            addAnotherButton.interactable = false;
        }

        public void OnFinishClicked()
        {
            Debug.Log($"Total NPCs created: {npcCount}");
            wizardManager.NextStep();
        }

        void ClearForm()
        {
            nameInput.text = "";
            ageInput.text = "";
            roleInput.text = "";
            appearanceInput.text = "";
            personalityInput.text = "";
            romanceableToggle.isOn = false;
            genderDropdown.value = 0;
            archetypeDropdown.value = 0;
        }
    }
}
