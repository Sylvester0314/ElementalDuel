using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Shared.Enums;
using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DeckCardInformation : MonoBehaviour, IPointerClickHandler
{
    [Header("Base Components")] 
    public CanvasGroup body;
    public CanvasGroup dark;
    public CanvasGroup mainBody;
    public InventoryDeckCard displayCard;
    
    [Header("Common Components")]
    public Image elementIcon;
    public GameObject elementWrapper;
    public LocalizeStringEvent nameEvent;
    public RectTransform tags;
    public List<Sprite> icons;

    [Header("Extend Components")] 
    public TextMeshProUGUI descriptionText;
    public LocalizeStringEvent descriptionEvent;
    public LocalizeStringEvent conditionsEvent;
    public RectTransform skills;
    public RectTransform content;
    public RectTransform emptyScroll;
    public ScrollRect scrollRect;

    [Header("Switch Page Components")] 
    public MiddleButton prevButton;
    public MiddleButton nextButton;
    
    [Header("Prefabs References")] 
    public TagItem propertyPrefab;
    public DeckSkillItem skillPrefab;
    
    private int _currentIndex;
    private List<ICardAsset> _currentAssets;
    private Tween _switchTween1;
    private Tween _switchTween2;
    private readonly List<TextMeshProUGUI> _texts = new ();
    private readonly List<DeckSkillItem> _skills = new ();
    
    private readonly List<Property> _weaponProperties =
        new List<Property>()
        {
            Property.WeaponBow,
            Property.WeaponSword,
            Property.WeaponClaymore,
            Property.WeaponCatalyst,
            Property.WeaponPole,
            Property.WeaponNone
        };
        
    private readonly List<Property> _elementProperties =
        new List<Property>()
        {
            Property.ElementCryo,
            Property.ElementHydro,
            Property.ElementPyro,
            Property.ElementElectro,
            Property.ElementGeo,
            Property.ElementDendro,
            Property.ElementAnemo
        };

    public void FadeIn()
    {
        gameObject.SetActive(true);
        dark.DOFade(1, 0.18f).SetEase(Ease.OutSine);
        body.DOFade(1, 0.18f).SetEase(Ease.OutSine);
    }

    public void FadeOut()
    {
        dark.DOFade(0, 0.18f).SetEase(Ease.OutSine);
        body.DOFade(0, 0.18f).SetEase(Ease.OutSine)
            .OnComplete(() => gameObject.SetActive(false));
    }

    public void Open(List<ICardAsset> assets, ICardAsset current)
    {
        _currentAssets = assets;
        _currentIndex = assets.IndexOf(current);
        
        FadeIn();
        SetDisplay(_currentIndex);
    }

    private void SetDisplay(int index, int direction = 0)
    {
        prevButton.gameObject.SetActive(index != 0);
        nextButton.gameObject.SetActive(index != _currentAssets.Count - 1);

        prevButton.Callback = () => SetDisplay(index - 1, -1);
        nextButton.Callback = () => SetDisplay(index + 1, 1);

        if (direction != 0)
        {
            mainBody.alpha = 0;
            mainBody.transform.localPosition = new Vector3(direction * 15, -4);
            _switchTween1?.Kill();
            _switchTween2?.Kill();
            
            _switchTween1 = mainBody.DOFade(1, 0.2f).SetEase(Ease.OutSine);
            _switchTween2 = mainBody.transform.DOLocalMoveX(0, 0.2f).SetEase(Ease.OutSine);
        }
        
        var asset = _currentAssets[index];
        
        displayCard.SetCardStyle(asset);
        nameEvent.SetEntry(asset.Name);

        scrollRect.verticalNormalizedPosition = 1;
        elementWrapper.SetActive(false);
        emptyScroll.sizeDelta = Vector2.zero;
        StaticMisc.DestroyAllChildren(tags);

        if (asset is ActionCardAsset actionAsset)
            AsAction(actionAsset);

        if (asset is CharacterAsset characterAsset)
            AsCharacter(characterAsset);
        
        DOVirtual.DelayedCall(0.05f, () =>
            LayoutRebuilder.ForceRebuildLayoutImmediate(content)
        );
        DOVirtual.DelayedCall(0.1f, () =>
        {
            var diffHeight = 199 - content.sizeDelta.y;
            if (diffHeight > 0)
                emptyScroll.sizeDelta = new Vector2(312, diffHeight);
        });
    }

    private void AsAction(ActionCardAsset data)
    {
        descriptionEvent.SetEntry(data.description);
        descriptionEvent.gameObject.SetActive(true);
        skills.gameObject.SetActive(false);

        var hasCondition = data.buildCondition.Count != 0;
        if (hasCondition)
            conditionsEvent.SetEntry(data.buildCondition[0].conditionDescription);
        conditionsEvent.gameObject.SetActive(hasCondition);
        
        NewProperty().Initialize(ParseTag(data.cardType));
        data.Properties
            .Where(property => (int)property <= 100).ToList()
            .ForEach(property => NewProperty().Initialize(ParseTag(property)));
        LayoutRebuilder.ForceRebuildLayoutImmediate(tags);
    }
    
    private void AsCharacter(CharacterAsset data)
    {
        descriptionEvent.gameObject.SetActive(false);
        skills.gameObject.SetActive(true);

        var elementType = data.Properties[0];
        var elementIndex = _elementProperties.FindIndex(p => p == elementType);
        elementIcon.sprite = icons[elementIndex];
        elementWrapper.SetActive(true);
        
        data.Properties.Skip(1).ToList()
            .ForEach(property => NewProperty().Initialize(ParseTag(property)));
        LayoutRebuilder.ForceRebuildLayoutImmediate(tags);

        if (_skills.Count == 0)
            CreateSkillItems(data.skillList);
        else
            ReplaceSkillItems(data.skillList);

        LayoutRebuilder.ForceRebuildLayoutImmediate(skills);
    }

    private TagItem NewProperty()
    {
        return Instantiate(propertyPrefab, tags, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var extra = _texts.Any(text => KeywordClickChecker(eventData, text));
        if (!extra && descriptionText.gameObject.activeSelf)
            KeywordClickChecker(eventData, descriptionText);
    }

    private bool KeywordClickChecker(PointerEventData eventData, TextMeshProUGUI text)
    {
        var index = TMP_TextUtilities.FindIntersectingLink(
            text, eventData.position,
            eventData.pressEventCamera
        );

        if (index == -1)
            return false;

        var linkId = text.textInfo.linkInfo[index].GetLinkID();
        DisplayKeywordInfo(linkId);
        
        return true;
    }

    private void DisplayKeywordInfo(string key)
    {
        // TODO Deck Card Information 中的点击关键词显示
        Debug.Log(key);
    }

    private ValueTuple<string, int, bool> ParseTag(Property property)
    {
        var key = $"property_{property.ToSnakeCase().ToLower()}";
        var index = _weaponProperties.Contains(property) &&
                    _currentAssets[_currentIndex] is CharacterAsset 
            ? 0 : 1;
        return (key, index, true);
    }

    private ValueTuple<string, int, bool> ParseTag(ActionCardType type)
    {
        var key = $"action_card_name_type_{type.ToString().ToLower()}";
        return (key, 0, true);
    }
    
    private void CreateSkillItems(List<SkillAsset> assets)
    {
        var newSkills = assets.Select(asset =>
        {
            var item = Instantiate(skillPrefab, skills, false);
            item.SetInformation(asset);
            _texts.Add(item.descriptionText);
            return item;
        }).ToList();
        
        _skills.AddRange(newSkills);
    }

    private void ReplaceSkillItems(List<SkillAsset> assets)
    {
        var currentCount = _skills.Count;
        var assetCount = assets.Count;
    
        for (var i = 0; i < currentCount; i++)
        {
            var skill = _skills[i];
            if (i < assetCount)
                skill.SetInformation(assets[i]);
            else
            {
                _texts.Remove(skill.descriptionText);
                Destroy(skill.gameObject);
            }
        }

        var diffCount = currentCount - assetCount;
        if (diffCount > 0)
        {
            _skills.RemoveRange(assetCount, diffCount);
            return;            
        }
    
        var newAssets = assets.GetRange(currentCount, -diffCount);
        if (currentCount < assetCount)
            CreateSkillItems(newAssets);
    }
}