using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using IyagiAI.AISystem;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 챕터 생성 및 캐싱 관리
    /// Gemini API로 챕터를 동적 생성하고 persistentDataPath에 캐싱
    /// </summary>
    public class ChapterGenerationManager : MonoBehaviour
    {
        [Header("Data")]
        public VNProjectData projectData;

        // 캐시 키: "{projectGuid}_Ch{ChapterNum}_{ValueHash}"
        private Dictionary<string, ChapterData> chapterCache = new Dictionary<string, ChapterData>();

        [Header("API")]
        public GeminiClient geminiClient;
        public NanoBananaClient nanoBananaClient;

        [Header("Cache Settings")]
        public bool enableCaching = true; // ✅ 캐싱 활성화 (Chapter-Level Convergence 구조)
        private string CACHE_FOLDER => Path.Combine(Application.persistentDataPath, "Chapters", projectData.projectGuid);

        void Start()
        {
            LoadCacheFromDisk();
        }

        /// <summary>
        /// 챕터 생성 또는 캐시에서 로드 (씬 단위로 분할 생성)
        /// </summary>
        public IEnumerator GenerateOrLoadChapter(int chapterId, GameStateSnapshot state, System.Action<List<DialogueRecord>> onComplete)
        {
            // 0. 캐시가 아직 로드되지 않았으면 로드
            if (enableCaching && chapterCache.Count == 0)
            {
                LoadCacheFromDisk();
            }

            // 1. 캐시 키 생성 (Core Value 기반)
            string cacheKey = GenerateCacheKey(chapterId, state);
            Debug.Log($"[ChapterGenerationManager] Cache key: {cacheKey}");

            // 2. 캐시 확인
            if (enableCaching && chapterCache.ContainsKey(cacheKey))
            {
                Debug.Log($"[ChapterGenerationManager] ✅ Loading cached chapter from key: {cacheKey}");
                onComplete?.Invoke(chapterCache[cacheKey].records);
                yield break;
            }

            // 3. 씬 단위로 분할 생성 (6개 씬: 선택지 3개 포함)
            Debug.Log($"[ChapterGenerationManager] Generating chapter {chapterId} in 6 scenes");

            int totalScenes = 6;
            List<DialogueRecord> allRecords = new List<DialogueRecord>();
            string previousScenesContext = "";
            int cumulativeLineCount = 0; // 누적된 라인 수 (씬 간 ID 중복 방지용)

            for (int sceneNum = 1; sceneNum <= totalScenes; sceneNum++)
            {
                Debug.Log($"[ChapterGenerationManager] Generating scene {sceneNum}/{totalScenes} (startOffset: {cumulativeLineCount})");

                string scenePrompt = BuildScenePrompt(chapterId, sceneNum, totalScenes, state, previousScenesContext);

                bool sceneCompleted = false;
                List<DialogueRecord> sceneRecords = null;

                yield return geminiClient.GenerateContent(
                    scenePrompt,
                    (jsonResponse) => {
                        Debug.Log($"[ChapterGenerationManager] Scene {sceneNum} response length: {jsonResponse?.Length ?? 0}");

                        // JSON 추출 및 변환 (startOffset 전달)
                        sceneRecords = AIDataConverter.FromAIJson(jsonResponse, chapterId, cumulativeLineCount);

                        if (sceneRecords != null && sceneRecords.Count > 0)
                        {
                            Debug.Log($"[ChapterGenerationManager] Scene {sceneNum}: {sceneRecords.Count} records");

                            // 이전 씬 컨텍스트 업데이트 (다음 씬에 전달)
                            previousScenesContext += $"\n=== Scene {sceneNum} ===\n";
                            foreach (var rec in sceneRecords)
                            {
                                previousScenesContext += $"{rec.Get("Speaker")}: {rec.Get("ParsedLine_ENG")}\n";
                            }
                        }

                        sceneCompleted = true;
                    },
                    (error) => {
                        Debug.LogError($"[ChapterGenerationManager] Scene {sceneNum} error: {error}");
                        sceneCompleted = true;
                    }
                );

                yield return new WaitUntil(() => sceneCompleted);

                if (sceneRecords != null && sceneRecords.Count > 0)
                {
                    // 이전 씬의 마지막 라인과 현재 씬의 첫 라인을 연결
                    if (allRecords.Count > 0 && sceneNum > 1)
                    {
                        var lastRecordOfPreviousScene = allRecords[allRecords.Count - 1];
                        var firstRecordOfCurrentScene = sceneRecords[0];

                        // 이전 씬 마지막 라인에 NextIndex1이 없으면 현재 씬 첫 라인으로 연결
                        if (!lastRecordOfPreviousScene.Has("NextIndex1") || string.IsNullOrEmpty(lastRecordOfPreviousScene.Get("NextIndex1")))
                        {
                            lastRecordOfPreviousScene.Fields["NextIndex1"] = firstRecordOfCurrentScene.Get("ID");
                            lastRecordOfPreviousScene.Fields["Auto"] = "TRUE";
                            Debug.Log($"[ChapterGenerationManager] Connected Scene {sceneNum - 1} last line to Scene {sceneNum} first line");
                        }
                    }

                    allRecords.AddRange(sceneRecords);

                    // 다음 씬을 위해 누적 라인 수 업데이트
                    cumulativeLineCount += sceneRecords.Count;
                    Debug.Log($"[ChapterGenerationManager] Cumulative line count: {cumulativeLineCount}");
                }
                else
                {
                    Debug.LogWarning($"[ChapterGenerationManager] Scene {sceneNum} failed, continuing...");
                }
            }

            if (allRecords.Count > 0)
            {
                Debug.Log($"[ChapterGenerationManager] ✅ Chapter {chapterId} complete: {allRecords.Count} total records");

                // 4. 캐시에 저장
                var chapterData = new ChapterData(chapterId, allRecords);
                chapterData.stateSnapshot = state;
                chapterCache[cacheKey] = chapterData;

                // 5. 디스크에 저장
                if (enableCaching)
                {
                    SaveChapterToFile(cacheKey, chapterData);
                }

                onComplete?.Invoke(allRecords);
            }
            else
            {
                Debug.LogError($"Failed to generate chapter {chapterId}");
                onComplete?.Invoke(new List<DialogueRecord>());
            }
        }

        /// <summary>
        /// 챕터 요약 생성 (다음 챕터의 맥락으로 사용)
        /// </summary>
        public IEnumerator GenerateChapterSummary(int chapterId, List<DialogueRecord> records, GameStateSnapshot state, System.Action<string> onComplete)
        {
            if (records == null || records.Count == 0)
            {
                onComplete?.Invoke("");
                yield break;
            }

            // 챕터 전체 대사를 요약용 텍스트로 변환
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Chapter {chapterId} Dialogue:");
            foreach (var rec in records)
            {
                string speaker = rec.Get("NameTag") ?? rec.Get("Speaker") ?? "Narrator";
                string text = rec.Get("ParsedLine_ENG") ?? rec.Get("Line_ENG") ?? "";
                if (!string.IsNullOrEmpty(text))
                {
                    sb.AppendLine($"{speaker}: {text}");
                }
            }

            string chapterDialogue = sb.ToString();

            // 현재 친밀도 상태
            string affectionContext = "";
            if (state != null && state.characterAffections != null && state.characterAffections.Count > 0)
            {
                var sb2 = new System.Text.StringBuilder();
                sb2.AppendLine("\nCurrent Character Relationships:");
                foreach (var kv in state.characterAffections)
                {
                    string relationshipLevel = kv.Value >= 70 ? "Close" : (kv.Value >= 50 ? "Friendly" : (kv.Value >= 30 ? "Neutral" : "Distant"));
                    sb2.AppendLine($"  - {kv.Key}: {kv.Value} ({relationshipLevel})");
                }
                affectionContext = sb2.ToString();
            }

            // AI에게 요약 요청
            string summaryPrompt = $@"You are a story summarizer for a visual novel game.

Summarize the following chapter in 2-3 sentences that capture:
1. The main events that happened
2. Key character interactions or relationship developments
3. Any important plot points or revelations
4. Notable changes in character relationships (if affection scores changed significantly)

Keep it concise and focused on story progression.
{affectionContext}

Chapter Dialogue:
{chapterDialogue}

Output ONLY the summary text (no JSON, no formatting).";

            bool completed = false;
            string summary = "";

            yield return geminiClient.GenerateContent(
                summaryPrompt,
                (response) => {
                    summary = response.Trim();
                    completed = true;
                },
                (error) => {
                    Debug.LogWarning($"[ChapterGenerationManager] Failed to generate summary: {error}");
                    summary = $"Chapter {chapterId} completed."; // Fallback
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            Debug.Log($"[ChapterGenerationManager] Chapter {chapterId} Summary: {summary}");
            onComplete?.Invoke(summary);
        }

        /// <summary>
        /// 씬별 프롬프트 빌드 (3-5개 대사만 생성)
        /// </summary>
        private string BuildScenePrompt(int chapterId, int sceneNumber, int totalScenes, GameStateSnapshot state, string previousScenes = "")
        {
            // 캐릭터 목록 (말투 예시 포함)
            string characterList = "";

            // Player 캐릭터
            characterList += $"\n  - Player: {projectData.playerCharacter.characterName}";
            if (!string.IsNullOrEmpty(projectData.playerCharacter.sampleDialogue))
            {
                characterList += $"\n    Speech Style: \"{projectData.playerCharacter.sampleDialogue}\"";
            }

            // NPCs
            foreach (var npc in projectData.npcs)
            {
                characterList += $"\n  - NPC: {npc.characterName}";
                if (!string.IsNullOrEmpty(npc.sampleDialogue))
                {
                    characterList += $"\n    Speech Style: \"{npc.sampleDialogue}\"";
                }
            }

            // Core Values 및 Derived Skills 목록
            string coreValuesInfo = "";
            foreach (var value in projectData.coreValues)
            {
                string skills = value.derivedSkills != null && value.derivedSkills.Count > 0
                    ? string.Join(", ", value.derivedSkills)
                    : "none";
                coreValuesInfo += $"\n  - {value.name}: [{skills}]";
            }

            // 게임 상태 요약
            string stateInfo = state != null ? state.ToPromptString() : "Initial chapter";

            // 이전 씬 컨텍스트 (있으면)
            string contextSection = "";
            if (!string.IsNullOrEmpty(previousScenes))
            {
                contextSection = $@"
# Previous Scenes in This Chapter
{previousScenes}

Continue the story naturally from where it left off.
";
            }

            string prompt = $@"You are a visual novel story generator.

# Game Information
- Title: {projectData.gameTitle}
- Premise: {projectData.gamePremise}
- Genre: {projectData.genre}
- Tone: {projectData.tone}
- Total Chapters: {projectData.totalChapters}
- Characters: {characterList}
- Core Values and Derived Skills:{coreValuesInfo}

# Current State
{stateInfo}
{contextSection}

# CRITICAL - Character Relationship Dynamics
**The affection scores above DIRECTLY AFFECT NPC dialogue tone and reactions.**

**IMPORTANT**: NPCs MUST still appear in scenes and interact with the player regardless of affection level.
Low affection changes HOW they interact, NOT whether they interact.

Affection-Based Dialogue Variations:
- **High Affection (70+)**:
  * NPC actively helps the player, offers support willingly
  * NPC shares personal stories or secrets
  * NPC shows concern when player is in danger
  * Dialogue is warm, affectionate, remembers past interactions
  * NPC may initiate romantic/intimate moments
  * Example: ""Of course I'll help! We're in this together.""

- **Medium Affection (50-69)**:
  * NPC cooperates but maintains professional distance
  * NPC gives advice but doesn't get emotionally involved
  * Dialogue is friendly but neutral
  * NPC treats player as colleague/acquaintance
  * Example: ""Fine, I'll help. But let's keep this professional.""

- **Low Affection (30-49)**:
  * NPC still participates but shows reluctance or skepticism
  * NPC questions player's decisions openly
  * Dialogue is distant, skeptical, brief
  * NPC needs convincing or demands something in return
  * Example: ""Why should I trust you? Prove you're worth it.""

- **Very Low Affection (<30)**:
  * NPC STILL appears in scenes (story must progress)
  * Dialogue is cold, hostile, sarcastic, or dismissive
  * NPC openly disagrees or criticizes player's plans
  * NPC may create minor obstacles or complain frequently
  * NPC cooperates ONLY when forced by circumstances
  * Example: ""Ugh, fine. But I'm doing this for myself, not you.""

**CRITICAL**: Generate different REACTIONS based on affection:
- High-affection NPCs should have ENTHUSIASTIC, SUPPORTIVE dialogue
- Low-affection NPCs should have RELUCTANT, CRITICAL dialogue
- NPCs MUST still appear in scenes to maintain story progression
- Player choices should allow opportunities to INCREASE or DECREASE affection

# CRITICAL - Chapter-Level Convergence Structure
This chapter has a FIXED NARRATIVE ARC that all players experience:

1. **Choices affect RELATIONSHIPS and VALUES, NOT the main plot**
   - Player choices should change dialogue tone, character reactions, and skill/affection scores
   - BUT all players must reach the same key story events and chapter ending

2. **Branching → Convergence Pattern**
   - After each choice, create 2-3 different dialogue responses reflecting the choice
   - Then CONVERGE back to the main narrative within 1-2 lines
   - Example:
     * Line 10: ""What do you do?"" → [Choice A / Choice B]
     * Line 11a (Choice A): ""You bravely charge forward!""
     * Line 11b (Choice B): ""You carefully retreat!""
     * Line 12 (CONVERGENCE): ""Either way, you spot the enemy's weakness."" ← Both paths merge here

3. **Chapter Arc Structure (TOTAL 6 SCENES PER CHAPTER)**
   Scene 1-2: Common introduction (8-10 lines, no choices)
   Scene 3: FIRST CHOICE → branch → converge (10 lines with 1 choice)
   Scene 4: SECOND CHOICE → branch → converge (10 lines with 1 choice)
   Scene 5: THIRD CHOICE → branch → converge (10 lines with 1 choice)
   Scene 6: Final convergence (8-10 lines, no choices)

4. **MANDATORY CHOICE REQUIREMENTS**
   - Each chapter MUST have EXACTLY 3 CHOICES (one in Scene 3, 4, and 5)
   - Each choice MUST have 2 options
   - Each option MUST affect 1-2 derived skills with changes of 10-15 points
   - Balanced changes ensure meaningful progression: 3 chapters × 3 choices × 12 avg = 108 points max per skill

5. **Next ID Management for Branching**
   - When a choice appears, set different next_id values for each option in the ""choices"" array
   - After branching dialogue (1-2 lines), all branch endings MUST have ""next_id"" field pointing to convergence line
   - Example:
     * Line 5: Choice with [choice A next_id: 6, choice B next_id: 8]
     * Line 6-7: Branch A dialogue, Line 7 has ""next_id"": 10
     * Line 8-9: Branch B dialogue, Line 9 has ""next_id"": 10
     * Line 10: CONVERGENCE (both branches jump here)
   - CRITICAL: Branch ending lines MUST have explicit ""next_id"" field to prevent sequential progression

# Task
Generate Scene {sceneNumber} of {totalScenes} for Chapter {chapterId}.

**CRITICAL for Scene {sceneNumber}:**
{(sceneNumber == 3 || sceneNumber == 4 || sceneNumber == 5 ? "- This scene MUST include 1 CHOICE with 2 OPTIONS" : "- This scene should NOT have choices (setup or conclusion)")}
{(sceneNumber == 3 || sceneNumber == 4 || sceneNumber == 5 ? "- Each option MUST have skill_impact with change values of 10-15" : "")}
{(sceneNumber == 3 || sceneNumber == 4 || sceneNumber == 5 ? "- Each option SHOULD have affection_impact (+10 or -10) for relevant NPCs" : "")}
- Scene content and NPC behavior MUST reflect current affection scores (see Character Relationship Dynamics above)

Output ONLY a JSON array of 10 dialogue lines.

[
  {{
    ""speaker"": ""character name or narrator"",
    ""text"": ""dialogue text"",
    ""character1_name"": ""character name (if visible)"",
    ""character1_expression"": ""neutral/happy/sad/angry/surprised/embarrassed/thinking"",
    ""character1_pose"": ""normal/handsonhips/armscrossed/pointing/waving/thinking/surprised"",
    ""character1_position"": ""Left/Center/Right"",
    ""character2_name"": ""character name (if 2nd character)"",
    ""character2_expression"": ""expression"",
    ""character2_pose"": ""pose"",
    ""character2_position"": ""Left/Center/Right"",
    ""bg_name"": ""background name"",
    ""bgm_name"": ""bgm name"",
    ""sfx_name"": ""sfx name (optional)"",
    ""choices"": [
      {{
        ""text"": ""Choice A text (decisive action)"",
        ""next_id"": (line_id + 1),
        ""skill_impact"": [
          {{
            ""skill_name"": ""[USE EXACT SKILL NAME FROM CORE VALUES LIST]"",
            ""change"": 12
          }}
        ],
        ""affection_impact"": [
          {{
            ""character_name"": ""CharacterName"",
            ""change"": 10
          }}
        ]
      }},
      {{
        ""text"": ""Choice B text (alternative approach)"",
        ""next_id"": (line_id + 3),
        ""skill_impact"": [
          {{
            ""skill_name"": ""[USE DIFFERENT SKILL FROM CORE VALUES]"",
            ""change"": 12
          }}
        ],
        ""affection_impact"": [
          {{
            ""character_name"": ""CharacterName"",
            ""change"": 10
          }}
        ]
      }}
    ],
    ""cg_id"": ""Ch{chapterId}_CG1 (only for climactic moments)"",
    ""cg_title"": ""CG title"",
    ""cg_scene_description"": ""full scene description"",
    ""cg_lighting"": ""warm sunset glow"",
    ""cg_mood"": ""romantic"",
    ""cg_camera_angle"": ""close-up"",
    ""cg_characters"": [""CharacterA"", ""CharacterB""]
  }}
]

Important:
- Generate EXACTLY 10 dialogue lines for this scene
- Output ONLY the JSON array, no text before or after
- CRITICAL: Ensure JSON is valid and properly closed with ]
- Omit optional fields if not needed
- CRITICAL: Branch ending lines MUST have ""next_id"" field to jump to convergence point
- Example branching structure:
  * Line 5: {{""text"": ""..."", ""choices"": [{{""next_id"": 6}}, {{""next_id"": 8}}]}}
  * Line 6: {{""text"": ""Branch A response 1""}}
  * Line 7: {{""text"": ""Branch A response 2"", ""next_id"": 10}}  ← MUST have next_id
  * Line 8: {{""text"": ""Branch B response 1""}}
  * Line 9: {{""text"": ""Branch B response 2"", ""next_id"": 10}}  ← MUST have next_id
  * Line 10: {{""text"": ""Convergence line""}}  ← Both branches jump here

CRITICAL - Choice Impact Rules:
- NEVER use ""value_impact"" field - it is deprecated!
- ONLY use ""skill_impact"" to affect derived skills
- Core values are calculated automatically as the sum of their derived skills
- Available derived skills for this project:{coreValuesInfo}
- Example skill_impact format (BALANCED VALUES FOR MEANINGFUL PROGRESSION):
  ""skill_impact"": [
    {{
      ""skill_name"": ""[choose from the derived skills listed above]"",
      ""change"": 10 to 15
    }}
  ]
- Each choice should affect 1-2 relevant derived skills with BALANCED impact (10-15 points)
- Use skill names EXACTLY as listed in the Core Values section above
- With 3 chapters × 3 choices × 12 average points = 108 total points possible per skill
- This ensures diverse endings without extreme polarization!

EXAMPLE - Affection-Based Story Variation:

**Scenario: Player needs NPC's help for a dangerous mission**

IF NPC Affection = 80 (High):
  NPC: """"""Of course I'll help you! We've been through so much together. I wouldn't let you face this alone.""""""
  [NPC eagerly participates, shows concern, offers resources without hesitation]

IF NPC Affection = 55 (Medium):
  NPC: """"""I'll help, but this is risky. Let's make a plan first and minimize the danger.""""""
  [NPC cooperates but maintains cautious tone, gives practical advice]

IF NPC Affection = 35 (Low):
  NPC: """"""Fine, I'll help. But don't expect me to go out of my way for you. What's in it for me?""""""
  [NPC participates reluctantly, complains, demands compensation, but DOES participate]

IF NPC Affection = 20 (Very Low):
  NPC: """"""Ugh, seriously? Fine, but only because I need this mission to succeed too. Don't talk to me unless necessary.""""""
  [NPC STILL participates but is hostile, sarcastic, openly criticizes player, minimal cooperation]

**CRITICAL**:
- Apply this logic throughout the chapter
- NPCs MUST appear in scenes regardless of affection (story progression requirement)
- Low affection = HOSTILE PARTICIPATION, not absence
- Affection only changes TONE and ATTITUDE, not whether NPC appears";

            return prompt;
        }

        /// <summary>
        /// 챕터 생성 프롬프트 빌드 (전체 챕터 - 현재 사용 안 함)
        /// </summary>
        private string BuildChapterPrompt(int chapterId, GameStateSnapshot state)
        {
            // 캐릭터 목록 (말투 예시 포함)
            string characterList = "";

            // Player 캐릭터
            characterList += $"\n  - Player: {projectData.playerCharacter.characterName}";
            if (!string.IsNullOrEmpty(projectData.playerCharacter.sampleDialogue))
            {
                characterList += $"\n    Speech Style: \"{projectData.playerCharacter.sampleDialogue}\"";
            }

            // NPCs
            foreach (var npc in projectData.npcs)
            {
                characterList += $"\n  - NPC: {npc.characterName}";
                if (!string.IsNullOrEmpty(npc.sampleDialogue))
                {
                    characterList += $"\n    Speech Style: \"{npc.sampleDialogue}\"";
                }
            }

            // Core Values 및 Derived Skills 목록
            string coreValuesInfo = "";
            foreach (var value in projectData.coreValues)
            {
                string skills = value.derivedSkills != null && value.derivedSkills.Count > 0
                    ? string.Join(", ", value.derivedSkills)
                    : "none";
                coreValuesInfo += $"\n  - {value.name}: [{skills}]";
            }

            // 게임 상태 요약
            string stateInfo = state != null ? state.ToPromptString() : "Initial chapter";

            string prompt = $@"You are a visual novel story generator.

# Game Information
- Title: {projectData.gameTitle}
- Premise: {projectData.gamePremise}
- Genre: {projectData.genre}
- Tone: {projectData.tone}
- Total Chapters: {projectData.totalChapters}
- Characters: {characterList}
- Core Values and Derived Skills:{coreValuesInfo}

# Current State
{stateInfo}

# Task
Generate Chapter {chapterId} of {projectData.totalChapters}.
Output ONLY a JSON array of dialogue lines in this exact format:

[
  {{
    ""speaker"": ""character name or narrator"",
    ""text"": ""dialogue text"",
    ""character1_name"": ""character name (if visible)"",
    ""character1_expression"": ""neutral/happy/sad/angry/surprised/embarrassed/thinking"",
    ""character1_pose"": ""normal/handsonhips/armscrossed/pointing/waving/thinking/surprised"",
    ""character1_position"": ""Left/Center/Right"",
    ""character2_name"": ""character name (if 2nd character)"",
    ""character2_expression"": ""expression"",
    ""character2_pose"": ""pose"",
    ""character2_position"": ""Left/Center/Right"",
    ""bg_name"": ""background name"",
    ""bgm_name"": ""bgm name"",
    ""sfx_name"": ""sfx name (optional)"",
    ""choices"": [
      {{
        ""text"": ""choice text"",
        ""next_id"": {chapterId * 1000 + 50},
        ""skill_impact"": [
          {{
            ""skill_name"": ""Swordsmanship"",
            ""change"": 10
          }}
        ],
        ""affection_impact"": [
          {{
            ""character_name"": ""CharacterName"",
            ""change"": 10
          }}
        ]
      }}
    ],
    ""cg_id"": ""Ch{chapterId}_CG1 (only for important dramatic moments)"",
    ""cg_title"": ""CG title"",
    ""cg_scene_description"": ""full scene description"",
    ""cg_lighting"": ""warm sunset glow"",
    ""cg_mood"": ""romantic"",
    ""cg_camera_angle"": ""close-up"",
    ""cg_characters"": [""CharacterA"", ""CharacterB""]
  }}
]

Important:
- Generate 8-12 dialogue lines for this chapter (shorter = more reliable JSON)
- Include 5-7 choice points that affect Core Values (recommended, but no hard limit)
- You can include more or fewer choices as the story naturally requires
- Include 1 CG only if this is a climactic moment (optional)
- Use only available expressions and poses listed above
- Output ONLY the JSON array, no other text before or after
- CRITICAL: Ensure the JSON array is properly closed with ]
- Each object must end with a comma except the last one
- Keep JSON simple: omit optional fields if not needed
- If a field is not needed, omit it entirely (don't use empty strings or null)

Note on Affection System:
- affection_impact in choices should reflect how the choice affects NPC relationships
- Affection does NOT affect chapter branching (only Core Values do)
- Affection is used for dialogue tone and ending determination only
- You can reference current affection scores when writing NPC dialogue/reactions";

            return prompt;
        }

        /// <summary>
        /// 챕터 내 CG 처리
        /// </summary>
#if UNITY_EDITOR
        private IEnumerator ProcessCGsInChapter(List<DialogueRecord> records, int chapterNumber)
        {
            int cgIndex = 1;
            foreach (var record in records)
            {
                string cgId = record.Get("CG_ID");
                if (!string.IsNullOrEmpty(cgId))
                {
                    Debug.Log($"Generating CG: {cgId}");

                    // CG 메타데이터 생성
                    var cgMeta = new CGMetadata(
                        chapterNumber,
                        cgIndex,
                        record.Get("CG_Title"),
                        record.Get("cg_scene_description")
                    );
                    cgMeta.lighting = record.Get("cg_lighting");
                    cgMeta.mood = record.Get("cg_mood");
                    cgMeta.cameraAngle = record.Get("cg_camera_angle");

                    // 등장 캐릭터 파싱
                    string cgCharsJson = record.Get("cg_characters");
                    if (!string.IsNullOrEmpty(cgCharsJson))
                    {
                        // 간단한 배열 파싱 (예: ["A", "B"])
                        cgCharsJson = cgCharsJson.Trim('[', ']').Replace("\"", "");
                        string[] chars = cgCharsJson.Split(',');
                        foreach (var c in chars)
                        {
                            cgMeta.characterNames.Add(c.Trim());
                        }
                    }

                    // 이미지 생성
                    yield return GenerateCGImage(cgMeta);

                    // 프로젝트에 등록
                    if (!projectData.allCGs.Any(cg => cg.cgId == cgMeta.cgId))
                    {
                        projectData.allCGs.Add(cgMeta);
                        UnityEditor.EditorUtility.SetDirty(projectData);
                    }

                    cgIndex++;
                }
            }
        }

        /// <summary>
        /// CG 이미지 생성 (레퍼런스 기반)
        /// </summary>
        private IEnumerator GenerateCGImage(CGMetadata cgMeta)
        {
            // 레퍼런스 이미지 수집 (캐릭터 얼굴 프리뷰)
            List<Texture2D> referenceImages = new List<Texture2D>();

            foreach (var charName in cgMeta.characterNames)
            {
                CharacterData charData = null;

                // 플레이어 또는 NPC 찾기
                if (projectData.playerCharacter.characterName == charName)
                {
                    charData = projectData.playerCharacter;
                }
                else
                {
                    charData = projectData.npcs.Find(n => n.characterName == charName);
                }

                if (charData != null)
                {
                    // 헬퍼 메서드로 얼굴 프리뷰 로드
                    Sprite facePreview = charData.GetFacePreview();

                    if (facePreview != null)
                    {
                        // Sprite → Texture2D 변환
                        Texture2D faceTexture = NanoBananaClient.SpriteToTexture2D(facePreview);
                        referenceImages.Add(faceTexture);
                    }
                }
            }

            // CG 프롬프트 빌드
            string fullPrompt = BuildCGPrompt(cgMeta, referenceImages.Count);

            // NanoBanana API로 CG 생성 (레퍼런스 이미지 포함)
            bool completed = false;
            Texture2D cgTexture = null;

            yield return nanoBananaClient.GenerateImageWithReferences(
                fullPrompt,
                referenceImages,
                width: 1920,
                height: 1080,
                (texture) => {
                    cgTexture = texture;
                    completed = true;
                },
                (error) => {
                    Debug.LogError($"CG generation failed: {error}");
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (cgTexture != null)
            {
                // Resources/Image/CG/ 폴더에 저장
                string savePath = $"Assets/Resources/Image/CG/{cgMeta.cgId}.png";
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                byte[] bytes = cgTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(savePath, bytes);

                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"CG saved: {savePath}");
            }
        }

        /// <summary>
        /// CG 프롬프트 빌드 (수채화/페인터리 스타일)
        /// </summary>
        private string BuildCGPrompt(CGMetadata cgMeta, int refImageCount)
        {
            return $@"A high-quality full-screen illustration in detailed watercolor / painterly style, inspired by Japanese visual novel event CGs.
Use the {refImageCount} provided reference face(s) to preserve the character's identity (hair color, eye color, and facial features),
but redraw in a more artistic, semi-realistic illustration style.
The overall composition should depict: {cgMeta.sceneDescription}.
Lighting: {cgMeta.lighting}.
Mood: {cgMeta.mood}.
Art style: painterly brush texture, soft blending, subtle outlines, watercolor texture visible on surfaces.
Background: fully painted, integrated with the character; no transparency.
Color palette: natural light tones, slightly desaturated hues for realism.
Camera angle: {cgMeta.cameraAngle}.
Resolution: 1920×1080.";
        }
#endif

        // ===== 캐시 키 생성 =====

        /// <summary>
        /// Alternating Branching Strategy: 챕터별 교차 분기 캐시 키 생성
        /// - Chapter 1: 공통 프롤로그
        /// - 짝수 챕터 (2, 4, 6...): Core Value 기준 분기
        /// - 홀수 챕터 (3, 5, 7...): NPC Affection 기준 분기
        /// 최대 4개 Core Values, 3명 NPCs 지원
        /// </summary>
        private string GenerateCacheKey(int chapterId, GameStateSnapshot state)
        {
            string projectId = projectData.projectGuid;

            // Chapter 1: 항상 동일 (프롤로그)
            if (chapterId == 1)
            {
                return $"{projectId}_Ch1";
            }

            // 짝수 챕터: Core Value 기준 (2, 4, 6...)
            if (chapterId % 2 == 0)
            {
                string coreRoute = GetDominantCoreValue(state);           // 예: "Courage"
                string coreBucket = GetCoreValueBucket(state, coreRoute); // "LOW"/"MID"/"HIGH"
                string flags = GetMajorFlagsHash(state);                  // "helped_Alice_lied_Bob"
                return $"{projectId}_Ch{chapterId}_{coreRoute}_{coreBucket}_{flags}";
            }

            // 홀수 챕터: NPC Affection 기준 (3, 5, 7...)
            else
            {
                string coreRoute = GetDominantCoreValue(state);           // 이전 경로 유지
                string loveRoute = GetDominantNPC(state);                 // 예: "Alice"
                string affBucket = GetAffectionBucket(state, loveRoute);  // "Alice_HIGH"/"Bob_HIGH"/"BALANCED"
                string flags = GetMajorFlagsHash(state);
                return $"{projectId}_Ch{chapterId}_{coreRoute}_{loveRoute}_{affBucket}_{flags}";
            }
        }

        /// <summary>
        /// Core Value 중 가장 높은 값 반환 (최대 4개 지원)
        /// </summary>
        private string GetDominantCoreValue(GameStateSnapshot state)
        {
            if (state == null || state.coreValueScores == null || state.coreValueScores.Count == 0)
            {
                return "None";
            }

            return state.coreValueScores
                .OrderByDescending(kvp => kvp.Value)
                .First().Key;
        }

        /// <summary>
        /// NPC 중 가장 호감도 높은 캐릭터 반환 (최대 3명 지원)
        /// </summary>
        private string GetDominantNPC(GameStateSnapshot state)
        {
            if (state == null || state.characterAffections == null || state.characterAffections.Count == 0)
            {
                return "None";
            }

            return state.characterAffections
                .OrderByDescending(kvp => kvp.Value)
                .First().Key;
        }

        /// <summary>
        /// 특정 Core Value를 LOW/MID/HIGH로 양자화
        /// </summary>
        private string GetCoreValueBucket(GameStateSnapshot state, string coreValueName)
        {
            if (state == null || state.coreValueScores == null || !state.coreValueScores.ContainsKey(coreValueName))
            {
                return "LOW";
            }

            int score = state.coreValueScores[coreValueName];

            if (score < 30) return "LOW";
            if (score < 70) return "MID";
            return "HIGH";
        }

        /// <summary>
        /// 특정 NPC의 호감도를 양자화 (HIGH/MID/LOW)
        /// 또는 여러 NPC 간 균형 상태를 반환 (BALANCED)
        /// </summary>
        private string GetAffectionBucket(GameStateSnapshot state, string npcName)
        {
            if (state == null || state.characterAffections == null || state.characterAffections.Count == 0)
            {
                return "BALANCED";
            }

            // 최고 호감도 NPC와 2등 NPC 찾기
            var sortedNPCs = state.characterAffections
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            if (sortedNPCs.Count == 1)
            {
                // NPC가 1명만 있으면 그냥 점수 기준
                int score = sortedNPCs[0].Value;
                if (score < 30) return $"{sortedNPCs[0].Key}_LOW";
                if (score < 70) return $"{sortedNPCs[0].Key}_MID";
                return $"{sortedNPCs[0].Key}_HIGH";
            }

            // 1등과 2등 점수 차이
            int topScore = sortedNPCs[0].Value;
            int secondScore = sortedNPCs[1].Value;
            int diff = topScore - secondScore;

            // 차이가 20 미만이면 균형 상태
            if (diff < 20)
            {
                return "BALANCED";
            }

            // 1등 NPC의 호감도 등급
            string topNPC = sortedNPCs[0].Key;
            if (topScore < 30) return $"{topNPC}_LOW";
            if (topScore < 70) return $"{topNPC}_MID";
            return $"{topNPC}_HIGH";
        }

        /// <summary>
        /// 중요 플래그만 추출하여 해시 생성
        /// </summary>
        private string GetMajorFlagsHash(GameStateSnapshot state)
        {
            if (state == null || state.flags == null || state.flags.Count == 0)
            {
                return "none";
            }

            // 실제로 스토리에 영향을 주는 플래그만 필터링
            var majorFlags = state.flags
                .Where(kvp => kvp.Value && IsMajorFlag(kvp.Key)) // true인 플래그만
                .Select(kvp => kvp.Key)
                .OrderBy(f => f)
                .ToList();

            if (majorFlags.Count == 0)
            {
                return "none";
            }

            return string.Join("_", majorFlags);
        }

        /// <summary>
        /// 플래그가 Major Flag인지 판단
        /// (실제 스토리 분기에 영향을 주는 중요한 플래그만)
        /// </summary>
        private bool IsMajorFlag(string flag)
        {
            // 중요 플래그 접두어 (스토리에 실제 영향을 주는 것만)
            string[] majorPrefixes = { "helped_", "lied_", "saved_", "failed_", "betrayed_", "romance_" };
            return majorPrefixes.Any(prefix => flag.StartsWith(prefix));
        }

        // ===== 엔딩 씬 생성 =====

        [Header("Ending Database")]
        public EndingSceneDatabase endingDatabase;

        /// <summary>
        /// 엔딩 씬 로드 (미리 작성된 엔딩 데이터베이스에서 가져오기, 없으면 AI 생성)
        /// </summary>
        public IEnumerator GenerateEndingScene(string endingType, GameStateSnapshot state, System.Action<List<DialogueRecord>> onComplete)
        {
            Debug.Log($"[ChapterGenerationManager] Loading ending scene: {endingType}");

            // 엔딩 데이터베이스 확인
            if (endingDatabase == null)
            {
                Debug.LogWarning("[ChapterGenerationManager] Ending database not assigned! Generating ending scene with AI...");
                yield return GenerateEndingSceneWithAI(endingType, state, onComplete);
                yield break;
            }

            // EndingType enum 파싱
            EndingType parsedEndingType;
            string dominantCoreValue = null;

            if (endingType.Contains("True"))
            {
                parsedEndingType = EndingType.TrueEnding;
            }
            else if (endingType.Contains("Value") || endingType.Contains("Ending"))
            {
                parsedEndingType = EndingType.ValueEnding;
                // Dominant Core Value 추출 (예: "용기 Ending" → "용기")
                dominantCoreValue = GetDominantCoreValue(state);
            }
            else
            {
                parsedEndingType = EndingType.NormalEnding;
            }

            // 데이터베이스에서 엔딩 씬 가져오기
            EndingSceneData endingScene = endingDatabase.GetEndingScene(parsedEndingType, dominantCoreValue);

            if (endingScene == null)
            {
                Debug.LogError($"[ChapterGenerationManager] No ending scene found for type: {parsedEndingType}");
                onComplete?.Invoke(null);
                yield break;
            }

            // DialogueRecord로 변환
            List<DialogueRecord> endingRecords = endingScene.ToDialogueRecords();

            if (endingRecords != null && endingRecords.Count > 0)
            {
                Debug.Log($"[ChapterGenerationManager] Loaded ending scene: {endingScene.endingTitle} ({endingRecords.Count} lines)");
            }

            onComplete?.Invoke(endingRecords);
            yield break; // 즉시 완료 (AI 호출 없음)
        }

        /// <summary>
        /// AI로 엔딩 씬 생성 (EndingDatabase가 없을 때 fallback)
        /// </summary>
        private IEnumerator GenerateEndingSceneWithAI(string endingType, GameStateSnapshot state, System.Action<List<DialogueRecord>> onComplete)
        {
            Debug.Log($"[ChapterGenerationManager] Generating ending scene with AI: {endingType}");

            // 엔딩 씬 프롬프트 빌드
            string prompt = BuildEndingPrompt(endingType, state);

            bool completed = false;
            string jsonResponse = null;
            string errorMessage = null;

            yield return geminiClient.GenerateContent(
                prompt,
                (response) => {
                    jsonResponse = response;
                    completed = true;
                },
                (error) => {
                    errorMessage = error;
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError($"[ChapterGenerationManager] Ending scene generation failed: {errorMessage}");
                onComplete?.Invoke(null);
                yield break;
            }

            // JSON 파싱 및 DialogueRecord 변환
            List<DialogueRecord> endingRecords = ParseEndingSceneJSON(jsonResponse, 999); // 엔딩 씬 ID = 999

            if (endingRecords != null && endingRecords.Count > 0)
            {
                Debug.Log($"[ChapterGenerationManager] ✅ Generated ending scene with {endingRecords.Count} lines");
            }
            else
            {
                Debug.LogWarning("[ChapterGenerationManager] Ending scene generation produced no records");
            }

            onComplete?.Invoke(endingRecords);
        }

        /// <summary>
        /// 엔딩 씬 JSON 파싱
        /// </summary>
        private List<DialogueRecord> ParseEndingSceneJSON(string jsonResponse, int chapterId)
        {
            // JSON 추출 (```json ... ``` 제거)
            int jsonStart = jsonResponse.IndexOf('[');
            int jsonEnd = jsonResponse.LastIndexOf(']');

            if (jsonStart < 0 || jsonEnd < 0 || jsonEnd <= jsonStart)
            {
                Debug.LogError($"[ChapterGenerationManager] Invalid JSON format in ending scene response");
                return null;
            }

            string jsonArray = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

            // AIDataConverter로 변환
            return AIDataConverter.FromAIJson(jsonArray, chapterId);
        }

        /// <summary>
        /// 엔딩 씬 프롬프트 빌드
        /// </summary>
        private string BuildEndingPrompt(string endingType, GameStateSnapshot state)
        {
            // 캐릭터 목록 (말투 예시 포함)
            string characterList = "";

            // Player 캐릭터
            characterList += $"\n  - Player: {projectData.playerCharacter.characterName}";
            if (!string.IsNullOrEmpty(projectData.playerCharacter.sampleDialogue))
            {
                characterList += $"\n    Speech Style: \"{projectData.playerCharacter.sampleDialogue}\"";
            }

            // NPCs
            foreach (var npc in projectData.npcs)
            {
                characterList += $"\n  - NPC: {npc.characterName}";
                if (!string.IsNullOrEmpty(npc.sampleDialogue))
                {
                    characterList += $"\n    Speech Style: \"{npc.sampleDialogue}\"";
                }
            }

            // Core Values 점수
            string coreValuesInfo = "";
            if (state.coreValueScores != null && state.coreValueScores.Count > 0)
            {
                var sortedValues = state.coreValueScores.OrderByDescending(kv => kv.Value);
                foreach (var kv in sortedValues)
                {
                    coreValuesInfo += $"\n  - {kv.Key}: {kv.Value} points";
                }
            }

            // 주요 선택지들
            string choicesInfo = "";
            if (state.previousChoices != null && state.previousChoices.Count > 0)
            {
                int displayCount = Mathf.Min(5, state.previousChoices.Count);
                choicesInfo = "\n\nKey Choices Made:";
                for (int i = state.previousChoices.Count - displayCount; i < state.previousChoices.Count; i++)
                {
                    choicesInfo += $"\n  - {state.previousChoices[i]}";
                }
            }

            // 챕터 요약들
            string chapterSummaries = "";
            if (state.chapterSummaries != null && state.chapterSummaries.Count > 0)
            {
                chapterSummaries = "\n\nChapter Summaries:";
                foreach (var kv in state.chapterSummaries)
                {
                    chapterSummaries += $"\n  Chapter {kv.Key}: {kv.Value}";
                }
            }

            string prompt = $@"You are a visual novel ending scene generator.

# Game Information
- Title: {projectData.gameTitle}
- Premise: {projectData.gamePremise}
- Genre: {projectData.genre}
- Tone: {projectData.tone}
- Characters: {characterList}

# Player's Journey
Core Values Achieved:{coreValuesInfo}{choicesInfo}{chapterSummaries}

# Ending Type: {endingType}

Generate a FINAL ENDING SCENE (5-10 dialogue lines) that:
1. Reflects the player's journey and choices
2. Shows the consequences of their Core Value focus
3. Provides emotional closure
4. Ends with a final statement or image that captures the essence of this ending

The scene should feel conclusive and satisfying, wrapping up the main story threads.

Output Format (JSON Array):
[
  {{
    ""speaker"": ""character name or narrator"",
    ""text"": ""dialogue text"",
    ""character1_name"": ""character name (if visible)"",
    ""character1_expression"": ""neutral/happy/sad/angry/surprised/embarrassed/thinking"",
    ""character1_pose"": ""normal/handsonhips/armscrossed/pointing/waving/thinking/surprised"",
    ""character1_position"": ""Left/Center/Right"",
    ""character2_name"": """",
    ""character2_expression"": """",
    ""character2_pose"": """",
    ""character2_position"": """",
    ""bg_name"": ""background name"",
    ""bgm_name"": ""bgm name"",
    ""sfx_name"": """",
    ""next_id"": next_line_id_or_0_for_end
  }}
]

IMPORTANT: The last line should have ""next_id"": 0 to indicate the story ends.
";

            return prompt;
        }

        // ===== 캐시 저장/로드 =====

        /// <summary>
        /// 개별 챕터를 파일로 저장
        /// </summary>
        private void SaveChapterToFile(string cacheKey, ChapterData chapterData)
        {
            try
            {
                // 캐시 폴더 생성
                if (!Directory.Exists(CACHE_FOLDER))
                {
                    Directory.CreateDirectory(CACHE_FOLDER);
                }

                string filePath = Path.Combine(CACHE_FOLDER, $"{cacheKey}.json");
                string json = JsonUtility.ToJson(chapterData, true);
                File.WriteAllText(filePath, json);

                Debug.Log($"[ChapterGenerationManager] Chapter saved: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChapterGenerationManager] Failed to save chapter: {e.Message}");
            }
        }

        /// <summary>
        /// 캐시 폴더에서 모든 챕터 파일 로드
        /// </summary>
        private void LoadCacheFromDisk()
        {
            if (!Directory.Exists(CACHE_FOLDER))
            {
                Debug.Log("[ChapterGenerationManager] No chapter cache folder found");
                return;
            }

            try
            {
                string[] files = Directory.GetFiles(CACHE_FOLDER, "*.json");

                chapterCache.Clear();
                foreach (string filePath in files)
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        ChapterData chapter = JsonUtility.FromJson<ChapterData>(json);

                        // DialogueRecord의 JSON 역직렬화 후처리 (리스트→Dictionary 복원)
                        if (chapter.records != null)
                        {
                            foreach (var record in chapter.records)
                            {
                                record.OnAfterDeserialize();
                            }
                        }

                        // 파일명에서 캐시 키 추출
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        chapterCache[fileName] = chapter;

                        Debug.Log($"[ChapterGenerationManager] Loaded chapter: {fileName}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ChapterGenerationManager] Failed to load chapter file {filePath}: {e.Message}");
                    }
                }

                Debug.Log($"[ChapterGenerationManager] Total {chapterCache.Count} chapters loaded from cache");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChapterGenerationManager] Failed to load cache folder: {e.Message}");
            }
        }
    }
}
