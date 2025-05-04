using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSubButton : MonoBehaviour, 
    IPointerClickHandler, IPointerEnterHandler,
    IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public string key;
    public Action Callback;
    
    [Header("Components")]
    public MenuDropDownList parent;
    public TextMeshProUGUI text;
    public Image background;
    public Image lights;

    [Header("Configurations")] 
    public Color defaultTextColor;
    public Color clickingTextColor;
    public Color defaultBackgroundColor;
    public Color hoverBackgroundColor;
    public Color clickingBackgroundColor;
    
    public void OnPointerClick(PointerEventData eventData)  
    {
        Callback?.Invoke();
        parent.SetStatus(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parent.clickingSub)
            return;
        
        lights.gameObject.SetActive(true);
        background.color = hoverBackgroundColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (parent.clickingSub)
            return;
        
        lights.gameObject.SetActive(false);
        background.color = defaultBackgroundColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        parent.clickingSub = true;
        lights.gameObject.SetActive(false);
        text.color = clickingTextColor;
        background.color = clickingBackgroundColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        parent.clickingSub = false;
        text.color = defaultTextColor;
        background.color = defaultBackgroundColor;
    }
}