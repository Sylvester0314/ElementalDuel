using Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Shared.Classes;
using UnityEngine;

public enum CombatTransfer
{
    Transparent,
    Active,
    Switch,
    Choose
}

public class CombatActionUI : MonoBehaviour
{
    public Global global;
    
    [Header("References")]
    public SkillButtonList switchActive;
    public SkillButtonList chooseActive;
    public ChooseActiveButton chooseActiveButton;
    public SkillButtonList skillButtonListPrefab;
    public SkillAsset switchActiveAsset;
    
    [Header("In Game Data")]
    public bool forced;
    public bool choosing;
    public string currentKey = "";

    private Dictionary<string, SkillButtonList> _actions;

    public void Initialize()
    {
        var switchButton = switchActive.GetSubButton(0);
        if (switchButton is UseSkillButton useSwitchButton)
        {
            var skill = CharacterSkill.FromStatic(global, switchActiveAsset);
            useSwitchButton.Awake();
            useSwitchButton.SetSkillData(skill);
            useSwitchButton.SetParent(switchActive);
            
            switchActive.buttons.Add(useSwitchButton);
        }

        var chooseButton = chooseActive.GetSubButton(0);
        chooseButton.SetParent(chooseActive);
        chooseActive.buttons.Add(chooseButton);

        _actions = new Dictionary<string, SkillButtonList>
        {
            { "switch_active", switchActive.SetInformation("switch_active") },
            { "choose_active", chooseActive.SetInformation("choose_active") }
        };
    }

    private void Display(string key)
    {
        if (currentKey == key)
            return;

        var flag = false;
        if (currentKey.Length != 0)
            flag = _actions[currentKey].FadeOut();
        if (key.Length != 0)
            flag = _actions[key].FadeIn();

        currentKey = key;
        if (!flag)
            return;

        if (SkillButtonList.IsAnimating)
            SkillButtonList.Delay?.Kill();
        else
        {
            SkillButtonList.IsAnimating = true;
            DOVirtual.DelayedCall(
                SkillButtonList.AnimationDuration,
                () => SkillButtonList.IsAnimating = false
            );
        }
    }

    public void Append(CharacterCard card)
    {
        var instance = Instantiate(
            skillButtonListPrefab,
            switchActive.transform.parent, false
        );
        instance.SetInformation(card, global);
        _actions.Add(card.uniqueId, instance);
    }

    public void ForcedTransferStatus(CombatTransfer transfer)
    {
        forced = true;
        Transfer(transfer);
    }

    public void TransferStatus(CombatTransfer transfer)
    {
        if (forced || global.hand.usingCard || global.hand.isExtending || choosing)
            return;
        
        Transfer(transfer);
    }

    private void Transfer(CombatTransfer transfer)
    {
        var active = global.GetZone<CharacterZone>(Site.Self).Active;
        if (active == null && transfer == CombatTransfer.Active)
            return;
        
        var key = transfer switch
        {
            CombatTransfer.Active => active.uniqueId,
            CombatTransfer.Switch => "switch_active",
            CombatTransfer.Choose => "choose_active",
            CombatTransfer.Transparent => "",
            _ => null
        };

        if (transfer is CombatTransfer.Choose)
            choosing = true;
            
        Display(key);
    }

    public void ResetLayout()
    {
        if (currentKey.Length == 0 || !gameObject.activeInHierarchy || global.startingPhase || choosing)
            return;
        
        _actions[currentKey].Reset();
    }

    public void SetStatus(bool status)
    {
        if (!status)
        {
            _actions.Values
                .Where(list => list.gameObject.activeSelf)
                .ToList()
                .ForEach(list => list.FadeOut());
            DOVirtual.DelayedCall(
                SkillButtonList.AnimationDuration,
                () => gameObject.SetActive(false)
            );
            return;
        }
        
        gameObject.SetActive(true);
        Display(currentKey);
    }

    public UseSkillButton GetSkill(string character, string skill)
    {
        var buttons = string.IsNullOrEmpty(character)
            ? switchActive.buttons
            : _actions[character].buttons;

        return buttons.Find(button => button.Key == skill) as UseSkillButton;
    }

    public void NetworkSynchronous(Dictionary<string, CostMatchResult> skills)
    {
        var activeKey = global.GetZone<CharacterZone>(Site.Self).Active.uniqueId;

        var activeList = _actions[activeKey];
        var switchList = _actions["switch_active"];

        foreach (var button in activeList.buttons)
            if (skills.TryGetValue(button.Key, out var result))
                button.NetworkSynchronous(result);

        var switchResult = skills["switch_active"];
        switchList.buttons.First().NetworkSynchronous(switchResult);
    }
}