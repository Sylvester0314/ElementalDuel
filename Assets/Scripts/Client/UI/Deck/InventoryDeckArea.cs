using System.Collections.Generic;
using System.Linq;
using Shared.Misc;
using UnityEngine;

public class InventoryDeckArea : MonoBehaviour
{
    public Transform content;
    
    [Header("Prefabs References")]
    public InventoryDeckCard cardPrefab;
    
    public List<ICardAsset> Assets { get; private set; }

    private readonly List<InventoryDeckCard> _cards = new ();

    public void Initialize(int count)
    {
        for (var i = 0; i < count; i++)
            _cards.Add(Instantiate(cardPrefab, content, false));
    }
    
    public void ReplaceCardItem(List<ICardAsset> assets)
    {
        if (assets.TryGetValue(0) is ActionCardAsset)
        {
            var characterList = BuildDeckContainer.Instance.chosenCharacterArea.ToList();
            
            assets = assets
                .OfType<ActionCardAsset>()
                .Select(asset => asset.CheckValidity(characterList))
                .OrderByDescending(asset => asset)
                .OfType<ICardAsset>()
                .ToList();
        }
        
        Assets = assets;
        var assetsCount = Assets.Count;
        
        for (var i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            var go = card.gameObject;

            if (i < assetsCount)
                card.SetCardStyle(Assets[i]);
            else
                go.SetActive(false);
        }
    }

    public InventoryDeckCard SearchInventoryCard(string cardName)
    {
        foreach (var card in _cards)
        {
            if (!card.gameObject.activeSelf)
                break;

            if (card.Name == cardName)
                return card;
        }

        return null;
    }
}