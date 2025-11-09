using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class SaveSlot : SlotBase
{
    private SaveSlotCanvas _canvas;
    public event Action<int> OnPointerEntered;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEntered?.Invoke(index);
    }


    protected override void Awake()
    {
        _canvas = GetComponentInParent<SaveSlotCanvas>();
        base.Awake();
    }

    public override IEnumerator ClickRoutine()
    {
        int fileIndex = (_canvas.curPageNo - 1) * 8 + index;
        GameManager.Instance.Initialize(DataManager.Instance.SaveData.Blocks[fileIndex]);
        yield return UIManager.Instance.GlobalFadeOut(1f);
        SceneManager.LoadScene(1);
    }

    // public override void OnPointerEnter(PointerEventData eventData)
    // {
    //     _canvas.ShowSlotDetail(index);
    // }

    public override void OnPointerExit(PointerEventData eventData)
    {
        _canvas.HideSlotDetail();
    }
}
