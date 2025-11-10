using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace IyagiAI.Editor
{
    /// <summary>
    /// ProjectSlotItem 프리팹 생성 에디터 스크립트
    /// </summary>
    public class CreateProjectSlotUI
    {
        [MenuItem("Iyagi/Create ProjectSlot UI Prefab")]
        public static void CreatePrefab()
        {
            // 1. 루트 GameObject 생성
            GameObject projectItem = new GameObject("ProjectItemPrefab");
            projectItem.AddComponent<RectTransform>();

            // ProjectSlotItem 스크립트 추가
            var slotItem = projectItem.AddComponent<IyagiAI.TitleScene.ProjectSlotItem>();

            // 레이아웃 설정
            var itemRect = projectItem.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(800, 120);

            // 2. Background Image 추가
            var bgImage = projectItem.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 3. Project Name Text
            GameObject nameObj = new GameObject("ProjectNameText");
            nameObj.transform.SetParent(projectItem.transform, false);
            var nameText = nameObj.AddComponent<TMP_Text>();
            nameText.text = "프로젝트 이름";
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.6f);
            nameRect.anchorMax = new Vector2(0.6f, 1);
            nameRect.offsetMin = new Vector2(20, 10);
            nameRect.offsetMax = new Vector2(-10, -10);

            // 4. Last Played Text
            GameObject lastPlayedObj = new GameObject("LastPlayedText");
            lastPlayedObj.transform.SetParent(projectItem.transform, false);
            var lastPlayedText = lastPlayedObj.AddComponent<TMP_Text>();
            lastPlayedText.text = "최근 플레이: 2025-01-10";
            lastPlayedText.fontSize = 14;
            lastPlayedText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            var lastPlayedRect = lastPlayedObj.GetComponent<RectTransform>();
            lastPlayedRect.anchorMin = new Vector2(0, 0.3f);
            lastPlayedRect.anchorMax = new Vector2(0.6f, 0.6f);
            lastPlayedRect.offsetMin = new Vector2(20, 0);
            lastPlayedRect.offsetMax = new Vector2(-10, 0);

            // 5. Save Count Text
            GameObject saveCountObj = new GameObject("SaveCountText");
            saveCountObj.transform.SetParent(projectItem.transform, false);
            var saveCountText = saveCountObj.AddComponent<TMP_Text>();
            saveCountText.text = "1개의 저장 파일";
            saveCountText.fontSize = 14;
            saveCountText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            var saveCountRect = saveCountObj.GetComponent<RectTransform>();
            saveCountRect.anchorMin = new Vector2(0, 0);
            saveCountRect.anchorMax = new Vector2(0.3f, 0.3f);
            saveCountRect.offsetMin = new Vector2(20, 10);
            saveCountRect.offsetMax = new Vector2(0, 0);

            // 6. Chapter Info Text
            GameObject chapterInfoObj = new GameObject("ChapterInfoText");
            chapterInfoObj.transform.SetParent(projectItem.transform, false);
            var chapterInfoText = chapterInfoObj.AddComponent<TMP_Text>();
            chapterInfoText.text = "3개 챕터";
            chapterInfoText.fontSize = 14;
            chapterInfoText.color = new Color(0.7f, 0.7f, 0.7f, 1);
            var chapterInfoRect = chapterInfoObj.GetComponent<RectTransform>();
            chapterInfoRect.anchorMin = new Vector2(0.3f, 0);
            chapterInfoRect.anchorMax = new Vector2(0.6f, 0.3f);
            chapterInfoRect.offsetMin = new Vector2(10, 10);
            chapterInfoRect.offsetMax = new Vector2(0, 0);

            // 7. Select Button
            GameObject selectBtnObj = new GameObject("SelectButton");
            selectBtnObj.transform.SetParent(projectItem.transform, false);
            var selectBtn = selectBtnObj.AddComponent<Button>();
            var selectBtnImage = selectBtnObj.AddComponent<Image>();
            selectBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 1);
            var selectBtnRect = selectBtnObj.GetComponent<RectTransform>();
            selectBtnRect.anchorMin = new Vector2(0.62f, 0.5f);
            selectBtnRect.anchorMax = new Vector2(0.78f, 0.9f);
            selectBtnRect.offsetMin = Vector2.zero;
            selectBtnRect.offsetMax = Vector2.zero;

            GameObject selectBtnTextObj = new GameObject("Text");
            selectBtnTextObj.transform.SetParent(selectBtnObj.transform, false);
            var selectBtnText = selectBtnTextObj.AddComponent<TMP_Text>();
            selectBtnText.text = "선택";
            selectBtnText.fontSize = 18;
            selectBtnText.alignment = TextAlignmentOptions.Center;
            selectBtnText.color = Color.white;
            var selectBtnTextRect = selectBtnTextObj.GetComponent<RectTransform>();
            selectBtnTextRect.anchorMin = Vector2.zero;
            selectBtnTextRect.anchorMax = Vector2.one;
            selectBtnTextRect.offsetMin = Vector2.zero;
            selectBtnTextRect.offsetMax = Vector2.zero;

            // 8. CG Gallery Button
            GameObject cgBtnObj = new GameObject("CGGalleryButton");
            cgBtnObj.transform.SetParent(projectItem.transform, false);
            var cgBtn = cgBtnObj.AddComponent<Button>();
            var cgBtnImage = cgBtnObj.AddComponent<Image>();
            cgBtnImage.color = new Color(0.2f, 0.4f, 0.6f, 1);
            var cgBtnRect = cgBtnObj.GetComponent<RectTransform>();
            cgBtnRect.anchorMin = new Vector2(0.8f, 0.5f);
            cgBtnRect.anchorMax = new Vector2(0.96f, 0.9f);
            cgBtnRect.offsetMin = Vector2.zero;
            cgBtnRect.offsetMax = Vector2.zero;

            GameObject cgBtnTextObj = new GameObject("Text");
            cgBtnTextObj.transform.SetParent(cgBtnObj.transform, false);
            var cgBtnText = cgBtnTextObj.AddComponent<TMP_Text>();
            cgBtnText.text = "CG";
            cgBtnText.fontSize = 18;
            cgBtnText.alignment = TextAlignmentOptions.Center;
            cgBtnText.color = Color.white;
            var cgBtnTextRect = cgBtnTextObj.GetComponent<RectTransform>();
            cgBtnTextRect.anchorMin = Vector2.zero;
            cgBtnTextRect.anchorMax = Vector2.one;
            cgBtnTextRect.offsetMin = Vector2.zero;
            cgBtnTextRect.offsetMax = Vector2.zero;

            // 9. Delete Button
            GameObject delBtnObj = new GameObject("DeleteButton");
            delBtnObj.transform.SetParent(projectItem.transform, false);
            var delBtn = delBtnObj.AddComponent<Button>();
            var delBtnImage = delBtnObj.AddComponent<Image>();
            delBtnImage.color = new Color(0.6f, 0.2f, 0.2f, 1);
            var delBtnRect = delBtnObj.GetComponent<RectTransform>();
            delBtnRect.anchorMin = new Vector2(0.62f, 0.1f);
            delBtnRect.anchorMax = new Vector2(0.96f, 0.4f);
            delBtnRect.offsetMin = Vector2.zero;
            delBtnRect.offsetMax = Vector2.zero;

            GameObject delBtnTextObj = new GameObject("Text");
            delBtnTextObj.transform.SetParent(delBtnObj.transform, false);
            var delBtnText = delBtnTextObj.AddComponent<TMP_Text>();
            delBtnText.text = "삭제";
            delBtnText.fontSize = 18;
            delBtnText.alignment = TextAlignmentOptions.Center;
            delBtnText.color = Color.white;
            var delBtnTextRect = delBtnTextObj.GetComponent<RectTransform>();
            delBtnTextRect.anchorMin = Vector2.zero;
            delBtnTextRect.anchorMax = Vector2.one;
            delBtnTextRect.offsetMin = Vector2.zero;
            delBtnTextRect.offsetMax = Vector2.zero;

            // 10. 스크립트에 참조 연결
            slotItem.projectNameText = nameText;
            slotItem.lastPlayedText = lastPlayedText;
            slotItem.saveCountText = saveCountText;
            slotItem.chapterInfoText = chapterInfoText;
            slotItem.selectButton = selectBtn;
            slotItem.cgGalleryButton = cgBtn;
            slotItem.deleteButton = delBtn;

            // 11. 프리팹으로 저장
            string prefabPath = "Assets/Prefabs/UI/ProjectItemPrefab.prefab";
            string directory = System.IO.Path.GetDirectoryName(prefabPath);

            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            PrefabUtility.SaveAsPrefabAsset(projectItem, prefabPath);
            GameObject.DestroyImmediate(projectItem);

            AssetDatabase.Refresh();

            Debug.Log($"✅ ProjectItemPrefab created at: {prefabPath}");
            Debug.Log("Inspector에서 ProjectSelectPanel의 'Project Item Prefab' 필드에 이 프리팹을 연결하세요.");
        }
    }
}
