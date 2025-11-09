using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DummyAssetReference : MonoBehaviour
{
    [Header("스프라이트 이미지들")]
    public List<Sprite> sprites;

    [Header("오디오클립")]
    public List<AudioClip> chapterBGMs;

    [Header("대사 파일")]
    public List<TextAsset> dialogueFiles;

    void Awake()
    {
        Sprite[] loaded =  Resources.LoadAll<Sprite>(""); 
        Debug.Log(loaded.Length);
        sprites.AddRange(loaded); // foreach문과 정확히 같은 기능
    }
}
