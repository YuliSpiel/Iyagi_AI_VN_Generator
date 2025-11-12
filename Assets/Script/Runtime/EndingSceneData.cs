using System.Collections.Generic;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 미리 작성된 엔딩 씬 데이터
    /// EndingType별로 고정된 대사와 연출을 저장
    /// </summary>
    [CreateAssetMenu(fileName = "EndingSceneData", menuName = "Iyagi/Ending Scene Data")]
    public class EndingSceneData : ScriptableObject
    {
        [Header("Ending Configuration")]
        public EndingType endingType;
        public string endingTitle;
        [TextArea(3, 5)]
        public string endingDescription;

        [Header("Dialogue Lines")]
        public List<EndingDialogueLine> dialogueLines = new List<EndingDialogueLine>();

        /// <summary>
        /// DialogueRecord 리스트로 변환
        /// </summary>
        public List<DialogueRecord> ToDialogueRecords()
        {
            List<DialogueRecord> records = new List<DialogueRecord>();
            int baseId = 999000; // 엔딩 씬 ID: 999000부터 시작

            for (int i = 0; i < dialogueLines.Count; i++)
            {
                var line = dialogueLines[i];
                var record = new DialogueRecord();

                // 기본 필드
                record.Fields["ID"] = (baseId + i).ToString();
                record.Fields["Scene"] = "999";
                record.Fields["Index"] = i.ToString();

                // 대화 텍스트
                record.Fields["ParsedLine_ENG"] = line.dialogueText;
                record.Fields["NameTag"] = line.speakerName;

                // 배경/BGM/SFX
                if (!string.IsNullOrEmpty(line.bgName))
                    record.Fields["BG"] = line.bgName;
                if (!string.IsNullOrEmpty(line.bgmName))
                    record.Fields["BGM"] = line.bgmName;
                if (!string.IsNullOrEmpty(line.sfxName))
                    record.Fields["SFX"] = line.sfxName;

                // 캐릭터 표시
                if (!string.IsNullOrEmpty(line.characterName))
                {
                    record.Fields["Char1Name"] = line.characterName;
                    record.Fields["Char1Look"] = $"{line.expression}_{line.pose}";
                    record.Fields["Char1Pos"] = line.position;
                    record.Fields["Char1Size"] = "Medium";
                }

                // CG 표시
                if (!string.IsNullOrEmpty(line.cgId))
                {
                    record.Fields["CG_ID"] = line.cgId;
                    record.Fields["IsCGLine"] = "TRUE";
                }

                // 마지막 라인에 엔딩 마커 추가
                if (i == dialogueLines.Count - 1)
                {
                    record.Fields["NextIndex1"] = "0"; // 엔딩 표시
                }
                else
                {
                    record.Fields["Auto"] = "TRUE"; // 자동 진행
                }

                record.FinalizeIndex();
                records.Add(record);
            }

            return records;
        }
    }

    /// <summary>
    /// 엔딩 대사 라인
    /// </summary>
    [System.Serializable]
    public class EndingDialogueLine
    {
        [Header("Dialogue")]
        public string speakerName;
        [TextArea(2, 4)]
        public string dialogueText;

        [Header("Visual")]
        public string characterName;
        public string expression = "neutral";
        public string pose = "normal";
        public string position = "Center";

        [Header("Scene")]
        public string bgName;
        public string bgmName;
        public string sfxName;
        public string cgId;
    }
}
