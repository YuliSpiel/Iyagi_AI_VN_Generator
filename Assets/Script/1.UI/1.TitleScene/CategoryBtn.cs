using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CategoryBtn : TextButton
{
    private ETCSlotCanvas _etcCanvas;
    private Category _category;
    
    protected virtual void Awake()
    {
        btn = GetComponent<Button>();
        txt = GetComponent<TextMeshProUGUI>();
        _etcCanvas = GetComponentInParent<ETCSlotCanvas>();
        SetHoverColor();
    }
    
    protected override void OnClick()
    {
        // Debug.Log("OnClicked - {this}");
        //
        // _canvas.BoldOnlyBtn(this);
        //
        // curCategory = Category.BG;
        // ShowSlots(1);
        // _canvas.GetComponent<ETCSlotCanvas>().curCategory = _category;
        // _canvas.ShowSlots(PageNo);
    }
    
}
