using System.Collections;
using UnityEngine;
using IyagiAI.AISystem;
using IyagiAI.SetupWizard;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 런타임 스탠딩 스프라이트 매니저
    /// 챕터 진행 중 필요한 Expression+Pose 조합을 on-demand로 생성
    /// </summary>
    public class RuntimeSpriteManager : MonoBehaviour
    {
        public static RuntimeSpriteManager Instance { get; private set; }

        private VNProjectData projectData;
        private NanoBananaClient nanoBananaClient;
        private StandingSpriteGenerator generator;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                generator = gameObject.AddComponent<StandingSpriteGenerator>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(VNProjectData project, NanoBananaClient client)
        {
            this.projectData = project;
            this.nanoBananaClient = client;
        }

        /// <summary>
        /// 특정 캐릭터의 Expression_Pose 스프라이트 가져오기 (없으면 생성)
        /// </summary>
        public IEnumerator GetOrGenerateSprite(
            string characterName,
            Expression expression,
            Pose pose,
            System.Action<Sprite> onComplete)
        {
            // 캐릭터 찾기
            CharacterData character = null;
            if (projectData.playerCharacter.characterName == characterName)
            {
                character = projectData.playerCharacter;
            }
            else
            {
                character = projectData.npcs.Find(n => n.characterName == characterName);
            }

            if (character == null)
            {
                Debug.LogError($"Character not found: {characterName}");
                onComplete?.Invoke(null);
                yield break;
            }

            string key = $"{expression.ToString().ToLower()}_{pose.ToString().ToLower()}";

            // 이미 있으면 반환
            if (character.standingSprites != null && character.standingSprites.ContainsKey(key))
            {
                onComplete?.Invoke(character.standingSprites[key]);
                yield break;
            }

            // 없으면 생성
            Debug.Log($"Generating new sprite: {characterName}/{key}");

            bool isFirst = (characterName == projectData.playerCharacter.characterName);

            bool completed = false;
            Sprite result = null;

            yield return generator.GenerateSingleSprite(
                character,
                expression,
                pose,
                nanoBananaClient,
                isFirst,
                (sprite) => {
                    result = sprite;
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            onComplete?.Invoke(result);
        }

        /// <summary>
        /// AI가 요청한 Expression/Pose가 있는지 확인
        /// </summary>
        public bool HasSprite(string characterName, string expressionPoseKey)
        {
            CharacterData character = null;
            if (projectData.playerCharacter.characterName == characterName)
            {
                character = projectData.playerCharacter;
            }
            else
            {
                character = projectData.npcs.Find(n => n.characterName == characterName);
            }

            return character != null &&
                   character.standingSprites != null &&
                   character.standingSprites.ContainsKey(expressionPoseKey);
        }

        /// <summary>
        /// 사용 가능한 스프라이트 목록 반환 (AI 프롬프트에 제공용)
        /// </summary>
        public System.Collections.Generic.List<string> GetAvailableSprites(string characterName)
        {
            CharacterData character = null;
            if (projectData.playerCharacter.characterName == characterName)
            {
                character = projectData.playerCharacter;
            }
            else
            {
                character = projectData.npcs.Find(n => n.characterName == characterName);
            }

            if (character == null || character.standingSprites == null)
                return new System.Collections.Generic.List<string>();

            return new System.Collections.Generic.List<string>(character.standingSprites.Keys);
        }
    }
}
