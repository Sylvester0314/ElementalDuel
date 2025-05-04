using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SettingOption : MonoBehaviour, 
    IPointerClickHandler, IPointerEnterHandler, 
    IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public TextMeshProUGUI text;
    public LocalizeStringEvent textEvent;
    public Image background;
    public CanvasGroup backgroundCanvas;
    public GameObject checkingIcon;
    
    public Color defaultTextColor;
    public Color clickingTextColor;
    public Color defaultBackgroundColor;
    public Color clickingBackgroundColor;

    public string key;
    
    private SettingItem _parent;
    
    public void Initialize(string entry, SettingItem setting)
    {
        _parent = setting;
     
        key = entry;
        textEvent.SetEntry(key);
    }

    public void SetChoosingOption()
    {
        var option = _parent.choosing;
        if (option != null && option != this)
        {
            option.background.gameObject.SetActive(false);
            option.checkingIcon.SetActive(false);
        }
        
        _parent.choosing = this;
        _parent.dropdownList.gameObject.SetActive(false);
        _parent.valueEvent.SetEntry(key);
        
        background.gameObject.SetActive(true);
        checkingIcon.SetActive(true);
    }
    
    public void OnPointerClick(PointerEventData eventData)  
    {
        SetChoosingOption();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.gameObject.SetActive(true);
        background.color = defaultBackgroundColor;
        backgroundCanvas.alpha = 0;
        backgroundCanvas.DOFade(1, 0.2f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundCanvas
            .DOFade(0, 0.2f)
            .SetEase(Ease.OutExpo)
            .OnComplete(() => background.gameObject.SetActive(false));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        text.color = clickingTextColor;
        background.color = clickingBackgroundColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        text.color = defaultTextColor;
        background.color = defaultBackgroundColor;
    }
}