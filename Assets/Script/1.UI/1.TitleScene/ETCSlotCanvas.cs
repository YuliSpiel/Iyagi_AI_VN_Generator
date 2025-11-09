using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum Category { CG, BG, Keyword }

public class ETCSlotCanvas : SlotCanvasBase
{
    public Category curCategory;

    public List<Sprite> CGSprites;
    public List<Sprite> BGSprites;
    public List<Sprite> KeywordSprites;
    public List<bool> CGFlags;
    public List<bool> BGFlags;
    public List<bool> KeywordFlags;
    public Sprite lockedSprite;

    public List<CategoryBtn> categoryBtns;
    private Button CGButton;
    private Button BGButton;
    private Button KeywordButton;

    protected override void Awake()
    {
        pageNoBtns = GetComponentsInChildren<PageNoBtn>().ToList();
        this.gameObject.SetActive(false);
        // PageNoBtns[0].SetBold();
        // categoryBtns = GetComponentsInChildren<CategoryBtn>().ToList(); 직접할당
        
        // categoryBtns[0].SetBold();
    }

    protected override void OnEnable()
    {
        Debug.Log("일어나아아아아아아앗");
        pageNoBtns[0].SetBold();
        categoryBtns[0].SetBold();
        ShowSlots(1);
    }

    private void Start()
    {
        categoryBtns[0].btn.onClick.AddListener(OnCGClicked);
        categoryBtns[1].btn.onClick.AddListener(OnBGClicked);
        categoryBtns[2].btn.onClick.AddListener(OnKWClicked);
    }

    public override void ShowSlots(int pageNo)
    {
        List<ETCSlot> etcSlots = slots.Cast<ETCSlot>().ToList();
        // base.ShowSlots(pageNo);

        curPageNo = pageNo;
        ClearAllSlots();
        
        List<Sprite> spriteList = null;
        List<bool> flagList = null;

        switch (curCategory)
        {
            case Category.CG:
                spriteList = CGSprites; // 일단 임시로 인스펙터에서 바로 볼 수 있게
                flagList = CGFlags;
                break;
            case Category.BG:
                spriteList = BGSprites;
                flagList = BGFlags;
                break;
            case Category.Keyword:
                spriteList = KeywordSprites;
                flagList = KeywordFlags;
                break;
        }

        int startIndex = (pageNo - 1) * 8;
        for (int i = 0; i < 8; i++)
        {
            if (etcSlots[i].targetImage == null)
            {
                return; // 임시.....
            }

            int dataIndex = startIndex + i;
            if (dataIndex < spriteList.Count)
            {
                // Debug.Log(slots[i].image);
                // Debug.Log(flagList[dataIndex]);
                // Debug.Log(spriteList[dataIndex]);
                // etcSlots[i].targetImage.sprite = flagList[dataIndex] ? spriteList[dataIndex] : lockedSprite;
                Sprite spriteToShow;

                if (flagList[dataIndex])
                {
                    spriteToShow = spriteList[dataIndex];
                    etcSlots[i].isNull = false;
                }
                else
                {
                    spriteToShow = lockedSprite;
                    etcSlots[i].isNull =  true;
                }

                etcSlots[i].targetImage.sprite = spriteToShow;

            }
            else
            {
                // etcSlots[i].SetEmpty();
                etcSlots[i].isNull = true;
            }
        }
    }
    
    public void BoldOnlyCategoryBtn(TextButton btn)
    {
        foreach (var b in categoryBtns)
            b.ResetBold();
        btn.SetBold();
    }

    public void OnCGClicked()
    {
        BoldOnlyCategoryBtn(categoryBtns[0]);
        // categoryBtns[1].ResetBold();
        // categoryBtns[2].ResetBold();
        // categoryBtns[0].SetBold();

        curCategory = Category.CG;
        ShowSlots(1);
    }

    public void OnBGClicked()
    {
        BoldOnlyCategoryBtn(categoryBtns[1]);
        // categoryBtns[0].ResetBold();
        // categoryBtns[2].ResetBold();
        // categoryBtns[1].SetBold();

        curCategory = Category.BG;
        ShowSlots(1);
    }

    public void OnKWClicked()
    {
        BoldOnlyCategoryBtn(categoryBtns[2]);
        // categoryBtns[0].ResetBold();
        // categoryBtns[1].ResetBold();
        // categoryBtns[2].SetBold();

        curCategory = Category.Keyword;
        ShowSlots(1);
    }

    public void OpenFullImage(int index)
    {
        // 전체 이미지 패널 띄우기 로직
    }

    // public void AnimateSlotExpand(int index)
    // {
    //     // 마스크 확장 애니메이션
    //     Debug.Log("확장됐지롱");
    // }
    //
    // public void AnimateSlotShrink(int index)
    // {
    //     // 마스크 축소 애니메이션
    //     Debug.Log("축소됐지롱");
    // }
}

