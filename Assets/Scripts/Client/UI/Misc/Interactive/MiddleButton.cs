using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class MiddleButton : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public CanvasGroup lights;
    public Image background;
    public Graphic body;
    public LocalizeStringEvent textEvent;
    
    public Color bodyDefaultColor;
    public Color bodyClickingColor;
    public Color backgroundDefaultColor;
    public Color backgroundClickingColor;

    [HideInInspector] 
    public bool clicking;
    
    public Action Callback;
    public Func<Task> AsyncCallback;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        lights.DOFade(1, 0.2f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        lights.DOFade(0, 0.2f).SetEase(Ease.OutExpo);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        clicking = true;
        
        lights.alpha = 0;
        body.color = bodyClickingColor;
        background.color = backgroundClickingColor;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        clicking = false;

        lights.alpha = 1;
        body.color = bodyDefaultColor;
        background.color = backgroundDefaultColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Callback?.Invoke();
        AsyncCallback?.Invoke();
    }
}