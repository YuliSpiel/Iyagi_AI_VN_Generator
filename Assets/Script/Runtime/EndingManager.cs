using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 엔딩 결정 매니저
    /// Core Value 점수 + NPC 친밀도를 조합하여 엔딩 분기
    ///
    /// 엔딩 조건:
    /// - TrueEnding: Core Value 70+ AND 친밀도 70+
    /// - ValueEnding: Core Value 60+ (친밀도 무관)
    /// - NormalEnding: 모든 조건 미달
    /// </summary>
    public class EndingManager : MonoBehaviour
    {
        [Header("Project Data")]
        public VNProjectData projectData;

        /// <summary>
        /// 게임 상태를 기반으로 엔딩을 결정
        /// </summary>
        /// <param name="state">현재 게임 상태</param>
        /// <returns>엔딩 결과 (타입, 설명, 관련 캐릭터)</returns>
        public EndingResult DetermineEnding(GameStateSnapshot state)
        {
            EndingResult result = new EndingResult();

            // 1. 가장 높은 Core Value 찾기
            string dominantValue = GetDominantCoreValue(state);
            result.dominantCoreValue = dominantValue;

            Debug.Log($"[EndingManager] Dominant Core Value: {dominantValue}");

            // 2. 트루 엔딩 조건 체크
            if (IsTrueEnding(state, dominantValue))
            {
                result.endingType = EndingType.TrueEnding;
                result.endingTitle = "True Ending";
                result.endingDescription = $"You have mastered {dominantValue} and reached the ultimate ending.";

                Debug.Log($"[EndingManager] ✨ TRUE ENDING achieved!");
            }
            // 3. Value 엔딩 조건 체크 (Core Value만 높음)
            else if (IsValueEnding(state, dominantValue))
            {
                result.endingType = EndingType.ValueEnding;
                result.endingTitle = $"{dominantValue} Ending";
                result.endingDescription = $"Your journey ends with {dominantValue} as your guiding principle.";

                Debug.Log($"[EndingManager] ⭐ VALUE ENDING: {dominantValue}");
            }
            // 4. 일반 엔딩 (모든 조건 미달)
            else
            {
                result.endingType = EndingType.NormalEnding;
                result.endingTitle = "Normal Ending";
                result.endingDescription = "Your journey ends, though your path remains unclear.";

                Debug.Log($"[EndingManager] 🌟 NORMAL ENDING");
            }

            // 5. 로맨스 Achievement 체크 (별도로 추가)
            result.romanceCharacters = GetRomanceCharacters(state);
            if (result.romanceCharacters.Count > 0)
            {
                Debug.Log($"[EndingManager] 💕 Romance Achievements: {string.Join(", ", result.romanceCharacters)}");
            }

            return result;
        }

        /// <summary>
        /// 가장 높은 Core Value 찾기
        /// </summary>
        private string GetDominantCoreValue(GameStateSnapshot state)
        {
            if (state.coreValueScores == null || state.coreValueScores.Count == 0)
            {
                Debug.LogWarning("[EndingManager] No core value scores found, using first value from project");
                return projectData.coreValues.Count > 0 ? projectData.coreValues[0].name : "Unknown";
            }

            var maxValue = state.coreValueScores.OrderByDescending(kv => kv.Value).First();
            return maxValue.Key;
        }

        /// <summary>
        /// 로맨스 Achievement를 달성한 캐릭터 목록 (호감도 80+ & 로맨스 가능)
        /// </summary>
        private List<string> GetRomanceCharacters(GameStateSnapshot state)
        {
            List<string> romanceChars = new List<string>();

            if (state.characterAffections == null || state.characterAffections.Count == 0)
            {
                return romanceChars;
            }

            foreach (var kvp in state.characterAffections)
            {
                string npcName = kvp.Key;
                int affection = kvp.Value;

                // 1. 호감도 80 이상인지 확인
                if (affection < 80)
                {
                    continue;
                }

                // 2. 로맨스 가능한 NPC인지 확인
                var npc = projectData.npcs.Find(n => n.characterName == npcName);
                if (npc == null || !npc.isRomanceable)
                {
                    continue;
                }

                romanceChars.Add(npcName);
                Debug.Log($"[EndingManager] Romance Achievement unlocked: {npcName} (Affection: {affection})");
            }

            return romanceChars;
        }

        /// <summary>
        /// 트루 엔딩 조건 체크
        /// Core Value + 친밀도 모두 고려
        /// </summary>
        private bool IsTrueEnding(GameStateSnapshot state, string dominantValue)
        {
            // 1. 트루 엔딩 Core Value가 설정되어 있는지 확인
            if (string.IsNullOrEmpty(projectData.trueValueName))
            {
                Debug.Log("[EndingManager] No true value set, true ending not available");
                return false;
            }

            // 2. Dominant Value가 트루 엔딩 Value와 일치하는지 확인
            if (dominantValue != projectData.trueValueName)
            {
                Debug.Log($"[EndingManager] Dominant value ({dominantValue}) != True value ({projectData.trueValueName})");
                return false;
            }

            // 3. 트루 엔딩 Value 점수가 일정 수준 이상인지 확인 (예: 70 이상)
            if (!state.coreValueScores.ContainsKey(projectData.trueValueName))
            {
                return false;
            }

            int trueValueScore = state.coreValueScores[projectData.trueValueName];
            bool valueHighEnough = trueValueScore >= 70;

            Debug.Log($"[EndingManager] True value score: {trueValueScore} (threshold: 70, passed: {valueHighEnough})");

            // 4. 친밀도 조건 체크: 최소 1명의 NPC와 높은 친밀도 (70+)
            int maxAffection = GetMaxAffection(state);
            bool affectionHighEnough = maxAffection >= 70;

            Debug.Log($"[EndingManager] Max affection: {maxAffection} (threshold: 70, passed: {affectionHighEnough})");

            // True Ending 조건: Core Value 70+ AND 친밀도 70+
            bool isTrueEnding = valueHighEnough && affectionHighEnough;

            if (isTrueEnding)
            {
                Debug.Log($"[EndingManager] ✅ True Ending conditions met: Value={trueValueScore}, Affection={maxAffection}");
            }
            else if (valueHighEnough && !affectionHighEnough)
            {
                Debug.Log($"[EndingManager] ❌ True Ending failed: High value but low affection (max: {maxAffection})");
            }

            return isTrueEnding;
        }


        /// <summary>
        /// Value 엔딩 조건 체크 (Core Value 높음, 친밀도 중간 수준)
        /// </summary>
        private bool IsValueEnding(GameStateSnapshot state, string dominantValue)
        {
            // 1. Dominant Value가 일정 수준 이상인지 확인 (예: 60 이상)
            if (!state.coreValueScores.ContainsKey(dominantValue))
            {
                return false;
            }

            int valueScore = state.coreValueScores[dominantValue];
            bool valueHighEnough = valueScore >= 60;

            // 2. 친밀도 체크: 중간 수준 (50+) 또는 Core Value만 높은 경우
            int maxAffection = GetMaxAffection(state);

            Debug.Log($"[EndingManager] Value ending check: {dominantValue}={valueScore} (threshold: 60), Max affection={maxAffection}");

            // Value Ending 조건: Core Value 60+ (친밀도는 참고용으로만 사용)
            return valueHighEnough;
        }

        /// <summary>
        /// 최고 친밀도 점수 반환
        /// </summary>
        private int GetMaxAffection(GameStateSnapshot state)
        {
            if (state.characterAffections == null || state.characterAffections.Count == 0)
            {
                return 0;
            }

            return state.characterAffections.Values.Max();
        }
    }

    /// <summary>
    /// 엔딩 타입
    /// </summary>
    public enum EndingType
    {
        TrueEnding,     // 트루 엔딩 (Core Value 70+ AND 친밀도 70+)
        ValueEnding,    // Value 엔딩 (Core Value 60+, 친밀도 무관)
        NormalEnding    // 일반 엔딩 (모든 조건 미달)
    }

    /// <summary>
    /// 엔딩 결과 데이터
    /// </summary>
    [System.Serializable]
    public class EndingResult
    {
        public EndingType endingType;
        public string endingTitle;
        public string endingDescription;
        public string dominantCoreValue;
        public List<string> romanceCharacters = new List<string>(); // 로맨스 Achievement 달성한 캐릭터들
    }
}
