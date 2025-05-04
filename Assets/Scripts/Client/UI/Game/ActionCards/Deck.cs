using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Misc;
using DG.Tweening;
using Server.GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Deck : AbstractZone, IPointerClickHandler
{
    public Global global;
    public CardBuffer buffer;
    public OpponentHand opponentHand;

    [Header("In Game Data")] 
    public bool canClick;
    public List<DeckCard> drawing;
    
    [Header("Components")]
    public Transform deck;
    public TextMeshProUGUI countText;
    public CanvasGroup countIcon;
    public RectTransform countRect;
    
    [Header("Prefab References")]
    public DeckCard cardPrefab;
    
    private int _remaining;
    private Tween _tween;
    private Player _owner;
    private readonly Stack<DeckCard> _deck = new ();
    
    private const float Duration = 0.25f;
    
    public bool IsSwitching => (drawing != null && drawing.Count != 0 && drawing.First().canSwitch) 
                               || global.startingPhase;
    
    public void Initialize(Player player, int cardsCount)
    {
        _owner = player;
        _remaining = cardsCount;
        
        AppendCards(cardsCount);
    }
    
    public void AppendCards(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            var randomPositionX = Random.Range(0, 0.2f);
            var randomRotationZ = StaticMisc.RandomGaussian(0, 3);
            var card = Instantiate(cardPrefab, deck, false);
            
            card.transform.localRotation = Quaternion.Euler(0, 0, randomRotationZ);
            card.transform.localPosition = Vector3.right * randomPositionX;
            _deck.Push(card);
        }
    }

    #region Draw Animation

    public async Task Draw(List<ActionCardInformation> drewList, bool toHand = true)
    {
        _remaining -= drewList.Count;
        await buffer.AppendCards(drewList);
        
        var delay = 0.1f;
        drawing = buffer.placeholders
            .Select(cardholder =>
            {
                var deckCard = _deck.Pop();
                var site = _owner.site;
                TweenCallback cb = () => deckCard.MoveToBufferCenter();

                deckCard.ForcedCardRotation(10, true);
                deckCard.Initialize(cardholder, global);
                DOVirtual.DelayedCall(delay, () => deckCard.MoveFromDeck(site, cb));

                delay += 0.15f;
                return deckCard;
            })
            .ToList();

        if (toHand)
            return;

        var completion = new TaskCompletionSource<bool>();
        drawing[0].CompleteAnimation = () => completion.SetResult(true);
        
        await completion.Task;
    }
    
    public async Task DrawToSelfHand(List<ActionCardInformation> drew, List<ActionCardInformation> overdrew)
    {
        await Draw(drew.Concat(overdrew).ToList());

        var completion = new TaskCompletionSource<bool>();
        drawing[^1].CompleteAnimation = () => completion.SetResult(true);

        var timestamps = overdrew.Select(card => card.timestamp).ToList();
        var drewList = new List<DeckCard>();
        var overdrewList = new List<DeckCard>();
        
        foreach (var card in drawing)
        {
            var area = timestamps.Contains(card.timestamp)
                ? overdrewList : drewList;
            area.Add(card);
        }
        
        await completion.Task;
        await buffer.MoveToSelfHand(drewList, overdrewList);
        drawing = null;
    }

    public async Task DrawToOpponentHand(int amount)
    {
        var realAmount = Math.Min(amount, 10 - opponentHand.cards.Count);
        if (realAmount == 0)
            return;
        
        _remaining -= realAmount;
        
        var count = opponentHand.cards.Count;
        var indexes = await opponentHand.AppendCards(realAmount);
        var movementEndList = opponentHand.layout.CalculateLayout();
        var delay = 0.1f;
        
        movementEndList.Reverse();
        DOVirtual.DelayedCall(delay, () =>
        {
            for (var i = 0; i < count; i++)
                opponentHand.cards[i].transform
                    .DOMove(movementEndList[i], 0.25f)
                    .SetEase(Ease.OutCubic);
        });
        
        var completion = new TaskCompletionSource<bool>();
        drawing = indexes
            .Select((cardIndex, i) => 
            {
                var card = opponentHand.cards[cardIndex];
                var target = movementEndList[cardIndex];
                var deckCard = _deck.Pop();
                var site = _owner.site;

                card.cardRotation.ForceBack();
                card.transform.position = target;
                card.place.transform.position = target;
                card.place.transform.SetParent(opponentHand.transform, true);
                
                var isLast = i == indexes.Count - 1;
                TweenCallback cb = () =>
                {
                    card.canvas.alpha = 1;
                    deckCard.canvas.alpha = 0;
                    DOVirtual.DelayedCall(0.1f, () =>
                    {
                        Destroy(deckCard.place.gameObject);
                        Destroy(deckCard.gameObject);
                    });
                    
                    if (isLast)
                        completion.SetResult(true);
                };
                
                deckCard.ForcedCardRotation(100, true);
                deckCard.Initialize(card.place, global);
                DOVirtual.DelayedCall(delay, () => deckCard.MoveFromDeck(site, cb));
                
                delay += 0.1f;
                return deckCard;
            })
            .ToList();
        
        await completion.Task;
    }

    #endregion

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_tween != null && _tween.IsActive() && !_tween.IsComplete())
            return;
        if (_remaining == 0 || !canClick)
            return;
        
        countText.text = _remaining.ToString();
        countIcon.gameObject.SetActive(true);
        
        _tween = DOTween.Sequence()
            .Append(countRect.DOAnchorPosY(-0.2f, Duration))
            .Join(countIcon.DOFade(1, Duration))
            .Play()
            .OnComplete(() => _tween = DOVirtual.DelayedCall(Duration * 4, () =>
            {
                countIcon
                    .DOFade(0, Duration)
                    .OnComplete(() =>
                    {
                        countIcon.gameObject.SetActive(false);
                        countRect.DOAnchorPosY(-0.4f, 0);
                    });
            }));
    }
}
