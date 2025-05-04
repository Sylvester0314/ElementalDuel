using System.Collections.Generic;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CardInformation : AbstractInformationComponent
{
    public ScrollRect scrollRect;

    [Header("Normal Components")]
    public LocalizeStringEvent nameEvent;
    public RectTransform cardName;
    public RectTransform tags;
    public RectTransform negative;
    public RectTransform header;
    public VerticalLayoutGroup layout;

    [Header("Action Card Components")]
    public LocalizeStringEvent typeEvent;
    public LocalizeStringEvent descriptionEvent;
    public KeywordReplacer keywordReplacer;

    [Header("Character Card Components")]
    public RectTransform skills;

    [Header("Prefab References")]
    public Tag tagPrefab;
    public SkillItem skillItemPrefab;

    private bool _isShowingCharacter;
    private List<SkillItem> _skills;
    private CostSetComponent _costs;

    public void Awake()
    {
        _skills = new List<SkillItem>();
        _costs = transform.GetComponent<CostSetComponent>();
    }
    
    private void SetComponentsActive(bool active)
    {
        _costs.costs.gameObject.SetActive(active);
        typeEvent.gameObject.SetActive(active);
        descriptionEvent.gameObject.SetActive(active);
        skills.gameObject.SetActive(!active);
        tags.gameObject.SetActive(true);
    }
    
    private void SetSpacingLayout(float spacing, float height, float tagsHeight)
    {
        layout.spacing = spacing;
        negative.sizeDelta = new Vector2(negative.sizeDelta.x, spacing * -2f);
        header.sizeDelta = new Vector2(header.sizeDelta.x, height);
        tags.sizeDelta = new Vector2(tags.sizeDelta.x, tagsHeight);
    }
    
    private void SwitchToActionCardLayout()
    {
        SetComponentsActive(true);
        SetSpacingLayout(1.65f, 3f, 6.2f);
        
        foreach (var skill in _skills)
            Destroy(skill.gameObject);
        _skills.Clear();
    }
    
    private void SwitchToCharacterCardLayout()
    {
        SetComponentsActive(false);
        SetSpacingLayout(1.45f, 3.2f, 6.8f);
    }

    public void Update()
    {
        negative.gameObject.SetActive(cardName.sizeDelta.y > 9f);
        ForceRebuildLayoutImmediate();
    }

    private bool DisplayActionCard(PlayableActionCard card)
    {
        if (_isShowingCharacter)
            SwitchToActionCardLayout();

        ActionCardStyle(card.asset);
        card.SynchronousCost.RefreshCostDisplay(_costs);
        
        return false;
    }

    private bool DisplayStatusCard(StatusCard card)
    {
        if (_isShowingCharacter)
            SwitchToActionCardLayout();

        ActionCardStyle(card.asset.relatedCard);
        _costs.costs.gameObject.SetActive(false);
        tags.gameObject.SetActive(false);
        
        return false;
    }
    
    private bool DisplayDeckCard(DeckCard card)
    {
        if (_isShowingCharacter)
            SwitchToActionCardLayout();
        
        ActionCardStyle(card.place.asset);
        card.CostLogic.RefreshCostDisplay(_costs);
        
        return false;
    }
    
    private void ActionCardStyle(ActionCardAsset asset)
    {
        nameEvent.SetEntry(asset.cardName);
        typeEvent.SetEntry(asset.cardType switch
        {
            ActionCardType.Support => "action_card_name_type_support",
            ActionCardType.Event => "action_card_name_type_event",
            ActionCardType.Equipment => "action_card_name_type_equipment",
            ActionCardType.Summon => "action_card_name_type_summon",
            _ => string.Empty
        });
        descriptionEvent.SetEntry(asset.description);

        descriptionEvent.RefreshString();
        ForceRebuildLayoutImmediate();
        
        StaticMisc.DestroyAllChildren(tags.transform);
        if (asset.cardType == ActionCardType.Summon)
            return;
        
        foreach (var property in asset.Properties)
        {
            if ((int)property > 100)
                continue;
            
            var instance = Instantiate(tagPrefab);
            instance.SetInformation(property);
            instance.SetParent(tags.transform);
        }
        tags.gameObject.SetActive(asset.Properties.Count != 0);
    }

    private bool DisplayCharacterCard(CharacterCard card)
    {
        if (!_isShowingCharacter)
            SwitchToCharacterCardLayout();
        
        var asset = card.asset;
        nameEvent.SetEntry(asset.characterName);
        
        StaticMisc.DestroyAllChildren(tags.transform);
        foreach (var property in asset.Properties)
        {
            var instance = Instantiate(tagPrefab);
            instance.SetInformation(property);
            instance.SetParent(tags.transform);
        }
        
        if (_skills.Count == 0)
            CreateSkillItems(card.Skills);
        else
            ReplaceSkillItems(card.Skills);

        LayoutRebuilder.ForceRebuildLayoutImmediate(skills);
        ForceRebuildLayoutImmediate();
        
        return true;
    }

    private void CreateSkillItems(List<CharacterSkill> skillList)
    {
        foreach (var skill in skillList)
        {
            var instance = Instantiate(skillItemPrefab);
            instance.SetInformation((skill, true));
            instance.SetInfoBoxAttribute(scrollRect, this);
            instance.SetParent(skills.transform);
            _skills.Add(instance);
        }
    }

    private void ReplaceSkillItems(List<CharacterSkill> skillList)
    {
        var currentCount = _skills.Count;
        var skillListCount = skillList.Count;

        for (var i = 0; i < currentCount; i++)
        {
            var skill = _skills[i];
            if (i < skillListCount)
                skill.SetInformation((skillList[i], true));
            else
                Destroy(skill.gameObject);
        }

        if (currentCount > skillListCount)
        {
            _skills.RemoveRange(skillListCount, currentCount - skillListCount);
            return;            
        }

        var extraSkill = skillList.GetRange(
            currentCount, skillListCount - currentCount);
        if (currentCount < skillListCount)
            CreateSkillItems(extraSkill);
    }
    
    public override void SetInformation<T>(T data)
    {
        _costs.InitializeCostList("_Small");
        _isShowingCharacter = data switch
        {
            CharacterCard character   => DisplayCharacterCard(character),
            PlayableActionCard action => DisplayActionCard(action),
            DeckCard deckCard         => DisplayDeckCard(deckCard),
            StatusCard statusCard     => DisplayStatusCard(statusCard),
            _                         => false
        };
    }
    
    public void ClickActionCardDescription()
    {
        ui.Display(keywordReplacer.UniqueKeywords);
    }
    
    public void OnBeginDrag(BaseEventData eventData)
    {
        if (scrollRect != null && eventData is PointerEventData pointerEventData)
            scrollRect.OnBeginDrag(pointerEventData);
    }
    
    public void OnDrag(BaseEventData eventData)
    {
        if (scrollRect != null && eventData is PointerEventData pointerEventData)
            scrollRect.OnDrag(pointerEventData);
    }
    
    public void OnEndDrag(BaseEventData eventData)
    {
        if (scrollRect != null && eventData is PointerEventData pointerEventData)
            scrollRect.OnEndDrag(pointerEventData);
    }
}
