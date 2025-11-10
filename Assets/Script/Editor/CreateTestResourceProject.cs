using UnityEngine;
using UnityEditor;
using IyagiAI.Runtime;
using System.Collections.Generic;

namespace IyagiAI.Editor
{
    /// <summary>
    /// TestResources 폴더의 샘플 파일을 사용하는 테스트 프로젝트 생성
    /// - 선택지 3회
    /// - 배경 변경 2회 (riverside → council)
    /// - SFX 3회 (Knock, WakeUp, Gavel)
    /// - 발화 중인 캐릭터 강조
    /// </summary>
    public class CreateTestResourceProject : MonoBehaviour
    {
        [MenuItem("Iyagi/Create Test Resource Project")]
        public static void CreateProject()
        {
            // 1. 프로젝트 데이터 생성
            VNProjectData projectData = ScriptableObject.CreateInstance<VNProjectData>();
            projectData.gameTitle = "TestResources 샘플 프로젝트";
            projectData.gamePremise = "모든 기능 통합 테스트 (선택지 3회, 배경 2회, SFX 3회, 캐릭터 강조)";
            projectData.genre = Genre.Slice_of_Life;
            projectData.tone = Tone.Lighthearted;
            projectData.playtime = PlaytimeEstimate.Mins30;
            projectData.totalChapters = 1;
            projectData.projectGuid = System.Guid.NewGuid().ToString();
            projectData.createdTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();

            // Core Values
            projectData.coreValues.Add(new CoreValue
            {
                name = "정의",
                derivedSkills = new List<string> { "판단력", "공감", "용기" }
            });

            // 2. 플레이어 캐릭터 (Elian)
            CharacterData player = ScriptableObject.CreateInstance<CharacterData>();
            player.characterName = "Elian";
            player.age = 25;
            player.gender = Gender.Male;
            player.pov = POV.FirstPerson;
            player.appearanceDescription = "은색 머리의 침착한 청년";
            player.personality = "냉정하고 분석적인 성격";
            player.archetype = Archetype.Strategist;
            player.resourcePath = "TestResources/Standing/Elian";

            // 3. NPC 캐릭터 (Sarah)
            CharacterData npc1 = ScriptableObject.CreateInstance<CharacterData>();
            npc1.characterName = "Sarah";
            npc1.role = "조력자";
            npc1.age = 23;
            npc1.gender = Gender.Female;
            npc1.appearanceDescription = "갈색 머리의 활발한 여성";
            npc1.personality = "밝고 적극적인 성격";
            npc1.archetype = Archetype.Innocent;
            npc1.isRomanceable = false;
            npc1.initialAffection = 50;
            npc1.resourcePath = "TestResources/Standing/Sarah";

            projectData.playerCharacter = player;
            projectData.npcs.Add(npc1);

            // 4. 폴더 생성
            string projectFolder = "Assets/Resources/Projects";
            if (!System.IO.Directory.Exists(projectFolder))
            {
                System.IO.Directory.CreateDirectory(projectFolder);
            }

            // 5. ScriptableObject 저장
            AssetDatabase.CreateAsset(projectData, $"{projectFolder}/TestResourceProject.asset");
            AssetDatabase.AddObjectToAsset(player, projectData);
            AssetDatabase.AddObjectToAsset(npc1, projectData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 6. SaveDataManager 데이터 생성
            CreateSaveData(projectData);

            // 7. 더미 챕터 데이터 생성
            CreateTestChapterData(projectData);

            Debug.Log("✅ Test Resource Project created successfully!");
            Debug.Log($"Project GUID: {projectData.projectGuid}");
            Debug.Log("타이틀 씬에서 'Load Game' → 'TestResources 샘플 프로젝트' 선택하여 테스트하세요.");

            AssetDatabase.Refresh();
        }

        private static void CreateSaveData(VNProjectData projectData)
        {
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

            // 기존 슬롯 로드
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
            Debug.Log($"SaveFile ID: {saveFile.saveFileId}");
        }

        private static void CreateTestChapterData(VNProjectData projectData)
        {
            List<DialogueRecord> records = new List<DialogueRecord>();
            int lineId = 1000;

            // ===== Scene 1: riverside (배경 1) =====

            // Line 0: 나레이션 + BGM 시작
            var r0 = new DialogueRecord();
            r0.Fields["ID"] = (lineId++).ToString();
            r0.Fields["Scene"] = "1";
            r0.Fields["ParsedLine_ENG"] = "A quiet riverside at dawn...";
            r0.Fields["NameTag"] = "Narrator";
            r0.Fields["BG"] = "TestResources/Background/riverside";
            r0.Fields["BGM"] = "TestResources/BGM/D960_1";
            r0.Fields["Auto"] = "TRUE";
            r0.FinalizeIndex();
            records.Add(r0);

            // Line 1: Sarah 등장 + SFX_Knock
            var r1 = new DialogueRecord();
            r1.Fields["ID"] = (lineId++).ToString();
            r1.Fields["Scene"] = "1";
            r1.Fields["ParsedLine_ENG"] = "Elian! Wake up! We have a council meeting today!";
            r1.Fields["NameTag"] = "Sarah";
            r1.Fields["Char1Name"] = "Sarah";
            r1.Fields["Char1Look"] = "happy_waving";
            r1.Fields["Char1Pos"] = "Center";
            r1.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r1.Fields["SFX"] = "TestResources/SFX/SFX_Knock";
            r1.Fields["Auto"] = "TRUE";
            r1.FinalizeIndex();
            records.Add(r1);

            // Line 2: Elian 응답 + SFX_WakeUp
            var r2 = new DialogueRecord();
            r2.Fields["ID"] = (lineId++).ToString();
            r2.Fields["Scene"] = "1";
            r2.Fields["ParsedLine_ENG"] = "Ugh... Already? Give me five more minutes...";
            r2.Fields["NameTag"] = "Elian";
            r2.Fields["Char1Name"] = "Elian";
            r2.Fields["Char1Look"] = "thinking_thinking";
            r2.Fields["Char1Pos"] = "Center";
            r2.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r2.Fields["Char2Name"] = "Sarah";
            r2.Fields["Char2Look"] = "embarrassed_handsonhips";
            r2.Fields["Char2Pos"] = "Right";
            r2.Fields["Char2Size"] = "Medium";
            r2.Fields["SFX"] = "TestResources/SFX/SFX_WakeUp";
            r2.Fields["Auto"] = "TRUE";
            r2.FinalizeIndex();
            records.Add(r2);

            // Line 3: Sarah 재촉
            var r3 = new DialogueRecord();
            r3.Fields["ID"] = (lineId++).ToString();
            r3.Fields["Scene"] = "1";
            r3.Fields["ParsedLine_ENG"] = "No! We need to prepare. This is an important decision!";
            r3.Fields["NameTag"] = "Sarah";
            r3.Fields["Char1Name"] = "Elian";
            r3.Fields["Char1Look"] = "thinking_thinking";
            r3.Fields["Char1Pos"] = "Left";
            r3.Fields["Char1Size"] = "Medium";
            r3.Fields["Char2Name"] = "Sarah";
            r3.Fields["Char2Look"] = "surprised_pointing";
            r3.Fields["Char2Pos"] = "Center";
            r3.Fields["Char2Size"] = "Large"; // ✅ 발화 중 강조
            r3.Fields["Auto"] = "TRUE";
            r3.FinalizeIndex();
            records.Add(r3);

            // ===== Choice 1: 회의 참여 태도 =====
            var r4 = new DialogueRecord();
            r4.Fields["ID"] = (lineId++).ToString();
            r4.Fields["Scene"] = "1";
            r4.Fields["ParsedLine_ENG"] = "How should I approach this meeting?";
            r4.Fields["NameTag"] = "Elian";
            r4.Fields["Char1Name"] = "Elian";
            r4.Fields["Char1Look"] = "thinking_normal";
            r4.Fields["Char1Pos"] = "Center";
            r4.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r4.Fields["Choice1_ENG"] = "Listen carefully and analyze";
            r4.Fields["Next1"] = (lineId).ToString();
            r4.Fields["Choice1_ValueImpact_정의"] = "10";
            r4.Fields["Choice1_SkillImpact_판단력"] = "5";
            r4.Fields["Choice2_ENG"] = "Stay silent and observe";
            r4.Fields["Next2"] = (lineId + 1).ToString();
            r4.Fields["Choice2_ValueImpact_정의"] = "-5";
            r4.Fields["Choice2_SkillImpact_공감"] = "3";
            r4.FinalizeIndex();
            records.Add(r4);

            // Choice 1-1 결과
            var r5 = new DialogueRecord();
            r5.Fields["ID"] = (lineId++).ToString();
            r5.Fields["Scene"] = "1";
            r5.Fields["ParsedLine_ENG"] = "Good thinking! Let's go over the facts together.";
            r5.Fields["NameTag"] = "Sarah";
            r5.Fields["Char1Name"] = "Sarah";
            r5.Fields["Char1Look"] = "happy_normal";
            r5.Fields["Char1Pos"] = "Center";
            r5.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r5.Fields["Auto"] = "TRUE";
            r5.FinalizeIndex();
            records.Add(r5);

            // Choice 1-2 결과
            var r6 = new DialogueRecord();
            r6.Fields["ID"] = (lineId++).ToString();
            r6.Fields["Scene"] = "1";
            r6.Fields["ParsedLine_ENG"] = "Hmm... Well, at least you're being cautious.";
            r6.Fields["NameTag"] = "Sarah";
            r6.Fields["Char1Name"] = "Sarah";
            r6.Fields["Char1Look"] = "neutral_normal";
            r6.Fields["Char1Pos"] = "Center";
            r6.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r6.Fields["Auto"] = "TRUE";
            r6.FinalizeIndex();
            records.Add(r6);

            // ===== Scene 2: council (배경 변경) =====

            // Line 7: 배경 전환 + BGM 전환
            var r7 = new DialogueRecord();
            r7.Fields["ID"] = (lineId++).ToString();
            r7.Fields["Scene"] = "1";
            r7.Fields["ParsedLine_ENG"] = "At the council chamber...";
            r7.Fields["NameTag"] = "Narrator";
            r7.Fields["BG"] = "TestResources/Background/council"; // ✅ 배경 변경
            r7.Fields["BGM"] = "TestResources/BGM/D960_2"; // BGM 변경
            r7.Fields["Auto"] = "TRUE";
            r7.FinalizeIndex();
            records.Add(r7);

            // Line 8: Sarah 발언 + SFX_Gavel
            var r8 = new DialogueRecord();
            r8.Fields["ID"] = (lineId++).ToString();
            r8.Fields["Scene"] = "1";
            r8.Fields["ParsedLine_ENG"] = "The council will now hear the case of the disputed land.";
            r8.Fields["NameTag"] = "Sarah";
            r8.Fields["Char1Name"] = "Sarah";
            r8.Fields["Char1Look"] = "neutral_normal";
            r8.Fields["Char1Pos"] = "Center";
            r8.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r8.Fields["SFX"] = "TestResources/SFX/SFX_Gavel"; // ✅ SFX 3번째
            r8.Fields["Auto"] = "TRUE";
            r8.FinalizeIndex();
            records.Add(r8);

            // ===== Choice 2: 첫 번째 증거 평가 =====
            var r9 = new DialogueRecord();
            r9.Fields["ID"] = (lineId++).ToString();
            r9.Fields["Scene"] = "1";
            r9.Fields["ParsedLine_ENG"] = "The witness claims they saw someone at the riverside that night...";
            r9.Fields["NameTag"] = "Elian";
            r9.Fields["Char1Name"] = "Elian";
            r9.Fields["Char1Look"] = "thinking_thinking";
            r9.Fields["Char1Pos"] = "Center";
            r9.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r9.Fields["Choice1_ENG"] = "This witness is credible";
            r9.Fields["Next1"] = (lineId).ToString();
            r9.Fields["Choice1_ValueImpact_정의"] = "15";
            r9.Fields["Choice1_SkillImpact_판단력"] = "10";
            r9.Fields["Choice2_ENG"] = "We need more evidence";
            r9.Fields["Next2"] = (lineId + 1).ToString();
            r9.Fields["Choice2_ValueImpact_정의"] = "8";
            r9.Fields["Choice2_SkillImpact_판단력"] = "5";
            r9.FinalizeIndex();
            records.Add(r9);

            // Choice 2-1 결과
            var r10 = new DialogueRecord();
            r10.Fields["ID"] = (lineId++).ToString();
            r10.Fields["Scene"] = "1";
            r10.Fields["ParsedLine_ENG"] = "Your judgment is sound. The witness's testimony is consistent.";
            r10.Fields["NameTag"] = "Sarah";
            r10.Fields["Char1Name"] = "Sarah";
            r10.Fields["Char1Look"] = "happy_pointing";
            r10.Fields["Char1Pos"] = "Center";
            r10.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r10.Fields["Auto"] = "TRUE";
            r10.FinalizeIndex();
            records.Add(r10);

            // Choice 2-2 결과
            var r11 = new DialogueRecord();
            r11.Fields["ID"] = (lineId++).ToString();
            r11.Fields["Scene"] = "1";
            r11.Fields["ParsedLine_ENG"] = "Caution is wise, but we must act on the information we have.";
            r11.Fields["NameTag"] = "Sarah";
            r11.Fields["Char1Name"] = "Sarah";
            r11.Fields["Char1Look"] = "neutral_normal";
            r11.Fields["Char1Pos"] = "Center";
            r11.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r11.Fields["Auto"] = "TRUE";
            r11.FinalizeIndex();
            records.Add(r11);

            // ===== Choice 3: 최종 판결 =====
            var r12 = new DialogueRecord();
            r12.Fields["ID"] = (lineId++).ToString();
            r12.Fields["Scene"] = "1";
            r12.Fields["ParsedLine_ENG"] = "Based on all evidence, what is your verdict?";
            r12.Fields["NameTag"] = "Elian";
            r12.Fields["Char1Name"] = "Elian";
            r12.Fields["Char1Look"] = "neutral_normal";
            r12.Fields["Char1Pos"] = "Center";
            r12.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r12.Fields["Choice1_ENG"] = "Guilty - evidence is clear";
            r12.Fields["Next1"] = (lineId).ToString();
            r12.Fields["Choice1_ValueImpact_정의"] = "20";
            r12.Fields["Choice1_SkillImpact_용기"] = "15";
            r12.Fields["Choice2_ENG"] = "Not guilty - doubt remains";
            r12.Fields["Next2"] = (lineId + 1).ToString();
            r12.Fields["Choice2_ValueImpact_정의"] = "-10";
            r12.Fields["Choice2_SkillImpact_공감"] = "10";
            r12.FinalizeIndex();
            records.Add(r12);

            // Choice 3-1 결과
            var r13 = new DialogueRecord();
            r13.Fields["ID"] = (lineId++).ToString();
            r13.Fields["Scene"] = "1";
            r13.Fields["ParsedLine_ENG"] = "A bold decision. Justice has been served.";
            r13.Fields["NameTag"] = "Sarah";
            r13.Fields["Char1Name"] = "Sarah";
            r13.Fields["Char1Look"] = "happy_normal";
            r13.Fields["Char1Pos"] = "Center";
            r13.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r13.Fields["Auto"] = "TRUE";
            r13.FinalizeIndex();
            records.Add(r13);

            // Choice 3-2 결과
            var r14 = new DialogueRecord();
            r14.Fields["ID"] = (lineId++).ToString();
            r14.Fields["Scene"] = "1";
            r14.Fields["ParsedLine_ENG"] = "Mercy is also a form of justice. I respect your decision.";
            r14.Fields["NameTag"] = "Sarah";
            r14.Fields["Char1Name"] = "Sarah";
            r14.Fields["Char1Look"] = "neutral_normal";
            r14.Fields["Char1Pos"] = "Center";
            r14.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r14.Fields["Auto"] = "TRUE";
            r14.FinalizeIndex();
            records.Add(r14);

            // Line 15: 마무리
            var r15 = new DialogueRecord();
            r15.Fields["ID"] = (lineId++).ToString();
            r15.Fields["Scene"] = "1";
            r15.Fields["ParsedLine_ENG"] = "Another day of difficult choices completed. Justice is never simple.";
            r15.Fields["NameTag"] = "Elian";
            r15.Fields["Char1Name"] = "Elian";
            r15.Fields["Char1Look"] = "thinking_normal";
            r15.Fields["Char1Pos"] = "Center";
            r15.Fields["Char1Size"] = "Large"; // ✅ 발화 중 강조
            r15.Fields["Auto"] = "TRUE";
            r15.FinalizeIndex();
            records.Add(r15);

            // ChapterData 저장
            var chapterData = new ChapterData(1, records);
            chapterData.generationPrompt = "Test chapter with choices, background changes, and SFX";
            chapterData.stateSnapshot = new GameStateSnapshot
            {
                currentChapter = 1,
                currentLineId = 1000,
                coreValueScores = new Dictionary<string, int> { { "정의", 0 } }
            };

            string cachePath = System.IO.Path.Combine(
                Application.persistentDataPath,
                $"{projectData.projectGuid}_chapters.json"
            );

            var wrapper = new ChapterCacheWrapper { chapters = new List<ChapterData> { chapterData } };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(cachePath, json);

            Debug.Log($"Test chapter data cached at: {cachePath}");
            Debug.Log($"Total dialogue lines: {records.Count}");
            Debug.Log("✅ Features: 3 choices, 2 background changes, 3 SFX, character size highlighting");
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
