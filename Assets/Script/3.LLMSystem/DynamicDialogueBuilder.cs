using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LLM이 생성한 JSON 스토리를 DialogueRecord 객체로 변환하는 빌더 클래스
/// 기존 CSV 시스템과 호환되도록 DialogueRecord를 동적으로 생성합니다.
/// </summary>
public class DynamicDialogueBuilder : MonoBehaviour
{
    private int nextId = 10000; // LLM 생성 대사는 10000번대 ID 사용

    /// <summary>
    /// JSON 배열 문자열을 파싱하여 DialogueRecord 리스트로 변환
    /// </summary>
    public List<DialogueRecord> BuildFromJson(string jsonArray)
    {
        List<DialogueRecord> records = new List<DialogueRecord>();

        try
        {
            // Unity's JsonUtility doesn't support arrays directly, so we wrap it
            string wrappedJson = $"{{\"dialogues\":{jsonArray}}}";
            LLMStoryResponse response = JsonUtility.FromJson<LLMStoryResponse>(wrappedJson);

            if (response == null || response.dialogues == null || response.dialogues.Count == 0)
            {
                Debug.LogError("[DynamicDialogueBuilder] Failed to parse JSON or empty response");
                return records;
            }

            Debug.Log($"[DynamicDialogueBuilder] Parsed {response.dialogues.Count} dialogue entries");

            for (int i = 0; i < response.dialogues.Count; i++)
            {
                var entry = response.dialogues[i];
                int currentId = nextId++;
                int nextDialogueId = (i < response.dialogues.Count - 1) ? nextId : -1;

                DialogueRecord record = BuildRecord(entry, currentId, nextDialogueId);
                records.Add(record);

                Debug.Log($"[DynamicDialogueBuilder] Created record ID={currentId}, Speaker={entry.nameTag}");
            }

            Debug.Log($"[DynamicDialogueBuilder] Successfully built {records.Count} DialogueRecords");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DynamicDialogueBuilder] Error parsing JSON: {e.Message}\nJSON: {jsonArray}");
        }

        return records;
    }

    /// <summary>
    /// 단일 LLMDialogueEntry를 DialogueRecord로 변환
    /// </summary>
    private DialogueRecord BuildRecord(LLMDialogueEntry entry, int id, int nextId)
    {
        DialogueRecord record = new DialogueRecord();

        // 기본 정보
        record.Fields["ID"] = id.ToString();
        record.Fields["Scene"] = "1";
        record.Fields["Index"] = (id - 10000 + 1).ToString();

        // 캐릭터 1
        if (!string.IsNullOrEmpty(entry.char1Name))
        {
            record.Fields["Char1Name"] = entry.char1Name;
            record.Fields["Char1Look"] = !string.IsNullOrEmpty(entry.char1Look) ? entry.char1Look : "Normal_Normal";
            record.Fields["Char1Pos"] = !string.IsNullOrEmpty(entry.char1Pos) ? entry.char1Pos : "Center";
            record.Fields["Char1Size"] = !string.IsNullOrEmpty(entry.char1Size) ? entry.char1Size : "Medium";
        }
        else
        {
            record.Fields["Char1Name"] = "";
            record.Fields["Char1Look"] = "";
            record.Fields["Char1Pos"] = "";
            record.Fields["Char1Size"] = "";
        }

        // 캐릭터 2
        if (!string.IsNullOrEmpty(entry.char2Name))
        {
            record.Fields["Char2Name"] = entry.char2Name;
            record.Fields["Char2Look"] = !string.IsNullOrEmpty(entry.char2Look) ? entry.char2Look : "Normal_Normal";
            record.Fields["Char2Pos"] = !string.IsNullOrEmpty(entry.char2Pos) ? entry.char2Pos : "Right";
            record.Fields["Char2Size"] = !string.IsNullOrEmpty(entry.char2Size) ? entry.char2Size : "Medium";
        }
        else
        {
            record.Fields["Char2Name"] = "";
            record.Fields["Char2Look"] = "";
            record.Fields["Char2Pos"] = "";
            record.Fields["Char2Size"] = "";
        }

        // 배경
        record.Fields["BG"] = !string.IsNullOrEmpty(entry.bg) ? entry.bg : "";

        // 화자명
        record.Fields["NameTag"] = !string.IsNullOrEmpty(entry.nameTag) ? entry.nameTag : "";

        // 대사 텍스트
        record.Fields["Line_ENG"] = !string.IsNullOrEmpty(entry.lineEng) ? entry.lineEng : "";
        record.Fields["Line_KR"] = !string.IsNullOrEmpty(entry.lineKr) ? entry.lineKr : "";
        record.Fields["ParsedLine_ENG"] = record.Fields["Line_ENG"];
        record.Fields["ParsedLine_KOR"] = record.Fields["Line_KR"];

        // 자동 진행
        record.Fields["Auto"] = "FALSE";

        // 선택지 처리
        if (entry.choices != null && entry.choices.Count > 0)
        {
            for (int i = 0; i < entry.choices.Count && i < 3; i++)
            {
                int choiceNum = i + 1;
                var choice = entry.choices[i];

                record.Fields[$"Choice{choiceNum}ENG"] = !string.IsNullOrEmpty(choice.textEng) ? choice.textEng : "";
                record.Fields[$"Choice{choiceNum}KR"] = !string.IsNullOrEmpty(choice.textKr) ? choice.textKr : "";
                record.Fields[$"C{choiceNum}_ENG"] = record.Fields[$"Choice{choiceNum}ENG"];
                record.Fields[$"C{choiceNum}_KOR"] = record.Fields[$"Choice{choiceNum}KR"];

                // 선택지가 있으면 다음 ID는 선택에 따라 분기 (현재는 단순히 nextId 사용)
                record.Fields[$"Next{choiceNum}"] = nextId > 0 ? nextId.ToString() : "";
            }

            // 나머지 선택지는 빈 값
            for (int i = entry.choices.Count; i < 3; i++)
            {
                int choiceNum = i + 1;
                record.Fields[$"Choice{choiceNum}ENG"] = "";
                record.Fields[$"Choice{choiceNum}KR"] = "";
                record.Fields[$"C{choiceNum}_ENG"] = "";
                record.Fields[$"C{choiceNum}_KOR"] = "";
                record.Fields[$"Next{choiceNum}"] = "";
            }
        }
        else
        {
            // 선택지 없음
            for (int i = 1; i <= 3; i++)
            {
                record.Fields[$"Choice{i}ENG"] = "";
                record.Fields[$"Choice{i}KR"] = "";
                record.Fields[$"C{i}_ENG"] = "";
                record.Fields[$"C{i}_KOR"] = "";
                record.Fields[$"Next{i}"] = "";
            }
        }

        // SFX, Trigger 등은 비워둠
        record.Fields["SFX"] = "";
        record.Fields["Trigger"] = "";
        record.Fields["Param1"] = "";
        record.Fields["Param2"] = "";

        // 인덱스 재구축
        record.FinalizeIndex();

        return record;
    }

    /// <summary>
    /// DialogueRecord 리스트를 Dictionary로 변환 (ID를 키로 사용)
    /// </summary>
    public Dictionary<int, DialogueRecord> ConvertToDict(List<DialogueRecord> records)
    {
        Dictionary<int, DialogueRecord> dict = new Dictionary<int, DialogueRecord>();

        foreach (var record in records)
        {
            if (record.TryGetInt("ID", out int id))
            {
                dict[id] = record;
            }
        }

        return dict;
    }

    /// <summary>
    /// 다음 생성될 ID를 설정 (선택적)
    /// </summary>
    public void SetNextId(int id)
    {
        nextId = id;
    }

    /// <summary>
    /// 현재 다음 ID 값을 반환
    /// </summary>
    public int GetNextId()
    {
        return nextId;
    }
}
