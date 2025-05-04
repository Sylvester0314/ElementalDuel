using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;

public class OpponentHand : MonoBehaviour
{
    [Header("In Game Data")]
    public List<DeckCard> cards = new ();

    [Header("Components")]
    public Global global;
    public AutomaticLayout layout;
    public CardBuffer buffer;
    public Deck deck;
    
    [Header("References")]
    public DeckCard cardPrefab;
    public Cardholder holderPrefab;

    public async Task<List<int>> AppendCards(int amount)
    {
        var indexes = new List<int>();
        for (var i = 0; i < amount; i++)
        {
            var instance = Instantiate(cardPrefab, transform, false);
            var holder = Instantiate(holderPrefab);

            instance.Initialize(holder, global);
            instance.canvas.alpha = 0;
            holder.sort = 4000 + cards.Count;
            indexes.Add(cards.Count);
            cards.Add(instance);
        }

        await RefreshChangedLayout();
        return indexes;
    }

    public async void RemoveCard()
    {
        var card = cards.Pop();
        Destroy(card.gameObject);

        if (cards.Count == 0)
            return;
        
        await RefreshChangedLayout();
        layout.MovementLayout(0.25f, Ease.OutCubic);
    }

    public async Task SwitchCards(int amount)
    {
        if (amount == 0)
            return;
        
        var count = cards.Count;
        amount = Mathf.Min(count, amount);
        
        var completion = new TaskCompletionSource<bool>();
        for (var i = count - 1; i >= count - amount; i--)
        {
            var card = cards[i];
            card.place = Instantiate(holderPrefab, transform, false);
            card.place.sort = 3000 + i;
            card.place.transform.localPosition = card.transform.localPosition;
            card.transform.SetParent(deck.transform, true);
            card.ForcedCardRotation(100, true);
            
            TweenCallback cb = i != count - amount 
                ? null 
                : () => completion.SetResult(true);
            card.MoveToDeck(Site.Opponent, cb);
        }
        
        await completion.Task;
        
        var returns = cards.GetRange(count - amount, amount);
        await BackToHand(returns);
    }

    public async Task BackToHand(List<DeckCard> returns)
    {
        var delay = 0.1f;
        var completion = new TaskCompletionSource<bool>();
        for (var i = 0; i < returns.Count; i++)
        {
            var card = returns[i];
            var isLast = i == returns.Count - 1;
            
            card.ForcedCardRotation(100, true);
            DOVirtual.DelayedCall(delay, () => card.MoveFromDeck(
                Site.Opponent, 
                () =>
                {
                    card.transform.SetParent(transform, true);
                    Destroy(card.place.gameObject);

                    if (isLast)
                        completion.SetResult(true);
                })
            );
            
            delay += 0.1f;
        }
        
        await completion.Task;
    }
    
    private void RefreshSpacingLayout()
    {
        var positions = layout.originPositions;
        
        for (var i = 0; i < layout.count; i++)
        {
            var position = positions[i];
            position.z = i * 0.001f;
            positions[i] = position;
        }
    }

    private async Task RefreshChangedLayout()
    {
        await Task.Yield();
     
        layout.InitLayoutData();
        layout.RefreshOriginPosition();
        RefreshSpacingLayout();
    }
}