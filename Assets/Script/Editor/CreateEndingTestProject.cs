using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using IyagiAI.Runtime;

/// <summary>
/// 엔딩 시스템 테스트용 더미 프로젝트 생성
/// 1챕터, 5~7개 선택지, 명확한 수치로 테스트
/// </summary>
public class CreateEndingTestProject : EditorWindow
{
    [MenuItem("Iyagi/Create Ending Test Project")]
    static void CreateTestProject()
    {
        // 1. VNProjectData 생성
        VNProjectData projectData = ScriptableObject.CreateInstance<VNProjectData>();
        projectData.gameTitle = "Ending Test Project";
        projectData.gamePremise = "테스트용 1챕터 프로젝트 - 엔딩 시스템 검증";
        projectData.genre = Genre.Adventure;
        projectData.tone = Tone.Lighthearted;
        projectData.playtime = PlaytimeEstimate.Mins30;
        projectData.totalChapters = 1;
        projectData.projectGuid = System.Guid.NewGuid().ToString();
        projectData.createdTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 2. Core Values 설정 (3개)
        projectData.coreValues = new List<CoreValue>
        {
            new CoreValue { name = "용기", derivedSkills = new List<string> { "검술", "방어" } },
            new CoreValue { name = "지혜", derivedSkills = new List<string> { "마법", "전략" } },
            new CoreValue { name = "우정", derivedSkills = new List<string> { "협동", "설득" } }
        };

        // 트루 엔딩 Value 설정
        projectData.trueValueName = "용기";

        // 3. 플레이어 캐릭터 생성
        CharacterData player = ScriptableObject.CreateInstance<CharacterData>();
        player.characterName = "주인공";
        player.age = 18;
        player.gender = Gender.Male;
        player.archetype = Archetype.Hero;
        player.appearanceDescription = "검은 머리, 푸른 눈";
        player.personality = "용감하고 정의로운";
        player.background = "평범한 마을 출신";
        player.resourcePath = "Generated/Characters/주인공";

        AssetDatabase.CreateAsset(player, "Assets/Resources/VNProjects/TestPlayer.asset");
        projectData.playerCharacter = player;

        // 4. NPC 2명 생성 (로맨스 가능)
        CharacterData npc1 = ScriptableObject.CreateInstance<CharacterData>();
        npc1.characterName = "엘리스";
        npc1.role = "전사";
        npc1.age = 20;
        npc1.gender = Gender.Female;
        npc1.archetype = Archetype.Hero;
        npc1.appearanceDescription = "금발, 초록 눈";
        npc1.personality = "활발하고 밝은";
        npc1.background = "기사 가문 출신";
        npc1.isRomanceable = true;
        npc1.initialAffection = 50;
        npc1.resourcePath = "Generated/Characters/엘리스";

        AssetDatabase.CreateAsset(npc1, "Assets/Resources/VNProjects/TestNPC1.asset");

        CharacterData npc2 = ScriptableObject.CreateInstance<CharacterData>();
        npc2.characterName = "루나";
        npc2.role = "마법사";
        npc2.age = 19;
        npc2.gender = Gender.Female;
        npc2.archetype = Archetype.Strategist;
        npc2.appearanceDescription = "은발, 보라색 눈";
        npc2.personality = "조용하고 신중한";
        npc2.background = "마법학교 출신";
        npc2.isRomanceable = true;
        npc2.initialAffection = 50;
        npc2.resourcePath = "Generated/Characters/루나";

        AssetDatabase.CreateAsset(npc2, "Assets/Resources/VNProjects/TestNPC2.asset");

        projectData.npcs.Add(npc1);
        projectData.npcs.Add(npc2);

        // 5. 프로젝트 저장
        AssetDatabase.CreateAsset(projectData, "Assets/Resources/VNProjects/EndingTestProject.asset");
        AssetDatabase.SaveAssets();

        Debug.Log("[CreateEndingTestProject] Test project created!");
        Debug.Log($"Core Values: 용기, 지혜, 우정");
        Debug.Log($"True Value: 용기 (70+ for True Ending)");
        Debug.Log($"NPCs: 엘리스 (romanceable), 루나 (romanceable)");
        Debug.Log($"Initial Affection: 50 each");

        // 6. 프로젝트 슬롯을 파일에 직접 등록 (Editor 모드에서는 SaveDataManager 사용 불가)
        RegisterProjectSlotDirectly(projectData);

        // 7. 테스트 챕터 JSON 생성
        CreateTestChapterJSON();

        EditorUtility.DisplayDialog("Success",
            "Ending Test Project Created!\n\n" +
            "- Project: Assets/Resources/VNProjects/EndingTestProject.asset\n" +
            "- Chapter JSON: Assets/Resources/TestChapter1.json\n\n" +
            "이제 GameScene에서 테스트하세요!",
            "OK");
    }

    static void CreateTestChapterJSON()
    {
        // 테스트용 챕터 1 JSON (2단계 선택지: 코어밸류 먼저, NPC 나중에)
        string chapterJSON = @"[
  {
    ""speaker"": ""narrator"",
    ""text"": ""당신은 모험을 시작합니다. 먼저 어떤 가치를 선택하시겠습니까?"",
    ""character1_name"": ""주인공"",
    ""character1_expression"": ""neutral"",
    ""character1_pose"": ""normal"",
    ""character1_position"": ""Center"",
    ""bg_name"": ""forest"",
    ""bgm_name"": ""adventure"",
    ""choices"": [
      {
        ""text"": ""[용기+100] 용기의 길 (True Ending 조건)"",
        ""next_id"": 1001,
        ""value_impact"": [
          {""value_name"": ""용기"", ""change"": 100}
        ],
        ""affection_impact"": []
      },
      {
        ""text"": ""[지혜+100] 지혜의 길 (Value Ending 조건)"",
        ""next_id"": 1001,
        ""value_impact"": [
          {""value_name"": ""지혜"", ""change"": 100}
        ],
        ""affection_impact"": []
      },
      {
        ""text"": ""[우정+100] 우정의 길 (Value Ending 조건)"",
        ""next_id"": 1001,
        ""value_impact"": [
          {""value_name"": ""우정"", ""change"": 100}
        ],
        ""affection_impact"": []
      },
      {
        ""text"": ""[아무것도 안함] 평범한 길 (Normal Ending)"",
        ""next_id"": 1001,
        ""value_impact"": [],
        ""affection_impact"": []
      }
    ]
  },
  {
    ""speaker"": ""narrator"",
    ""text"": ""이제 누구와 시간을 보내시겠습니까?"",
    ""bg_name"": ""forest"",
    ""choices"": [
      {
        ""text"": ""[엘리스+100] 엘리스와 시간 보내기 (엘리스 Romance)"",
        ""next_id"": 1002,
        ""value_impact"": [],
        ""affection_impact"": [
          {""character_name"": ""엘리스"", ""change"": 100}
        ]
      },
      {
        ""text"": ""[루나+100] 루나와 시간 보내기 (루나 Romance)"",
        ""next_id"": 1002,
        ""value_impact"": [],
        ""affection_impact"": [
          {""character_name"": ""루나"", ""change"": 100}
        ]
      },
      {
        ""text"": ""[엘리스+100, 루나+100] 둘 다와 시간 보내기 (모두 Romance)"",
        ""next_id"": 1002,
        ""value_impact"": [],
        ""affection_impact"": [
          {""character_name"": ""엘리스"", ""change"": 100},
          {""character_name"": ""루나"", ""change"": 100}
        ]
      },
      {
        ""text"": ""[아무도 안만남] 혼자 지내기 (Romance 없음)"",
        ""next_id"": 1002,
        ""value_impact"": [],
        ""affection_impact"": []
      }
    ]
  },
  {
    ""speaker"": ""narrator"",
    ""text"": ""모험이 끝났습니다. 엔딩을 확인합니다..."",
    ""bg_name"": ""sunset""
  }
]";

        System.IO.File.WriteAllText("Assets/Resources/TestChapter1.json", chapterJSON);
        AssetDatabase.Refresh();

        Debug.Log("[CreateEndingTestProject] Test chapter JSON created: Assets/Resources/TestChapter1.json");
        Debug.Log("\n=== 2단계 선택지 시스템 ===");
        Debug.Log("1단계: 코어 밸류 선택 (용기/지혜/우정/아무것도)");
        Debug.Log("2단계: NPC 선택 (엘리스/루나/둘 다/아무도)");
        Debug.Log("\n=== 엔딩 달성 조건 ===");
        Debug.Log("True Ending: 용기 70+ (1단계에서 용기 선택)");
        Debug.Log("Value Ending: 지혜/우정 60+ (1단계에서 지혜/우정 선택)");
        Debug.Log("Normal Ending: 모든 조건 미달 (1단계에서 아무것도 선택)");
        Debug.Log("Romance Achievement: 엘리스/루나 80+ (2단계에서 선택)");
    }

    /// <summary>
    /// Editor 모드에서 프로젝트 슬롯을 파일에 직접 등록
    /// </summary>
    static void RegisterProjectSlotDirectly(VNProjectData projectData)
    {
        string saveFolderPath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Saves");
        string projectSlotsFile = System.IO.Path.Combine(saveFolderPath, "projects.json");

        // 폴더 생성
        if (!System.IO.Directory.Exists(saveFolderPath))
        {
            System.IO.Directory.CreateDirectory(saveFolderPath);
        }

        // 기존 프로젝트 슬롯 로드
        List<IyagiAI.Editor.EndingTest.ProjectSlot> projectSlots = new List<IyagiAI.Editor.EndingTest.ProjectSlot>();
        if (System.IO.File.Exists(projectSlotsFile))
        {
            try
            {
                string json = System.IO.File.ReadAllText(projectSlotsFile);
                var wrapper = JsonUtility.FromJson<IyagiAI.Editor.EndingTest.ProjectSlotListWrapper>(json);
                if (wrapper != null && wrapper.projects != null)
                {
                    projectSlots = wrapper.projects;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CreateEndingTestProject] Failed to load existing projects: {e.Message}");
            }
        }

        // 중복 확인
        var existingSlot = projectSlots.Find(p => p.projectGuid == projectData.projectGuid);
        if (existingSlot != null)
        {
            Debug.Log("[CreateEndingTestProject] Project slot already exists, skipping registration");
            return;
        }

        // 새 프로젝트 슬롯 생성
        var newSlot = new IyagiAI.Editor.EndingTest.ProjectSlot
        {
            projectGuid = projectData.projectGuid,
            projectName = projectData.gameTitle,
            totalChapters = projectData.totalChapters,
            createdDate = System.DateTime.Now,
            lastPlayedDate = System.DateTime.Now,
            saveFiles = new List<IyagiAI.Editor.EndingTest.SaveFile>(),
            unlockedCGs = new List<string>()
        };

        projectSlots.Add(newSlot);

        // 저장
        var wrapper2 = new IyagiAI.Editor.EndingTest.ProjectSlotListWrapper { projects = projectSlots };
        string finalJson = JsonUtility.ToJson(wrapper2, true);
        System.IO.File.WriteAllText(projectSlotsFile, finalJson);

        Debug.Log($"[CreateEndingTestProject] Project slot registered: {projectSlotsFile}");
        Debug.Log($"[CreateEndingTestProject] Total projects: {projectSlots.Count}");
    }
}

// SaveDataManager의 데이터 클래스들 (Editor에서 사용하기 위한 복사본)
namespace IyagiAI.Editor.EndingTest
{
    [System.Serializable]
    public class ProjectSlotListWrapper
    {
        public List<ProjectSlot> projects;
    }

    [System.Serializable]
    public class ProjectSlot
    {
        public string projectGuid;
        public string projectName;
        public int totalChapters;
        public System.DateTime createdDate;
        public System.DateTime lastPlayedDate;
        public List<SaveFile> saveFiles;
        public List<string> unlockedCGs;
    }

    [System.Serializable]
    public class SaveFile
    {
        public string saveFileId;
        public string saveName;
        public System.DateTime lastPlayedDate;
        public int currentChapter;
        public int currentLineId;
    }
}
