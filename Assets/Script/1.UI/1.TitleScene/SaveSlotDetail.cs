using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveSlotDetail : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI statText;
    public TextMeshProUGUI sanityText;
    
    public Button slotButton;

    private int slotIndex;  // 이 슬롯의 번호

    public Image emptySlotImage;

    public RectTransform slotImage;
    public RectTransform slotDetail;
    
    private Vector2 imageLeftPos; 
    private Vector2 imageRightPos;
    private Vector2 detailLeftPos;
    private Vector2 detailRightPos;
    void Awake()
    {
        gameObject.SetActive(false);
        imageLeftPos = slotImage.anchoredPosition;
        imageRightPos = new Vector2(slotImage.anchoredPosition.x + 430, slotImage.anchoredPosition.y);
        detailLeftPos = new Vector2(slotDetail.anchoredPosition.x - 345, slotDetail.anchoredPosition.y);
        detailRightPos = slotDetail.anchoredPosition;
    }

    public void SetData(int index, SaveDataBlock data)
    {
        // if (data==null)
        // {
        //     SetEmpty(index);
        //     return;
        // }

        slotIndex = index;

        image.sprite = DataManager.Instance.ChapterImgs[data.Chapter.CurChapter];
        chapterText.text = $"CH{data.Chapter.CurChapter:D2}. " + DataManager.Instance.ChapterNames[data.Chapter.CurChapter];  // 인덱스로 챕터 표시
        saveTimeText.text = DateTime.Parse(data.SaveTime).ToString("yyyy.MM.dd HH:mm");
        statText.text = $"Obedience: {data.Stats.Obedience} Will: {data.Stats.Will}\n";
        
        // 슬롯 눌렀을 때 실행할 동작 연결
        slotButton.onClick.RemoveAllListeners();  // 중복 방지
        slotButton.onClick.AddListener(OnSlotClicked);
    }
    
    public void SetEmpty()
    {
        chapterText.text = $"EMPTY";
        saveTimeText.text = "0000.00.00";
    }
    
    public void SetEmpty(int index)
    {
        slotIndex = index;
        image.sprite = emptySlotImage.sprite;
        chapterText.text = $"EMPTY";
        saveTimeText.text = "0000.00.00";
        statText.text = "염력: 0 지능: 0\n직감: 0 신체: 0\n인성: 0  행운: 0";
        sanityText.text = "00";
        
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() =>
        {
            Debug.Log($"슬롯 {index}에 새 저장 시작");
            // 빈 슬롯이지만 선택 시 GameManager 초기화 or 새 데이터 생성 등 가능
        });
    }

    private void OnSlotClicked()
    {
        DataManager.Instance.LoadSlot(slotIndex);
        // 이후 씬 전환 or DialogueManager 초기화 등
        Debug.Log($"슬롯 {slotIndex} 로드 완료");
    }

    public void SetLeft() // 이미지가 왼쪽
    {
        slotImage.anchoredPosition = imageLeftPos;
        slotDetail.anchoredPosition = detailRightPos;
    }

    public void SetRight() // 이미지가 오른쪽
    {
        slotImage.anchoredPosition = imageRightPos;
        slotDetail.anchoredPosition = detailLeftPos;
    }
}
