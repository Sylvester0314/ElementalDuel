using System;
using Shared.Enums;
using System.Collections.Generic;
using DG.Tweening;
using Shared.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SkillItem : AbstractInformationComponent
{
    public RectTransform mainRoot;
    
    [Header("Preview Components")]
    public RectTransform previewRoot;
    public GameObject normalIconBackground;
    public GameObject burstIconBackground;
    public RectTransform baseInformation;
    public LocalizeStringEvent skillName;
    public Image skillIcon;
    public Image background;
    public Color highlightColor;
    
    [Header("Arrow Components")]
    public GameObject burstButtonBackground;
    public RectTransform arrow;
    public AnimationCurve curve;
    
    [Header("Detail Components")] 
    public GameObject detailRoot;
    public LocalizeStringEvent skillType;
    public GameObject subCards;
    public KeywordReplacer description;

    [Header("Frame Components")]
    public RectTransform previewBackground;
    public RectTransform outline;
    public RectTransform lightOutline;
    
    [Header("Prefab References")]
    public SubCardItem subCardPrefab;

    private bool _isExpending;
    private bool _isDragging;
    private bool _detailSet;
    private float _height;
    private HashSet<string> _keywords;

    private string _descriptionText;

    private ScrollRect _scroll;
    private CharacterSkill _skill;
    private AbstractInformationComponent _parent;
    private CostSetComponent _costs;
    
    protected void Awake()
    {
        _keywords = new HashSet<string>();
        _costs = transform.GetComponent<CostSetComponent>();
    }

    public void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(baseInformation);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        var backgroundHeight = baseInformation.rect.height / 0.15f + 12;
        previewBackground.sizeDelta = new Vector2(previewBackground.sizeDelta.x, backgroundHeight);
        lightOutline.sizeDelta = new Vector2(lightOutline.sizeDelta.x, backgroundHeight * 0.75f + 5);
        previewRoot.sizeDelta = new Vector2(previewRoot.sizeDelta.x, backgroundHeight * 0.15f);
        
        var totalHeight = content.rect.height + (_isExpending ? 1.8f : 1f);
        mainRoot.sizeDelta = new Vector2(mainRoot.sizeDelta.x, totalHeight); 
        outline.sizeDelta = new Vector2(outline.sizeDelta.x, totalHeight * 5f);
        
        _parent.ForceRebuildLayoutImmediate();
    }

    public void SetInfoBoxAttribute(ScrollRect scrollRect, AbstractInformationComponent info)
    {
        _scroll = scrollRect;
        _parent = info;
    }
    
    public override void SetInformation<T>(T data)
    {
        if (data is not ValueTuple<CharacterSkill, bool> tuple)
            return;
        
        var (skill, defaultExpend) = tuple;

        _costs.InitializeCostList("_Small");
        _skill = skill;
        _skill.CostLogic.RefreshCostDisplay(_costs);
        _keywords.Clear();

        if (_isExpending)
            arrow.transform.Rotate(new Vector3(0, 0, 180));
        if (defaultExpend)
            SetDetailShowStatus(false);
        
        var asset = skill.Asset;
        skillIcon.sprite = asset.icon;
        skillName.SetEntry(asset.skillName);

        var isBurst = asset.skillType == SkillType.ElementalBurst;
        normalIconBackground.SetActive(!isBurst);
        burstIconBackground.SetActive(isBurst);
        burstButtonBackground.SetActive(isBurst);
        if (isBurst)
            _keywords.Add("K501");
        
        var rawText = ResourceLoader.GetLocalizedValue("Skill", asset.description);
        _descriptionText = description.ProcessStringWithSkillData(
            rawText, asset.damage, asset.element);

        StaticMisc.DestroyAllChildren(subCards.transform);
        _keywords.UnionWith(description.UniqueKeywords);
        KeywordsAppend();
        foreach (var cardName in description.UsingCards)
        {
            var instance = Instantiate(subCardPrefab);
            instance.SetInformation(cardName);
            _keywords.UnionWith(instance.keywords.UsingCards);
            instance.SetParent(subCards.transform);
        }

        _detailSet = false;
    }

    public void SetDetailShowStatus(bool isExpending)
    {
        if (_isExpending == isExpending)
            return;
        
        _isExpending = isExpending;
        detailRoot.SetActive(_isExpending);
        if (!_detailSet && _isExpending)
            SetDetailInformation();
    }
    
    public void SwitchDetailShowStatus()
    {
        SetDetailShowStatus(!_isExpending);
    }

    #region Misc

    public bool CheckSame(CharacterSkill skill)
        => _skill?.Key.Equals(skill.Key) ?? false;
    
    public void SetDetailInformation()
    {
        description.SetText(_descriptionText);
        subCards.SetActive(description.UsingCards.Count != 0);
        
        skillType.SetEntry(_skill.Type switch
        {
            SkillType.NormalAttack => "skill_name_type_normal_attack",
            SkillType.ElementalSkill => "skill_name_type_elemental_skill",
            SkillType.ElementalBurst => "skill_name_type_elemental_burst",
            SkillType.PassiveSkill => "skill_name_type_passive_skill",
            _ => string.Empty
        });
            
        _detailSet = true;
    }
    
    private void KeywordsAppend()
    {
        if (_keywords.Contains("K101") || _keywords.Contains("K102"))
            _keywords.UnionWith(new[] { "K131", "K4" });
        
        if (_keywords.Contains("K102") || _keywords.Contains("K107"))
            _keywords.UnionWith(new[] { "K135", "K3" });

        if (_keywords.Contains("K103") || _keywords.Contains("K107"))
            _keywords.UnionWith(new[] { "K134", "K3" });
        
        if (_keywords.Contains("K104") || _keywords.Contains("K107"))
            _keywords.UnionWith(new[] { "K133", "K3" });
        
        if (_keywords.Contains("K106"))
            _keywords.Add("K132");
    }

    #endregion

    #region Interactive
    
    public void OnPointerEnter()
    {
        lightOutline.gameObject.SetActive(true);
        background.color = highlightColor;
    }

    public void OnPointerExit()
    {
        lightOutline.gameObject.SetActive(false);
        background.color = Color.white;
    }

    public void OnBeginDrag(BaseEventData eventData)
    {
        _isDragging = true;
        if (_scroll != null && eventData is PointerEventData pointerEventData)
            _scroll.OnBeginDrag(pointerEventData);
    }
    
    public void OnDrag(BaseEventData eventData)
    {
        if (_scroll != null && eventData is PointerEventData pointerEventData)
            _scroll.OnDrag(pointerEventData);
    }
    
    public void OnEndDrag(BaseEventData eventData)
    {
        _isDragging = false;
        if (_scroll != null && eventData is PointerEventData pointerEventData)
            _scroll.OnEndDrag(pointerEventData);
    }

    public void OnPointerClick()
    {
        if (_isDragging)
            return;
        var position = new Vector2(-0.25f, 0);
        arrow.transform.Rotate(new Vector3(0, 0, 180));
        arrow.DOAnchorPos(position, 0.15f).SetEase(curve);
        SwitchDetailShowStatus();
    }

    public void OnPointerDown()
    {
        var direction = _isExpending ? -1 : 1;
        var position = new Vector2(-0.25f, 1.2f * direction);
        arrow.DOAnchorPos(position, 0.15f);
    }

    public void OnPointerUp()
    {
        var position = new Vector2(-0.25f, 0);
        arrow.DOAnchorPos(position, 0.15f);
    }

    public void ClickDescription()
    {
        if (_isDragging)
            return;
        _parent.ui.Display(_keywords);
    }

    #endregion
}