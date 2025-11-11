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
            AIDialogueWrapper wrapper = null;

            try
            {
                // JSON 배열 추출 (Gemini 응답에서 [...] 부분만 추출)
                int startIndex = jsonResponse.IndexOf('[');
                int endIndex = jsonResponse.LastIndexOf(']');

                if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
                {
                    Debug.LogError("Invalid JSON format: No array found");
                    Debug.LogError($"Response preview: {jsonResponse.Substring(0, Mathf.Min(500, jsonResponse.Length))}");
                    return new List<Runtime.DialogueRecord>();
                }

                string jsonArray = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);

                // JSON 복구 시도: 닫는 괄호가 없으면 추가
                jsonArray = AttemptJSONRepair(jsonArray);

                // JSON 파싱
                string wrappedJson = "{\"lines\":" + jsonArray + "}";
                Debug.Log($"[AIDataConverter] Attempting to parse {wrappedJson.Length} chars of JSON");

                wrapper = JsonUtility.FromJson<AIDialogueWrapper>(wrappedJson);

                if (wrapper == null || wrapper.lines == null || wrapper.lines.Length == 0)
                {
                    Debug.LogError("Failed to parse JSON or empty lines");
                    Debug.LogError($"Wrapped JSON preview: {wrappedJson.Substring(0, Mathf.Min(500, wrappedJson.Length))}");
                    return new List<Runtime.DialogueRecord>();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON parsing error: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
                return new List<Runtime.DialogueRecord>();
            }

            List<Runtime.DialogueRecord> records = new List<Runtime.DialogueRecord>();
            baseId = chapterId * 1000; // 챕터 1 = 1000~, 챕터 2 = 2000~

            for (int i = 0; i < wrapper.lines.Length; i++)
            {
                try
                {
                    var line = wrapper.lines[i];

                    // Null 체크
                    if (line == null)
                    {
                        Debug.LogWarning($"[AIDataConverter] Line {i} is null, skipping");
                        continue;
                    }

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

                            // Skill Impact 처리
                            if (line.choices[c].skill_impact != null)
                            {
                                foreach (var skillImpact in line.choices[c].skill_impact)
                                {
                                    record.Fields[$"Choice{choiceNum}_SkillImpact_{skillImpact.skill_name}"] = skillImpact.change.ToString();
                                }
                            }

                            // Affection Impact 처리 (새로 추가)
                            if (line.choices[c].affection_impact != null)
                            {
                                foreach (var affectionImpact in line.choices[c].affection_impact)
                                {
                                    record.Fields[$"Choice{choiceNum}_AffectionImpact_{affectionImpact.character_name}"] = affectionImpact.change.ToString();
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
                catch (System.Exception e)
                {
                    Debug.LogError($"[AIDataConverter] Error processing line {i}: {e.Message}");
                    Debug.LogError($"[AIDataConverter] Stack trace: {e.StackTrace}");
                    // 에러 발생해도 계속 진행 (나머지 라인 처리)
                }
            }

            return records;
        }

        /// <summary>
        /// JSON 복구 시도: 불완전한 JSON 배열을 수정
        /// </summary>
        private static string AttemptJSONRepair(string jsonArray)
        {
            // 1. 기본 검증: 대괄호 균형 확인
            int openBrackets = 0;
            int closeBrackets = 0;
            int openBraces = 0;
            int closeBraces = 0;

            foreach (char c in jsonArray)
            {
                if (c == '[') openBrackets++;
                else if (c == ']') closeBrackets++;
                else if (c == '{') openBraces++;
                else if (c == '}') closeBraces++;
            }

            Debug.Log($"[JSON Repair] Brackets: [{openBrackets}] vs [{closeBrackets}], Braces: {{{openBraces}}} vs {{{closeBraces}}}");

            // 2. 마지막 완성된 객체 찾기 (마지막 }를 찾음)
            int lastCompleteBrace = jsonArray.LastIndexOf('}');

            if (lastCompleteBrace == -1)
            {
                Debug.LogWarning("[JSON Repair] No complete object found, returning empty array");
                return "[]";
            }

            // 3. 불완전한 부분 제거 (마지막 } 이후 버림)
            string repairedJson = jsonArray.Substring(0, lastCompleteBrace + 1);

            // 4. 배열 닫기 추가
            if (!repairedJson.TrimEnd().EndsWith("]"))
            {
                repairedJson += "\n]";
                Debug.Log("[JSON Repair] Added closing bracket ]");
            }

            // 5. 시작 대괄호 확인
            if (!repairedJson.TrimStart().StartsWith("["))
            {
                repairedJson = "[\n" + repairedJson;
                Debug.Log("[JSON Repair] Added opening bracket [");
            }

            Debug.Log($"[JSON Repair] Repaired JSON length: {repairedJson.Length} (original: {jsonArray.Length})");
            return repairedJson;
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
            public SkillImpact[] skill_impact; // 파생 스킬 임팩트
            public AffectionImpact[] affection_impact; // 친밀도 임팩트 (새로 추가)
        }

        [System.Serializable]
        public class ValueImpact
        {
            public string value_name; // 예: "Courage"
            public int change; // 예: +10 또는 -5
        }

        [System.Serializable]
        public class SkillImpact
        {
            public string skill_name; // 예: "공감능력", "판단력"
            public int change; // 예: +10 또는 -5
        }

        [System.Serializable]
        public class AffectionImpact
        {
            public string character_name; // 예: "Hans", "Heilner"
            public int change; // 예: +10 또는 -5
        }
    }
}
