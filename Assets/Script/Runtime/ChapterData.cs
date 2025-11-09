using System.Collections.Generic;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 런타임에 생성되는 챕터 데이터
    /// Gemini API로 생성된 대화 목록 + 메타데이터
    /// persistentDataPath에 JSON으로 캐싱됨
    /// </summary>
    [System.Serializable]
    public class ChapterData
    {
        public int chapterId;
        public List<DialogueRecord> records = new List<DialogueRecord>(); // 대화 레코드 리스트
        public string generationPrompt; // 재생성용 프롬프트 (디버깅/재생성 시 사용)
        public GameStateSnapshot stateSnapshot; // 생성 당시 게임 상태
        public long timestamp; // Unix timestamp (생성 시각)

        public ChapterData()
        {
        }

        public ChapterData(int id, List<DialogueRecord> dialogues)
        {
            this.chapterId = id;
            this.records = dialogues;
            this.timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
