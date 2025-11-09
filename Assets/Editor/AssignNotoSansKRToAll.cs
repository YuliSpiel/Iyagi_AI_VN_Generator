using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;

public class AssignNotoSansKRToAll : MonoBehaviour
{
    [MenuItem("Iyagi/Assign NotoSansKR to All TMP Texts")]
    public static void AssignFontToAllTexts()
    {
        // NotoSansKR 폰트 에셋 로드
        string fontPath = "Assets/TextMesh Pro/Fonts/NotoSansKR";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath + ".asset");

        if (fontAsset == null)
        {
            Debug.LogError($"Font asset not found at {fontPath}.asset");
            return;
        }

        // 폰트를 Dynamic 모드로 설정
        if (fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            Debug.Log("✅ Set font atlas to Dynamic mode for Korean characters");
        }

        Debug.Log($"=== Assigning NotoSansKR font to all TMP texts in current scene ===");

        // 현재 씬의 모든 TMP_Text 컴포넌트 찾기
        TMP_Text[] allTexts = Object.FindObjectsOfType<TMP_Text>(true); // includeInactive = true

        int assignedCount = 0;
        foreach (var text in allTexts)
        {
            text.font = fontAsset;
            EditorUtility.SetDirty(text);
            assignedCount++;
            Debug.Log($"Assigned font to: {text.gameObject.name}");
        }

        // 씬 저장
        Scene currentScene = SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);

        Debug.Log($"✅ Font assigned to {assignedCount} TMP_Text components");
        Debug.Log($"Scene saved: {currentScene.name}");
    }
}
