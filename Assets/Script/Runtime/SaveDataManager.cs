using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 세이브/로드 관리자 (싱글톤)
    /// - 프로젝트 슬롯 관리
    /// - 저장 파일 관리
    /// - CG 컬렉션 관리
    /// </summary>
    public class SaveDataManager : MonoBehaviour
    {
        // 싱글톤
        private static SaveDataManager _instance;
        public static SaveDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SaveDataManager");
                    _instance = go.AddComponent<SaveDataManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string SAVE_FOLDER = "Saves";
        private const int MAX_SAVE_SLOTS = 10;

        private string SaveFolderPath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        private string ProjectSlotsFile => Path.Combine(SaveFolderPath, "projects.json");

        private List<ProjectSlot> projectSlots = new List<ProjectSlot>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 저장 데이터 폴더 생성
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }

            LoadAllProjectSlots();
        }

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

        // ===== 프로젝트 슬롯 관리 (새로운 시스템) =====

        private void LoadAllProjectSlots()
        {
            Debug.Log($"[SaveDataManager] Loading project slots from: {ProjectSlotsFile}");

            if (!File.Exists(ProjectSlotsFile))
            {
                Debug.LogWarning($"[SaveDataManager] Project slots file not found: {ProjectSlotsFile}");
                projectSlots = new List<ProjectSlot>();
                return;
            }

            try
            {
                string json = File.ReadAllText(ProjectSlotsFile);
                Debug.Log($"[SaveDataManager] Read JSON ({json.Length} chars):");
                Debug.Log($"[SaveDataManager] JSON preview: {json.Substring(0, System.Math.Min(500, json.Length))}");

                var wrapper = JsonUtility.FromJson<ProjectSlotListWrapper>(json);

                if (wrapper == null)
                {
                    Debug.LogError("[SaveDataManager] Failed to parse JSON wrapper - wrapper is null");
                    projectSlots = new List<ProjectSlot>();
                    return;
                }

                if (wrapper.projects == null)
                {
                    Debug.LogError("[SaveDataManager] wrapper.projects is null");
                    projectSlots = new List<ProjectSlot>();
                    return;
                }

                projectSlots = wrapper.projects;
                Debug.Log($"[SaveDataManager] Successfully loaded {projectSlots.Count} project slots");

                foreach (var slot in projectSlots)
                {
                    Debug.Log($"[SaveDataManager] - Project: {slot.projectName} (GUID: {slot.projectGuid})");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveDataManager] Failed to load project slots: {e.Message}");
                Debug.LogError($"[SaveDataManager] Stack trace: {e.StackTrace}");
                projectSlots = new List<ProjectSlot>();
            }
        }

        private void SaveAllProjectSlots()
        {
            try
            {
                var wrapper = new ProjectSlotListWrapper { projects = projectSlots };
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(ProjectSlotsFile, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save project slots: {e.Message}");
            }
        }

        public List<ProjectSlot> GetAllProjectSlots()
        {
            return projectSlots.OrderByDescending(p => p.lastPlayedDate).ToList();
        }

        public ProjectSlot GetProjectSlot(string projectGuid)
        {
            return projectSlots.FirstOrDefault(p => p.projectGuid == projectGuid);
        }

        public ProjectSlot CreateProjectSlot(VNProjectData projectData)
        {
            var slot = new ProjectSlot
            {
                projectGuid = projectData.projectGuid,
                projectName = projectData.gameTitle,
                totalChapters = projectData.totalChapters,
                createdDate = DateTime.Now,
                lastPlayedDate = DateTime.Now,
                saveFiles = new List<SaveFile>(),
                unlockedCGs = new List<string>()
            };

            projectSlots.Add(slot);
            SaveAllProjectSlots();

            return slot;
        }

        public void DeleteProjectSlot(string projectGuid)
        {
            var slot = GetProjectSlot(projectGuid);
            if (slot != null)
            {
                foreach (var saveFile in slot.saveFiles.ToList())
                {
                    DeleteSaveFile(saveFile.saveFileId);
                }

                projectSlots.Remove(slot);
                SaveAllProjectSlots();
            }
        }

        // ===== 저장 파일 관리 (새로운 시스템) =====

        public SaveFile GetLastPlayedSaveFile()
        {
            SaveFile lastSaveFile = null;
            DateTime latestTime = DateTime.MinValue;

            foreach (var project in projectSlots)
            {
                foreach (var save in project.saveFiles)
                {
                    if (save.lastPlayedDate > latestTime)
                    {
                        latestTime = save.lastPlayedDate;
                        lastSaveFile = save;
                    }
                }
            }

            return lastSaveFile;
        }

        public SaveFile CreateNewSaveFile(string projectGuid, int startChapter)
        {
            var project = GetProjectSlot(projectGuid);
            if (project == null)
            {
                Debug.LogError($"Project not found: {projectGuid}");
                return null;
            }

            int slotNumber = project.saveFiles.Count + 1;

            var saveFile = new SaveFile
            {
                saveFileId = Guid.NewGuid().ToString(),
                projectGuid = projectGuid,
                slotNumber = slotNumber,
                currentChapter = startChapter,
                totalChapters = project.totalChapters,
                createdDate = DateTime.Now,
                lastPlayedDate = DateTime.Now,
                totalPlaytimeSeconds = 0,
                gameState = new GameState()
            };

            project.saveFiles.Add(saveFile);
            project.lastPlayedDate = DateTime.Now;
            SaveAllProjectSlots();

            return saveFile;
        }

        public void LoadSaveFile(string saveFileId)
        {
            PlayerPrefs.SetString("CurrentSaveFileId", saveFileId);
            PlayerPrefs.Save();
        }

        public void DeleteSaveFile(string saveFileId)
        {
            foreach (var project in projectSlots)
            {
                var saveFile = project.saveFiles.FirstOrDefault(s => s.saveFileId == saveFileId);
                if (saveFile != null)
                {
                    project.saveFiles.Remove(saveFile);
                    SaveAllProjectSlots();
                    return;
                }
            }
        }

        // ===== CG 컬렉션 관리 =====

        public List<CGMetadata> GetCGCollection(string projectGuid)
        {
            var project = GetProjectSlot(projectGuid);
            if (project == null)
                return new List<CGMetadata>();

            var projectData = Resources.LoadAll<VNProjectData>("VNProjects")
                .FirstOrDefault(p => p.projectGuid == projectGuid);

            if (projectData == null)
                return new List<CGMetadata>();

            foreach (var cg in projectData.allCGs)
            {
                cg.isUnlocked = project.unlockedCGs.Contains(cg.cgId.ToString());
            }

            return projectData.allCGs;
        }

        public void UnlockCG(string projectGuid, int cgId)
        {
            var project = GetProjectSlot(projectGuid);
            if (project != null)
            {
                string cgIdStr = cgId.ToString();
                if (!project.unlockedCGs.Contains(cgIdStr))
                {
                    project.unlockedCGs.Add(cgIdStr);
                    SaveAllProjectSlots();
                }
            }
        }
    }

    // ===== 새로운 데이터 클래스 =====

    [Serializable]
    public class ProjectSlotListWrapper
    {
        public List<ProjectSlot> projects;
    }

    [Serializable]
    public class ProjectSlot
    {
        public string projectGuid;
        public string projectName;
        public int totalChapters;
        public DateTime createdDate;
        public DateTime lastPlayedDate;
        public List<SaveFile> saveFiles;
        public List<string> unlockedCGs;
    }

    [Serializable]
    public class SaveFile
    {
        public string saveFileId;
        public string projectGuid;
        public int slotNumber;
        public int currentChapter;
        public int totalChapters;
        public DateTime createdDate;
        public DateTime lastPlayedDate;
        public int totalPlaytimeSeconds;
        public GameState gameState;
    }

    [Serializable]
    public class GameState
    {
        public Dictionary<string, int> coreValueScores = new Dictionary<string, int>();
        public Dictionary<string, int> skillScores = new Dictionary<string, int>();
        public Dictionary<string, int> npcAffections = new Dictionary<string, int>();
        public List<string> previousChoices = new List<string>();
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
