using System;
using System.Collections.Generic;
using System.Linq;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// CSV 한 행을 헤더명→값으로 보존하는 데이터 컨테이너.
    /// - 모든 컬럼 문자열 그대로 유지(파서가 양끝 큰따옴표 제거/""→" 복원)
    /// - 대소문자 무시 조회 인덱스 제공
    /// - 다국어 본문/선택지/화자명 헬퍼 제공
    /// </summary>
    [Serializable]
    public class DialogueRecord
    {
        public Dictionary<string, string> Fields = new Dictionary<string, string>();
        private Dictionary<string, string> _ci = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>대소문자 무시 인덱스 재구축</summary>
        public void FinalizeIndex()
        {
            _ci.Clear();
            foreach (var kv in Fields)
            {
                var key = kv.Key?.Trim() ?? string.Empty;
                if (key.Length == 0) continue;
                if (!_ci.ContainsKey(key)) _ci[key] = kv.Value ?? string.Empty;
            }
        }

        public string GetRaw(string key)
            => (key != null && Fields.TryGetValue(key, out var v)) ? (v ?? string.Empty) : string.Empty;

        public string Get(string key)
            => (key != null && _ci.TryGetValue(key, out var v)) ? (v ?? string.Empty) : string.Empty;

        /// <summary>여러 후보 키를 순서대로 조회해 처음 나오는 비어있지 않은 값을 반환</summary>
        public string GetFirst(params string[] keys)
        {
            if (keys == null) return string.Empty;
            foreach (var k in keys)
            {
                var v = Get(k);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return string.Empty;
        }

        public bool Has(string key) => key != null && _ci.ContainsKey(key);

        public bool TryGetInt(string key, out int value) => int.TryParse(Get(key), out value);
        public bool TryGetFloat(string key, out float v) => float.TryParse(Get(key), out v);
        public bool TryGetBool(string key, out bool v)
        {
            var s = Get(key)?.Trim().ToLowerInvariant();
            if (s == "true" || s == "1" || s == "yes" || s == "y") { v = true; return true; }
            if (s == "false"|| s == "0" || s == "no"  || s == "n") { v = false; return true; }
            v = false; return false;
        }

        // ───────────────────────────── 텍스트/선택지/화자명 ─────────────────────────────

        /// <summary>본문 텍스트(언어별) 폴백 포함</summary>
        public string GetTextByLang(GameLanguage lang)
        {
            switch (lang)
            {
                case GameLanguage.English: return FirstNonEmpty(Get("ENG"), Get("EN"), Get("ParsedLine_ENG"));
                default:   return FirstNonEmpty(Get("KOR"),  Get("KR"), Get("ParsedLine_KOR"));
            }
        }

        /// <summary>선택지 텍스트(언어별) 폴백 포함</summary>
        public string GetChoiceText(int idx, GameLanguage lang)
        {
            string baseKey = $"Choice_{idx}";
            switch (lang)
            {
                case GameLanguage.English: return FirstNonEmpty(Get($"{baseKey}_ENG"), Get($"{baseKey}_EN"), Get(baseKey));
                case GameLanguage.Japanese: return FirstNonEmpty(Get($"{baseKey}_JPN"), Get($"{baseKey}_JP"), Get(baseKey));
                default:   return FirstNonEmpty(Get($"{baseKey}_KOR"),  Get($"{baseKey}_KO"), Get(baseKey));
            }
        }

        /// <summary>
        /// 화자명(네임태그) 조회. 언어별 컬럼 우선 → 범용 컬럼 → 캐릭터명 폴백.
        /// 시트가 Speaker/Name/Nametag 등 어떤 표기를 써도 대응.
        /// </summary>
        public string GetSpeakerByLang(GameLanguage lang = GameLanguage.Korean)
        {
            // Speaker 컬럼 값(예: "sasha", "narrator")
            string key = GetFirst("NameTag", "Speaker", "CharacterName").Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(key))
                return "";

            // 하드코딩된 스피커 이름 매핑
            switch (key)
            {
                case "hans":
                    return lang switch
                    {
                        GameLanguage.Korean  => "한스",
                        GameLanguage.English => "Hans",
                        _ => "Hans"
                    };

                case "heilner":
                    return lang switch
                    {
                        GameLanguage.Korean  => "하일너",
                        GameLanguage.English => "Heilner",
                        _ => "Heilner"
                    };

                case "principal":
                    return lang switch
                    {
                        GameLanguage.Korean  => "교장",
                        GameLanguage.English => "Principal",
                        _ => "Principal"
                    };

                case "narrator":
                case "system":
                    return lang switch
                    {
                        GameLanguage.Korean  => "나레이터",
                        GameLanguage.English => "Narrator",
                        _ => "Narrator"
                    };

                default:
                    // 매핑 안 된 스피커는 그대로 키 반환
                    return key;
            }
        }

        // ───────────────────────────── AI 시스템용 추가 메서드 ─────────────────────────────

        /// <summary>문자열 값 가져오기 (Get과 동일하지만 명시적)</summary>
        public string GetString(string key) => Get(key);

        /// <summary>CG가 있는지 확인</summary>
        public bool HasCG()
        {
            string cgId = Get("CG_ID");
            return !string.IsNullOrEmpty(cgId) && cgId != "0" && cgId.ToLowerInvariant() != "none";
        }

        /// <summary>선택지가 있는지 확인</summary>
        public bool HasChoices()
        {
            // Choice1이 있으면 선택지가 있다고 판단
            string choice1 = Get("Choice1");
            return !string.IsNullOrEmpty(choice1);
        }

        /// <summary>선택지 개수 반환</summary>
        public int GetChoiceCount()
        {
            int count = 0;
            for (int i = 1; i <= 10; i++) // 최대 10개 선택지
            {
                string choice = Get($"Choice{i}");
                if (string.IsNullOrEmpty(choice))
                    break;
                count++;
            }
            return count;
        }

        // ───────────────────────────── 유틸 ─────────────────────────────

        private string FirstNonEmpty(params string[] arr)
            => arr?.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;

        // /// <summary>표준 언어코드에 대한 별칭 목록 반환. 우선순위 높은 순.</summary>
        // private IReadOnlyList<string> LangAliases(string std)
        // {
        //     switch (std)
        //     {
        //         case "EN": return _en;
        //         case "JP": return _jp;
        //         case "DE": return _de;
        //         default:   return _kr;
        //     }
        // }

        private static readonly string[] _kr = { "KOR", "KR" };
        private static readonly string[] _en = { "ENG", "EN" };
        private static readonly string[] _jp = { "JPN", "JP" };
        private static readonly string[] _de = { "GER", "DE" };

        public IReadOnlyDictionary<string, string> All => Fields;
    }
}
