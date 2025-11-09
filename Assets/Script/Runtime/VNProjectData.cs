using System;
using System.Collections.Generic;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// Setup Wizard에서 생성되는 VN 프로젝트 전체 데이터
    /// ScriptableObject로 Assets/VNProjects/에 저장됨
    /// </summary>
    [CreateAssetMenu(fileName = "VNProject", menuName = "Iyagi/VN Project Data")]
    public class VNProjectData : ScriptableObject
    {
        [Header("Game Overview")]
        public string gameTitle;

        [TextArea(3, 6)]
        public string gamePremise; // 게임 전체 줄거리

        public Genre genre;
        public Tone tone;
        public PlaytimeEstimate playtime;

        [Header("Core Values")]
        public List<string> coreValues = new List<string>(); // 예: ["Courage", "Wisdom", "Compassion"]

        [Header("Story Structure")]
        public int totalChapters = 4;
        public List<EndingCondition> endings = new List<EndingCondition>(); // 엔딩 조건 리스트

        [Header("Characters")]
        public CharacterData playerCharacter;
        public List<CharacterData> npcs = new List<CharacterData>();

        [Header("CG Metadata")]
        public List<CGMetadata> allCGs = new List<CGMetadata>(); // 모든 생성된 CG 목록

        [Header("Generated Metadata")]
        public string projectGuid; // 프로젝트 고유 ID (저장 파일 구분용)
        public long createdTimestamp; // Unix timestamp
    }

    // ===== Enums =====

    public enum Genre { Fantasy, SciFi, Mystery, Romance, Horror, Adventure, Slice_of_Life }
    public enum Tone { Lighthearted, Serious, Dark, Comedic, Dramatic }
    public enum PlaytimeEstimate { Mins30, Hour1, Hour2, Hour3Plus }

    // ===== Supporting Classes =====

    /// <summary>
    /// 엔딩 조건 정의
    /// </summary>
    [System.Serializable]
    public class EndingCondition
    {
        public int endingId;
        public string endingName; // 예: "True Ending", "Bad Ending"

        [TextArea(2, 4)]
        public string endingDescription; // 엔딩 설명

        public Dictionary<string, int> requiredValues; // Core Value 요구치 (예: Courage >= 50)
        public Dictionary<string, int> requiredAffections; // 캐릭터 호감도 요구치 (예: NPC1 >= 70)
    }

    /// <summary>
    /// CG 일러스트 메타데이터 (레퍼런스 기반 생성)
    /// </summary>
    [System.Serializable]
    public class CGMetadata
    {
        public string cgId; // 예: "Ch1_CG1"
        public int chapterNumber;
        public string title; // CG 제목 (예: "운명적 만남")

        [TextArea(3, 5)]
        public string sceneDescription; // 전체 장면 묘사

        public string lighting; // 조명 (예: "warm sunset glow", "moonlight")
        public string mood; // 분위기 (예: "nostalgic", "romantic", "dramatic")
        public string cameraAngle; // 카메라 앵글 (예: "close-up", "waist-up", "wide shot")

        public string imagePath; // 저장된 이미지 경로 (예: "Image/CG/Ch1_CG1")
        public List<string> characterNames = new List<string>(); // 등장 캐릭터 이름 (레퍼런스용)

        public CGMetadata() { }

        public CGMetadata(int chapter, int index, string title, string description)
        {
            this.chapterNumber = chapter;
            this.cgId = $"Ch{chapter}_CG{index}";
            this.title = title;
            this.sceneDescription = description;
            this.imagePath = $"Image/CG/{cgId}";
        }
    }
}
