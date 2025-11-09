using System.Collections.Generic;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 캐릭터별 데이터 (플레이어 또는 NPC)
    /// ScriptableObject로 Assets/VNProjects/Characters/에 저장됨
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "Iyagi/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Basic Info")]
        public string characterName;
        public string role; // NPC 전용: "친구", "멘토", "라이벌" 등
        public int age;
        public Gender gender;
        public POV pov; // 플레이어 전용

        [Header("Visual")]
        [TextArea(3, 5)]
        public string appearanceDescription; // AI 이미지 생성용 외모 설명

        public int confirmedSeed; // NanoBanana 확정 시드 (일관성 유지)
        public Sprite facePreview; // 확정된 얼굴 프리뷰 (CG 레퍼런스용)
        public string resourcePath; // Resources.Load 경로: "Generated/Characters/{characterName}"

        [Header("Personality")]
        [TextArea(3, 5)]
        public string personality;

        [TextArea(3, 5)]
        public string background;

        public Archetype archetype;
        public List<string> speechExamples = new List<string>(); // 말투 예시

        [Header("Gameplay (NPC만)")]
        public bool isRomanceable;
        public int initialAffection; // -100 ~ +100

        [Header("Generated Images")]
        public Dictionary<string, Sprite> standingSprites = new Dictionary<string, Sprite>();
        // Key: "Expression_Pose" (예: "happy_normal", "sad_thinking")

        // ===== 헬퍼 메서드 =====

        /// <summary>
        /// 특정 Expression + Pose 조합의 스프라이트 가져오기
        /// 캐시되어 있으면 반환, 없으면 Resources에서 로드
        /// </summary>
        public Sprite GetStandingSprite(Expression expression, Pose pose)
        {
            string key = $"{expression.ToString().ToLower()}_{pose.ToString().ToLower()}";

            // 이미 캐시되어 있으면 반환
            if (standingSprites != null && standingSprites.ContainsKey(key))
            {
                return standingSprites[key];
            }

            // Resources에서 로드 시도
            if (!string.IsNullOrEmpty(resourcePath))
            {
                string path = $"{resourcePath}/{key}";
                Sprite sprite = Resources.Load<Sprite>(path);

                if (sprite != null)
                {
                    if (standingSprites == null)
                        standingSprites = new Dictionary<string, Sprite>();

                    standingSprites[key] = sprite;
                    return sprite;
                }
            }

            Debug.LogWarning($"Standing sprite not found: {characterName}/{key}");
            return null;
        }

        /// <summary>
        /// 얼굴 프리뷰 가져오기 (CG 레퍼런스용)
        /// 캐시되어 있으면 반환, 없으면 Resources에서 로드
        /// </summary>
        public Sprite GetFacePreview()
        {
            if (facePreview != null)
                return facePreview;

            if (!string.IsNullOrEmpty(resourcePath))
            {
                string path = $"{resourcePath}/face_preview";
                facePreview = Resources.Load<Sprite>(path);
                return facePreview;
            }

            Debug.LogWarning($"Face preview not found: {characterName}");
            return null;
        }
    }

    // ===== Enums =====

    public enum Gender { Male, Female, NonBinary }
    public enum POV { FirstPerson, SecondPerson, ThirdPerson }
    public enum Archetype { Hero, Strategist, Innocent, Rebel, Mentor, Trickster }

    /// <summary>
    /// 표정 (Expression)
    /// </summary>
    public enum Expression
    {
        Neutral,    // 중립
        Happy,      // 행복
        Sad,        // 슬픔
        Angry,      // 화남
        Surprised,  // 놀람
        Embarrassed,// 당황/부끄러움
        Thinking    // 생각중
    }

    /// <summary>
    /// 포즈 (Pose)
    /// </summary>
    public enum Pose
    {
        Normal,         // 일반 자세
        HandsOnHips,    // 손을 허리에
        ArmsCrossed,    // 팔짱
        Pointing,       // 가리키기
        Waving,         // 손 흔들기
        Thinking,       // 생각하는 포즈 (손을 턱에)
        Surprised       // 놀란 포즈 (손을 들어올림)
    }
}
