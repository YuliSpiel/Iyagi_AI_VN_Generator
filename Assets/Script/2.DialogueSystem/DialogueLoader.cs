using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

/// <summary>테스트 단계에서만 사용: 구글 시트 CSV 다운로드.</summary>
public class DialogueLoader : MonoBehaviour
{
    [Tooltip("https://.../export?format=csv&gid=...")]
    public string csvUrl;

    public IEnumerator LoadCSV(Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(csvUrl))
        {
            Debug.LogError("[DialogueLoader] csvUrl empty");
            onComplete?.Invoke(null);
            yield break;
        }

        using (var req = UnityWebRequest.Get(csvUrl))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[DialogueLoader] download failed: " + req.error);
                onComplete?.Invoke(null);
            }
            else
            {
                onComplete?.Invoke(req.downloadHandler.text);
            }
        }
    }
}