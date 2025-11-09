using System.Collections.Generic;
using UnityEngine;
using IyagiAI.Runtime;

namespace IyagiAI.AISystem
{
    /// <summary>
    /// AI가 생성한 JSON 데이터를 DialogueRecord로 변환
    /// </summary>
    public static class AIDataConverter
    {
        private static int baseId = 1000; // 챕터별로 1000, 2000, 3000...

        /// <summary>
        /// AI JSON 응답을 DialogueRecord 리스트로 변환
        /// </summary>
        /// <param name="jsonResponse">Gemini API의 JSON 응답</param>
        /// <param name="chapterId">챕터 ID (1, 2, 3...)</param>
        /// <returns>DialogueRecord 리스트</returns>
        public static List<Runtime.DialogueRecord> FromAIJson(string jsonResponse, int chapterId)
        {
            // JSON 배열 추출 (Gemini 응답에서 [...] 부분만 추출)
            int startIndex = jsonResponse.IndexOf('[');
            int endIndex = jsonResponse.LastIndexOf(']');

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                Debug.LogError("Invalid JSON format: No array found");
                return new List<Runtime.DialogueRecord>();
            }

            string jsonArray = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);

            // JSON 파싱
            var wrapper = JsonUtility.FromJson<AIDialogueWrapper>("{\"lines\":" + jsonArray + "}");

            if (wrapper == null || wrapper.lines == null || wrapper.lines.Length == 0)
            {
                Debug.LogError("Failed to parse JSON or empty lines");
                return new List<Runtime.DialogueRecord>();
            }

            List<Runtime.DialogueRecord> records = new List<Runtime.DialogueRecord>();
            baseId = chapterId * 1000; // 챕터 1 = 1000~, 챕터 2 = 2000~

            for (int i = 0; i < wrapper.lines.Length; i++)
            {
                var line = wrapper.lines[i];
                Runtime.DialogueRecord record = new Runtime.DialogueRecord();

                // 기본 필드
                record.Fields["ID"] = (baseId + i).ToString();
                record.Fields["Scene"] = chapterId.ToString();
                record.Fields["Index"] = i.ToString();

                // 대화 텍스트
                record.Fields["ParsedLine_ENG"] = line.text ?? "";
                record.Fields["NameTag"] = line.speaker ?? "";

                // 배경/BGM/SFX
                record.Fields["BG"] = line.bg_name ?? "";
                record.Fields["BGM"] = line.bgm_name ?? "";
                record.Fields["SFX"] = line.sfx_name ?? "";

                // 캐릭터 1
                if (!string.IsNullOrEmpty(line.character1_name))
                {
                    record.Fields["Char1Name"] = line.character1_name;
                    record.Fields["Char1Look"] = $"{line.character1_expression}_{line.character1_pose}";
                    record.Fields["Char1Pos"] = line.character1_position ?? "Center";
                    record.Fields["Char1Size"] = "Medium";
                }

                // 캐릭터 2
                if (!string.IsNullOrEmpty(line.character2_name))
                {
                    record.Fields["Char2Name"] = line.character2_name;
                    record.Fields["Char2Look"] = $"{line.character2_expression}_{line.character2_pose}";
                    record.Fields["Char2Pos"] = line.character2_position ?? "Left";
                    record.Fields["Char2Size"] = "Medium";
                }

                // 선택지
                if (line.choices != null && line.choices.Length > 0)
                {
                    for (int c = 0; c < line.choices.Length && c < 4; c++) // 최대 4개
                    {
                        int choiceNum = c + 1;
                        record.Fields[$"Choice{choiceNum}_ENG"] = line.choices[c].text ?? "";
                        record.Fields[$"Next{choiceNum}"] = line.choices[c].next_id.ToString();

                        // Value Impact 처리
                        if (line.choices[c].value_impact != null)
                        {
                            foreach (var impact in line.choices[c].value_impact)
                            {
                                record.Fields[$"Choice{choiceNum}_ValueImpact_{impact.value_name}"] = impact.change.ToString();
                            }
                        }
                    }
                }
                else
                {
                    // 선택지 없으면 다음 라인으로 자동 진행
                    record.Fields["Auto"] = "TRUE";
                }

                // CG 정보
                if (!string.IsNullOrEmpty(line.cg_id))
                {
                    record.Fields["CG_ID"] = line.cg_id;
                    record.Fields["CG_Title"] = line.cg_title ?? "";
                    record.Fields["IsCGLine"] = "TRUE";
                }

                record.FinalizeIndex();
                records.Add(record);
            }

            return records;
        }

        // ===== AI JSON 스키마 =====

        [System.Serializable]
        private class AIDialogueWrapper
        {
            public AIDialogueLine[] lines;
        }

        [System.Serializable]
        public class AIDialogueLine
        {
            public string speaker;
            public string text;

            // 캐릭터 1
            public string character1_name;
            public string character1_expression;
            public string character1_pose;
            public string character1_position;

            // 캐릭터 2
            public string character2_name;
            public string character2_expression;
            public string character2_pose;
            public string character2_position;

            // 배경/음향
            public string bg_name;
            public string bgm_name;
            public string sfx_name;

            // 선택지
            public ChoiceData[] choices;

            // CG 정보 (레퍼런스 기반)
            public string cg_id;
            public string cg_title;
            public string cg_scene_description;
            public string cg_lighting;
            public string cg_mood;
            public string cg_camera_angle;
            public string[] cg_characters;
        }

        [System.Serializable]
        public class ChoiceData
        {
            public string text;
            public int next_id;
            public ValueImpact[] value_impact;
        }

        [System.Serializable]
        public class ValueImpact
        {
            public string value_name; // 예: "Courage"
            public int change; // 예: +10 또는 -5
        }
    }
}
