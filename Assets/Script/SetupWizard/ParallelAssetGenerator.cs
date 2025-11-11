using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IyagiAI.Runtime;
using IyagiAI.AISystem;

namespace IyagiAI.SetupWizard
{
    /// <summary>
    /// 병렬 에셋 생성 관리자 (Fan-Out Barrier 패턴)
    /// 프로젝트 생성 시 모든 캐릭터 스탠딩, 챕터1 JSON, 배경/CG/BGM/SFX를 병렬로 생성
    /// </summary>
    public class ParallelAssetGenerator : MonoBehaviour
    {
        [Header("References")]
        public VNProjectData projectData;
        public NanoBananaClient nanoBananaClient;
        public GeminiClient geminiClient;
        public ElevenLabsClient elevenLabsClient;
        public ChapterGenerationManager chapterManager;

        [Header("Progress Tracking")]
        private int totalTasks = 0;
        private int completedTasks = 0;

        /// <summary>
        /// Cycle 1 & 2 병렬 실행 (50% 진행률)
        /// Cycle 1: 모든 캐릭터 스탠딩 생성
        /// Cycle 2: 챕터1 JSON 생성
        /// </summary>
        public IEnumerator RunCycle1And2Parallel(
            System.Action<float> onProgress,
            System.Action<string> onChapter1JSONReady,
            System.Action onComplete)
        {
            totalTasks = 2;
            completedTasks = 0;

            bool cycle1Done = false;
            bool cycle2Done = false;
            string chapter1JSON = null;

            // Cycle 1: 모든 캐릭터 스탠딩 생성
            StartCoroutine(GenerateAllStandingSprites(() => {
                cycle1Done = true;
                completedTasks++;
                float progress = (float)completedTasks / totalTasks * 0.5f;
                onProgress?.Invoke(progress);
                Debug.Log($"[ParallelAssetGenerator] Cycle 1 완료: 스탠딩 생성 완료 ({progress * 100}%)");
            }));

            // Cycle 2: 챕터1 JSON 생성
            StartCoroutine(GenerateChapter1JSON((json) => {
                chapter1JSON = json;
                cycle2Done = true;
                completedTasks++;
                float progress = (float)completedTasks / totalTasks * 0.5f;
                onProgress?.Invoke(progress);
                Debug.Log($"[ParallelAssetGenerator] Cycle 2 완료: 챕터1 JSON 생성 완료 ({progress * 100}%)");
            }));

            // Barrier: Cycle 1 & 2 완료 대기
            yield return new WaitUntil(() => cycle1Done && cycle2Done);

            Debug.Log("[ParallelAssetGenerator] Barrier 도달: Cycle 1 & 2 완료 (50%)");
            onChapter1JSONReady?.Invoke(chapter1JSON);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Cycle 3: 챕터1 JSON 파싱 → 에셋 병렬 생성 (50% → 100%)
        /// </summary>
        public IEnumerator RunCycle3(
            string chapter1JSON,
            System.Action<float> onProgress,
            System.Action onComplete)
        {
            // JSON 파싱하여 필요한 에셋 목록 추출
            var assetList = ParseChapter1Assets(chapter1JSON);

            int totalAssets = assetList.backgrounds.Count + assetList.cgs.Count +
                              assetList.bgmNames.Count + assetList.sfxNames.Count;
            totalTasks = totalAssets;
            completedTasks = 0;

            Debug.Log($"[ParallelAssetGenerator] Cycle 3 시작: {totalAssets}개 에셋 생성");

            // 배경 이미지 병렬 생성
            foreach (var bgName in assetList.backgrounds)
            {
                StartCoroutine(GenerateBackground(bgName, () => {
                    completedTasks++;
                    float progress = 0.5f + ((float)completedTasks / totalAssets * 0.5f);
                    onProgress?.Invoke(progress);
                }));
            }

            // CG 일러스트 병렬 생성
            foreach (var cgDesc in assetList.cgs)
            {
                StartCoroutine(GenerateCG(cgDesc, () => {
                    completedTasks++;
                    float progress = 0.5f + ((float)completedTasks / totalAssets * 0.5f);
                    onProgress?.Invoke(progress);
                }));
            }

            // BGM 병렬 생성
            foreach (var bgmName in assetList.bgmNames)
            {
                StartCoroutine(GenerateBGM(bgmName, () => {
                    completedTasks++;
                    float progress = 0.5f + ((float)completedTasks / totalAssets * 0.5f);
                    onProgress?.Invoke(progress);
                }));
            }

            // SFX 병렬 생성
            foreach (var sfxName in assetList.sfxNames)
            {
                StartCoroutine(GenerateSFX(sfxName, () => {
                    completedTasks++;
                    float progress = 0.5f + ((float)completedTasks / totalAssets * 0.5f);
                    onProgress?.Invoke(progress);
                }));
            }

            // Final Barrier: 모든 에셋 생성 완료 대기
            yield return new WaitUntil(() => completedTasks == totalAssets);

            Debug.Log("[ParallelAssetGenerator] Final Barrier 도달: Cycle 3 완료 (100%)");
            onComplete?.Invoke();
        }

        /// <summary>
        /// Cycle 1: 모든 캐릭터 스탠딩 생성 (테스트 모드: TestResources에서 복사)
        /// </summary>
        private IEnumerator GenerateAllStandingSprites(System.Action onComplete)
        {
            List<CharacterData> allCharacters = new List<CharacterData>();

            // Player + NPCs
            if (projectData.playerCharacter != null)
            {
                allCharacters.Add(projectData.playerCharacter);
            }
            if (projectData.npcs != null)
            {
                allCharacters.AddRange(projectData.npcs);
            }

            Debug.Log($"[Cycle 1] [TEST MODE] 총 {allCharacters.Count}명 캐릭터 스탠딩 복사 시작 (TestResources 사용)");

            int completedCharacters = 0;

            // TestResources에서 스탠딩 이미지 복사
            for (int i = 0; i < allCharacters.Count; i++)
            {
                var character = allCharacters[i];

                // TestResources/Standing에서 이미지 복사
                yield return CopyStandingFromTestResources(character, () => {
                    completedCharacters++;
                    Debug.Log($"[Cycle 1] [TEST MODE] {character.characterName} 스탠딩 복사 완료 ({completedCharacters}/{allCharacters.Count})");
                });
            }

            Debug.Log($"[Cycle 1] [TEST MODE] 모든 캐릭터 스탠딩 복사 완료 (API 호출 없음)");
            onComplete?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>
        /// TestResources에서 스탠딩 이미지 복사
        /// </summary>
        private IEnumerator CopyStandingFromTestResources(CharacterData character, System.Action onComplete)
        {
            string sourceDir = "Assets/Resources/TestResources/Standing";
            string targetDir = $"Assets/Resources/Generated/Characters/{character.characterName}";

            // 타겟 폴더 생성
            if (!System.IO.Directory.Exists(targetDir))
            {
                System.IO.Directory.CreateDirectory(targetDir);
            }

            // TestResources/Standing에서 사용 가능한 첫 번째 캐릭터 폴더 찾기
            string[] testCharacterDirs = System.IO.Directory.GetDirectories(sourceDir);
            if (testCharacterDirs.Length == 0)
            {
                Debug.LogWarning($"[Cycle 1] TestResources에 스탠딩 폴더 없음. 스킵: {character.characterName}");
                onComplete?.Invoke();
                yield break;
            }

            string testSourceDir = testCharacterDirs[0]; // 첫 번째 테스트 캐릭터 사용

            // 모든 이미지 파일 복사 (.png만)
            string[] sourceFiles = System.IO.Directory.GetFiles(testSourceDir, "*.png");
            foreach (var sourceFile in sourceFiles)
            {
                string fileName = System.IO.Path.GetFileName(sourceFile);
                string targetFile = System.IO.Path.Combine(targetDir, fileName);

                System.IO.File.Copy(sourceFile, targetFile, true);
                Debug.Log($"[Cycle 1] [TEST MODE] 복사 완료: {fileName}");
            }

            UnityEditor.AssetDatabase.Refresh();

            onComplete?.Invoke();
            yield break;
        }
#endif

        /// <summary>
        /// Cycle 2: 챕터1 JSON 생성
        /// </summary>
        private IEnumerator GenerateChapter1JSON(System.Action<string> onComplete)
        {
            Debug.Log("[Cycle 2] 챕터1 JSON 생성 시작");

            // 초기 GameState 생성
            GameStateSnapshot initialState = new GameStateSnapshot
            {
                currentChapter = 1,
                currentLineId = 1000
            };

            // Core Values 초기화
            if (projectData.coreValues != null)
            {
                foreach (var value in projectData.coreValues)
                {
                    if (value != null && !string.IsNullOrEmpty(value.name))
                    {
                        initialState.coreValueScores[value.name] = 0;
                    }
                }
            }

            // NPC 호감도 초기화
            if (projectData.npcs != null)
            {
                foreach (var npc in projectData.npcs)
                {
                    if (npc != null && !string.IsNullOrEmpty(npc.characterName))
                    {
                        initialState.characterAffections[npc.characterName] = npc.initialAffection;
                    }
                }
            }

            // ChapterGenerationManager를 사용하여 챕터1 생성
            bool chapterComplete = false;
            List<DialogueRecord> records = null;

            yield return chapterManager.GenerateOrLoadChapter(
                1,
                initialState,
                (generatedRecords) => {
                    records = generatedRecords;
                    chapterComplete = true;
                }
            );

            yield return new WaitUntil(() => chapterComplete);

            // DialogueRecord 리스트를 JSON 문자열로 변환 (간단히 직렬화)
            string json = JsonUtility.ToJson(new RecordListWrapper { records = records }, true);

            Debug.Log($"[Cycle 2] 챕터1 JSON 생성 완료 ({records.Count} lines)");
            onComplete?.Invoke(json);
        }

        /// <summary>
        /// 챕터1 JSON에서 필요한 에셋 목록 파싱
        /// </summary>
        private AssetList ParseChapter1Assets(string chapter1JSON)
        {
            AssetList assetList = new AssetList
            {
                backgrounds = new HashSet<string>(),
                cgs = new List<string>(),
                bgmNames = new HashSet<string>(),
                sfxNames = new HashSet<string>()
            };

            try
            {
                var wrapper = JsonUtility.FromJson<RecordListWrapper>(chapter1JSON);
                if (wrapper.records == null)
                {
                    Debug.LogWarning("[ParseAssets] No records found in JSON");
                    return assetList;
                }

                foreach (var record in wrapper.records)
                {
                    // 배경 이미지
                    string bg = record.GetString("Background");
                    if (!string.IsNullOrEmpty(bg))
                    {
                        assetList.backgrounds.Add(bg);
                    }

                    // CG
                    if (record.HasCG())
                    {
                        string cgId = record.GetString("CG_ID");
                        if (!string.IsNullOrEmpty(cgId))
                        {
                            assetList.cgs.Add(cgId);
                        }
                    }

                    // BGM
                    string bgm = record.GetString("bgm_name");
                    if (!string.IsNullOrEmpty(bgm))
                    {
                        assetList.bgmNames.Add(bgm);
                    }

                    // SFX
                    string sfx = record.GetString("sfx_name");
                    if (!string.IsNullOrEmpty(sfx))
                    {
                        assetList.sfxNames.Add(sfx);
                    }
                }

                Debug.Log($"[ParseAssets] 배경: {assetList.backgrounds.Count}, CG: {assetList.cgs.Count}, BGM: {assetList.bgmNames.Count}, SFX: {assetList.sfxNames.Count}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ParseAssets] JSON 파싱 실패: {e.Message}");
            }

            return assetList;
        }

        /// <summary>
        /// 배경 이미지 생성 (테스트 모드: TestResources에서 복사)
        /// </summary>
        private IEnumerator GenerateBackground(string bgName, System.Action onComplete)
        {
            Debug.Log($"[Cycle 3] [TEST MODE] 배경 복사 시작: {bgName} (API 호출 없음)");

#if UNITY_EDITOR
            string sourceDir = "Assets/Resources/TestResources/Background";
            string targetDir = "Assets/Resources/Image/Background";

            if (!System.IO.Directory.Exists(targetDir))
            {
                System.IO.Directory.CreateDirectory(targetDir);
            }

            // TestResources/Background에서 첫 번째 이미지 복사
            string[] sourceFiles = System.IO.Directory.GetFiles(sourceDir, "*.png");
            if (sourceFiles.Length > 0)
            {
                string sourceFile = sourceFiles[0];
                string targetFile = System.IO.Path.Combine(targetDir, $"{bgName}.png");

                System.IO.File.Copy(sourceFile, targetFile, true);
                Debug.Log($"[Cycle 3] [TEST MODE] 배경 복사 완료: {bgName}");

                UnityEditor.AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning($"[Cycle 3] TestResources에 배경 이미지 없음");
            }
#endif

            onComplete?.Invoke();
            yield break;
        }

        /// <summary>
        /// CG 일러스트 생성 (테스트 모드: 스킵)
        /// </summary>
        private IEnumerator GenerateCG(string cgDescription, System.Action onComplete)
        {
            Debug.Log($"[Cycle 3] [TEST MODE] CG 생성 스킵: {cgDescription} (API 호출 없음)");

            // 테스트 모드에서는 CG 생성 스킵
            onComplete?.Invoke();
            yield break;
        }

        /// <summary>
        /// BGM 생성 (테스트 모드: TestResources에서 복사)
        /// </summary>
        private IEnumerator GenerateBGM(string bgmName, System.Action onComplete)
        {
            Debug.Log($"[Cycle 3] [TEST MODE] BGM 복사 시작: {bgmName} (API 호출 없음)");

#if UNITY_EDITOR
            string sourceDir = "Assets/Resources/TestResources/BGM";
            string targetDir = "Assets/Resources/Sound/BGM";

            if (!System.IO.Directory.Exists(targetDir))
            {
                System.IO.Directory.CreateDirectory(targetDir);
            }

            // TestResources/BGM에서 첫 번째 파일 복사
            string[] sourceFiles = System.IO.Directory.GetFiles(sourceDir, "*.mp3");
            if (sourceFiles.Length > 0)
            {
                string sourceFile = sourceFiles[0];
                string targetFile = System.IO.Path.Combine(targetDir, $"{bgmName}.mp3");

                System.IO.File.Copy(sourceFile, targetFile, true);
                Debug.Log($"[Cycle 3] [TEST MODE] BGM 복사 완료: {bgmName}");

                UnityEditor.AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning($"[Cycle 3] TestResources에 BGM 파일 없음");
            }
#endif

            onComplete?.Invoke();
            yield break;
        }

        /// <summary>
        /// SFX 생성 (테스트 모드: 스킵)
        /// </summary>
        private IEnumerator GenerateSFX(string sfxName, System.Action onComplete)
        {
            Debug.Log($"[Cycle 3] [TEST MODE] SFX 생성 스킵: {sfxName} (API 호출 없음)");

            // 테스트 모드에서는 SFX 생성 스킵
            onComplete?.Invoke();
            yield break;
        }

        // ===== 에셋 저장 헬퍼 메서드 =====

#if UNITY_EDITOR
        private void SaveBackgroundAsset(string bgName, Texture2D texture)
        {
            string dir = "Assets/Resources/Image/Background";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/{bgName}.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"[Cycle 3] 배경 저장 완료: {path}");
        }

        private void SaveCGAsset(string cgId, Texture2D texture)
        {
            string dir = "Assets/Resources/Image/CG";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/{cgId}.png";
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"[Cycle 3] CG 저장 완료: {path}");
        }

        private void SaveBGMAsset(string bgmName, AudioClip audioClip)
        {
            // AudioClip을 WAV 파일로 저장 (Unity는 MP3 직접 저장 불가)
            string dir = "Assets/Resources/Sound/BGM";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/{bgmName}.wav";
            SaveAudioClipAsWAV(audioClip, path);
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"[Cycle 3] BGM 저장 완료: {path}");
        }

        private void SaveSFXAsset(string sfxName, AudioClip audioClip)
        {
            string dir = "Assets/Resources/Sound/SFX";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            string path = $"{dir}/{sfxName}.wav";
            SaveAudioClipAsWAV(audioClip, path);
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"[Cycle 3] SFX 저장 완료: {path}");
        }

        /// <summary>
        /// AudioClip을 WAV 파일로 저장 (Unity SavWav 헬퍼 사용)
        /// </summary>
        private void SaveAudioClipAsWAV(AudioClip clip, string filepath)
        {
            // WAV 파일 저장 로직 (간단한 구현)
            // 실제 구현은 SavWav 라이브러리나 커스텀 인코더 필요
            // 여기서는 placeholder로 둠
            Debug.LogWarning($"[SaveAudioClip] WAV 저장 기능은 추가 구현 필요: {filepath}");
            // TODO: Implement WAV encoding
        }
#endif

        // ===== 데이터 클래스 =====

        [System.Serializable]
        private class RecordListWrapper
        {
            public List<DialogueRecord> records;
        }

        private class AssetList
        {
            public HashSet<string> backgrounds;
            public List<string> cgs;
            public HashSet<string> bgmNames;
            public HashSet<string> sfxNames;
        }
    }
}
