using UnityEngine;
using UnityEditor;
using IyagiAI.Runtime;
using System.Collections.Generic;

namespace IyagiAI.Editor
{
    /// <summary>
    /// 선택지 테스트용 더미 프로젝트 생성
    /// 기존 이미지 사용 (API 호출 없음)
    /// </summary>
    public class CreateChoiceTestProject : MonoBehaviour
    {
        [MenuItem("Iyagi/Create Choice Test Project")]
        public static void CreateChoiceTest()
        {
            // 1. 프로젝트 데이터 생성
            VNProjectData projectData = ScriptableObject.CreateInstance<VNProjectData>();
            projectData.gameTitle = "선택지 테스트 프로젝트";
            projectData.gamePremise = "선택지와 스킬 시스템 테스트";
            projectData.genre = Genre.Slice_of_Life;
            projectData.tone = Tone.Lighthearted;
            projectData.playtime = PlaytimeEstimate.Mins30;
            projectData.totalChapters = 1;
            projectData.projectGuid = System.Guid.NewGuid().ToString();
            projectData.createdTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();

            // Core Values
            projectData.coreValues.Add(new CoreValue
            {
                name = "우정",
                derivedSkills = new List<string> { "공감", "협력", "신뢰" }
            });

            projectData.coreValues.Add(new CoreValue
            {
                name = "용기",
                derivedSkills = new List<string> { "결단력", "리더십" }
            });

            // 2. 캐릭터 생성 (기존 이미지 사용)
            CharacterData player = ScriptableObject.CreateInstance<CharacterData>();
            player.characterName = "주인공";
            player.age = 20;
            player.gender = Gender.Male;
            player.pov = POV.FirstPerson;
            player.appearanceDescription = "평범한 학생";
            player.personality = "성실하고 친절한 성격";
            player.archetype = Archetype.Hero;
            player.resourcePath = "Generated/Characters/주인공";

            // 세라 (기존 이미지: angry_normal.png, happy_normal.png)
            CharacterData sera = ScriptableObject.CreateInstance<CharacterData>();
            sera.characterName = "세라";
            sera.role = "친구";
            sera.age = 19;
            sera.gender = Gender.Female;
            sera.appearanceDescription = "활발한 여학생";
            sera.personality = "밝고 긍정적";
            sera.archetype = Archetype.Innocent;
            sera.isRomanceable = true;
            sera.initialAffection = 50;
            sera.resourcePath = "Generated/Characters/세라";

            // 엘리안 (기존 이미지: angry_normal.png, happy_normal.png)
            CharacterData elian = ScriptableObject.CreateInstance<CharacterData>();
            elian.characterName = "엘리안";
            elian.role = "친구";
            elian.age = 20;
            elian.gender = Gender.Male;
            elian.appearanceDescription = "침착한 남학생";
            elian.personality = "차분하고 신중함";
            elian.archetype = Archetype.Strategist;
            elian.isRomanceable = false;
            elian.initialAffection = 50;
            elian.resourcePath = "Generated/Characters/엘리안";

            projectData.playerCharacter = player;
            projectData.npcs.Add(sera);
            projectData.npcs.Add(elian);

            // 3. 폴더 생성
            string projectFolder = "Assets/Resources/Projects";
            if (!System.IO.Directory.Exists(projectFolder))
            {
                System.IO.Directory.CreateDirectory(projectFolder);
            }

            // 4. ScriptableObject 저장
            AssetDatabase.CreateAsset(projectData, $"{projectFolder}/ChoiceTestProject.asset");
            AssetDatabase.AddObjectToAsset(player, projectData);
            AssetDatabase.AddObjectToAsset(sera, projectData);
            AssetDatabase.AddObjectToAsset(elian, projectData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 5. 세이브 데이터 생성
            CreateSaveData(projectData);

            // 6. 더미 스프라이트 생성 (기존 이미지 로드)
            CreateDummySprites(sera, elian);

            Debug.Log("✅ Choice Test Project created successfully!");
            Debug.Log($"Project GUID: {projectData.projectGuid}");
            Debug.Log("타이틀 씬에서 '선택지 테스트 프로젝트'를 선택하여 테스트하세요.");

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 세이브 데이터 생성
        /// </summary>
        private static void CreateSaveData(VNProjectData projectData)
        {
            string saveFolder = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Saves");
            if (!System.IO.Directory.Exists(saveFolder))
            {
                System.IO.Directory.CreateDirectory(saveFolder);
            }

            // 프로젝트 슬롯 파일
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

            // 기존 프로젝트 로드 및 병합
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
                catch { }
            }

            allSlots.RemoveAll(p => p.projectGuid == projectData.projectGuid);
            allSlots.Add(projectSlot);

            string projectsJson = SerializeProjectSlots(allSlots);
            System.IO.File.WriteAllText(projectSlotsFile, projectsJson);

            Debug.Log($"Created save file: {saveFile.saveFileId}");

            // 챕터 데이터 생성
            CreateChapterData(projectData, saveFile);
        }

        /// <summary>
        /// ProjectSlot 리스트를 JSON으로 직렬화
        /// </summary>
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

        /// <summary>
        /// 챕터 데이터 생성 (선택지 중심 시나리오)
        /// </summary>
        private static void CreateChapterData(VNProjectData projectData, SaveFile saveFile)
        {
            List<DialogueRecord> records = new List<DialogueRecord>();

            // === 씬 1: 도입 ===
            var r1 = new DialogueRecord();
            r1.Fields["ID"] = "1000";
            r1.Fields["Scene"] = "1";
            r1.Fields["Index"] = "0";
            r1.Fields["ParsedLine_ENG"] = "학교 복도에서 세라와 엘리안을 만났다.";
            r1.Fields["NameTag"] = "Narrator";
            r1.Fields["Auto"] = "TRUE";
            r1.FinalizeIndex();
            records.Add(r1);

            // === 씬 2: 세라 등장 ===
            var r2 = new DialogueRecord();
            r2.Fields["ID"] = "1001";
            r2.Fields["Scene"] = "1";
            r2.Fields["Index"] = "1";
            r2.Fields["ParsedLine_ENG"] = "안녕! 오늘 점심 같이 먹을래?";
            r2.Fields["NameTag"] = "세라";
            r2.Fields["Char1Name"] = "세라";
            r2.Fields["Char1Look"] = "happy_normal";
            r2.Fields["Char1Pos"] = "Left";
            r2.Fields["Auto"] = "TRUE";
            r2.FinalizeIndex();
            records.Add(r2);

            // === 씬 3: 엘리안 등장 ===
            var r3 = new DialogueRecord();
            r3.Fields["ID"] = "1002";
            r3.Fields["Scene"] = "1";
            r3.Fields["Index"] = "2";
            r3.Fields["ParsedLine_ENG"] = "나도 같이 가도 될까?";
            r3.Fields["NameTag"] = "엘리안";
            r3.Fields["Char1Name"] = "세라";
            r3.Fields["Char1Look"] = "happy_normal";
            r3.Fields["Char1Pos"] = "Left";
            r3.Fields["Char2Name"] = "엘리안";
            r3.Fields["Char2Look"] = "happy_normal";
            r3.Fields["Char2Pos"] = "Right";
            r3.Fields["Auto"] = "TRUE";
            r3.FinalizeIndex();
            records.Add(r3);

            // === 선택지 1: 어디서 먹을까? ===
            var r4 = new DialogueRecord();
            r4.Fields["ID"] = "1003";
            r4.Fields["Scene"] = "1";
            r4.Fields["Index"] = "3";
            r4.Fields["ParsedLine_ENG"] = "어디서 먹을까?";
            r4.Fields["NameTag"] = "세라";
            r4.Fields["Char1Name"] = "세라";
            r4.Fields["Char1Look"] = "happy_normal";
            r4.Fields["Char1Pos"] = "Left";
            r4.Fields["Char2Name"] = "엘리안";
            r4.Fields["Char2Look"] = "happy_normal";
            r4.Fields["Char2Pos"] = "Right";

            // 선택지 1: 옥상 (우정+10, 공감+5, 협력+8)
            r4.Fields["Choice1_ENG"] = "옥상에서 먹자!";
            r4.Fields["Next1"] = "1004";
            r4.Fields["Choice1_ValueImpact_우정"] = "10";
            r4.Fields["Choice1_SkillImpact_공감"] = "5";
            r4.Fields["Choice1_SkillImpact_협력"] = "8";
            r4.Fields["Choice1_Affection_세라"] = "5";

            // 선택지 2: 식당 (우정+5, 신뢰+6)
            r4.Fields["Choice2_ENG"] = "식당으로 가자";
            r4.Fields["Next2"] = "1010";
            r4.Fields["Choice2_ValueImpact_우정"] = "5";
            r4.Fields["Choice2_SkillImpact_신뢰"] = "6";
            r4.Fields["Choice2_Affection_엘리안"] = "5";

            r4.FinalizeIndex();
            records.Add(r4);

            // === 옥상 루트 (1004~1009) ===
            var r5 = new DialogueRecord();
            r5.Fields["ID"] = "1004";
            r5.Fields["Scene"] = "1";
            r5.Fields["Index"] = "4";
            r5.Fields["ParsedLine_ENG"] = "좋아! 날씨도 좋은데 옥상에서 먹으면 좋겠다!";
            r5.Fields["NameTag"] = "세라";
            r5.Fields["Char1Name"] = "세라";
            r5.Fields["Char1Look"] = "happy_normal";
            r5.Fields["Char1Pos"] = "Center";
            r5.Fields["Auto"] = "TRUE";
            r5.FinalizeIndex();
            records.Add(r5);

            var r6 = new DialogueRecord();
            r6.Fields["ID"] = "1005";
            r6.Fields["Scene"] = "1";
            r6.Fields["Index"] = "5";
            r6.Fields["ParsedLine_ENG"] = "바람도 시원하고 기분이 좋네.";
            r6.Fields["NameTag"] = "엘리안";
            r6.Fields["Char1Name"] = "엘리안";
            r6.Fields["Char1Look"] = "happy_normal";
            r6.Fields["Char1Pos"] = "Center";
            r6.Fields["Auto"] = "TRUE";
            r6.FinalizeIndex();
            records.Add(r6);

            // 옥상 루트 선택지 2: 게임 제안
            var r7 = new DialogueRecord();
            r7.Fields["ID"] = "1006";
            r7.Fields["Scene"] = "1";
            r7.Fields["Index"] = "6";
            r7.Fields["ParsedLine_ENG"] = "우리 게임 하나 할까?";
            r7.Fields["NameTag"] = "세라";
            r7.Fields["Char1Name"] = "세라";
            r7.Fields["Char1Look"] = "happy_normal";
            r7.Fields["Char1Pos"] = "Left";
            r7.Fields["Char2Name"] = "엘리안";
            r7.Fields["Char2Look"] = "happy_normal";
            r7.Fields["Char2Pos"] = "Right";

            // 선택지: 게임 종류
            r7.Fields["Choice1_ENG"] = "진실게임 하자!";
            r7.Fields["Next1"] = "1007";
            r7.Fields["Choice1_ValueImpact_용기"] = "10";
            r7.Fields["Choice1_SkillImpact_결단력"] = "8";
            r7.Fields["Choice1_Affection_세라"] = "10";

            r7.Fields["Choice2_ENG"] = "그냥 이야기하자";
            r7.Fields["Next2"] = "1008";
            r7.Fields["Choice2_ValueImpact_우정"] = "8";
            r7.Fields["Choice2_SkillImpact_공감"] = "7";

            r7.FinalizeIndex();
            records.Add(r7);

            // 진실게임 결과
            var r8 = new DialogueRecord();
            r8.Fields["ID"] = "1007";
            r8.Fields["Scene"] = "1";
            r8.Fields["Index"] = "7";
            r8.Fields["ParsedLine_ENG"] = "오, 재밌겠다! 나부터 시작할게~";
            r8.Fields["NameTag"] = "세라";
            r8.Fields["Char1Name"] = "세라";
            r8.Fields["Char1Look"] = "happy_normal";
            r8.Fields["Char1Pos"] = "Center";
            r8.Fields["Auto"] = "TRUE";
            r8.FinalizeIndex();
            records.Add(r8);

            // 이야기 결과
            var r9 = new DialogueRecord();
            r9.Fields["ID"] = "1008";
            r9.Fields["Scene"] = "1";
            r9.Fields["Index"] = "8";
            r9.Fields["ParsedLine_ENG"] = "그래, 편하게 이야기하는 게 좋지.";
            r9.Fields["NameTag"] = "엘리안";
            r9.Fields["Char1Name"] = "엘리안";
            r9.Fields["Char1Look"] = "happy_normal";
            r9.Fields["Char1Pos"] = "Center";
            r9.Fields["Auto"] = "TRUE";
            r9.FinalizeIndex();
            records.Add(r9);

            // 옥상 루트 마무리
            var r10 = new DialogueRecord();
            r10.Fields["ID"] = "1009";
            r10.Fields["Scene"] = "1";
            r10.Fields["Index"] = "9";
            r10.Fields["ParsedLine_ENG"] = "오늘 정말 즐거웠어!";
            r10.Fields["NameTag"] = "세라";
            r10.Fields["Char1Name"] = "세라";
            r10.Fields["Char1Look"] = "happy_normal";
            r10.Fields["Char1Pos"] = "Left";
            r10.Fields["Char2Name"] = "엘리안";
            r10.Fields["Char2Look"] = "happy_normal";
            r10.Fields["Char2Pos"] = "Right";
            r10.Fields["Auto"] = "TRUE";
            r10.FinalizeIndex();
            records.Add(r10);

            // === 식당 루트 (1010~1015) ===
            var r11 = new DialogueRecord();
            r11.Fields["ID"] = "1010";
            r11.Fields["Scene"] = "1";
            r11.Fields["Index"] = "10";
            r11.Fields["ParsedLine_ENG"] = "식당이 좋겠어. 오늘 메뉴가 맛있대.";
            r11.Fields["NameTag"] = "엘리안";
            r11.Fields["Char1Name"] = "엘리안";
            r11.Fields["Char1Look"] = "happy_normal";
            r11.Fields["Char1Pos"] = "Center";
            r11.Fields["Auto"] = "TRUE";
            r11.FinalizeIndex();
            records.Add(r11);

            var r12 = new DialogueRecord();
            r12.Fields["ID"] = "1011";
            r12.Fields["Scene"] = "1";
            r12.Fields["Index"] = "11";
            r12.Fields["ParsedLine_ENG"] = "그러네! 빨리 가자~";
            r12.Fields["NameTag"] = "세라";
            r12.Fields["Char1Name"] = "세라";
            r12.Fields["Char1Look"] = "happy_normal";
            r12.Fields["Char1Pos"] = "Center";
            r12.Fields["Auto"] = "TRUE";
            r12.FinalizeIndex();
            records.Add(r12);

            // 식당 루트 선택지: 메뉴 선택
            var r13 = new DialogueRecord();
            r13.Fields["ID"] = "1012";
            r13.Fields["Scene"] = "1";
            r13.Fields["Index"] = "12";
            r13.Fields["ParsedLine_ENG"] = "뭐 먹을까?";
            r13.Fields["NameTag"] = "세라";
            r13.Fields["Char1Name"] = "세라";
            r13.Fields["Char1Look"] = "happy_normal";
            r13.Fields["Char1Pos"] = "Left";
            r13.Fields["Char2Name"] = "엘리안";
            r13.Fields["Char2Look"] = "happy_normal";
            r13.Fields["Char2Pos"] = "Right";

            r13.Fields["Choice1_ENG"] = "매운 요리!";
            r13.Fields["Next1"] = "1013";
            r13.Fields["Choice1_ValueImpact_용기"] = "5";
            r13.Fields["Choice1_SkillImpact_리더십"] = "4";

            r13.Fields["Choice2_ENG"] = "순한 요리";
            r13.Fields["Next2"] = "1014";
            r13.Fields["Choice2_ValueImpact_우정"] = "5";
            r13.Fields["Choice2_SkillImpact_협력"] = "3";

            r13.FinalizeIndex();
            records.Add(r13);

            // 매운 요리
            var r14 = new DialogueRecord();
            r14.Fields["ID"] = "1013";
            r14.Fields["Scene"] = "1";
            r14.Fields["Index"] = "13";
            r14.Fields["ParsedLine_ENG"] = "와, 도전적이네! 나도 같이 먹을래!";
            r14.Fields["NameTag"] = "세라";
            r14.Fields["Char1Name"] = "세라";
            r14.Fields["Char1Look"] = "happy_normal";
            r14.Fields["Char1Pos"] = "Center";
            r14.Fields["Auto"] = "TRUE";
            r14.FinalizeIndex();
            records.Add(r14);

            // 순한 요리
            var r15 = new DialogueRecord();
            r15.Fields["ID"] = "1014";
            r15.Fields["Scene"] = "1";
            r15.Fields["Index"] = "14";
            r15.Fields["ParsedLine_ENG"] = "나도 그게 좋을 것 같아. 같이 먹자.";
            r15.Fields["NameTag"] = "엘리안";
            r15.Fields["Char1Name"] = "엘리안";
            r15.Fields["Char1Look"] = "happy_normal";
            r15.Fields["Char1Pos"] = "Center";
            r15.Fields["Auto"] = "TRUE";
            r15.FinalizeIndex();
            records.Add(r15);

            // 식당 루트 마무리
            var r16 = new DialogueRecord();
            r16.Fields["ID"] = "1015";
            r16.Fields["Scene"] = "1";
            r16.Fields["Index"] = "15";
            r16.Fields["ParsedLine_ENG"] = "맛있게 먹었어. 다음에 또 같이 먹자!";
            r16.Fields["NameTag"] = "엘리안";
            r16.Fields["Char1Name"] = "세라";
            r16.Fields["Char1Look"] = "happy_normal";
            r16.Fields["Char1Pos"] = "Left";
            r16.Fields["Char2Name"] = "엘리안";
            r16.Fields["Char2Look"] = "happy_normal";
            r16.Fields["Char2Pos"] = "Right";
            r16.Fields["Auto"] = "TRUE";
            r16.FinalizeIndex();
            records.Add(r16);

            // === 엔딩 ===
            var r17 = new DialogueRecord();
            r17.Fields["ID"] = "1016";
            r17.Fields["Scene"] = "1";
            r17.Fields["Index"] = "16";
            r17.Fields["ParsedLine_ENG"] = "즐거운 점심시간이었다.";
            r17.Fields["NameTag"] = "Narrator";
            r17.Fields["Auto"] = "TRUE";
            r17.FinalizeIndex();
            records.Add(r17);

            // ChapterData로 저장
            var chapterData = new ChapterData(1, records);
            chapterData.generationPrompt = "Choice test chapter";
            chapterData.stateSnapshot = new GameStateSnapshot
            {
                currentChapter = 1,
                currentLineId = 1000,
                coreValueScores = new Dictionary<string, int>
                {
                    { "우정", 0 },
                    { "용기", 0 }
                },
                skillScores = new Dictionary<string, int>
                {
                    { "공감", 0 },
                    { "협력", 0 },
                    { "신뢰", 0 },
                    { "결단력", 0 },
                    { "리더십", 0 }
                }
            };

            string cachePath = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath,
                $"{projectData.projectGuid}_chapters.json"
            );

            var wrapper = new ChapterCacheWrapper { chapters = new List<ChapterData> { chapterData } };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(cachePath, json);

            Debug.Log($"Chapter data cached at: {cachePath}");
            Debug.Log($"Total dialogue lines: {records.Count}");
            Debug.Log($"Choice points: 4 (with multiple branches)");
        }

        /// <summary>
        /// 기존 이미지를 캐릭터 데이터에 로드
        /// </summary>
        private static void CreateDummySprites(CharacterData sera, CharacterData elian)
        {
            // 세라 스프라이트 로드
            sera.standingSprites = new Dictionary<string, Sprite>();
            LoadSpriteIfExists(sera, "angry_normal");
            LoadSpriteIfExists(sera, "happy_normal");

            // 엘리안 스프라이트 로드
            elian.standingSprites = new Dictionary<string, Sprite>();
            LoadSpriteIfExists(elian, "angry_normal");
            LoadSpriteIfExists(elian, "happy_normal");

            Debug.Log($"세라 스프라이트 로드: {sera.standingSprites.Count}개");
            Debug.Log($"엘리안 스프라이트 로드: {elian.standingSprites.Count}개");
        }

        private static void LoadSpriteIfExists(CharacterData character, string key)
        {
            string path = $"Generated/Characters/{character.characterName}/{key}";
            Sprite sprite = Resources.Load<Sprite>(path);

            if (sprite != null)
            {
                character.standingSprites[key] = sprite;
                Debug.Log($"✅ Loaded sprite: {path}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Sprite not found: {path}");
            }
        }

        [System.Serializable]
        private class ChapterCacheWrapper
        {
            public List<ChapterData> chapters;
        }
    }
}
