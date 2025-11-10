using UnityEngine;
using UnityEditor;
using IyagiAI.Runtime;
using System.Collections.Generic;

namespace IyagiAI.Editor
{
    /// <summary>
    /// 더미 프로젝트 생성 에디터 스크립트
    /// 게임씬 테스트용으로 대사, 네임태그, 선택지가 포함된 프로젝트 생성
    /// </summary>
    public class CreateDummyProject : MonoBehaviour
    {
        [MenuItem("Iyagi/Create Dummy Project")]
        public static void CreateDummy()
        {
            // 1. 프로젝트 데이터 생성
            VNProjectData projectData = ScriptableObject.CreateInstance<VNProjectData>();
            projectData.gameTitle = "더미 테스트 프로젝트";
            projectData.gamePremise = "대사, 네임태그, 선택지 시스템 테스트용 프로젝트";
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

            // 2. 플레이어 캐릭터 생성
            CharacterData player = ScriptableObject.CreateInstance<CharacterData>();
            player.characterName = "주인공";
            player.age = 20;
            player.gender = Gender.Male;
            player.pov = POV.FirstPerson;
            player.appearanceDescription = "검은 머리의 평범한 청년";
            player.personality = "성실하고 친절한 성격";
            player.archetype = Archetype.Hero;
            player.resourcePath = "Generated/Characters/주인공";

            // 3. NPC 캐릭터 생성
            CharacterData npc1 = ScriptableObject.CreateInstance<CharacterData>();
            npc1.characterName = "친구A";
            npc1.role = "친구";
            npc1.age = 20;
            npc1.gender = Gender.Female;
            npc1.appearanceDescription = "갈색 머리의 활발한 여성";
            npc1.personality = "밝고 활발한 성격";
            npc1.archetype = Archetype.Innocent;
            npc1.isRomanceable = true;
            npc1.initialAffection = 50;
            npc1.resourcePath = "Generated/Characters/친구A";

            projectData.playerCharacter = player;
            projectData.npcs.Add(npc1);

            // 4. 폴더 생성
            string projectFolder = "Assets/Resources/VNProjects/DummyProject";
            if (!System.IO.Directory.Exists(projectFolder))
            {
                System.IO.Directory.CreateDirectory(projectFolder);
            }

            // 5. ScriptableObject 저장
            AssetDatabase.CreateAsset(projectData, $"{projectFolder}/DummyProject.asset");
            AssetDatabase.CreateAsset(player, $"{projectFolder}/Player.asset");
            AssetDatabase.CreateAsset(npc1, $"{projectFolder}/NPC_FriendA.asset");
            AssetDatabase.SaveAssets();

            // 6. SaveDataManager에 프로젝트 슬롯 생성
            var saveManager = SaveDataManager.Instance;
            var projectSlot = saveManager.GetProjectSlot(projectData.projectGuid);

            if (projectSlot == null)
            {
                projectSlot = saveManager.CreateProjectSlot(projectData);
                Debug.Log($"Created project slot: {projectSlot.projectName}");
            }

            // 7. 첫 챕터용 SaveFile 생성
            var saveFile = saveManager.CreateNewSaveFile(projectData.projectGuid, 1);
            Debug.Log($"Created save file: {saveFile.saveFileId}");

            // 8. 더미 대사 데이터 생성 (ChapterData로 캐싱)
            CreateDummyChapterData(projectData, saveFile);

            Debug.Log("✅ Dummy project created successfully!");
            Debug.Log($"Project GUID: {projectData.projectGuid}");
            Debug.Log($"SaveFile ID: {saveFile.saveFileId}");
            Debug.Log("타이틀 씬에서 'Load Game' → '더미 테스트 프로젝트' 선택하여 테스트하세요.");

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 더미 챕터 데이터 생성 (대사, 네임태그, 선택지 포함)
        /// </summary>
        private static void CreateDummyChapterData(VNProjectData projectData, SaveFile saveFile)
        {
            List<DialogueRecord> records = new List<DialogueRecord>();

            // 대사 1: 나레이션
            var record1 = new DialogueRecord();
            record1.Fields["ID"] = "1000";
            record1.Fields["Scene"] = "1";
            record1.Fields["Index"] = "0";
            record1.Fields["ParsedLine_ENG"] = "어느 평화로운 오후, 학교 운동장에서...";
            record1.Fields["NameTag"] = "Narrator";
            record1.Fields["Auto"] = "TRUE";
            record1.FinalizeIndex();
            records.Add(record1);

            // 대사 2: 주인공
            var record2 = new DialogueRecord();
            record2.Fields["ID"] = "1001";
            record2.Fields["Scene"] = "1";
            record2.Fields["Index"] = "1";
            record2.Fields["ParsedLine_ENG"] = "오늘 날씨가 정말 좋네!";
            record2.Fields["NameTag"] = "주인공";
            record2.Fields["Char1Name"] = "주인공";
            record2.Fields["Char1Look"] = "happy_normal";
            record2.Fields["Char1Pos"] = "Center";
            record2.Fields["Auto"] = "TRUE";
            record2.FinalizeIndex();
            records.Add(record2);

            // 대사 3: 친구A
            var record3 = new DialogueRecord();
            record3.Fields["ID"] = "1002";
            record3.Fields["Scene"] = "1";
            record3.Fields["Index"] = "2";
            record3.Fields["ParsedLine_ENG"] = "그러게! 오늘은 뭐 하고 놀까?";
            record3.Fields["NameTag"] = "친구A";
            record3.Fields["Char1Name"] = "친구A";
            record3.Fields["Char1Look"] = "happy_normal";
            record3.Fields["Char1Pos"] = "Center";
            record3.Fields["Auto"] = "TRUE";
            record3.FinalizeIndex();
            records.Add(record3);

            // 대사 4: 선택지 포함
            var record4 = new DialogueRecord();
            record4.Fields["ID"] = "1003";
            record4.Fields["Scene"] = "1";
            record4.Fields["Index"] = "3";
            record4.Fields["ParsedLine_ENG"] = "우리 뭐 할까?";
            record4.Fields["NameTag"] = "주인공";
            record4.Fields["Char1Name"] = "주인공";
            record4.Fields["Char1Look"] = "thinking_normal";
            record4.Fields["Char1Pos"] = "Center";

            // 선택지 1
            record4.Fields["Choice1_ENG"] = "운동장에서 놀자";
            record4.Fields["Next1"] = "1004";
            record4.Fields["Choice1_ValueImpact_우정"] = "10";

            // 선택지 2
            record4.Fields["Choice2_ENG"] = "도서관에 가자";
            record4.Fields["Next2"] = "1005";
            record4.Fields["Choice2_ValueImpact_우정"] = "5";

            record4.FinalizeIndex();
            records.Add(record4);

            // 대사 5: 선택지 1 결과
            var record5 = new DialogueRecord();
            record5.Fields["ID"] = "1004";
            record5.Fields["Scene"] = "1";
            record5.Fields["Index"] = "4";
            record5.Fields["ParsedLine_ENG"] = "좋아! 운동장에서 축구하자!";
            record5.Fields["NameTag"] = "친구A";
            record5.Fields["Char1Name"] = "친구A";
            record5.Fields["Char1Look"] = "happy_normal";
            record5.Fields["Char1Pos"] = "Center";
            record5.Fields["Auto"] = "TRUE";
            record5.FinalizeIndex();
            records.Add(record5);

            // 대사 6: 선택지 2 결과
            var record6 = new DialogueRecord();
            record6.Fields["ID"] = "1005";
            record6.Fields["Scene"] = "1";
            record6.Fields["Index"] = "5";
            record6.Fields["ParsedLine_ENG"] = "도서관도 좋지! 조용히 공부할 수 있어.";
            record6.Fields["NameTag"] = "친구A";
            record6.Fields["Char1Name"] = "친구A";
            record6.Fields["Char1Look"] = "neutral_normal";
            record6.Fields["Char1Pos"] = "Center";
            record6.Fields["Auto"] = "TRUE";
            record6.FinalizeIndex();
            records.Add(record6);

            // 대사 7: 마무리
            var record7 = new DialogueRecord();
            record7.Fields["ID"] = "1006";
            record7.Fields["Scene"] = "1";
            record7.Fields["Index"] = "6";
            record7.Fields["ParsedLine_ENG"] = "오늘도 즐거운 하루였다!";
            record7.Fields["NameTag"] = "주인공";
            record7.Fields["Char1Name"] = "주인공";
            record7.Fields["Char1Look"] = "happy_normal";
            record7.Fields["Char1Pos"] = "Center";
            record7.Fields["Auto"] = "TRUE";
            record7.FinalizeIndex();
            records.Add(record7);

            // ChapterData로 저장 (캐시)
            var chapterData = new ChapterData(1, records);
            chapterData.generationPrompt = "Dummy chapter for testing";
            chapterData.stateSnapshot = new GameStateSnapshot
            {
                currentChapter = 1,
                currentLineId = 1000,
                coreValueScores = new Dictionary<string, int> { { "우정", 0 } }
            };

            // ChapterGenerationManager 캐시에 저장
            string cachePath = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath,
                $"{projectData.projectGuid}_chapters.json"
            );

            var wrapper = new ChapterCacheWrapper { chapters = new List<ChapterData> { chapterData } };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(cachePath, json);

            Debug.Log($"Dummy chapter data cached at: {cachePath}");
            Debug.Log($"Total dialogue lines: {records.Count}");
        }

        [System.Serializable]
        private class ChapterCacheWrapper
        {
            public List<ChapterData> chapters;
        }
    }
}
