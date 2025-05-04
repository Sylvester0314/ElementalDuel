using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Logic.Request;
using DG.Tweening;
using Server.GameLogic;
using Shared.Enums;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class DiceReroll : MonoBehaviour
{
    public Image darkBackground;
    public Header header;
    public Signal signal;
    public ConfirmButton button;
    public Transform area;
    public GridLayoutGroup grid;
    
    [Header("Configurations")]
    public float rerollDuration;
    public AnimationCurve speedCurve;
    
    [Header("Prefab References")]
    public LargeDice dicePrefab;

    [Header("In Game Data")] 
    public bool currentOperationStatus;
    
    private Global _game;
    private DiceFunction _function;

    private Dictionary<int, Vector3> _positions;
    private Dictionary<string, DiceEntity> _entities;
    private Dictionary<CostType, List<CostType>> _map;

    public void Start()
    {
        _game = FindObjectOfType<Global>();
        _game.reroll = this;

        _map = ResourceLogic.DiceTypes
            .ToDictionary(
                type => type,
                type => ResourceLogic.DiceTypes.Where(t => t != type).ToList()
            );

        _positions = new Dictionary<int, Vector3>();
        _entities = new Dictionary<string, DiceEntity>();
        _function = _game.diceFunction;
        
        SelectingLayout();
        InitializeDices(() => _game.sceneLoader.ActiveFlag());
    }

    private async void InitializeDices(Action callback)
    {
        var dices = _function.RerollDices
            .OrderByDescending(dice => dice.Weight)
            .ToDictionary(dice => dice.Id, dice => dice);
        _function.Clear();
        _function.Append(_function.RerollDices);

        foreach (var (_, dice) in dices)
        {
            var large = Instantiate(dicePrefab, area, false);
            var entity = new DiceEntity(large, dice);
            _entities.Add(dice.Id, entity);
            
            await large.SetStyle(dice.Type);
            large.SetEntity(entity);
            large.SetScale(1.55f);
            large.SetEnterSelectable(true);
        }

        await Task.Delay(30);

        var index = 0;
        foreach (Transform child in area)
            _positions.Add(index++, child.position);
        
        callback?.Invoke();
    }

    private void SelectingLayout()
    {
        var times = _function.rerollTimes;
        if (times > 1)
        {
            var pattern = ResourceLoader.GetLocalizedUIText("roll_times");
            var timesStr = pattern.Replace("$[Times]", _function.Times);
            signal.Display(timesStr, false);
        }
        else if (times == 0)
        {
            Synchronous();
            return;
        }
        
        foreach (var (_, entity) in _entities)
            entity.Large.SetInteractable(true);
        
        ValueTuple<string, Action> buttonParams = ("confirm", () =>
        {
            header.Hide();
            button.Hide();
            signal.Hide();
            
            var selectingList = _entities.Values
                .Where(entity => entity.Selecting)
                .Select(entity => entity.Logic.Id)
                .ToList();
            
            foreach (var (_, entity) in _entities)
            {
                entity.SetSelectStatus(false);
                entity.Large.SetInteractable(false);
            }
            
            // If no dice are selected, the current phase ends immediately
            if (selectingList.Count == 0)
            {
                Synchronous();
                return;
            }

            var request = new RerollRequest(selectingList);
            var wrapper = ActionRequestWrapper.Create(request);
            _game.manager.RequestServerRpc(wrapper);
        });
        
        header.Display(("header_reroll", "header_select_reroll"));        
        button.Display(buttonParams);
    }

    public void WaitingLayout()
    {
        header.Display("header_roll_result");
        signal.Display("roll_complete");
    }

    public async Task RerollAnimation(List<DiceLogic> dices)
    {
        var elapsed = 0f;
        var rand = new Random();
        
        while (elapsed < rerollDuration)
        {
            var t = elapsed / rerollDuration;
            var interval = speedCurve.Evaluate(t);
            elapsed += interval;
            
            foreach (var dice in dices)
            {
                var entity = _entities[dice.Id];
                var lastType = entity.Logic.Type;
                var nextType = elapsed >= rerollDuration
                    ? dice.Type
                    : _map[lastType][rand.Next(7)];
                
                await entity.Large.SetStyle(nextType);
            }
            
            await Task.Delay(Mathf.RoundToInt(interval * 1000));
        }

        _function.Remove(dices);
        _function.Append(dices);
        _function.rerollTimes -= 1;
        
        await Task.Delay(50);

        grid.enabled = false;
        for (var i = 0; i < _positions.Count; i++)
        {
            var id = _function.DiceEntities[i].Logic.Id;
            var large = _entities[id].Large;

            large.transform.DOMove(_positions[i], 0.2f);
        }
        
        await Task.Delay(50);
        
        SelectingLayout();
    }
    
    private void Synchronous()
    {
        _game.manager.SynchronousServerRpc("reroll", "Client.Logic.Request.RerollRequest");
    }

    public void Exit(Action afterExit)
    {
        _game.reroll = null;
        _function.rerollTimes = -1;
        _function.RerollDices = null;
        _function.canvas.alpha = 1;
        
        _game.sceneLoader.UnloadCurrentScene(afterExit);
    }
}