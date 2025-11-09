using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PageNoBtn : TextButton
{
    protected override void OnClick()
    {
        Debug.Log(_canvas);
        _canvas.curPageNo = PageNo;
        _canvas.BoldOnlyPageBtn(this);
        _canvas.ShowSlots(PageNo);
    }
}
