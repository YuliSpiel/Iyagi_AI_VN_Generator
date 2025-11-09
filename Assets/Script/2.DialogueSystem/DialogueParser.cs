using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DialogueParser : MonoBehaviour
{
    public static Dictionary<int, DialogueRecord> ParseToDict(
        string csv,
        int idColumnIndex = 45,
        bool skipDescriptionRow = true
    )
    {
        var dict = new Dictionary<int, DialogueRecord>();
        if (string.IsNullOrEmpty(csv)) return dict;

        var lines = SplitLines(csv);
        if (lines.Count == 0) return dict;

        // 1) 헤더 탐색
        int headerIdx = -1;
        List<string> headerRaw = null;
        for (int i = 0; i < lines.Count; i++)
        {
            var cols0 = ParseCsvLine(lines[i]);
            if (cols0.Count == 0) continue;
            headerIdx = i;
            headerRaw = cols0;
            break;
        }
        if (headerIdx < 0 || headerRaw == null || headerRaw.Count == 0) return dict;

        // 1-1) 헤더 정규화(트리밍/특수공백/BOM 제거 + 별칭 표준화)
        var header = new List<string>(headerRaw.Count);
        for (int c = 0; c < headerRaw.Count; c++)
            header.Add(CanonHeader(headerRaw[c]));

        // 2) 데이터 시작(설명행 스킵 옵션)
        int dataStart = headerIdx + 1 + (skipDescriptionRow ? 1 : 0);

        int autoId = 0;
        for (int row = dataStart; row < lines.Count; row++)
        {
            var cols = ParseCsvLine(lines[row]);
            if (cols.Count == 0 || (cols.Count == 1 && string.IsNullOrEmpty(cols[0]))) continue;

            var rec = new DialogueRecord();

            // 헤더 개수 기준으로 매핑(부족한 열은 빈 문자열)
            for (int c = 0; c < header.Count; c++)
            {
                string key = header[c];               // ← 정규화된 헤더명
                string val = (c < cols.Count) ? (cols[c] ?? string.Empty) : string.Empty;
                rec.Fields[key] = val;
            }
            rec.FinalizeIndex();

            // ID 결정: 46열 → "ID" 헤더 → 자동
            int id;
            string idRaw = string.Empty;

            if (idColumnIndex >= 0)
                idRaw = (idColumnIndex < cols.Count) ? (cols[idColumnIndex] ?? string.Empty) : string.Empty;

            if (!int.TryParse(idRaw, out id))
                idRaw = rec.GetFirst("ID", "Id", "id");

            if (!int.TryParse(idRaw, out id))
            {
                id = ++autoId;
                rec.Fields["ID"] = id.ToString();
                UnityEngine.Debug.LogWarning($"[DialogueParser] ID empty at CSV line {row + 1}. Assigned auto ID={id}");
            }
            else
            {
                if (id > autoId) autoId = id;
            }

            if (!dict.ContainsKey(id)) dict[id] = rec;
            else UnityEngine.Debug.LogWarning($"[DialogueParser] Duplicate ID {id} at CSV line {row + 1}. Skipped.");
        }

        return dict;
    }

    // ── 헤더 정규화: trim + BOM/특수공백 제거 + 별칭 표준화 ─────────────
    private static string CanonHeader(string h)
    {
        if (string.IsNullOrEmpty(h)) return string.Empty;

        // 트리밍 + BOM 제거 + NBSP(비가시 공백) 정규화
        string t = h.Trim().Trim('\uFEFF').Replace('\u00A0', ' ');
        // 다중 공백 → 한 칸
        while (t.Contains("  ")) t = t.Replace("  ", " ");

        // 소문자 비교로 별칭 매핑
        string L = t.ToLowerInvariant();

        // EventTrigger
        if (L == "trigger" || L == "event trigger" || L == "이벤트트리거" || L == "이벤트 트리거")
            return "EventTrigger";

        // Param1 / Param2
        if (L == "param1" || L == "param 1" || L == "param_1" || L == "파라미터1" || L == "이벤트파라미터1")
            return "Param1";
        if (L == "param2" || L == "param 2" || L == "param_2" || L == "파라미터2" || L == "이벤트파라미터2")
            return "Param2";

        // NextIndex1/2/3 (공백/언더스코어 표기 허용)
        if (L.Replace(" ", "").Replace("_", "") == "nextindex1") return "NextIndex1";
        if (L.Replace(" ", "").Replace("_", "") == "nextindex2") return "NextIndex2";
        if (L.Replace(" ", "").Replace("_", "") == "nextindex3") return "NextIndex3";

        // Auto
        if (L == "auto" || L == "자동" || L == "오토") return "Auto";

        // KR/ENG/JPN/GER 등은 그대로 두되, 트리밍 결과만 반영
        return t;
    }

    // ── 이하 CSV 유틸 그대로 ──────────────────────────────────────────
    private static List<string> SplitLines(string text)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(text)) return list;

        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                string line = text.Substring(start, i - start);
                if (line.EndsWith("\r")) line = line.Substring(0, line.Length - 1);
                list.Add(line);
                start = i + 1;
            }
        }
        if (start < text.Length)
        {
            string line = text.Substring(start);
            if (line.EndsWith("\r")) line = line.Substring(0, line.Length - 1);
            list.Add(line);
        }
        return list;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var cols = new List<string>();
        if (line == null) return cols;
        if (line.Length == 0) { cols.Add(string.Empty); return cols; }

        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                { sb.Append('"'); i++; } // "" → "
                else { inQuotes = !inQuotes; }
            }
            else if (ch == ',' && !inQuotes)
            { cols.Add(sb.ToString()); sb.Length = 0; }
            else
            { sb.Append(ch); }
        }
        cols.Add(sb.ToString());

        // 양끝 큰따옴표 제거(내부 공백/특수문자/\n 보존)
        for (int c = 0; c < cols.Count; c++)
        {
            string v = cols[c];
            if (v.Length >= 2 && v[0] == '"' && v[v.Length - 1] == '"')
                v = v.Substring(1, v.Length - 2);
            cols[c] = v;
        }
        return cols;
    }
}
