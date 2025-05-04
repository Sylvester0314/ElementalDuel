using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LittleButton : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, 
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    public Image background;
    public TextMeshProUGUI text;
    public Color backgroundClickingColor;
    public Color backgroundDefaultColor;
    public Color textClickingColor;
    public Color textDefaultColor;
    
    public Action Callback;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        background.color = backgroundClickingColor;
        text.color = textClickingColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        background.color = backgroundDefaultColor;
        text.color = textDefaultColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform
            .DOScale(Vector3.one * 1.05f, 0.15f)
            .SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform
            .DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.OutExpo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Callback?.Invoke();
    }
}