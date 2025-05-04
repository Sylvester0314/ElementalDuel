using System;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEntity : MonoBehaviour, IGameEntity, IStatusEntity
{
    public Global global;
    public CanvasGroup canvas;
    public StatusZone parent;
    
    [Header("In Game Data")]
    public int index;
    public bool isOccupied;
    public string uniqueId;
    public StatusCardAsset asset;
    
    [Header("Components")]
    public Image statusLight;
    public Image statusIcon;
    public GameObject lifeHint;
    public TextMeshProUGUI remainingLife;
    
    public Status Status;
    
    public void Initialize(StatusZone p, int i)
    {
        global = p.global;
        parent = p;
        index = i;
    }
    
    public void CancelPreviewStatus()
    {
        
    }

    public async Task Occupy(Status status)
    {
        Status = status;
        asset = await ResourceLoader.LoadSoAsset<StatusCardAsset>(status.Key);
        gameObject.SetActive(true);

        uniqueId = status.UniqueId;
        isOccupied = true;
        global.EntitiesMap.Add(uniqueId, this);
        
        statusIcon.sprite = asset.displayImage;
        lifeHint.SetActive(asset.hintValueField != string.Empty);
        remainingLife.text = Status.GetFieldText(asset.hintValueField);
    }

    public void DoAction(CharacterCard character, Element element, Action feedbackAction = null)
    {
        
    }
    
    public void Discard()
    {
        canvas
            .DOFade(0, 0.25f)
            .OnComplete(() => parent.Resort(index));
    }

    public void RefreshLiftHint(Status status)
    {
        Status = status;
        remainingLife.text = Status.GetFieldText(asset.hintValueField);
    }

    public void HidePreviewComponents() { }
}
