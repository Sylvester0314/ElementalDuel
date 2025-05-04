using System;
using System.Collections.Generic;
using System.Linq;
using Client.Logic.Request;
using Client.Logic.Response;
using Server.GameLogic;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;

public class DiceFunction : MonoBehaviour
{
    public Global global;
    public CanvasGroup canvas;
    public DiceDisplayer displayer;
    public DiceSelector selector;

    [Header("In Game Data")]
    public bool tuning;
    public List<DiceEntity> DiceEntities = new ();
    public Dictionary<string, DiceEntity> Map = new ();
    
    [Header("Reroll Data")]
    public int rerollTimes = -1;
    public List<DiceLogic> RerollDices;

    public string Times => rerollTimes.ToString();

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            displayer.header.count.text = value.ToString();
            global.indicator.selfCountdown.diceCount.Count = value;
        }
    }
    private int _count;

    public void ResetLayout()
    {
        selector.CloseBackground();
        foreach (var entity in DiceEntities)
        {
            entity.SetSelectStatus(false);
            entity.SetLockingStatus(false);            
        }
        tuning = false;
    }
    
    public void OpenChooseDiceUI(List<string> ids)
    {
        var dices = ids.Select(id => Map[id]).ToList();
        foreach (var entity in dices)
            entity.SetSelectStatus(true);

        var count = dices.Count;
        selector.maxChoosing = count;
        selector.currentChoosing = count;
        selector.OpenBackground();
        global.SetTurnInformationStatus(false);
    }

    #region Base Operation

    public void Clear()
    {
        StaticMisc.DestroyAllChildren(displayer.dices);
        StaticMisc.DestroyAllChildren(selector.dices);

        Count = 0;
        DiceEntities.Clear();
        Map.Clear();
    }

    public List<string> GetSelectingDices()
    {
        return DiceEntities
            .Where(entity => entity.Selecting)
            .Select(entity => entity.Logic.Id)
            .ToList();
    }

    public void Append(List<DiceLogic> dices)
    {
        var diceEntities = InitializeEntities(dices);
        
        foreach (var entity in diceEntities)
        {
            var index = DiceEntities.FixedBinarySearch(entity);
            DiceEntities.Insert(index, entity);
            Map.Add(entity.Logic.Id, entity);

            entity.Small.transform.SetParent(displayer.dices, false);
            entity.Large.transform.SetParent(selector.dices, false);
            
            entity.Small.transform.SetSiblingIndex(index);
            entity.Large.transform.SetSiblingIndex(index);
        }

        Count += dices.Count;
    }

    public void Remove(List<DiceLogic> dices)
    {
        var ids = dices.Select(entity => entity.Id).ToList();
        
        DiceEntities
            .Where(entity => ids.Contains(entity.Logic.Id))
            .ToList()
            .ForEach(entity =>
            {
                Destroy(entity.Small.gameObject);
                Destroy(entity.Large.gameObject);
                DiceEntities.Remove(entity);
                Map.Remove(entity.Logic.Id);
            });

        Count -= ids.Count;
    }

    #endregion
    
    #region Element Tuning

    public DiceEntity PrioriElementalTuning(CostType element)
    {
        var invalidDices = DiceEntities
            .Where(entity => !entity.Logic.Match(element))
            .Reverse()
            .ToList();

        return invalidDices.Count == 0 ? null : invalidDices.First();
    }

    public void PreviewElementalTuning(DiceLogic target, List<string> disableIds, CostType element, int card)
    {
        var dice = Map[target.Id];

        dice.SetSelectStatus(true);
        foreach (var id in disableIds)
            Map[id].SetLockingStatus(true);
        
        selector.maxChoosing = 1;
        selector.currentChoosing = 1;
        tuning = true;

        ValueTuple<string, Action> buttonParams = ("elemental_tuning", () =>
        {
            var choosingList = DiceEntities
                .Where(entity => entity.Selecting)
                .Select(entity => entity.Logic)
                .ToList();

            if (choosingList.Count == 0)
            {
                global.prompt.dialog.Display("select_dice_transform");    
                return;
            }

            var choosing = choosingList.First();
            var request = new TuningRequest(TuningAction.Finish, choosing, card);
            var wrapper = ActionRequestWrapper.Create(request);
            global.manager.RequestServerRpc(wrapper);
        });

        global.indicator.Close(false);
        global.prompt.tuning.Display((target.Type, element));
        global.prompt.button.Display(buttonParams);
        selector.OpenBackground();
    }

    public void DoElementalTuning(DiceLogic target, int timestamp)
    {
        tuning = false;
        global.prompt.CloseAll();
        selector.CloseBackground();
        
        foreach (var entity in DiceEntities)
            entity.SetLockingStatus(false);
        
        Remove(target.SingleList());
        Append(target.SingleList());
        
        global.SetTurnInformationStatus(true);
        global.hand.RemoveActionCard(timestamp);
    }
    
    #endregion

    #region Dice Entity

    private List<DiceEntity> InitializeEntities(List<DiceLogic> dices)
    {
        return dices
            .Select(CreateDiceEntity)
            .OrderByDescending(v => v.Logic.Weight)
            .ToList();
    }

    private DiceEntity CreateDiceEntity(DiceLogic logic)
    {
        var small = displayer.CreateDice(logic);
        var large = selector.CreateDice(logic);
        
        return new DiceEntity(small, large, logic);
    }

    #endregion
}