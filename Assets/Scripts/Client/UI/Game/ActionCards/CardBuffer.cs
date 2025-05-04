using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Logic.Request;
using DG.Tweening;
using Server.GameLogic;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

public class CardBuffer : MonoBehaviour
{
    public Global global;
    public HandCards selfHand;
    public OpponentHand oppoHand;
    public Deck selfDeck;
    public Deck oppoDeck;
    public AutomaticLayout layout;
    
    [Header("Animation Settings")]
    public float selfHandDuration = 0.4f;
    
    [Header("References")]
    public Cardholder placeholder;
    public Material dissolve;
    
    [Header("In Game Data")]
    public List<Cardholder> placeholders;
    
    public async Task AppendCards(List<ActionCardInformation> infos)
    {
        StaticMisc.DestroyAllChildren(transform);

        var tasks = infos
            .OrderBy(information => information.name)
            .Select(async information => 
                await Instantiate(placeholder).Initialize(information)
            );
        var result = await Task.WhenAll(tasks);
        
        var index = infos.Count;
        var holders = result
            // Sort by card id order
            // .OrderByDescending(cardholder => cardholder.asset)
            // Assign the card a z offset in the buffer to ensure
            // that the card is displayed in the hierarchy
            .Select(cardholder => cardholder.SetAttribute(transform, index--));
            // Re-order the cards according to the index to ensure
            // that the animation sequence is based on the order of the deck
            // .OrderBy(cardholder => cardholder.index);
            
        placeholders = holders.ToList();

        await Task.Yield();
        
        layout.InitLayoutData();
        layout.RefreshOriginPosition();
        layout.spacing = CalculateSpacing(placeholders.Count);
        layout.StaticLayout();
    }

    public async Task StartSwitchCard(bool isStarting)
    {
        var mainEntry = isStarting ? "header_starting_hand" : "header_switch_hand";
        global.prompt.header.Display((mainEntry, "header_select"));
        
        ValueTuple<string, Action> param = ("confirm", () =>
        {
            var returns = selfDeck.drawing
                .Where(card => card.canSwitch && card.isSelecting)
                .ToList();

            selfDeck.drawing.ForEach(card => card.canSwitch = false);
            returns.ForEach(card => card.CloseSelectIcon());

            global.prompt.button.Hide();
            global.information.CloseAll();

            var request = new SwitchCardRequest(returns, isStarting);
            var wrapper = ActionRequestWrapper.Create(request);
            global.manager.RequestServerRpc(wrapper);
        });
        global.prompt.button.Display(param);

        selfDeck.drawing.ForEach(card => card.canSwitch = true);
        await Task.CompletedTask;
    }

    public async Task SwitchCards(List<ActionCardInformation> infos, bool isStarting)
    {
        if (infos.Count != 0)
            await ReturnCardsAndRedraw(Site.Self, infos);

        if (isStarting)
            global.manager.SynchronousServerRpc("switch_cards", "Client.Logic.Request.SwitchCardRequest");
        else
            await SwitchingCardsMoveToHand(false);
    }

    public async Task SwitchingCardsMoveToHand(bool isStarting)
    {
        global.prompt.CloseAll(true);
        await MoveToSelfHand(selfDeck.drawing);
        selfDeck.drawing = null;

        if (isStarting)
            global.GameStart();
    }

    private async Task ReturnCardsAndRedraw(Site site, List<ActionCardInformation> infos)
    {
        global.prompt.CloseAll();
        var switchingList = selfDeck.drawing.Where(card => card.isSelecting).ToList();
        var remainingList = selfDeck.drawing.Where(card => !card.isSelecting).ToList();
        
        var returnCompletion = new TaskCompletionSource<bool>();
        switchingList.ForEach(card => card.RotateToBack());
        switchingList[^1].CompleteAnimation = () =>
        {
            TweenCallback cb = () => DOVirtual.DelayedCall(
                0.05f, () => returnCompletion.SetResult(true)
            );
            
            var count = switchingList.Count;
            for (var i = 0; i < count; i++)
                switchingList[i].MoveToDeck(site, i == count - 1 ? cb : null);
        };

        await returnCompletion.Task;
        
        // Reassign the assets of switched cards
        for (var i = 0; i < infos.Count; i++)
        {
            var asset = await ResourceLoader.LoadSoAsset<ActionCardAsset>(infos[i].name);
            switchingList[i].SetAsset(asset, infos[i].timestamp);
        }
        var positions = placeholders
            .Select(cardholder => cardholder.transform.localPosition)
            .ToList();
        
        // Sort the new card sequence
        var count = placeholders.Count;
        placeholders = placeholders
            .OrderByDescending(cardholder => cardholder.asset)
            .Select((cardholder, i) => cardholder.SetAttribute(positions[i], count--))
            .ToList();
        
        remainingList.ForEach(card => card.Translation(0.15f));
        
        var delay = 0.1f;
        switchingList.ForEach(card =>
        {
            TweenCallback cb = () => card.MoveToBufferCenter(0);
            DOVirtual.DelayedCall(delay, () => card.MoveFromDeck(site, cb));
            delay += 0.15f;
        });
        
        var redrawCompletion = new TaskCompletionSource<bool>();
        switchingList
            .OrderByDescending(card => card.place.sort)
            .Last()
            .CompleteAnimation = () => redrawCompletion.SetResult(true);

        await redrawCompletion.Task;
    }

    public async Task MoveToSelfHand(List<DeckCard> drewList, List<DeckCard> overdrew = null)
    {
        if (overdrew != null)
            DissolveCards(overdrew);
        
        var amount = drewList.Count;
        if (amount <= 0)
            return;

        drewList = drewList.OrderByDescending(card => card.place.asset).ToList();
        var (isExtend, indexes) = await selfHand.AppendActionCards(drewList);
        
        var movementEndList = selfHand.layout.CalculateLayout();
        var hand = selfHand.transform;
        var delay = 0f;
        
        var completion = new TaskCompletionSource<bool>();
        for (var i = 0; i < amount; i++, delay += 0.05f)
        {
            var index = indexes[i];
            var origin = drewList[i];
            var target = movementEndList[index];
            var isLast = i == amount - 1;

            DOVirtual.DelayedCall(
                delay,
                () => origin.MoveToSelfHand(hand, isExtend, target, () =>
                {
                    var card = selfHand.cards[index];
                    card.transform.position = target;
                    card.canvasGroup.alpha = 1;
                    card.SetInteractable(isExtend);
                    Destroy(origin.gameObject);

                    if (!isLast)
                        return;

                    selfHand.isAppendingCards = false;
                    completion.SetResult(true);
                })
            );
        }

        for (var i = 0; i < selfHand.cards.Count; i++)
        {
            if (indexes.Contains(i))
                continue;
            
            selfHand.cards[i].transform
                .DOMove(movementEndList[i], selfHandDuration)
                .SetEase(Ease.OutCubic);
        }
        
        await completion.Task;
    }

    private void DissolveCards(List<DeckCard> cardList)
    {
        dissolve.SetFloat(Shader.PropertyToID("_DissolveAmount"), 1);
        cardList.ForEach(card => card.Dissolve());
    }
    
    private static float CalculateSpacing(int count)
    {
        return count switch
        {
            1 => 20f,
            2 => 10.3f,
            3 => 6.3f,
            4 => 3.9f,
            5 => 2.3f,
            6 => 1.1f,
            7 => 0.25f,
            8 => -0.4f,
            9 => -0.9f,
            10 => -1.35f,
            _ => (48 - count * 5.7705375f) / (count - 1)
        };
    }
}
