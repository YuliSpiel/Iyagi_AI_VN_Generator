using System.Collections.Generic;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 모든 엔딩 씬 데이터를 관리하는 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "EndingSceneDatabase", menuName = "Iyagi/Ending Scene Database")]
    public class EndingSceneDatabase : ScriptableObject
    {
        [Header("Ending Scenes")]
        public List<EndingSceneData> endingScenes = new List<EndingSceneData>();

        /// <summary>
        /// 엔딩 타입에 해당하는 엔딩 씬 데이터 가져오기
        /// </summary>
        public EndingSceneData GetEndingScene(EndingType endingType, string dominantCoreValue = null)
        {
            // ValueEnding의 경우 dominantCoreValue에 따라 다른 엔딩 반환 가능
            if (endingType == EndingType.ValueEnding && !string.IsNullOrEmpty(dominantCoreValue))
            {
                // 특정 Core Value에 대한 전용 엔딩이 있는지 확인
                var specificEnding = endingScenes.Find(e =>
                    e.endingType == EndingType.ValueEnding &&
                    e.endingTitle.Contains(dominantCoreValue)
                );

                if (specificEnding != null)
                    return specificEnding;
            }

            // 일반적으로 endingType만으로 찾기
            return endingScenes.Find(e => e.endingType == endingType);
        }

        /// <summary>
        /// 로맨스 캐릭터가 있는 경우 특별 엔딩이 있는지 확인
        /// </summary>
        public EndingSceneData GetRomanceEnding(string characterName)
        {
            // 로맨스 엔딩은 endingTitle에 캐릭터 이름 포함으로 구분
            return endingScenes.Find(e => e.endingTitle.Contains(characterName));
        }

        /// <summary>
        /// 모든 엔딩 타입이 등록되어 있는지 확인
        /// </summary>
        public bool ValidateDatabase()
        {
            bool hasTrue = endingScenes.Exists(e => e.endingType == EndingType.TrueEnding);
            bool hasValue = endingScenes.Exists(e => e.endingType == EndingType.ValueEnding);
            bool hasNormal = endingScenes.Exists(e => e.endingType == EndingType.NormalEnding);

            if (!hasTrue)
                Debug.LogWarning("[EndingSceneDatabase] Missing TrueEnding scene!");
            if (!hasValue)
                Debug.LogWarning("[EndingSceneDatabase] Missing ValueEnding scene!");
            if (!hasNormal)
                Debug.LogWarning("[EndingSceneDatabase] Missing NormalEnding scene!");

            return hasTrue && hasValue && hasNormal;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 에디터에서 자동 검증
            ValidateDatabase();
        }
#endif
    }
}
