using System.Collections.Generic;
using DG.Tweening;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FilterItem : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Components")]
    public GameObject chosenBar;
    public GameObject chosenBackground;
    public Image icon;
    
    [Header("Configurations")]
    public List<Sprite> icons;
    public Color defaultColor;
    public Color hoveringColor;
    public Color choosingColor;
    public Color clickingColor;
    
    [Header("In Game Data")] 
    public FilterSet parent;

    private string _key;
    private bool _isClicking;
    
    public void Initialize(string key)
    {
        _key = key;

        var type = "Filter_" + _key;
        icon.sprite = icons.Find(el => el.name == type);
    }

    public void SetDefaultStyle()
    {
        chosenBar.SetActive(false);
        chosenBackground.SetActive(false);
        icon.color = defaultColor;
        transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutExpo);
    }

    public void SetChoosingStyle()
    {
        chosenBar.SetActive(true);
        chosenBackground.SetActive(true);
        icon.color = choosingColor;
        transform.localScale = Vector3.one;
    }

    public void Choose()
    {
        parent.choosingItem?.SetDefaultStyle();
        parent.choosingItem = this;
        parent.choosingItem.SetChoosingStyle();
        
        parent.OnChoosingChanged?.Invoke(_key);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isClicking || parent.choosingItem == this)
            return;
        
        icon.color = hoveringColor;
        transform.DOScale(Vector3.one * 1.05f, 0.15f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isClicking || parent.choosingItem == this)
            return;

        SetDefaultStyle();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (parent.choosingItem == this)
            return;
        
        Choose();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (parent.choosingItem == this)
            return;
        
        _isClicking = true;
        icon.color = clickingColor;
        transform.DOScale(Vector3.one * 0.95f, 0.15f).SetEase(Ease.OutExpo);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (parent.choosingItem == this)
            return;
        
        _isClicking = false;
        SetDefaultStyle();
    }
}