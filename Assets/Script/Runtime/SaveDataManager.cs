using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 세이브/로드 관리자
    /// GameStateSnapshot을 JSON으로 저장/로드
    /// </summary>
    public class SaveDataManager : MonoBehaviour
    {
        private const string SAVE_FOLDER = "Saves";
        private const int MAX_SAVE_SLOTS = 10;

        private string SaveFolderPath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        /// <summary>
        /// 게임 저장
        /// </summary>
        public bool SaveGame(GameStateSnapshot state, int slotIndex, string saveName = null)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            {
                Debug.LogError($"Invalid save slot: {slotIndex}");
                return false;
            }

            try
            {
                // 폴더 생성
                if (!Directory.Exists(SaveFolderPath))
                {
                    Directory.CreateDirectory(SaveFolderPath);
                }

                // SaveData 객체 생성
                SaveData saveData = new SaveData
                {
                    slotIndex = slotIndex,
                    saveName = string.IsNullOrEmpty(saveName) ? $"Save {slotIndex + 1}" : saveName,
                    saveTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
                    gameState = state
                };

                // JSON 직렬화
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = GetSaveFilePath(slotIndex);

                // 파일 저장
                File.WriteAllText(filePath, json);

                Debug.Log($"Game saved to slot {slotIndex}: {filePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 게임 로드
        /// </summary>
        public GameStateSnapshot LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            {
                Debug.LogError($"Invalid save slot: {slotIndex}");
                return null;
            }

            try
            {
                string filePath = GetSaveFilePath(slotIndex);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found: {filePath}");
                    return null;
                }

                // JSON 읽기 및 역직렬화
                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                Debug.Log($"Game loaded from slot {slotIndex}");
                return saveData.gameState;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 세이브 존재 여부 확인
        /// </summary>
        public bool HasSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
                return false;

            return File.Exists(GetSaveFilePath(slotIndex));
        }

        /// <summary>
        /// 세이브 삭제
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            {
                Debug.LogError($"Invalid save slot: {slotIndex}");
                return false;
            }

            try
            {
                string filePath = GetSaveFilePath(slotIndex);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Save deleted: slot {slotIndex}");
                    return true;
                }

                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Delete failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 세이브 정보 가져오기 (UI 표시용)
        /// </summary>
        public SaveInfo GetSaveInfo(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
                return null;

            try
            {
                string filePath = GetSaveFilePath(slotIndex);

                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                return new SaveInfo
                {
                    slotIndex = saveData.slotIndex,
                    saveName = saveData.saveName,
                    saveTimestamp = saveData.saveTimestamp,
                    currentChapter = saveData.gameState.currentChapter,
                    currentLineId = saveData.gameState.currentLineId
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to get save info: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 모든 세이브 정보 가져오기
        /// </summary>
        public List<SaveInfo> GetAllSaveInfos()
        {
            List<SaveInfo> infos = new List<SaveInfo>();

            for (int i = 0; i < MAX_SAVE_SLOTS; i++)
            {
                SaveInfo info = GetSaveInfo(i);
                if (info != null)
                {
                    infos.Add(info);
                }
            }

            return infos;
        }

        /// <summary>
        /// 빠른 저장 (슬롯 0)
        /// </summary>
        public bool QuickSave(GameStateSnapshot state)
        {
            return SaveGame(state, 0, "Quick Save");
        }

        /// <summary>
        /// 빠른 로드 (슬롯 0)
        /// </summary>
        public GameStateSnapshot QuickLoad()
        {
            return LoadGame(0);
        }

        /// <summary>
        /// 자동 저장 (슬롯 9)
        /// </summary>
        public bool AutoSave(GameStateSnapshot state)
        {
            return SaveGame(state, MAX_SAVE_SLOTS - 1, "Auto Save");
        }

        string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(SaveFolderPath, $"save_{slotIndex}.json");
        }
    }

    /// <summary>
    /// 세이브 데이터 (JSON 직렬화용)
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public int slotIndex;
        public string saveName;
        public long saveTimestamp;
        public GameStateSnapshot gameState;
    }

    /// <summary>
    /// 세이브 정보 (UI 표시용)
    /// </summary>
    [System.Serializable]
    public class SaveInfo
    {
        public int slotIndex;
        public string saveName;
        public long saveTimestamp;
        public int currentChapter;
        public int currentLineId;

        public string GetFormattedDate()
        {
            System.DateTimeOffset dateTime = System.DateTimeOffset.FromUnixTimeSeconds(saveTimestamp);
            return dateTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
        }

        public string GetChapterText()
        {
            return $"Chapter {currentChapter}";
        }
    }
}
