using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SlotBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int index;
    protected Button button;
    public Image image; 
    // public event Action<int> OnPointerEntered;
    
    protected virtual void Awake()
    {
        Init();
        button.onClick.AddListener(() => StartCoroutine(ClickRoutine()));
    }

    public virtual void Init()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    ///  TODO: 이녀셕 과연 쓸모 있을지 고민해보기
    public void SetEmpty()
    {
        if (image == null)
        {
            return;
        }
        image.sprite = null;
    }

    public abstract IEnumerator ClickRoutine();
    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnPointerExit(PointerEventData eventData);
}
