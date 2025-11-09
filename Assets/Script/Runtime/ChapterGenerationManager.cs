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
        private Dictionary<int, ChapterData> chapterCache = new Dictionary<int, ChapterData>();

        [Header("API")]
        public GeminiClient geminiClient;
        public NanoBananaClient nanoBananaClient;

        [Header("Cache Settings")]
        public bool enableCaching = true;
        private string CACHE_PATH => Path.Combine(Application.persistentDataPath, $"{projectData.projectGuid}_chapters.json");

        void Start()
        {
            LoadCacheFromDisk();
        }

        /// <summary>
        /// 챕터 생성 또는 캐시에서 로드
        /// </summary>
        public IEnumerator GenerateOrLoadChapter(int chapterId, GameStateSnapshot state, System.Action<List<DialogueRecord>> onComplete)
        {
            // 1. 캐시 확인
            if (enableCaching && chapterCache.ContainsKey(chapterId))
            {
                Debug.Log($"Loading cached chapter {chapterId}");
                onComplete?.Invoke(chapterCache[chapterId].records);
                yield break;
            }

            // 2. 새로 생성
            Debug.Log($"Generating new chapter {chapterId}");

            string prompt = BuildChapterPrompt(chapterId, state);

            bool completed = false;
            List<IyagiAI.Runtime.DialogueRecord> records = null;

            yield return geminiClient.GenerateContent(
                prompt,
                (jsonResponse) => {
                    // JSON 추출 및 변환
                    records = AIDataConverter.FromAIJson(jsonResponse, chapterId);
                    completed = true;
                },
                (error) => {
                    Debug.LogError($"Chapter generation failed: {error}");
                    completed = true;
                }
            );

            yield return new WaitUntil(() => completed);

            if (records != null && records.Count > 0)
            {
                // 3. 캐시에 저장
                var chapterData = new ChapterData(chapterId, records);
                chapterData.generationPrompt = prompt;
                chapterData.stateSnapshot = state;
                chapterCache[chapterId] = chapterData;

                // 4. 디스크에 저장
                if (enableCaching)
                {
                    SaveCacheToDisk();
                }

                // 5. CG 처리 (AI가 CG를 요청했다면)
#if UNITY_EDITOR
                yield return ProcessCGsInChapter(records, chapterId);
#endif

                onComplete?.Invoke(records);
            }
            else
            {
                Debug.LogError($"Failed to generate chapter {chapterId}");
                onComplete?.Invoke(new List<DialogueRecord>());
            }
        }

        /// <summary>
        /// 챕터 생성 프롬프트 빌드
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
            string coreValuesList = string.Join(", ", projectData.coreValues);

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
- Generate 20-30 dialogue lines for this chapter
- Include 2-3 choice points that affect Core Values
- Include 1 CG for the most dramatic/important moment
- Use only available expressions and poses listed above
- Output ONLY the JSON array, no other text";

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

        // ===== 캐시 저장/로드 =====

        private void SaveCacheToDisk()
        {
            try
            {
                var wrapper = new ChapterCacheWrapper
                {
                    chapters = chapterCache.Values.ToList()
                };

                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(CACHE_PATH, json);
                Debug.Log($"Chapter cache saved: {CACHE_PATH}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save cache: {e.Message}");
            }
        }

        private void LoadCacheFromDisk()
        {
            if (!File.Exists(CACHE_PATH))
            {
                Debug.Log("No chapter cache found");
                return;
            }

            try
            {
                string json = File.ReadAllText(CACHE_PATH);
                var wrapper = JsonUtility.FromJson<ChapterCacheWrapper>(json);

                chapterCache.Clear();
                foreach (var chapter in wrapper.chapters)
                {
                    chapterCache[chapter.chapterId] = chapter;
                }

                Debug.Log($"Loaded {chapterCache.Count} cached chapters");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load cache: {e.Message}");
            }
        }

        [System.Serializable]
        private class ChapterCacheWrapper
        {
            public List<ChapterData> chapters;
        }
    }
}
