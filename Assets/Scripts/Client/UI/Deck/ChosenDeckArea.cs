using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Misc;
using UnityEngine;

public class ChosenDeckCardArea : MonoBehaviour
{
    public class CardWrapper : IComparable<CardWrapper>
    {
        public string Key;
        public ChosenDeckCard Card;
        
        public int CompareTo(CardWrapper other)
        {
            return string.Compare(Key, other.Key, StringComparison.Ordinal);
        }
    }
    
    public Transform content;
    public GameObject emptyHint;
    
    [Header("Prefabs References")]
    public ChosenDeckCard cardPrefab;

    [HideInInspector] 
    public int maxCount = 30;
    public readonly List<CardWrapper> Cards = new ();

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set
        {
            _totalCount = value;
            BuildDeckContainer.Instance.
                    actionCardCount.text = $"{value}/{maxCount}";
        }
    }

    public void SetChoosingCount(int count, ActionCardAsset asset)
    {
        BuildDeckContainer.Instance.inventoryArea
            .SearchInventoryCard(asset.name)
            ?.SetChoosingCount(count, asset.name);
    }

    public void Append(ActionCardAsset asset)
    {
        if (TryGetValue(asset.name, out var target))
        {
            target.ModifyCount(1);
            return;
        }

        if (TotalCount >= maxCount)
        {
            BuildDeckContainer.Instance.DisplayHint("card_limited_exceeded");
            return;
        }
        
        emptyHint.SetActive(false);

        var card = Instantiate(cardPrefab, content, false)
            .Initialize(asset);
        var wrapper = new CardWrapper
        {
            Card = card,
            Key = asset.name
        };
        var index = Cards.FixedBinarySearch(wrapper);
        
        Cards.Insert(index, wrapper);
        card.transform.SetSiblingIndex(index);
    }

    public void Remove(ChosenDeckCard card)
    {
        Cards.RemoveAll(wrapper => wrapper.Key == card.Asset.name);
        Destroy(card.gameObject);

        if (Cards.Count == 0)
            emptyHint.SetActive(true);
    }
    
    public void Remove(ActionCardAsset asset)
    {
        var wrapper = Cards.Find(wrapper => wrapper.Key == asset.name);

        TotalCount -= wrapper.Card.Count;
        Cards.Remove(wrapper);
        Destroy(wrapper.Card.gameObject);

        if (Cards.Count == 0)
            emptyHint.SetActive(true);
    }

    public void Clear()
    {
        foreach (var wrapper in Cards)
            Destroy(wrapper.Card.gameObject);
        
        TotalCount = 0;
        Cards.Clear();
        emptyHint.SetActive(true);
    }
    
    public bool TryGetValue(string key, out ChosenDeckCard card)
    {
        var wrapper = Cards.Find(wrapper => wrapper.Key == key);
        card = wrapper?.Card;
        return wrapper != null;
    }

    public List<ActionCardAsset> ToList()
    {
        var cards = new List<ActionCardAsset>();

        foreach (var wrapper in Cards)
            cards.AddRange(Enumerable.Repeat(
                wrapper.Card.Asset, wrapper.Card.Count
            ));

        return cards;
    }
}