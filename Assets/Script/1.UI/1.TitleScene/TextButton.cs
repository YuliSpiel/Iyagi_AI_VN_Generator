using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public abstract class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int PageNo;
    public Button btn;
    public TextMeshProUGUI txt;
    protected string _colorCode = "#fbcd17";
    protected Color _color;
    
    protected SlotCanvasBase _canvas;

    protected virtual void Awake()
    {
        btn = GetComponent<Button>();
        txt = GetComponent<TextMeshProUGUI>();
        _canvas = GetComponentInParent<SlotCanvasBase>();
        SetHoverColor();
    }

    protected void Start()
    {
        btn.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTxtColor();    
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetTxtColor();
    }

    protected virtual void OnClick()
    {
        // Debug.Log(_canvas);
        // _canvas.curPageNo = PageNo;
        // _canvas.BoldOnlyPageBtn(this);
        // _canvas.ShowSlots(PageNo);
    }

    protected void SetHoverColor()
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(_colorCode, out color))
        {
            _color = color;
        }
    }

    protected void SetTxtColor()
    {
        txt.color = _color;
    }

    protected void ResetTxtColor()
    {
        txt.color = Color.black;
    }

    public void SetBold()
    {
        txt.fontStyle = FontStyles.Bold;
    }
    
    public void ResetBold()
    {
        txt.fontStyle = FontStyles.Normal;
    }
}
