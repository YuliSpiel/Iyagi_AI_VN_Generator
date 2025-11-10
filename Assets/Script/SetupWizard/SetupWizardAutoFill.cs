using UnityEngine;
using UnityEngine.UI;
using IyagiAI.Runtime;
using System.Collections;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// Setup Wizard 테스트용 자동 완성
    /// Option+Shift+T 키를 누르면 현재 스텝을 테스트 데이터로 자동 완성
    /// </summary>
    public class SetupWizardAutoFill : MonoBehaviour
    {
        [Header("References")]
        public SetupWizardManager wizardManager;

        [Header("Test Settings")]
        public bool enableAutoFill = true;

        private void Update()
        {
            // Option+Shift+T: macOS는 LeftAlt = Option, Windows는 LeftAlt = Alt
            if (enableAutoFill &&
                Input.GetKey(KeyCode.LeftAlt) &&
                Input.GetKey(KeyCode.LeftShift) &&
                Input.GetKeyDown(KeyCode.T))
            {
                AutoFillCurrentStep();
            }
        }

        void AutoFillCurrentStep()
        {
            int currentStep = wizardManager.currentStep;
            GameObject currentPanel = wizardManager.stepPanels[currentStep];

            Debug.Log($"[AutoFill] Filling Step {currentStep + 1}");

            switch (currentStep)
            {
                case 0: // Step 1: Game Overview
                    AutoFillStep1(currentPanel);
                    break;
                case 1: // Step 2: Core Values
                    AutoFillStep2(currentPanel);
                    break;
                case 2: // Step 3: Chapters
                    AutoFillStep3(currentPanel);
                    break;
                case 3: // Step 4: Player Character
                    StartCoroutine(AutoFillStep4(currentPanel));
                    break;
                case 4: // Step 5: NPCs
                    StartCoroutine(AutoFillStep5(currentPanel));
                    break;
                case 5: // Step 6: Finalize
                    Debug.Log("[AutoFill] Step 6 doesn't need auto-fill. Just click Create Project button.");
                    break;
            }
        }

        // ===== Step 1: Game Overview =====
        void AutoFillStep1(GameObject panel)
        {
            var step1 = panel.GetComponent<Step1_GameOverview>();
            if (step1 == null) return;

            step1.titleInput.text = "테스트 프로젝트 " + System.DateTime.Now.ToString("HHmmss");
            step1.premiseInput.text = "평범한 학생이 갑자기 판타지 세계로 떨어지면서 시작되는 모험 이야기. 동료들과 함께 세계를 구하는 여정을 떠난다.";
            step1.genreDropdown.value = (int)Genre.Fantasy;
            step1.toneDropdown.value = (int)Tone.Lighthearted;
            step1.playtimeDropdown.value = (int)PlaytimeEstimate.Hour1;

            Debug.Log("[AutoFill] Step 1 filled. Press Next to continue.");
        }

        // ===== Step 2: Core Values =====
        void AutoFillStep2(GameObject panel)
        {
            var step2 = panel.GetComponent<Step2_CoreValues>();
            if (step2 == null) return;

            step2.value1NameInput.text = "용기";
            step2.value1SkillsInput.text = "검술, 방어, 돌격";
            step2.value2NameInput.text = "지혜";
            step2.value2SkillsInput.text = "마법, 분석, 전략";
            step2.value3NameInput.text = "우정";
            step2.value3SkillsInput.text = "협동, 설득, 치유";

            Debug.Log("[AutoFill] Step 2 filled with 3 core values. Press Next to continue.");
        }

        // ===== Step 3: Story Structure =====
        void AutoFillStep3(GameObject panel)
        {
            var step3 = panel.GetComponent<Step3_StoryStructure>();
            if (step3 == null) return;

            step3.chapterCountDropdown.value = 0; // 3개 챕터

            Debug.Log("[AutoFill] Step 3 filled with 3 chapters. Press Next to continue.");
        }

        // ===== Step 4: Player Character =====
        IEnumerator AutoFillStep4(GameObject panel)
        {
            var step4 = panel.GetComponent<Step4_PlayerCharacter>();
            if (step4 == null) yield break;

            step4.nameInput.text = "주인공";
            step4.ageInput.text = "18";
            step4.genderDropdown.value = (int)Gender.Male;
            step4.povDropdown.value = (int)POV.FirstPerson;
            step4.archetypeDropdown.value = (int)Archetype.Hero;
            step4.appearanceInput.text = "검은 머리에 갈색 눈을 가진 평범한 외모의 청년";
            step4.personalityInput.text = "정의감이 강하고 친구를 소중히 여기는 성격";

            // Stub 얼굴 이미지 생성
            yield return CreateStubFaceImage(step4.faceGenerator);

            // Confirm 버튼 활성화
            step4.confirmButton.interactable = true;

            Debug.Log("[AutoFill] Step 4 filled with stub face image. Click Confirm and Next to continue.");
        }

        // ===== Step 5: NPCs =====
        IEnumerator AutoFillStep5(GameObject panel)
        {
            var step5 = panel.GetComponent<Step5_NPCs>();
            if (step5 == null) yield break;

            step5.nameInput.text = "테스트 NPC";
            step5.ageInput.text = "20";
            step5.genderDropdown.value = (int)Gender.Female;
            step5.archetypeDropdown.value = (int)Archetype.Strategist;
            step5.roleInput.text = "친구";
            step5.appearanceInput.text = "긴 금발 머리와 파란 눈을 가진 아름다운 여성";
            step5.personalityInput.text = "똑똑하고 신중한 성격";
            step5.romanceableToggle.isOn = true;

            // Stub 얼굴 이미지 생성
            yield return CreateStubFaceImage(step5.faceGenerator);

            // Confirm 버튼 활성화
            step5.confirmButton.interactable = true;

            Debug.Log("[AutoFill] Step 5 filled with stub face image. Click Confirm (or Finish to skip NPCs) to continue.");
        }

        // ===== Stub 이미지 생성 =====
        IEnumerator CreateStubFaceImage(CharacterFaceGenerator faceGenerator)
        {
            // 단색 stub 이미지 생성 (512x512)
            Texture2D stubTexture = new Texture2D(512, 512);
            Color stubColor = new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f)
            );

            // 그라데이션 효과 추가 (좀 더 시각적으로)
            for (int y = 0; y < stubTexture.height; y++)
            {
                for (int x = 0; x < stubTexture.width; x++)
                {
                    float gradient = (float)y / stubTexture.height;
                    Color pixelColor = Color.Lerp(stubColor, stubColor * 0.7f, gradient);
                    stubTexture.SetPixel(x, y, pixelColor);
                }
            }
            stubTexture.Apply();

            // Sprite 생성
            Sprite sprite = Sprite.Create(
                stubTexture,
                new Rect(0, 0, stubTexture.width, stubTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            // FaceGenerator에 추가
            faceGenerator.previewHistory.Add(sprite);
            faceGenerator.seedHistory.Add(Random.Range(1000, 9999));
            faceGenerator.currentIndex = faceGenerator.previewHistory.Count - 1;
            faceGenerator.previewImage.sprite = sprite;

            Debug.Log("[AutoFill] Stub face image created with gradient");

            yield return null;
        }
    }
}
