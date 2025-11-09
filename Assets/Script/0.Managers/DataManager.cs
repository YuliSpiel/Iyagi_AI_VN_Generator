using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private string SavePath => Application.persistentDataPath + "/iyagi_save.json";

    public SaveData SaveData = new SaveData();
    public int CurrentSlotIndex = 0;
    
    //챕터 번호에 따른 타이틀, 썸네일 매핑
    public Dictionary<int, string> ChapterNames = new Dictionary<int, string>(12)
    {
        { 1, "빨간 딸기" },
        { 2, "노란 꽃" },
        { 3, "주황 당근" },
        { 4, "보라 하트" },
        { 5, "파란 고래" },
        { 6, "초록 개구리"},
        { 7, "빨간 사과" },
        { 8, "노란 병아리" },
        { 9, "보라 버섯" },
        { 10, "주황 사탕" },
        { 11, "초록 잎" },
        { 12, "노란 별"},
    };
     
    public Dictionary<int, Sprite> ChapterImgs = new Dictionary<int, Sprite>(12);
    public List<Sprite> thumbnails;
    
    // public ChapterThumbnails thumbnails; // 썸네일 SO

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(ImgsInit()); // Initialize chapter images
        }
        else Destroy(gameObject);
    }

    // 마지막 세이브 슬롯만 로드
    public SaveDataBlock LoadLastSlot()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            SaveData tempData = JsonUtility.FromJson<SaveData>(json);
            if (tempData != null && tempData.Blocks.Count > 0)
            {
                CurrentSlotIndex = tempData.Blocks.Count - 1;
                return tempData.Blocks[CurrentSlotIndex];
            }
        }
        return null;
    }

    // UI를 위해 모든 세이브 데이터 로드
    public IEnumerator LoadAllForUI()
    {
        yield return null;
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            SaveData = JsonUtility.FromJson<SaveData>(json);
            if (SaveData == null)
            {
                Debug.Log("fromJSON의 결과가 null");
            }
        }
        else
        {
            Debug.LogWarning("세이브파일이 없음");
        }
    }

    public void SaveAll()
    {
        string json = JsonUtility.ToJson(SaveData, true);
        File.WriteAllText(SavePath, json);
    }

    public void LoadSlot(int slotIndex)
    {
        CurrentSlotIndex = slotIndex;

        // 범위 검사
        if (slotIndex >= 0 && slotIndex < SaveData.Blocks.Count)
        {
            GameManager.Instance.Initialize(SaveData.Blocks[slotIndex]);
        }
        else
        {
            Debug.LogWarning("존재하지 않는 슬롯입니다.");
        }
    }

    public void SaveCurrentSlot()
    {
        if (CurrentSlotIndex >= 0 && CurrentSlotIndex < SaveData.Blocks.Count)
        {
            SaveData.Blocks[CurrentSlotIndex] = GameManager.Instance.ToSaveDataBlock();
            SaveAll();
        }
    }

    public void CreateNewSlot()
    {
        SaveData.Blocks.Add(new SaveDataBlock());
        SaveAll();
    }

    public void DeleteSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < SaveData.Blocks.Count)
        {
            SaveData.Blocks.RemoveAt(slotIndex);
            SaveAll();
        }
    }

    public IEnumerator ImgsInit()
    {
        // Debug.Log($"Starting ImgsInit. Thumbnails count: {thumbnails?.Count ?? 0}");
        
        if (thumbnails == null || thumbnails.Count == 0)
        {
            Debug.LogError("Thumbnails list is null or empty!");
            yield break;
        }

        for (int i = 0; i < thumbnails.Count; i++)
        {
            if (thumbnails[i] == null)
            {
                Debug.LogError($"Thumbnail at index {i} is null!");
                continue;
            }
            ChapterImgs[i + 1] = thumbnails[i];
            // Debug.Log($"Loaded thumbnail {i + 1}: {thumbnails[i].name}");
        }
        
        // Debug.Log($"Finished ImgsInit. ChapterImgs count: {ChapterImgs.Count}");
        yield break;
    }
}