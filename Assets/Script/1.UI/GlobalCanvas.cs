using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

// 게임 전체에서 최우선순위를 갖는 최상위 캔버스입니다.
// 로딩, 페이드 인/아웃 등을 구현하기 위함
public class GlobalCanvas : MonoBehaviour
{
    public static GlobalCanvas Instance {get; private set;}
    public GameObject fadePanel;
    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
