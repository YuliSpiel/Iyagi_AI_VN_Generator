using UnityEngine;
using UnityEditor;
using IyagiAI.Runtime;
using System.Collections.Generic;

namespace IyagiAI.Editor
{
    /// <summary>
    /// 정적 선택지 테스트 프로젝트
    /// API 호출 없이 대사, 선택지, 스킬 시스템만 테스트
    /// 모든 챕터 데이터를 미리 생성하여 캐시에 저장
    /// </summary>
    public class CreateStaticChoiceTest : MonoBehaviour
    {
        [MenuItem("Iyagi/Create Static Choice Test")]
        public static void CreateTest()
        {
            // 1. 프로젝트 데이터 생성 (이미지 없음)
            VNProjectData projectData = ScriptableObject.CreateInstance<VNProjectData>();
            projectData.gameTitle = "선택지 스킬 테스트";
            projectData.gamePremise = "대사, 선택지, 스킬 시스템 테스트 (이미지/API 없음)";
            projectData.genre = Genre.Slice_of_Life;
            projectData.tone = Tone.Lighthearted;
            projectData.playtime = PlaytimeEstimate.Mins30;
            projectData.totalChapters = 1;
            projectData.projectGuid = System.Guid.NewGuid().ToString();
            projectData.createdTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();

            // Core Values (스킬 시스템 테스트용)
            projectData.coreValues.Add(new CoreValue
            {
                name = "우정",
                derivedSkills = new List<string> { "공감", "협력", "신뢰" }
            });
            projectData.coreValues.Add(new CoreValue
            {
                name = "지식",
                derivedSkills = new List<string> { "관찰", "분석", "기억력" }
            });

            // 2. 캐릭터 없음 (이미지 생성 방지)
            // playerCharacter와 npcs를 비워둠

            // 3. 폴더 생성
            string projectFolder = "Assets/Resources/Projects";
            if (!System.IO.Directory.Exists(projectFolder))
            {
                System.IO.Directory.CreateDirectory(projectFolder);
            }

            // 4. ScriptableObject 저장
            AssetDatabase.CreateAsset(projectData, $"{projectFolder}/StaticChoiceTest.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 5. SaveDataManager 데이터 생성
            string saveFolder = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            if (!System.IO.Directory.Exists(saveFolder))
            {
                System.IO.Directory.CreateDirectory(saveFolder);
            }

            string projectSlotsFile = System.IO.Path.Combine(saveFolder, "projects.json");

            var projectSlot = new ProjectSlot
            {
                projectGuid = projectData.projectGuid,
                projectName = projectData.gameTitle,
                totalChapters = projectData.totalChapters,
                createdDate = System.DateTime.Now,
                lastPlayedDate = System.DateTime.Now,
                saveFiles = new List<SaveFile>(),
                unlockedCGs = new List<string>()
            };

            var saveFile = new SaveFile
            {
                saveFileId = System.Guid.NewGuid().ToString(),
                projectGuid = projectData.projectGuid,
                slotNumber = 1,
                currentChapter = 1,
                totalChapters = projectData.totalChapters,
                createdDate = System.DateTime.Now,
                lastPlayedDate = System.DateTime.Now,
                totalPlaytimeSeconds = 0,
                gameState = new GameState()
            };

            projectSlot.saveFiles.Add(saveFile);

            // 기존 프로젝트 슬롯 로드
            List<ProjectSlot> allSlots = new List<ProjectSlot>();
            if (System.IO.File.Exists(projectSlotsFile))
            {
                try
                {
                    string existingJson = System.IO.File.ReadAllText(projectSlotsFile);
                    var wrapper = JsonUtility.FromJson<ProjectSlotListWrapper>(existingJson);
                    if (wrapper != null && wrapper.projects != null)
                    {
                        allSlots = wrapper.projects;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to load existing project slots: {e.Message}");
                }
            }

            allSlots.RemoveAll(p => p.projectGuid == projectData.projectGuid);
            allSlots.Add(projectSlot);

            string projectsJson = SerializeProjectSlots(allSlots);
            System.IO.File.WriteAllText(projectSlotsFile, projectsJson);

            Debug.Log($"Created project slot: {projectSlot.projectName}");

            // 6. 챕터 데이터를 미리 생성하여 캐시에 직접 저장
            CreateStaticChapterData(projectData, saveFile);

            Debug.Log("✅ Static Choice Test project created successfully!");
            Debug.Log($"Project GUID: {projectData.projectGuid}");
            Debug.Log($"SaveFile ID: {saveFile.saveFileId}");
            Debug.Log("타이틀 씬에서 'Load Game' → '선택지 스킬 테스트' 선택하여 테스트하세요.");
            Debug.Log("⚠️ 이미지 없음, API 호출 없음, 대사+선택지+스킬만 테스트");

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 정적 챕터 데이터 생성 (API 호출 없이 미리 만듦)
        /// </summary>
        private static void CreateStaticChapterData(VNProjectData projectData, SaveFile saveFile)
        {
            List<DialogueRecord> records = new List<DialogueRecord>();

            // 대사 1: 시작
            var record1 = new DialogueRecord();
            record1.Fields["ID"] = "2000";
            record1.Fields["Scene"] = "1";
            record1.Fields["Index"] = "0";
            record1.Fields["ParsedLine_ENG"] = "Welcome to the choice and skill test!";
            record1.Fields["NameTag"] = "Narrator";
            record1.Fields["Auto"] = "TRUE";
            record1.FinalizeIndex();
            records.Add(record1);

            // 대사 2: 첫 번째 선택지
            var record2 = new DialogueRecord();
            record2.Fields["ID"] = "2001";
            record2.Fields["Scene"] = "1";
            record2.Fields["Index"] = "1";
            record2.Fields["ParsedLine_ENG"] = "What should we focus on?";
            record2.Fields["NameTag"] = "System";

            // 선택지 1: 우정 중심
            record2.Fields["Choice1_ENG"] = "Build stronger friendships";
            record2.Fields["Next1"] = "2002";
            record2.Fields["Choice1_ValueImpact_우정"] = "15";
            record2.Fields["Choice1_SkillImpact_공감"] = "10";
            record2.Fields["Choice1_SkillImpact_협력"] = "8";

            // 선택지 2: 지식 중심
            record2.Fields["Choice2_ENG"] = "Learn and study more";
            record2.Fields["Next2"] = "2003";
            record2.Fields["Choice2_ValueImpact_지식"] = "15";
            record2.Fields["Choice2_SkillImpact_관찰"] = "10";
            record2.Fields["Choice2_SkillImpact_분석"] = "8";

            record2.FinalizeIndex();
            records.Add(record2);

            // 대사 3: 선택지 1 결과
            var record3 = new DialogueRecord();
            record3.Fields["ID"] = "2002";
            record3.Fields["Scene"] = "1";
            record3.Fields["Index"] = "2";
            record3.Fields["ParsedLine_ENG"] = "Great choice! Friendship is important. Your empathy and cooperation skills have increased.";
            record3.Fields["NameTag"] = "System";
            record3.Fields["Auto"] = "TRUE";
            record3.Fields["Next"] = "2004";
            record3.FinalizeIndex();
            records.Add(record3);

            // 대사 4: 선택지 2 결과
            var record4 = new DialogueRecord();
            record4.Fields["ID"] = "2003";
            record4.Fields["Scene"] = "1";
            record4.Fields["Index"] = "3";
            record4.Fields["ParsedLine_ENG"] = "Excellent! Knowledge is power. Your observation and analysis skills have increased.";
            record4.Fields["NameTag"] = "System";
            record4.Fields["Auto"] = "TRUE";
            record4.Fields["Next"] = "2004";
            record4.FinalizeIndex();
            records.Add(record4);

            // 대사 5: 두 번째 선택지
            var record5 = new DialogueRecord();
            record5.Fields["ID"] = "2004";
            record5.Fields["Scene"] = "1";
            record5.Fields["Index"] = "4";
            record5.Fields["ParsedLine_ENG"] = "Now let's test another choice. What's more important?";
            record5.Fields["NameTag"] = "System";

            // 선택지 1: 신뢰 강화
            record5.Fields["Choice1_ENG"] = "Build trust with others";
            record5.Fields["Next1"] = "2005";
            record5.Fields["Choice1_ValueImpact_우정"] = "10";
            record5.Fields["Choice1_SkillImpact_신뢰"] = "12";

            // 선택지 2: 기억력 향상
            record5.Fields["Choice2_ENG"] = "Improve memory skills";
            record5.Fields["Next2"] = "2006";
            record5.Fields["Choice2_ValueImpact_지식"] = "10";
            record5.Fields["Choice2_SkillImpact_기억력"] = "12";

            record5.FinalizeIndex();
            records.Add(record5);

            // 대사 6: 선택지 1 결과
            var record6 = new DialogueRecord();
            record6.Fields["ID"] = "2005";
            record6.Fields["Scene"] = "1";
            record6.Fields["Index"] = "5";
            record6.Fields["ParsedLine_ENG"] = "Trust is the foundation of all relationships. Your trust skill has greatly improved!";
            record6.Fields["NameTag"] = "System";
            record6.Fields["Auto"] = "TRUE";
            record6.Fields["Next"] = "2007";
            record6.FinalizeIndex();
            records.Add(record6);

            // 대사 7: 선택지 2 결과
            var record7 = new DialogueRecord();
            record7.Fields["ID"] = "2006";
            record7.Fields["Scene"] = "1";
            record7.Fields["Index"] = "6";
            record7.Fields["ParsedLine_ENG"] = "A strong memory is key to learning. Your memory skill has greatly improved!";
            record7.Fields["NameTag"] = "System";
            record7.Fields["Auto"] = "TRUE";
            record7.Fields["Next"] = "2007";
            record7.FinalizeIndex();
            records.Add(record7);

            // 대사 8: 마무리
            var record8 = new DialogueRecord();
            record8.Fields["ID"] = "2007";
            record8.Fields["Scene"] = "1";
            record8.Fields["Index"] = "7";
            record8.Fields["ParsedLine_ENG"] = "Test complete! Check your skill scores in the top-left corner.";
            record8.Fields["NameTag"] = "System";
            record8.Fields["Auto"] = "TRUE";
            record8.FinalizeIndex();
            records.Add(record8);

            // ChapterData 생성
            var chapterData = new ChapterData(1, records);
            chapterData.generationPrompt = "Static test chapter - no generation";
            chapterData.stateSnapshot = new GameStateSnapshot
            {
                currentChapter = 1,
                currentLineId = 2000,
                coreValueScores = new Dictionary<string, int>
                {
                    { "우정", 0 },
                    { "지식", 0 }
                },
                skillScores = new Dictionary<string, int>
                {
                    { "공감", 0 },
                    { "협력", 0 },
                    { "신뢰", 0 },
                    { "관찰", 0 },
                    { "분석", 0 },
                    { "기억력", 0 }
                }
            };

            // 캐시에 직접 저장
            string cachePath = System.IO.Path.Combine(
                Application.persistentDataPath,
                $"{projectData.projectGuid}_chapters.json"
            );

            var wrapper = new ChapterCacheWrapper { chapters = new List<ChapterData> { chapterData } };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(cachePath, json);

            Debug.Log($"Static chapter data saved to cache: {cachePath}");
            Debug.Log($"Total dialogue lines: {records.Count}");
            Debug.Log("✅ ChapterGenerationManager will load from cache (NO API calls)");
        }

        private static string SerializeProjectSlots(List<ProjectSlot> slots)
        {
            var json = new System.Text.StringBuilder();
            json.AppendLine("{");
            json.AppendLine("  \"projects\": [");

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                json.AppendLine("    {");
                json.AppendLine($"      \"projectGuid\": \"{slot.projectGuid}\",");
                json.AppendLine($"      \"projectName\": \"{slot.projectName}\",");
                json.AppendLine($"      \"totalChapters\": {slot.totalChapters},");
                json.AppendLine($"      \"createdDate\": \"{slot.createdDate:O}\",");
                json.AppendLine($"      \"lastPlayedDate\": \"{slot.lastPlayedDate:O}\",");
                json.AppendLine("      \"saveFiles\": [");

                for (int j = 0; j < slot.saveFiles.Count; j++)
                {
                    var save = slot.saveFiles[j];
                    json.AppendLine("        {");
                    json.AppendLine($"          \"saveFileId\": \"{save.saveFileId}\",");
                    json.AppendLine($"          \"projectGuid\": \"{save.projectGuid}\",");
                    json.AppendLine($"          \"slotNumber\": {save.slotNumber},");
                    json.AppendLine($"          \"currentChapter\": {save.currentChapter},");
                    json.AppendLine($"          \"totalChapters\": {save.totalChapters},");
                    json.AppendLine($"          \"createdDate\": \"{save.createdDate:O}\",");
                    json.AppendLine($"          \"lastPlayedDate\": \"{save.lastPlayedDate:O}\",");
                    json.AppendLine($"          \"totalPlaytimeSeconds\": {save.totalPlaytimeSeconds},");
                    json.AppendLine("          \"gameState\": {");
                    json.AppendLine("            \"coreValueScores\": {},");
                    json.AppendLine("            \"skillScores\": {},");
                    json.AppendLine("            \"npcAffections\": {},");
                    json.AppendLine("            \"previousChoices\": []");
                    json.AppendLine("          }");
                    json.Append("        }");
                    if (j < slot.saveFiles.Count - 1) json.Append(",");
                    json.AppendLine();
                }

                json.AppendLine("      ],");
                json.AppendLine("      \"unlockedCGs\": []");
                json.Append("    }");
                if (i < slots.Count - 1) json.Append(",");
                json.AppendLine();
            }

            json.AppendLine("  ]");
            json.AppendLine("}");

            return json.ToString();
        }

        [System.Serializable]
        private class ChapterCacheWrapper
        {
            public List<ChapterData> chapters;
        }
    }
}
