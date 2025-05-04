using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bookmark : MonoBehaviour, 
    IPointerDownHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public AbstractHandbookContainer container;
    
    [Header("Components")]
    public TextMeshProUGUI title;
    public Image background;
    public Image choosingImage;
    public Canvas subCanvas;
    
    [Header("References")]
    public Material lightMaterial;
    public Material hoverMaterial;
    public Color choosingColor;
    public Color normalColor;
    
    public int index;

    private const float Duration = 0.2f;

    public void Display()
    {
        subCanvas.overrideSorting = true;
    }
    
    private void Hide()
    {
        subCanvas.overrideSorting = false;
    }

    private static void DelayCall(bool isDelay, Action action, float duration = Duration)
    {
        var delay = isDelay ? duration : 0;
        if (delay == 0)
            action.Invoke();
        else
            DOVirtual.DelayedCall(delay, action.Invoke);
    }
    
    public void SetChoosingStatus(bool isDelay, float duration)
    {
        DelayCall(isDelay, Display, duration);
        
        choosingImage.gameObject.SetActive(true);
        choosingImage.transform
            .DOScale(new Vector3(1.36f, 1.2f, 1), Duration)
            .SetEase(Ease.OutExpo);
        title.transform.localScale = Vector3.one * 1.125f;
        title.color = choosingColor;
        
        background.material = lightMaterial;
        background.transform
            .DOScale(new Vector3(1.15f, 1.2f, 1), Duration)
            .SetEase(Ease.OutExpo);
    }

    public void CancelChoosingStatus(bool isDelay)
    {
        DelayCall(isDelay, Hide);
        
        choosingImage.gameObject.SetActive(false);
        choosingImage.transform.localScale = new Vector3(1.2f, 1, 1);
        title.transform.localScale = Vector3.one;
        title.color = normalColor;
        
        background.material = null;
        background.transform
            .DOScale(new Vector3(1.01f, 1.07f, 1), Duration)
            .SetEase(Ease.OutExpo);
    }

    public void ChickBookmark(bool isDelay, float duration = Duration)
    {
        SetChoosingStatus(isDelay, duration);
        container.SwitchContentPage(index);
        container.choosingMark = this;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (container.choosingMark == this)
            return;
        
        background.material = null;
        background.transform
            .DOScale(Vector3.one * 0.965f, Duration)
            .SetEase(Ease.OutExpo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (container.choosingMark != this)
            ChickBookmark(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (container.choosingMark != this)
            background.material = hoverMaterial;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (container.choosingMark != this)
            CancelChoosingStatus(false);
    }
}
