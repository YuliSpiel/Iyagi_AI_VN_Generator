using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ETCSlot : SlotBase
{
    private ETCSlotCanvas _canvas;
    public Image targetImage;  // 슬롯상의 이미지
    public Image maskImage; // 마스크용 이미지
    public float fadeDuration = 0.5f;
    private Coroutine fadeRoutine;

    public bool isNull; // 이미지 확대 방지용 불변수

    protected override void Awake()
    {
        _canvas = GetComponentInParent<ETCSlotCanvas>();
        targetImage = GetComponent<Image>();
        // maskImage = GetComponentInChildren<Image>();
        base.Awake();
    }

    public override IEnumerator ClickRoutine()
    {
        _canvas.OpenFullImage(index);
        yield return null;
    }
    
    // public override void OnPointerEnter(PointerEventData eventData)
    // {
    //     _canvas.AnimateSlotExpand(index);
    // }
    //
    // public override void OnPointerExit(PointerEventData eventData)
    // {
    //     _canvas.AnimateSlotShrink(index);
    // }
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (isNull)
        {
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOut(maskImage, fadeDuration));
    }
    
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (isNull)
        {
            return;
        }
        
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeIn(maskImage, fadeDuration));
    }
    
    IEnumerator FadeOut(Image img, float duration)
    {
        // 이거 되게 부드러워서 일단 남겨둘게
        float startAlpha = img.color.a;
        float time = 0f;
        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
    }
    
    IEnumerator FadeIn(Image img, float duration)
    {
        // 이것도!!
        float startAlpha = img.color.a;
        float time = 0f;
        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, 1f, time / duration);
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
    }
}
