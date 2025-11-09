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
Pose: {poseDesc}.
Expression: {expression}.
Outfit: {character.appearanceDescription}.
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
Pose: {poseDesc}.
Expression: {expression}.
Outfit: {character.appearanceDescription}.
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

            foreach (var expr in expressions)
            {
                string exprText = expr.ToString().ToLower();
                string poseText = "normal";
                string key = $"{exprText}_{poseText}";

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
                        Debug.LogError($"Standing image generation failed: {error}");
                        completed = true;
                    }
                );

                yield return new WaitUntil(() => completed);

                if (result != null)
                {
                    Sprite sprite = Sprite.Create(
                        result,
                        new Rect(0, 0, result.width, result.height),
                        new Vector2(0.5f, 0.5f)
                    );

                    character.standingSprites[key] = sprite;

#if UNITY_EDITOR
                    SaveSpriteToResources(character.characterName, key, result);
#endif
                }

                yield return new WaitForSeconds(1f); // API 레이트 리밋 대비
            }

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
                sprite = Sprite.Create(
                    result,
                    new Rect(0, 0, result.width, result.height),
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
        private void SaveSpriteToResources(string charName, string key, Texture2D texture)
        {
            // 캐릭터별 폴더: Assets/Resources/Generated/Characters/{CharName}/
            string dir = $"Assets/Resources/Generated/Characters/{charName}";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // 파일명: {expression}_{pose}.png (예: happy_normal.png)
            string path = $"{dir}/{key}.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Sprite saved: {path}");
        }
#endif
    }
}
