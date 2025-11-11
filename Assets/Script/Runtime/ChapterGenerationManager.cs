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
        public bool enableCaching = true;
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

            // 3. 씬 단위로 분할 생성 (3개 씬)
            Debug.Log($"[ChapterGenerationManager] Generating chapter {chapterId} in 3 scenes");

            int totalScenes = 3;
            List<DialogueRecord> allRecords = new List<DialogueRecord>();
            string previousScenesContext = "";

            for (int sceneNum = 1; sceneNum <= totalScenes; sceneNum++)
            {
                Debug.Log($"[ChapterGenerationManager] Generating scene {sceneNum}/{totalScenes}");

                string scenePrompt = BuildScenePrompt(chapterId, sceneNum, totalScenes, state, previousScenesContext);

                bool sceneCompleted = false;
                List<DialogueRecord> sceneRecords = null;

                yield return geminiClient.GenerateContent(
                    scenePrompt,
                    (jsonResponse) => {
                        Debug.Log($"[ChapterGenerationManager] Scene {sceneNum} response length: {jsonResponse?.Length ?? 0}");

                        // JSON 추출 및 변환
                        sceneRecords = AIDataConverter.FromAIJson(jsonResponse, chapterId);

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
                    allRecords.AddRange(sceneRecords);
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

            // Core Values 목록
            string coreValuesList = string.Join(", ", projectData.coreValues.Select(v => v.name));

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
- Core Values: {coreValuesList}

# Current State
{stateInfo}
{contextSection}
# Task
Generate Scene {sceneNumber} of {totalScenes} for Chapter {chapterId}.
Output ONLY a JSON array of 3-5 dialogue lines (IMPORTANT: keep it short!).

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
        ""value_impact"": [
          {{
            ""value_name"": ""Courage"",
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
- Generate ONLY 3-5 dialogue lines for this scene (keep JSON short!)
- Include 1-2 choice points if appropriate for this scene
- Include CG only if this is a dramatic climax
- Output ONLY the JSON array, no text before or after
- CRITICAL: Ensure JSON is valid and properly closed with ]
- Omit optional fields if not needed";

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

            // Core Values 목록
            string coreValuesList = string.Join(", ", projectData.coreValues.Select(v => v.name));

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
- Core Values: {coreValuesList}

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
        ""value_impact"": [
          {{
            ""value_name"": ""Courage"",
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
        /// 캐시 키 생성: {ProjectGuid}_Ch{ChapterNum}_{ValueHash}
        /// </summary>
        private string GenerateCacheKey(int chapterId, GameStateSnapshot state)
        {
            string valueHash = state != null ? state.GetCacheHash() : "00000000";
            return $"{projectData.projectGuid}_Ch{chapterId}_{valueHash}";
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
