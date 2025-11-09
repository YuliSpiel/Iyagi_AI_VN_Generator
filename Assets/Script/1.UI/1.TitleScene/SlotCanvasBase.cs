using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SlotCanvasBase :  MonoBehaviour
{
    public List<SlotBase> slots = new List<SlotBase>(8);
    public List<PageNoBtn> pageNoBtns;
    public int curPageNo = 1;

    protected virtual void Awake()
    {
        // pageNoBtns = GetComponentsInChildren<PageNoBtn>().ToList();
        // Debug.Log(pageNoBtns.Count);
    }

    protected virtual void OnEnable()
    {
        pageNoBtns[0].SetBold();
    }

    // IEnumerator Start()
    // {
    //     yield return null; // 한 프레임 기다림으로써 초기화 보장
    //     ShowSlots(1);
    // }
    
    public virtual void ShowSlots(int pageNo)
    {
        curPageNo = pageNo;
        ClearAllSlots();
    }

    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.SetEmpty();
        }
    }

    public void BoldOnlyPageBtn(TextButton btn)
    {
        foreach (var b in pageNoBtns)
            b.ResetBold();
        btn.SetBold();
    }
    
}
