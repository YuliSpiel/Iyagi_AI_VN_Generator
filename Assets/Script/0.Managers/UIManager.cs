using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 일단은 UI 이펙트(페이드인, 페이드아웃) 관련 매니저
public class UIManager : MonoBehaviour
{
    public static UIManager Instance{get; private set;}
    private GameObject fadePanel;
    
    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Start()
    {
        fadePanel = GlobalCanvas.Instance.fadePanel;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "02_GameScene")
        {
            StartCoroutine(GlobalFadeIn(1f));
        }
    }

    public void GlobalFI(float dur)
    {
        StartCoroutine(GlobalFadeIn(dur));
    }
    
    public void GlobalFI(float fadeDur, float waitTime)
    {
        StartCoroutine(GlobalFadeIn(fadeDur, waitTime));
    }

    public void GlobalFO(float dur)
    {
        StartCoroutine(GlobalFadeOut(dur));
    }
    
    public void GlobalFO(float fadeDur, float waitTime)
    {
        StartCoroutine(GlobalFadeOut(fadeDur, waitTime));
    }

    public IEnumerator WaitForSec(float dur)
    {
        yield return new WaitForSeconds(dur);
    }

    // 화면 전체 페이드 아웃
    public IEnumerator GlobalFadeOut(float duration)
    {
        yield return null; // 씬 전환시 참조 유실 방지를 위함
        yield return FadeInCoroutine(fadePanel, duration, true, true);
    }
    
    public IEnumerator GlobalFadeOut(float duration, float waitTime)
    {
        yield return null; // 씬 전환시 참조 유실 방지를 위함
        yield return new WaitForSeconds(waitTime);
        yield return FadeOutCoroutine(fadePanel, duration, true, true);
    }
    
    // 화면 전체 페이드 인
    public IEnumerator GlobalFadeIn(float duration)
    {
        yield return null; // 씬 전환시 참조 유실 방지를 위함
        yield return FadeOutCoroutine(fadePanel, duration, true, true);
    }

    public IEnumerator GlobalFadeIn(float duration, float waitTime)
    {
        yield return null; // 씬 전환시 참조 유실 방지를 위함
        yield return new WaitForSeconds(waitTime);
        yield return FadeInCoroutine(fadePanel, duration, true, true);
    }

    /// <summary>
    ///  화면 전체 페이드아웃 -> 페이드인 효과
    /// </summary>
    /// <param name="fadePanel">페이드인/아웃 적용할 대상(이미지 컴포넌트를 가진 GameObject)</param>
    /// <param name="newPanel">페이드인/아웃 사이에 활성화할 대상</param>
    /// <param name="FOduration">페이드아웃 시간</param>
    /// <param name="FIDuration">페이드인 시간</param>
    public IEnumerator FadeAndOpenPanel(GameObject newPanel, float FOduration, float FIduration)
    {
        yield return FadeInCoroutine(fadePanel, FOduration, true, false);
        newPanel.SetActive(true);
        yield return FadeOutCoroutine(fadePanel, FIduration, true, true);
    }
    
    /// <summary>
    ///  타겟패널이 점점 드러남. 즉, 패널의 알파값이 0->1로 변함
    /// </summary>
    /// <param name="target">대상 패널. 이미지 컴포넌트가 있는 GameObject</param>
    /// <param name="duration">페이드인 소요시간</param>
    /// <param name="lockDuring">페이드인 도중 클릭을 막을건지 여부</param>
    /// <param name="unlockAfter">페이드인 완료 후 클릭 잠금을 해제할 것인지 여부</param>
    /// <returns></returns>
    public IEnumerator FadeInCoroutine(GameObject target, float duration, bool lockDuring, bool unlockAfter)
    {
        float curTime = 0;
        Image img = target.GetComponent<Image>();
        Color color = img.color;
        color.a = 0;
        img.color = color;
        if (lockDuring)
        {
            img.raycastTarget = true;
        }
        
        while (curTime < duration)
        {
            color.a = Mathf.Lerp(0, 1, curTime / duration);
            img.color = color;
            curTime += Time.deltaTime;
            yield return null;
        }
        color.a = 1;
        img.color = color;
        if (lockDuring && unlockAfter)
        {
            img.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// 타겟패널이 점점 사라짐. 즉, 패널의 알파값이 1->0로 변함
    /// </summary>
    /// <param name="target">대상 패널. 이미지 컴포넌트가 있는 GameObject</param>
    /// <param name="duration">페이드아웃 소요시간</param>
    /// <param name="lockDuring">페이드인아웃 도중 클릭을 막을건지 여부</param>
    /// <param name="unlockAfter">페이드아웃 완료 후 클릭 잠금을 해제할 것인지 여부</param>
    /// <returns></returns>
    public IEnumerator FadeOutCoroutine(GameObject target, float duration, bool lockDuring, bool unlockAfter)
    {
        float curTime = 0;
        Image img = target.GetComponent<Image>();
        Color color = img.color;
        color.a = 1;
        img.color = color;
        if (lockDuring)
        {
            img.raycastTarget = true;
        }
        
        while (curTime < duration)
        {
            color.a = Mathf.Lerp(1, 0, curTime / duration);
            img.color = color;
            curTime += Time.deltaTime;
            yield return null;
        }
        color.a = 0;
        img.color = color;
        
        if (lockDuring && unlockAfter)
        {
            img.raycastTarget = false;
        }
    }
}
