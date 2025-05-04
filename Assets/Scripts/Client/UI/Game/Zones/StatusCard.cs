using System;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusCard : AbstractCard, IGameEntity, IStatusEntity, IPointerClickHandler
{
    public Global global;
    public StatusCardZone parent;

    [Header("In Game Data")] 
    public int index;
    public bool isOccupied;
    public string uniqueId;
    public StatusCardAsset asset;
    
    [Header("Components")] 
    public Canvas canvas;
    public CanvasGroup canvasGroup;
    public Image cardImage;
    public Image cardFrame;
    public HealthPreview healthPreview;

    public GameObject hint;
    public Image hintIcon;
    public TextMeshProUGUI hintValue;
    
    public GameObject eventType;
    public Image eventIcon;
    public TextMeshProUGUI damageValue;

    [Header("References")] 
    public Material dissolve;

    public Status Status;

    public void Initialize(StatusCardZone p, int i)
    {
        global = p.global;
        parent = p;
        index = i;
    }
    
    public void SetDisplay()
    {
        gameObject.SetActive(true);
        cardImage.sprite = asset.displayImage;
        
        hint.gameObject.SetActive(asset.showHintIcon);
        hintIcon.sprite = asset.hintTypeIcon;
        hintValue.text = Status.GetFieldText(asset.hintValueField);
        
        eventType.SetActive(asset.showEventType);
        eventIcon.sprite = asset.eventTypeIcon;
    }
    
    public async Task Preview(Status status)
    {
        Status = status;
        transform.localScale = Vector3.one * 0.325f;
        canvas.sortingOrder = 1;
        
        asset = await ResourceLoader.LoadSoAsset<StatusCardAsset>(status.Key);
        
        if (isOccupied)
            ExistedPreview();
        else
            IncomingPreview();
    }

    public void ExistedPreview()
    {
        
    }

    public void IncomingPreview()
    {
        SetDisplay();
        hint.SetActive(false);
        eventType.SetActive(false); 
    }
    
    public void CancelPreviewStatus()
    {
        healthPreview.Reset();
        transform.localScale = Vector3.one * 0.3f;
        canvas.sortingOrder = 0;
        
        if (!isOccupied)
            gameObject.SetActive(false);
    }

    public async Task Occupy(Status status)
    {
        Status = status;
        asset = await ResourceLoader.LoadSoAsset<StatusCardAsset>(status.Key);
        SetDisplay();

        uniqueId = status.UniqueId;
        isOccupied = true;
        global.EntitiesMap.Add(uniqueId, this);

        canvasGroup.alpha = 0.6f;
        transform.localScale = Vector3.one * 0.18f;

        canvasGroup.DOFade(1, 0.2f).SetEase(Ease.OutSine);
        transform.DOScale(Vector3.one * 0.3f, 0.2f).SetEase(Ease.OutSine);
        await Task.Delay(100);
    }

    public void DoAction(CharacterCard character, Element element, Action feedbackAction = null)
    {
        global.shoot.Play(transform, character, element, () => feedbackAction?.Invoke());
    }

    public void HidePreviewComponents() => CancelPreviewStatus();
    
    public void Discard()
    {
        hint.SetActive(false);
        eventType.SetActive(false);
        
        cardFrame.material = dissolve;
        cardImage.material = dissolve;
        
        dissolve
            .DOFloat(0, "_DissolveAmount", 0.25f)
            .OnComplete(() => parent.Resort(index));
    }

    public void RefreshLiftHint(Status status)
    {
        Status = status;
        Debug.Log(ResourceLoader.GetLocalizedCard(asset.statusName) + "   " + Status.GetFieldText(asset.hintValueField));
        hintValue.text = Status.GetFieldText(asset.hintValueField);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        global.SetSelectingCard(this);
        RotateTargetAnimation();
    }
}