using System;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ConfirmButton : AbstractPromptComponent, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Self Components")]
    public RectTransform rect;
    public CanvasGroup lights;
    public CanvasGroup outline;
    public TextMeshProUGUI text;
    public LocalizeStringEvent textEvent;
    public GameObject mask;
    
    public Color normalColor;
    public Color clickingColor;

    public Action Callback;
    public Func<Task> AsyncCallback;

    public override void Reset()
    {
        rect.gameObject.SetActive(false);
        lights.alpha = 0f;
        outline.alpha = 0f;
        Callback = () => { };
        AsyncCallback = null;
    }

    public override void Display<T>(T data, Action onComplete = null)
    {
        if (data is not ValueTuple<string, Action> tuple)
            return;
        
        var (entry, callback) = tuple;
        isShowing = true;
        Callback += callback;
        rect.gameObject.SetActive(true);
        textEvent.SetEntry(entry);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    public override void Hide()
    {
        isShowing = false;
        Callback = () => { };
        rect.gameObject.SetActive(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        lights.DOFade(1, 0.2f).SetEase(Ease.OutExpo);
        outline.DOFade(1, 0.2f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        lights.DOFade(0, 0.2f).SetEase(Ease.OutExpo);
        outline.DOFade(0, 0.2f).SetEase(Ease.OutExpo);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        lights.alpha = 0;
        outline.alpha = 0;
        mask.gameObject.SetActive(true);
        text.color = clickingColor;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        lights.alpha = 1;
        outline.alpha = 1;
        mask.gameObject.SetActive(false);
        text.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Callback?.Invoke();
        AsyncCallback?.Invoke();
    }
}