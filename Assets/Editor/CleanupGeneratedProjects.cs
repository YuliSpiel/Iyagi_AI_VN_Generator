using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace IyagiAI.Editor
{
    /// <summary>
    /// 생성된 모든 프로젝트 데이터를 삭제하는 유틸리티
    /// </summary>
    public class CleanupGeneratedProjects
    {
        [MenuItem("Iyagi/Cleanup/Delete All Generated Projects", priority = 100)]
        public static void DeleteAllGeneratedProjects()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "모든 생성 프로젝트 삭제",
                "정말로 생성된 모든 프로젝트 데이터를 삭제하시겠습니까?\n\n" +
                "다음 항목들이 삭제됩니다:\n" +
                "- Resources/Projects/*.asset\n" +
                "- Resources/Generated/Characters/*\n" +
                "- Resources/Image/Background/*\n" +
                "- Resources/Sound/BGM/*\n" +
                "- Resources/Sound/SFX/*\n" +
                "- Resources/Image/CG/*\n" +
                "- 모든 저장 파일 (persistentDataPath)\n\n" +
                "이 작업은 되돌릴 수 없습니다!",
                "삭제",
                "취소"
            );

            if (!confirmed)
            {
                Debug.Log("[Cleanup] 사용자가 취소했습니다.");
                return;
            }

            int deletedCount = 0;

            // 1. Resources/Projects/*.asset 삭제
            string projectsPath = "Assets/Resources/Projects";
            if (Directory.Exists(projectsPath))
            {
                var assetFiles = Directory.GetFiles(projectsPath, "*.asset", SearchOption.TopDirectoryOnly);
                foreach (var file in assetFiles)
                {
                    AssetDatabase.DeleteAsset(file);
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted: {file}");
                }
            }

            // 2. Resources/Generated/Characters/* 폴더 삭제
            string generatedCharsPath = "Assets/Resources/Generated/Characters";
            if (Directory.Exists(generatedCharsPath))
            {
                var characterDirs = Directory.GetDirectories(generatedCharsPath);
                foreach (var dir in characterDirs)
                {
                    // bin 폴더는 건너뛰기
                    if (Path.GetFileName(dir) == "bin")
                        continue;

                    Directory.Delete(dir, true);
                    File.Delete(dir + ".meta");
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted folder: {dir}");
                }
            }

            // 3. Resources/Image/Background/* 파일 삭제
            string backgroundPath = "Assets/Resources/Image/Background";
            if (Directory.Exists(backgroundPath))
            {
                var bgFiles = Directory.GetFiles(backgroundPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".meta"));
                foreach (var file in bgFiles)
                {
                    File.Delete(file);
                    File.Delete(file + ".meta");
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted: {file}");
                }
            }

            // 4. Resources/Sound/BGM/* 파일 삭제
            string bgmPath = "Assets/Resources/Sound/BGM";
            if (Directory.Exists(bgmPath))
            {
                var bgmFiles = Directory.GetFiles(bgmPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".meta"));
                foreach (var file in bgmFiles)
                {
                    File.Delete(file);
                    File.Delete(file + ".meta");
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted: {file}");
                }
            }

            // 5. Resources/Sound/SFX/* 파일 삭제
            string sfxPath = "Assets/Resources/Sound/SFX";
            if (Directory.Exists(sfxPath))
            {
                var sfxFiles = Directory.GetFiles(sfxPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".meta"));
                foreach (var file in sfxFiles)
                {
                    File.Delete(file);
                    File.Delete(file + ".meta");
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted: {file}");
                }
            }

            // 6. Resources/Image/CG/* 파일 삭제
            string cgPath = "Assets/Resources/Image/CG";
            if (Directory.Exists(cgPath))
            {
                var cgFiles = Directory.GetFiles(cgPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".meta"));
                foreach (var file in cgFiles)
                {
                    File.Delete(file);
                    File.Delete(file + ".meta");
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted: {file}");
                }
            }

            // 7. persistentDataPath 저장 파일 삭제
            string savesPath = Path.Combine(Application.persistentDataPath, "Saves");
            if (Directory.Exists(savesPath))
            {
                Directory.Delete(savesPath, true);
                Debug.Log($"[Cleanup] Deleted save data folder: {savesPath}");
            }

            // 8. PlayerPrefs 초기화
            PlayerPrefs.DeleteKey("CurrentSaveFileId");
            PlayerPrefs.Save();
            Debug.Log("[Cleanup] Cleared PlayerPrefs CurrentSaveFileId");

            // Asset Database 갱신
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "삭제 완료",
                $"총 {deletedCount}개의 파일/폴더가 삭제되었습니다.\n\n" +
                "저장 파일 및 PlayerPrefs도 초기화되었습니다.",
                "확인"
            );

            Debug.Log($"[Cleanup] ✅ 삭제 완료: {deletedCount}개 항목");
        }

        [MenuItem("Iyagi/Cleanup/Delete Generated Characters Only", priority = 101)]
        public static void DeleteGeneratedCharactersOnly()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "생성된 캐릭터 삭제",
                "Resources/Generated/Characters 폴더의 모든 캐릭터 데이터를 삭제하시겠습니까?\n\n" +
                "이 작업은 되돌릴 수 없습니다!",
                "삭제",
                "취소"
            );

            if (!confirmed)
            {
                Debug.Log("[Cleanup] 사용자가 취소했습니다.");
                return;
            }

            int deletedCount = 0;
            string generatedCharsPath = "Assets/Resources/Generated/Characters";

            if (Directory.Exists(generatedCharsPath))
            {
                var characterDirs = Directory.GetDirectories(generatedCharsPath);
                foreach (var dir in characterDirs)
                {
                    if (Path.GetFileName(dir) == "bin")
                        continue;

                    Directory.Delete(dir, true);
                    File.Delete(dir + ".meta");
                    deletedCount++;
                    Debug.Log($"[Cleanup] Deleted folder: {dir}");
                }
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "삭제 완료",
                $"총 {deletedCount}개의 캐릭터 폴더가 삭제되었습니다.",
                "확인"
            );

            Debug.Log($"[Cleanup] ✅ 캐릭터 삭제 완료: {deletedCount}개");
        }

        [MenuItem("Iyagi/Cleanup/Delete Save Files Only", priority = 102)]
        public static void DeleteSaveFilesOnly()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "저장 파일 삭제",
                "모든 저장 파일 및 PlayerPrefs를 초기화하시겠습니까?\n\n" +
                "이 작업은 되돌릴 수 없습니다!",
                "삭제",
                "취소"
            );

            if (!confirmed)
            {
                Debug.Log("[Cleanup] 사용자가 취소했습니다.");
                return;
            }

            // persistentDataPath 저장 파일 삭제
            string savesPath = Path.Combine(Application.persistentDataPath, "Saves");
            if (Directory.Exists(savesPath))
            {
                Directory.Delete(savesPath, true);
                Debug.Log($"[Cleanup] Deleted save data folder: {savesPath}");
            }

            // PlayerPrefs 초기화
            PlayerPrefs.DeleteKey("CurrentSaveFileId");
            PlayerPrefs.Save();
            Debug.Log("[Cleanup] Cleared PlayerPrefs CurrentSaveFileId");

            EditorUtility.DisplayDialog(
                "삭제 완료",
                "모든 저장 파일 및 PlayerPrefs가 초기화되었습니다.",
                "확인"
            );

            Debug.Log($"[Cleanup] ✅ 저장 파일 삭제 완료");
        }
    }
}
