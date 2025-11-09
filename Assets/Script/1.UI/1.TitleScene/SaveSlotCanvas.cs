using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveSlotCanvas : SlotCanvasBase
{
    public SaveSlotDetail slotInfo;
    private RectTransform _slotInfoPos;

    protected override void Awake()
    {
        pageNoBtns = GetComponentsInChildren<PageNoBtn>().ToList();
        _slotInfoPos = slotInfo.GetComponent<RectTransform>();
        
        foreach (SaveSlot slot in slots)
        {
            slot.OnPointerEntered += ShowSlotDetail;
        }
        this.gameObject.SetActive(false);
        
    }
    
    private void OnEnable()
    {

        ShowSlots(1);
        
    }

    private void Start()
    {
        pageNoBtns[0].SetBold();
    }


    public override void ShowSlots(int pageNo)
    {
        base.ShowSlots(pageNo);
        int fileCount = DataManager.Instance.SaveData.Blocks.Count;
        int startIndex = (pageNo - 1) * 8;
        int endIndex = Mathf.Min(startIndex + 8, fileCount);

        for (int i = startIndex; i < endIndex; i++)
        {
            int slotIdx = i % 8;
         
            Debug.Log(slots[slotIdx].image); // 여기서 널이 왜 떠......
            Debug.Log(DataManager.Instance.ChapterImgs[DataManager.Instance.SaveData.Blocks[i].Chapter.CurChapter]);
            slots[slotIdx].image.sprite = DataManager.Instance.ChapterImgs[DataManager.Instance.SaveData.Blocks[i].Chapter.CurChapter];
        }
    }

    /// <summary>
    /// 슬롯에 마우스오버 했을 때, 팝업을 띄움
    /// </summary>
    /// <param name="index">현재 페이지의 슬롯의 인덱스(0~7)</param>
    public void ShowSlotDetail(int index)     
    {
        switch (index)
        {
            case 0: 
            case 1:
                _slotInfoPos.anchoredPosition = slots[0].GetComponent<RectTransform>().anchoredPosition;
                break;
            case 2: 
            case 3:
                _slotInfoPos.anchoredPosition = slots[2].GetComponent<RectTransform>().anchoredPosition;
                break;
            case 4: 
            case 5:
                _slotInfoPos.anchoredPosition = slots[4].GetComponent<RectTransform>().anchoredPosition;
                break;
            case 6: 
            case 7:
                _slotInfoPos.anchoredPosition = slots[6].GetComponent<RectTransform>().anchoredPosition;
                break;
        }
    
        if (index % 2 == 0) //인덱스가 짝수일 때, 이미지가 좌측으로
        {
            slotInfo.SetLeft();
        }
        else if (index % 2 == 1) // 인덱스가 홀수일 때, 이미지가 우측으로
        {
            slotInfo.SetRight();
        }
    
        slotInfo.gameObject.SetActive(true); // 정보 팝업을 띄움
        
        // 현재 받는 매개변수는, 현재 페이지 내에서의 슬롯의 인덱스
        // 지금 필요한 건, 해당 슬롯의 실제 인덱스 즉, 파일 인덱스
        // fileCount, curPageNo 활용해서 구해야함! 
        int fileCount = DataManager.Instance.SaveData.Blocks.Count; // 총 파일 개수
        int pageCount = 0; // 총 페이지 수
    
        if (fileCount % 8 != 0) // 파일이 8의 배수 개가 아닌 경우
        {
            pageCount = fileCount / 8 + 1; 
        }
        
        else if (fileCount % 8 == 0) // 파일이 8의 배수 개인 경우
        {
            pageCount = fileCount / 8; 
        }
        
        if (pageCount == curPageNo) // 현재 페이지가 마지막 페이지일 경우
        {
            if (fileCount%8 > index)
            {
                slotInfo.SetData(index, DataManager.Instance.SaveData.Blocks[index+(curPageNo-1)*8]);
            }
            else if (fileCount%8 <= index)
            {
                slotInfo.SetEmpty(index);
            }
        }
    
        else if (pageCount > curPageNo) // 현재 페이지가 마지막 페이지가 아닌 경우
        {
            slotInfo.SetData(index, DataManager.Instance.SaveData.Blocks[index+(curPageNo-1)*8]);
        }
        
        else if (pageCount < curPageNo)
        {
            slotInfo.SetEmpty(index);
        }
    }

    public void HideSlotDetail()
    {
        slotInfo.gameObject.SetActive(false);
    }
}
