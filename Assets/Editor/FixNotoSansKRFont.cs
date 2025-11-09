using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class FixNotoSansKRFont : MonoBehaviour
{
    [MenuItem("Tools/Fix NotoSansKR Font Asset")]
    public static void RegenerateFontAsset()
    {
        string sourceFontPath = "Assets/TextMesh Pro/Fonts/NotoSansCJKkr-VF.otf";
        string outputPath = "Assets/TextMesh Pro/Fonts/NotoSansKR.asset";

        // 소스 폰트 로드
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"Source font not found at {sourceFontPath}");
            return;
        }

        Debug.Log("=== Regenerating NotoSansKR Font Asset ===");

        // 기존 Font Asset 삭제
        if (File.Exists(outputPath))
        {
            AssetDatabase.DeleteAsset(outputPath);
            Debug.Log("Deleted old font asset");
        }

        // 새 Font Asset 생성
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
        fontAsset.name = "NotoSansKR";

        // Atlas 설정
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

        // 저장
        AssetDatabase.CreateAsset(fontAsset, outputPath);

        // Material도 함께 저장
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "NotoSansKR Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Font Asset regenerated successfully at {outputPath}");
        Debug.Log($"Material: {fontAsset.material != null}");
        Debug.Log($"Atlas Texture: {fontAsset.atlasTexture != null}");

        Debug.Log("\n=== Next Steps ===");
        Debug.Log("1. Select the font asset at: Assets/TextMesh Pro/Fonts/NotoSansKR.asset");
        Debug.Log("2. Check that Material and Atlas Texture are assigned in the Inspector");
        Debug.Log("3. If needed, set as default font in: Edit > Project Settings > TextMesh Pro > Settings");
    }
}
