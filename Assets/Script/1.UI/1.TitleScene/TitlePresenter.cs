using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// MVP 패턴의 Presenter에 해당
public class TitlePresenter : MonoBehaviour
{
    [SerializeField] private TitleCanvas view;

    private void Start()
    {
        if (view == null)
        {
            Debug.LogError("TitleCanvas(View)가 연결되지 않았습니다.");
            return;
        }

        // 버튼 이벤트 구독
        view.loadButton.onClick.AddListener(OnLoadButtonClicked);
        view.aimodeButton.onClick.AddListener(OnAIModeButtonClicked);
        view.settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        view.exitButton.onClick.AddListener(OnExitButtonClicked);

        // Start 버튼은 새로운 씬으로 넘어가는 등 다른 역할일 수 있음
        view.startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnLoadButtonClicked()
    {
        StartCoroutine(LoadAndShowSlots());
    }

    // 글로벌 페이드아웃
    // 슬롯 파일을 로드하고
    // 완료시 글로벌 페이드인
    // ... 으로 하면 이상적이겠으나 일단은 굳이인 것 같다
    private IEnumerator LoadAndShowSlots()
    {
        yield return UIManager.Instance.GlobalFadeOut(1f);
        
        // 모든 세이브 파일 로드
        yield return StartCoroutine(DataManager.Instance.LoadAllForUI());
        
        view.OpenSaveSlotsPanel();
        
        yield return UIManager.Instance.GlobalFadeIn(1f);
        
        // 슬롯 패널 표시
        // yield return StartCoroutine(UIManager.Instance.FadeAndOpenPanel(view.saveSlotsPanel, 1f, 1f));
    }

    private void OnStartButtonClicked()
    {
        StartCoroutine(ClickRoutine());
    }

    private IEnumerator ClickRoutine()
    {
        // 마지막 세이브 슬롯 로드
        SaveDataBlock lastSave = DataManager.Instance.LoadLastSlot();
        if (lastSave != null)
        {
            GameManager.Instance.Initialize(lastSave);
        }
        yield return UIManager.Instance.GlobalFadeOut(1f);
        SceneManager.LoadScene(1);
    }

    private void OnAIModeButtonClicked()
    {
        SceneManager.LoadScene("LLMGameScene");
    }

    private void OnSettingsButtonClicked()
    {
        view.OpenOnlyPanel(view.settingsPanel);
    }
    
    private void OnExitButtonClicked()
    {
        GameManager.Instance.ExitGame();
    }
}

