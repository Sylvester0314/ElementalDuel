using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;

public class SettingItem : MonoBehaviour
{
    [Header("In Game Data")] 
    public SettingOptionAsset asset;
    public SettingOption choosing;
    public SettingLogic logic;

    [Header("Components")]
    public GameObject backgroundLight;
    public GameObject outlineLight;
    public LocalizeStringEvent valueEvent;
    public LocalizeStringEvent titleEvent;
    public CanvasGroup dropdownList;
    
    [Header("Prefabs References")]
    public SettingOption optionPrefab;

    private readonly List<SettingOption> _optionInstances = new ();
    
    public void Initialize(SettingLogic settingLogic)
    {
        foreach (var option in asset.options)
        {
            var instance = Instantiate(
                optionPrefab,
                dropdownList.transform,
                false
            );
            instance.Initialize(option, this);
            _optionInstances.Add(instance);
        }
        
        logic = settingLogic;
        titleEvent.SetEntry(asset.titleEntry);
        _optionInstances[0].SetChoosingOption();
    }

    public void EnterSettingArea(BaseEventData eventData)
    {
        outlineLight.SetActive(true);
    }

    public void ExitSettingArea(BaseEventData eventData)
    {
        outlineLight.SetActive(false);
    }

    public void ClickValueAreaDown()
    {
        if (logic.disableClick)
            return;
        
        logic.clickingSetting = true;
        backgroundLight.SetActive(true);
    }
    
    public void ClickValueAreaUp()
    {
        if (logic.disableClick)
            return;
        
        logic.clickingSetting = false;
        backgroundLight.SetActive(false);

        if (logic.editingSetting)
            logic.editingSetting.dropdownList.gameObject.SetActive(false);
        
        if (logic.editingSetting == this)
        {
            FadeOutDropDownList();
            return;
        }
        
        logic.editingSetting = this;
        
        dropdownList.alpha = 0;
        dropdownList.gameObject.SetActive(true);
        dropdownList.DOFade(1, 0.25f).SetEase(Ease.OutExpo);
    }
    
    public void OnBeginDrag(BaseEventData eventData)
    {
        if (logic.scroll != null && eventData is PointerEventData pointerEventData)
            logic.scroll.OnBeginDrag(pointerEventData);
    }
    
    public void OnDrag(BaseEventData eventData)
    {
        if (logic.scroll != null && eventData is PointerEventData pointerEventData)
            logic.scroll.OnDrag(pointerEventData);
    }
    
    public void OnEndDrag(BaseEventData eventData)
    {
        if (logic.scroll != null && eventData is PointerEventData pointerEventData)
            logic.scroll.OnEndDrag(pointerEventData);
    }

    public void FadeOutDropDownList()
    {
        if (logic.editingSetting == this)   
            logic.editingSetting = null;
        
        dropdownList
            .DOFade(0, 0.25f)
            .SetEase(Ease.OutExpo)
            .OnComplete(() => dropdownList.gameObject.SetActive(false));
    }
}