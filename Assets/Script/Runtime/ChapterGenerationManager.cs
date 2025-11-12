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
        public IEnumerator GenerateChapterSummary(int chapterId, List<DialogueRecord> records, System.Action<string> onComplete)
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

            // AI에게 요약 요청
            string summaryPrompt = $@"You are a story summarizer for a visual novel game.

Summarize the following chapter in 2-3 sentences that capture:
1. The main events that happened
2. Key character interactions or developments
3. Any important plot points or revelations

Keep it concise and focused on story progression (not player choices).

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
            // 캐릭터 목록
            string characterList = $"Player: {projectData.playerCharacter.characterName}";
            foreach (var npc in projectData.npcs)
            {
                characterList += $", {npc.characterName}";
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
   - Each option MUST affect 1-2 derived skills with changes of 20-30 points
   - Higher changes ensure diverse endings after 3 chapters (3 choices × 25 points = 75 points difference)

5. **Next ID Management for Branching**
   - When a choice appears, set different next_id values for each option
   - After branching dialogue (1-2 lines), all branches must point to the SAME convergence line
   - Example:
     * Line 5: Choice with [next_id: 6 for A, next_id: 8 for B]
     * Line 6-7: Branch A dialogue
     * Line 8-9: Branch B dialogue
     * Line 10: CONVERGENCE (both line 7 and 9 should have next_id: 10)

# Task
Generate Scene {sceneNumber} of {totalScenes} for Chapter {chapterId}.

**CRITICAL for Scene {sceneNumber}:**
{(sceneNumber == 3 || sceneNumber == 4 || sceneNumber == 5 ? "- This scene MUST include 1 CHOICE with 2 OPTIONS" : "- This scene should NOT have choices (setup or conclusion)")}
{(sceneNumber == 3 || sceneNumber == 4 || sceneNumber == 5 ? "- Each option MUST have skill_impact with change values of 20-30" : "")}

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
            ""change"": 25
          }}
        ],
        ""affection_impact"": [
          {{
            ""character_name"": ""CharacterName"",
            ""change"": 15
          }}
        ]
      }},
      {{
        ""text"": ""Choice B text (alternative approach)"",
        ""next_id"": (line_id + 3),
        ""skill_impact"": [
          {{
            ""skill_name"": ""[USE DIFFERENT SKILL FROM CORE VALUES]"",
            ""change"": 25
          }}
        ],
        ""affection_impact"": [
          {{
            ""character_name"": ""CharacterName"",
            ""change"": 15
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

CRITICAL - Choice Impact Rules:
- NEVER use ""value_impact"" field - it is deprecated!
- ONLY use ""skill_impact"" to affect derived skills
- Core values are calculated automatically as the sum of their derived skills
- Available derived skills for this project:{coreValuesInfo}
- Example skill_impact format (HIGHER VALUES FOR MEANINGFUL PROGRESSION):
  ""skill_impact"": [
    {{
      ""skill_name"": ""[choose from the derived skills listed above]"",
      ""change"": 20 to 30
    }}
  ]
- Each choice should affect 1-2 relevant derived skills with HIGH impact (20-30 points)
- Use skill names EXACTLY as listed in the Core Values section above
- With 3 chapters × 3 choices × 25 average points = 225 total points possible per skill
- This ensures diverse endings based on player choices!";

            return prompt;
        }

        /// <summary>
        /// 챕터 생성 프롬프트 빌드 (전체 챕터 - 현재 사용 안 함)
        /// </summary>
        private string BuildChapterPrompt(int chapterId, GameStateSnapshot state)
        {
            // 캐릭터 목록
            string characterList = $"Player: {projectData.playerCharacter.characterName}";
            foreach (var npc in projectData.npcs)
            {
                characterList += $", {npc.characterName}";
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
        /// 캐시 키 생성: {ProjectGuid}_Ch{ChapterNum}
        /// Chapter-Level Convergence: Core Value 무시, 챕터 번호만 사용
        /// </summary>
        private string GenerateCacheKey(int chapterId, GameStateSnapshot state)
        {
            // ✅ 같은 챕터면 같은 캐시 사용 (Core Value 상태 무시)
            return $"{projectData.projectGuid}_Ch{chapterId}";
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
        /// 가장 높은 Core Value 가져오기
        /// </summary>
        private string GetDominantCoreValue(GameStateSnapshot state)
        {
            if (state.coreValueScores == null || state.coreValueScores.Count == 0)
            {
                return null;
            }

            var maxValue = state.coreValueScores.OrderByDescending(kv => kv.Value).First();
            return maxValue.Key;
        }

        /// <summary>
        /// 엔딩 씬 프롬프트 빌드
        /// </summary>
        private string BuildEndingPrompt(string endingType, GameStateSnapshot state)
        {
            // 캐릭터 목록
            string characterList = $"Player: {projectData.playerCharacter.characterName}";
            foreach (var npc in projectData.npcs)
            {
                characterList += $", {npc.characterName}";
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
