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

        // 캐릭터 호감도 (예: {"NPC1": 70, "NPC2": 30})
        public Dictionary<string, int> characterAffections = new Dictionary<string, int>();

        // 선택 히스토리 (이전 선택들)
        public List<string> previousChoices = new List<string>();

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

            return sb.ToString();
        }
    }
}
