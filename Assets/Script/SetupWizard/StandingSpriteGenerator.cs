using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IyagiAI.Runtime;
using IyagiAI.AISystem;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// 스탠딩 스프라이트 생성기
    /// Setup Wizard: 기본 5종 생성 (Neutral, Happy, Sad, Angry, Surprised + Normal 포즈)
    /// Runtime: 특정 Expression + Pose 조합 생성
    /// </summary>
    public class StandingSpriteGenerator : MonoBehaviour
    {
        // Pose 설명 매핑
        private Dictionary<Runtime.Pose, string> poseDescriptions = new Dictionary<Runtime.Pose, string>
        {
            { Runtime.Pose.Normal, "front-facing, full body centered, hands visible, neutral stance" },
            { Runtime.Pose.HandsOnHips, "standing confidently with hands on hips" },
            { Runtime.Pose.ArmsCrossed, "arms crossed over chest, confident/defensive stance" },
            { Runtime.Pose.Pointing, "one arm extended, pointing finger forward" },
            { Runtime.Pose.Waving, "one hand raised in friendly wave" },
            { Runtime.Pose.Thinking, "hand on chin, contemplative pose" },
            { Runtime.Pose.Surprised, "hands raised slightly, body leaning back" }
        };

        // 첫 번째 캐릭터용 프롬프트 (스타일 기준 설정)
        private string BuildFirstCharacterPrompt(CharacterData character, string expression, string poseDesc)
        {
            string gender = character.gender == Gender.Male ? "male" : "female";
            return $@"A full-body standing sprite of a {gender} character for a Japanese-style visual novel.
High-quality anime illustration style with clean outlines and soft gradient shading.
Large expressive eyes, natural lighting, smooth skin tone.
Line art is thin and consistent, coloring uses soft airbrush-style highlights and shadows.
Character appearance: {character.appearanceDescription}.
IMPORTANT: The character's outfit and clothing MUST remain exactly the same across all expressions and poses. Only the facial expression and body pose should change.
Pose: {poseDesc}.
Expression: {expression} (only change facial expression, keep outfit identical).
Background: transparent or solid white (no scenery).
Camera angle: straight-on, waist-to-feet ratio realistic, overall balanced proportions.
Resolution: 2048×4096.";
        }

        // 추가 캐릭터용 프롬프트 (스타일 통일)
        private string BuildAdditionalCharacterPrompt(CharacterData character, string expression, string poseDesc)
        {
            string gender = character.gender == Gender.Male ? "male" : "female";
            return $@"A full-body standing sprite of a {gender} character for a Japanese-style visual novel.
Same art style, same proportions, and same camera angle as the previous character.
Thin clean line art, soft gradient anime shading, expressive eyes.
Character appearance: {character.appearanceDescription}.
IMPORTANT: The character's outfit and clothing MUST remain exactly the same across all expressions and poses. Only the facial expression and body pose should change.
Pose: {poseDesc}.
Expression: {expression} (only change facial expression, keep outfit identical).
Background: transparent or solid white (no scenery).
Resolution: 2048×4096.";
        }

        /// <summary>
        /// Setup Wizard: 기본 5종 생성 (Normal 포즈만)
        /// </summary>
        public IEnumerator GenerateStandingSet(
            CharacterData character,
            NanoBananaClient client,
            bool isFirst,
            System.Action onComplete)
        {
            Expression[] expressions =
            {
                Expression.Neutral,
                Expression.Happy,
                Expression.Sad,
                Expression.Angry,
                Expression.Surprised
            };

            if (character.standingSprites == null)
            {
                character.standingSprites = new Dictionary<string, Sprite>();
            }

            Debug.Log($"=== Starting standing sprite generation for {character.characterName} ===");
            Debug.Log($"Total expressions to generate: {expressions.Length}");

            int currentIndex = 0;
            foreach (var expr in expressions)
            {
                currentIndex++;
                string exprText = expr.ToString().ToLower();
                string poseText = "normal";
                string key = $"{exprText}_{poseText}";

                Debug.Log($"[{currentIndex}/{expressions.Length}] Generating {key} for {character.characterName}...");

                string poseDesc = poseDescriptions[Runtime.Pose.Normal];
                string fullPrompt = isFirst
                    ? BuildFirstCharacterPrompt(character, exprText, poseDesc)
                    : BuildAdditionalCharacterPrompt(character, exprText, poseDesc);

                bool completed = false;
                Texture2D result = null;

                yield return client.GenerateImage(
                    fullPrompt,
                    character.confirmedSeed,
                    (texture, seed) => {
                        result = texture;
                        completed = true;
                    },
                    (error) => {
                        Debug.LogError($"Standing image generation failed for {key}: {error}");
                        completed = true;
                    }
                );

                yield return new WaitUntil(() => completed);

                if (result != null)
                {
                    // 배경 제거 처리
                    Texture2D finalTexture = result;

#if UNITY_EDITOR
                    yield return RemoveBackgroundAndSave(character.characterName, key, result, (processedTexture) =>
                    {
                        if (processedTexture != null)
                        {
                            finalTexture = processedTexture;
                        }
                    });
#endif

                    Sprite sprite = Sprite.Create(
                        finalTexture,
                        new Rect(0, 0, finalTexture.width, finalTexture.height),
                        new Vector2(0.5f, 0.5f)
                    );

                    character.standingSprites[key] = sprite;
                    Debug.Log($"[{currentIndex}/{expressions.Length}] Successfully created sprite for {key}");
                }
                else
                {
                    Debug.LogWarning($"[{currentIndex}/{expressions.Length}] Failed to generate {key} - result is null");
                }

                Debug.Log($"[{currentIndex}/{expressions.Length}] Waiting 1 second before next generation...");
                yield return new WaitForSeconds(1f); // API 레이트 리밋 대비
            }

            Debug.Log($"=== Completed all {expressions.Length} standing sprites for {character.characterName} ===");
            onComplete?.Invoke();
        }

        /// <summary>
        /// 런타임: 특정 Expression + Pose 조합 생성
        /// </summary>
        public IEnumerator GenerateSingleSprite(
            CharacterData character,
            Expression expression,
            Runtime.Pose pose,
            NanoBananaClient client,
            bool isFirst,
            System.Action<Sprite> onComplete)
        {
            string exprText = expression.ToString().ToLower();
            string poseText = pose.ToString().ToLower();
            string key = $"{exprText}_{poseText}";

            // 이미 있으면 재사용
            if (character.standingSprites.ContainsKey(key))
            {
                onComplete?.Invoke(character.standingSprites[key]);
                yield break;
            }

            string poseDesc = poseDescriptions[pose];
            string fullPrompt = isFirst
                ? BuildFirstCharacterPrompt(character, exprText, poseDesc)
                : BuildAdditionalCharacterPrompt(character, exprText, poseDesc);

            bool completed = false;
            Texture2D result = null;

            yield return client.GenerateImage(
                fullPrompt,
                character.confirmedSeed,
                (texture, seed) => {
                    result = texture;
                    completed = true;
                },
                (error) => {
                    Debug.LogError($"Standing image generation failed: {error}");
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            Sprite sprite = null;
            if (result != null)
            {
                // 배경 제거 처리
                Texture2D finalTexture = result;

#if UNITY_EDITOR
                yield return RemoveBackgroundAndSave(character.characterName, key, result, (processedTexture) =>
                {
                    if (processedTexture != null)
                    {
                        finalTexture = processedTexture;
                    }
                });
#endif

                sprite = Sprite.Create(
                    finalTexture,
                    new Rect(0, 0, finalTexture.width, finalTexture.height),
                    new Vector2(0.5f, 0.5f)
                );

                character.standingSprites[key] = sprite;

#if UNITY_EDITOR
                SaveSpriteToResources(character.characterName, key, result);
#endif
            }

            onComplete?.Invoke(sprite);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 배경 제거 후 저장
        /// </summary>
        private IEnumerator RemoveBackgroundAndSave(string charName, string key, Texture2D originalTexture, System.Action<Texture2D> onComplete)
        {
            // 캐릭터별 폴더 생성
            string dir = $"Assets/Resources/Generated/Characters/{charName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // 임시 파일 경로 (배경 제거 전)
            string tempInputPath = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, $"{charName}_{key}_input.png");
            string tempOutputPath = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, $"{charName}_{key}_output.png");

            // 원본 이미지 임시 저장
            System.IO.File.WriteAllBytes(tempInputPath, originalTexture.EncodeToPNG());
            Debug.Log($"[BackgroundRemoval] Saved temp input: {tempInputPath}");

            // 배경 제거 실행
            bool bgRemovalSuccess = false;
            yield return BackgroundRemover.RemoveBackground(tempInputPath, tempOutputPath, (success) =>
            {
                bgRemovalSuccess = success;
            });

            Texture2D resultTexture = originalTexture;

            if (bgRemovalSuccess && System.IO.File.Exists(tempOutputPath))
            {
                // 배경 제거된 이미지 로드
                byte[] pngData = System.IO.File.ReadAllBytes(tempOutputPath);
                Texture2D processedTexture = new Texture2D(2, 2);
                if (processedTexture.LoadImage(pngData))
                {
                    resultTexture = processedTexture;
                    Debug.Log($"[BackgroundRemoval] Successfully loaded background-removed image");
                }
                else
                {
                    Debug.LogWarning($"[BackgroundRemoval] Failed to load processed image, using original");
                }

                // 임시 파일 삭제
                System.IO.File.Delete(tempOutputPath);
            }
            else
            {
                Debug.LogWarning($"[BackgroundRemoval] Background removal failed, using original image");
            }

            // 최종 이미지 저장 (Resources 폴더)
            string finalPath = $"{dir}/{key}.png";
            System.IO.File.WriteAllBytes(finalPath, resultTexture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Sprite saved: {finalPath}");

            // 임시 입력 파일 삭제
            if (System.IO.File.Exists(tempInputPath))
            {
                System.IO.File.Delete(tempInputPath);
            }

            onComplete?.Invoke(resultTexture);
        }

        private void SaveSpriteToResources(string charName, string key, Texture2D texture)
        {
            // 이 메서드는 더 이상 직접 호출되지 않음 (RemoveBackgroundAndSave가 대체)
            // 하위 호환성을 위해 유지
            string dir = $"Assets/Resources/Generated/Characters/{charName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/{key}.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Sprite saved: {path}");
        }
#endif
    }
}
