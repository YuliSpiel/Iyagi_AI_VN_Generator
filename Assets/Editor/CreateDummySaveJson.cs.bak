using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class CreateDummySaveJson
{
    public static int fileCount = 70;

    [MenuItem("Tools/Save System/Create Dummy save.json")]
    public static void CreateDummySaveFile()
    {
        SaveData data = new SaveData();
        data.Blocks = new List<SaveDataBlock>();

        for (int i = 0; i < fileCount; i++)
        {
            SaveDataBlock block = new SaveDataBlock();
            block.Stats = new PlayerStats
            {
                Obedience = Random.Range(1, 9),
                Will = Random.Range(1, 9),
            };

            block.Chapter = new ChapterProgress
            {
                CurChapter = Random.Range(1, 13),
                CurIndex = i
            };

            block.SaveTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
            data.Blocks.Add(block);
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "iyagi_save.json");
        File.WriteAllText(path, json);

        Debug.Log($"Dummy save.json created at: {path}");
        EditorUtility.RevealInFinder(path);
    }
}