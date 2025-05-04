using System;
using Server.GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDeckCard : AbstractDeckCard
{
    [Header("Components")]
    public Image cardFace;
    public CostSetComponent costSet;
    public GameObject health;
    public TextMeshProUGUI healthValue;
    public GameObject chosenBar;
    public TextMeshProUGUI chosenText;
    public DarkMask darkMask;
    public GameObject invalid;

    [Header("References")]
    public TMP_FontAsset fontAsset;
    
    public string Name { get; private set; }
    
    protected Action OnChoosing;
    protected Action OnChecking;
    
    public void SetCardStyle(ICardAsset asset)
    {
        gameObject.SetActive(true);
        
        if (asset is CharacterAsset characterAsset)
            AsCharacter(characterAsset);
        
        if (asset is ActionCardAsset actionCardAsset)
            AsAction(actionCardAsset);
    }

    public void SetChoosingCount(int count, string sourceName)
    {
        if (Name != sourceName)
            return;
        
        chosenBar.SetActive(count != 0);
        darkMask.SetActive(count is 2 or -1);

        // it means player chosen a character card
        if (count == -1)
        {
            chosenText.text = ResourceLoader.GetLocalizedUIText("selected_character_card_hint");
            return;
        }
        
        var pattern = ResourceLoader.GetLocalizedUIText("selected_action_card_hint");
        chosenText.text = pattern.Replace("$[Number]", count.ToString());
    }
    
    private void AsAction(ActionCardAsset asset)
    {
        health.gameObject.SetActive(false);
        cardFace.sprite = asset.cardImage;
        Name = asset.name;
        
        costSet.costs.gameObject.SetActive(true);
        costSet.InitializeCostList(
            "_Outline",
            list => list.ForEach(cost =>
            {
                if (!isInteractable)
                    return;
                darkMask.imgTargets.Add(cost.type);
                darkMask.AppendText(cost.count);
                cost.count.font = fontAsset;
            })
        );
        
        var logic = new CostLogic(asset.costs);
        logic.RefreshCostDisplay(costSet);
        
        if (!isInteractable)
            return;

        var global = BuildDeckContainer.Instance;
        var actionArea = global.chosenCardArea;
        SetChoosingCount(
            actionArea.TryGetValue(asset.name, out var chosenCard)
                ? chosenCard.Count : 0,
            Name
        );

        invalid.gameObject.SetActive(!asset.isValid);

        OnChecking = () => global.information.Open(global.inventoryArea.Assets, asset);
        OnChoosing = () =>
        {
            if (!asset.isValid)
                global.DisplayHint("invalid_card_can_not_add");
            else
                actionArea.Append(asset);
        };
    }

    private void AsCharacter(CharacterAsset asset)
    {
        costSet.costs.gameObject.SetActive(false);
        health.gameObject.SetActive(true);
        healthValue.text = asset.baseMaxHealth.ToString();
        cardFace.sprite = asset.cardImage;
        Name = asset.name;
        
        if (!isInteractable)
            return;

        var global = BuildDeckContainer.Instance;
        var characterArea = global.chosenCharacterArea;
        SetChoosingCount(
            characterArea.TryGetValue(asset.name, out var chosenCard)
                ? chosenCard.Count : 0,
            Name
        );
        
        OnChecking = () => global.information.Open(global.inventoryArea.Assets, asset);
        OnChoosing = () => characterArea.Append(asset);
    }

    protected override void OnLeftClick()
    {
        OnChecking.Invoke();
    }
    
    protected override void OnRightClick()
    {
        OnChoosing.Invoke();
    }
}