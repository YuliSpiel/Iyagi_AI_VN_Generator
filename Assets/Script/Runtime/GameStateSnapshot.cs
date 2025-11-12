using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 게임 진행 상태 스냅샷
    /// 챕터 생성 시 현재 상태를 전달하여 AI가 맥락을 파악하도록 함
    /// 세이브/로드 시스템에서도 사용
    /// </summary>
    [System.Serializable]
    public class GameStateSnapshot
    {
        public int currentChapter;
        public int currentLineId; // 현재 대화 라인 ID

        // Core Value 점수 (예: {"Courage": 30, "Wisdom": 50})
        public Dictionary<string, int> coreValueScores = new Dictionary<string, int>();

        // Derived Skill 점수 (예: {"자긍심": 20, "공감능력": 35, "판단력": 15})
        public Dictionary<string, int> skillScores = new Dictionary<string, int>();

        // 캐릭터 호감도 (예: {"NPC1": 70, "NPC2": 30})
        public Dictionary<string, int> characterAffections = new Dictionary<string, int>();

        // 선택 히스토리 (이전 선택들)
        public List<string> previousChoices = new List<string>();

        // 챕터 요약 (챕터 번호 → 요약 텍스트)
        public Dictionary<int, string> chapterSummaries = new Dictionary<int, string>();

        // 해금된 CG 목록
        public List<string> unlockedCGs = new List<string>();

        // 달성한 엔딩 ID
        public List<int> unlockedEndings = new List<int>();

        // 커스텀 플래그 (예: {"met_king": true, "found_sword": false})
        public Dictionary<string, bool> flags = new Dictionary<string, bool>();

        /// <summary>
        /// AI 프롬프트용으로 현재 상태를 텍스트로 변환
        /// </summary>
        public string ToPromptString()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Current Chapter: {currentChapter}");

            if (coreValueScores != null && coreValueScores.Count > 0)
            {
                sb.Append("Core Values: ");
                foreach (var kv in coreValueScores)
                {
                    sb.Append($"{kv.Key}={kv.Value}, ");
                }
                sb.AppendLine();
            }

            if (skillScores != null && skillScores.Count > 0)
            {
                sb.Append("Skills: ");
                foreach (var kv in skillScores)
                {
                    sb.Append($"{kv.Key}={kv.Value}, ");
                }
                sb.AppendLine();
            }

            if (characterAffections != null && characterAffections.Count > 0)
            {
                sb.Append("Character Affections: ");
                foreach (var kv in characterAffections)
                {
                    sb.Append($"{kv.Key}={kv.Value}, ");
                }
                sb.AppendLine();
            }

            if (previousChoices != null && previousChoices.Count > 0)
            {
                sb.AppendLine($"Previous Choices: {string.Join(", ", previousChoices.ToArray())}");
            }

            if (chapterSummaries != null && chapterSummaries.Count > 0)
            {
                sb.AppendLine("\nPrevious Chapters:");
                foreach (var kv in chapterSummaries)
                {
                    sb.AppendLine($"  Chapter {kv.Key}: {kv.Value}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 챕터 캐싱을 위한 해시 키 생성
        /// Core Value 점수 + 친밀도 점수 모두 사용
        /// 10단위로 반올림하여 캐시 효율 향상
        /// </summary>
        public string GetCacheHash()
        {
            var allValues = new List<string>();

            // 1. Core Value 점수 추가 (알파벳 순서로 정렬)
            if (coreValueScores != null && coreValueScores.Count > 0)
            {
                var sortedCoreValues = new List<KeyValuePair<string, int>>(coreValueScores);
                sortedCoreValues.Sort((a, b) => string.Compare(a.Key, b.Key));

                foreach (var kv in sortedCoreValues)
                {
                    int roundedValue = (kv.Value / 10) * 10; // 10단위 반올림
                    allValues.Add($"CV:{kv.Key}:{roundedValue}");
                }
            }

            // 2. 친밀도 점수 추가 (알파벳 순서로 정렬)
            if (characterAffections != null && characterAffections.Count > 0)
            {
                var sortedAffections = new List<KeyValuePair<string, int>>(characterAffections);
                sortedAffections.Sort((a, b) => string.Compare(a.Key, b.Key));

                foreach (var kv in sortedAffections)
                {
                    int roundedValue = (kv.Value / 10) * 10; // 10단위 반올림
                    allValues.Add($"AF:{kv.Key}:{roundedValue}");
                }
            }

            // 초기 상태이거나 값이 없으면 기본 해시
            if (allValues.Count == 0)
            {
                return "00000000";
            }

            string stateString = string.Join(",", allValues.ToArray());
            int hash = stateString.GetHashCode();

            // 8자리 16진수 문자열 (예: "A1B2C3D4")
            return hash.ToString("X8");
        }
    }
}
