using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ChosenDeckCard : AbstractDeckCard, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Components")] 
    public LocalizeStringEvent cardName;
    public TextMeshProUGUI chosenNumber;
    public Image snapshot;
    public CanvasGroup lights;

    public ActionCardAsset Asset { get; private set; }
    
    private int _chosenCount;

    public int Count
    {
        get => _chosenCount;
        private set
        {
            _chosenCount = value;
            var area = BuildDeckContainer.Instance.chosenCardArea;
            area.SetChoosingCount(value, Asset);

            if (value <= 0)
            {
                area.Remove(this);
                return;
            }

            chosenNumber.text = value.ToString();
        }
    }

    public void ModifyCount(int value)
    {
        var global = BuildDeckContainer.Instance;
        if (Count + value > 2)
        {
            global.DisplayHint("identical_card_can_only_have_2");
            return;
        }
        
        var parent = global.chosenCardArea;
        var totalCount = parent.TotalCount + value;
        
        if (totalCount > parent.maxCount)
        {
            global.DisplayHint("card_limited_exceeded");
            return;
        }

        parent.TotalCount = totalCount;
        Count += value;
    }

    public ChosenDeckCard Initialize(ActionCardAsset asset, int count = 1)
    {
        Asset = asset;
        ModifyCount(count);

        snapshot.sprite = Asset.cardSnapshot;
        cardName.SetEntry(Asset.cardName);

        return this;
    }

    protected override void OnLeftClick()
    {
        var global = BuildDeckContainer.Instance;
        var assets = global.chosenCardArea.Cards
            .Select(wrapper => wrapper.Card.Asset)
            .OfType<ICardAsset>()
            .ToList();

        global.information.Open(assets, Asset);
    }
    
    // Deselect this card
    protected override void OnRightClick()
    {
        ModifyCount(-1);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        lights.DOFade(1, 0.2f).SetEase(Ease.OutExpo);
        transform.DOScale(Vector3.one * 1.015f, 0.2f).SetEase(Ease.OutExpo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        lights.DOFade(0, 0.2f);
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutExpo);
    }
}