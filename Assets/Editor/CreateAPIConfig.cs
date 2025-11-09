using UnityEngine;
using UnityEditor;
using IyagiAI.AISystem;

/// <summary>
/// APIConfig ScriptableObject 생성 헬퍼
/// </summary>
public class CreateAPIConfig : EditorWindow
{
    [MenuItem("Iyagi/Create API Config")]
    public static void CreateConfig()
    {
        // Resources 폴더 확인
        string resourcesPath = "Assets/Resources";
        if (!System.IO.Directory.Exists(resourcesPath))
        {
            System.IO.Directory.CreateDirectory(resourcesPath);
            AssetDatabase.Refresh();
        }

        // APIConfig 생성
        string assetPath = $"{resourcesPath}/APIConfig.asset";

        // 이미 존재하는지 확인
        APIConfigData existingConfig = AssetDatabase.LoadAssetAtPath<APIConfigData>(assetPath);
        if (existingConfig != null)
        {
            Debug.LogWarning("APIConfig.asset already exists at " + assetPath);
            Selection.activeObject = existingConfig;
            EditorGUIUtility.PingObject(existingConfig);
            return;
        }

        // 새로 생성
        APIConfigData config = ScriptableObject.CreateInstance<APIConfigData>();
        config.geminiApiKey = "";
        config.elevenLabsApiKey = "";

        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"APIConfig.asset created at {assetPath}");
        Debug.Log("Please set your API keys in the Inspector!");

        // Inspector에서 선택
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }
}
