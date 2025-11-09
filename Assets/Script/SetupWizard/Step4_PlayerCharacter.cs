using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IyagiAI.Runtime;
using System.Collections;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard - Step 4: 플레이어 캐릭터 생성
    /// 얼굴 프리뷰 생성 + 스탠딩 5종 자동 생성
    /// </summary>
    public class Step4_PlayerCharacter : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;
        public CharacterFaceGenerator faceGenerator;

        [Header("UI Elements")]
        public TMP_InputField nameInput;
        public TMP_InputField ageInput;
        public TMP_Dropdown genderDropdown;
        public TMP_Dropdown povDropdown;
        public TMP_Dropdown archetypeDropdown;
        public TMP_InputField appearanceInput;
        public TMP_InputField personalityInput;

        [Header("Face Preview UI")]
        public Image previewImage; // 얼굴 프리뷰 표시
        public Button generateFaceButton;
        public Button previousButton;
        public Button nextButton;
        public TMP_Text previewIndexText; // "1 / 5"

        [Header("Buttons")]
        public Button confirmButton;
        public Button nextStepButton;

        void Start()
        {
            // faceGenerator 연결
            if (faceGenerator != null)
            {
                faceGenerator.previewImage = previewImage;
            }

            // 드롭다운 초기화
            InitializeDropdowns();

            // 버튼 이벤트 연결
            generateFaceButton.onClick.AddListener(OnGenerateFaceClicked);
            previousButton.onClick.AddListener(OnPreviousClicked);
            nextButton.onClick.AddListener(OnNextClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);
            nextStepButton.onClick.AddListener(OnNextStepClicked);

            // 초기 상태
            nextStepButton.interactable = false;
            confirmButton.interactable = false;
            UpdatePreviewNavigation();
        }

        void InitializeDropdowns()
        {
            // Gender (한국어)
            genderDropdown.ClearOptions();
            genderDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "남성",
                "여성",
                "논바이너리"
            });

            // POV (한국어)
            povDropdown.ClearOptions();
            povDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "1인칭",
                "2인칭",
                "3인칭"
            });

            // Archetype (한국어)
            archetypeDropdown.ClearOptions();
            archetypeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "영웅",
                "전략가",
                "순수한 자",
                "반항아",
                "멘토",
                "트릭스터"
            });
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
            // 캐릭터 데이터 생성
            CharacterData character = ScriptableObject.CreateInstance<CharacterData>();
            character.characterName = nameInput.text;
            character.age = int.Parse(ageInput.text);
            character.gender = (Gender)genderDropdown.value;
            character.pov = (POV)povDropdown.value;
            character.appearanceDescription = appearanceInput.text;
            character.personality = personalityInput.text;
            character.archetype = (Archetype)archetypeDropdown.value;
            character.confirmedSeed = faceGenerator.GetCurrentSeed();
            character.facePreview = faceGenerator.GetCurrentPreview();
            character.resourcePath = $"Generated/Characters/{character.characterName}"; // Resources.Load 경로 설정

            wizardManager.projectData.playerCharacter = character;

#if UNITY_EDITOR
            // 얼굴 프리뷰 저장
            SaveFacePreview(character, previewImage.sprite.texture);
#endif

            // 스탠딩 5종 자동 생성
            StartCoroutine(GenerateStandingSprites(character));
        }

#if UNITY_EDITOR
        private void SaveFacePreview(CharacterData character, Texture2D texture)
        {
            // 캐릭터별 폴더 생성
            string dir = $"Assets/Resources/Generated/Characters/{character.characterName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // 얼굴 프리뷰 저장
            string path = $"{dir}/face_preview.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Face preview saved: {path}");
        }
#endif

        IEnumerator GenerateStandingSprites(CharacterData character)
        {
            var generator = gameObject.AddComponent<StandingSpriteGenerator>();

            // 플레이어 캐릭터는 첫 번째 캐릭터 (스타일 기준)
            bool isFirst = (wizardManager.projectData.npcs.Count == 0);

            bool completed = false;
            yield return generator.GenerateStandingSet(
                character,
                wizardManager.nanoBananaClient,
                isFirst, // 첫 캐릭터 여부 전달
                () => completed = true
            );

            yield return new WaitUntil(() => completed);

            Debug.Log($"Standing sprites generated for {character.characterName}");
            nextStepButton.interactable = true;
        }

        public void OnNextStepClicked()
        {
            wizardManager.NextStep();
        }
    }
}
