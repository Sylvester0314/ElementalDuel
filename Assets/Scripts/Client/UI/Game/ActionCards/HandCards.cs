using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Misc;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class HandCards : MonoBehaviour
{
    public Global global;
    public Canvas rootCanvas;
    public Canvas selfCanvas;
    public RectTransform checkbox;
    public AutomaticLayout layout;
    
    [Header("In Game Data")] 
    public List<PlayableActionCard> cards = new ();
    public bool isExtending;
    public bool usingCard;
    public bool isAppendingCards;
    public bool hasCardHovering;
    
    [Header("Prefab References")]
    public PlayableActionCard cardPrefab;

    public bool CanDrag => global.Acting;
    public bool HasCardDragging => cards.Any(card => card.drag.IsDragging);
    public PlayableActionCard DraggingCard => cards?.FirstOrDefault(card => card.drag.IsDragging);

    public void ResetLayout()
    {
        // TODO 修复 - 换人技能期间应该直接返回
        if (isAppendingCards)
            return;

        if (!usingCard)
            ContractArea();
        else
        {
            gameObject.SetActive(true);
            ExtendAreaLayout();
        }

        usingCard = false;
    }

    private void RefreshSpacingLayout()
    {
        var positions = layout.originPositions;
        
        for (var i = 0; i < layout.count; i++)
        {
            var position = positions[i];
            position.z = i * -0.001f;
            positions[i] = position;
        }
    }

    private async Task RefreshChangedLayout(bool extend)
    {
        await Task.Yield();
        
        layout.InitLayoutData();
        layout.RefreshOriginPosition();
        SetLayoutSpacing(extend);
        RefreshSpacingLayout();
        
        if (extend) 
            ExtendAreaLayout();
    }
    
    public async void RemoveActionCard(int timestamp)
    {
        var card = cards.Find(card => card.timestamp == timestamp);
        
        cards.Remove(card);
        gameObject.SetActive(true);
        usingCard = false;
        
        Destroy(card.gameObject);
        await RefreshChangedLayout(true);
    }

    public async Task<ValueTuple<bool, List<int>>> AppendActionCards(List<DeckCard> deckCards)
    {
        isAppendingCards = true;

        var scale = isExtending
            ? cards.FirstOrDefault()?.transform.localScale ?? Vector3.one
            : Vector3.one;
        
        var indexes = deckCards
            .Select(card =>
            {
                var instance = Instantiate(cardPrefab);
                instance.Initialize(card.place.asset, this);
                instance.SetInteractable(false);
                instance.transform.localScale = scale;
                instance.timestamp = card.timestamp;
                
                var index = cards.FixedBinarySearch(instance);
                instance.SetParent(transform);
                instance.transform.SetSiblingIndex(index);
                instance.canvasGroup.alpha = 0;

                cards.Insert(index, instance);
                return index;
            })
            .ToList();
        
        await RefreshChangedLayout(isExtending);
        return (isExtending, indexes);
    }

    public void ExtendAreaLayout(PlayableActionCard expected = null)
    {
        if (isAppendingCards)
            return;
   
        if (!HasCardDragging)
            DisplayCountIcon();
        ResetCanvasBodyPosition(expected);
        global.combatAction.TransferStatus(CombatTransfer.Transparent);
        isExtending = true;
        
        var dragging = GetDraggingCardTransform();
        cards
            .Where(card => card != expected)
            .ToList()
            .ForEach(card => card.SetScale(1.075f));

        SetLayoutSpacing(true);
        layout.SetOffset(4.25f, 4.34f, 0);
        layout.MovementLayout(
            PlayableActionCard.ExtendCardDuration, Ease.OutExpo,
            new List<Transform> { dragging }
        );
    }

    public void ContractAreaLayout(bool lower = false, PlayableActionCard expected = null)
    {
        if (isAppendingCards)
            return;
        isExtending = false;

        DisappearCountIcon();
        ResetCanvasBodyPosition(expected);
        var dragging = GetDraggingCardTransform();
        cards
            .Where(card => card.transform != dragging)
            .ToList()
            .ForEach(card =>
            {
                card.CloseSelectIcon();
                card.SetScale(1);
            });

        var offset = lower ? Vector3.down * 8 : Vector3.zero;

        SetLayoutSpacing(false);
        layout.SetOffset(offset);
        layout.MovementLayout(
            PlayableActionCard.ExtendCardDuration, Ease.OutExpo,
            new List<Transform> { dragging }
        );
    }

    public void ExtendArea()
    {
        if (isAppendingCards)
            return;
        
        ExtendAreaLayout();
        SetInteractableForAllCards(true);

        global.SetSelectingCard(null);
    }

    public void ContractArea()
    {
        if (isAppendingCards)
            return;
        
        ContractAreaLayout();
        SetInteractableForAllCards(false);
    }

    public async Task MoveToBuffer()
    {
        await Task.Yield();
    }

    private void SetLayoutSpacing(bool extend)
    {
        if (!extend)
        {
            layout.spacing = -2.3f;
            return;
        }
        
        layout.spacing = layout.count switch
        {
            7 => -0.55f,
            8 => -1.04f,
            9 => -1.435f,
            10 => -1.76f,
            _ => 0.55f
        };
    }

    private void DisplayCountIcon()
    {
        if (cards.Count > 3)
            cards[^1].ShowCountIcon();
    }
    
    private void DisappearCountIcon()
    {
        if (cards.Count != 0)
            cards[^1].HideCountIcon();
    }

    public void HoveringOtherCardAnimation(Transform hoveringCard)
    {
        var sign = -1f;
        foreach (var card in cards)
        {
            if (card.transform == hoveringCard)
            {
                sign *= -1;
                continue;
            }
            var offset = Vector3.right * sign * 0.48f;
            card.EaseAnimation(offset);
        }
    }

    public void ResetCanvasBodyPosition(PlayableActionCard expected = null)
    {
        cards.ForEach(card =>
        {
            if (card != expected)
                card.EaseAnimation(Vector3.zero);
        });
    }
    
    public PlayableActionCard GetCard(int timestamp)
    {
        return cards.Find(card => card.timestamp == timestamp);
    }
    
    private Transform GetDraggingCardTransform()
    {
        var dragging = DraggingCard;
        return ReferenceEquals(dragging, null) ? transform : dragging.transform;
    }
    
    private void SetInteractableForAllCards(bool value)
    {
        foreach (var card in cards)
            card.SetInteractable(value);
    }
}