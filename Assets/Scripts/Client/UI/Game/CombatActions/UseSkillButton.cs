using Shared.Enums;
using DG.Tweening;
using Server.GameLogic;
using Shared.Classes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UseSkillButton : AbstractSkillButton
{
    [Header("Skill Body Components")] 
    public CharacterSkill Skill;
    public Image highlight;
    public Image backgroundLight;
    public Image chosenEffect;
    public Image skillIcon;
    public Image lockingIcon;
    public GameObject burstBackground;
    public GameObject normalBackground;
    
    [Header("Expand Components")]
    public CostSetComponent costSet;
    public Hint warningHint;
    public Hint tipsHint;
    
    [Header("Skill Icon Colors")] 
    public Color defaultIconColor;
    public Color unableIconColor;
    public Color clickingIconColor;

    [Header("Background Light Colors")] 
    public Color defaultLightColor;
    public Color clickingLightColor;
    public Color unableHoverLightColor;

    [Header("Highlight Colors")] 
    public Color enableHoverHighlightColor;
    public Color unableHoverHighlightColor;
    public Material clickingHighlightMaterial;

    [Header("In Game Data")] 
    public bool isChoosing;
    public bool isUsable;
    public CostLogic SynchronousCost;

    public override string Key => Skill.Asset.skillName;
    public bool Locking => !Global.Acting || (isUsable && !Skill.Matched.Success) || Global.attackAnimating;
    public bool Usable => isUsable && !Locking;
    public ResourceMatchedResult Matched => Skill.Matched;

    #region Initalize / Data

    public void Awake()
    {
        costSet.InitializeCostList("_Small");
    }
    
    public void SetSkillData(CharacterSkill skill)
    {
        isChoosing = false;

        var asset = skill.Asset;
        var isBurst = asset.skillType == SkillType.ElementalBurst;
        burstBackground.SetActive(isBurst);
        normalBackground.SetActive(!isBurst);
        skillIcon.sprite = asset.icon;
        
        Skill = skill;
        Skill.CostLogic.RefreshCostDisplay(costSet);
    }
    
    public override void NetworkSynchronous(CostMatchResult result)
    {
        isUsable = result.Usable;
        Skill.Matched = result.MatchedResult;
        SynchronousCost = result.Cost;

        SynchronousCost.RefreshCostDisplay(costSet);
        SwitchToInitialState();
    }
    
    #endregion

    #region Operation

    private void PreviewSkill()
    {
        if (!Global.Acting)
        {
            Global.information.Display(Skill);
            
            if (!warningHint.Displaying)
                warningHint.Display(ParentRect, "warning_hint_turn");
            else
                Global.prompt.dialog.Display("warning_hint_turn");
            
            return;
        }
        
        if (Skill.Type == SkillType.SwitchActive)
        {
            const string entry = "switch_active";
            var mainText = ResourceLoader.GetLocalizedUIText(entry + "_banner");
            var subText = ResourceLoader.GetLocalizedUIText(entry);
            Global.prompt.banner.Display((mainText, subText));
            Global.information.CloseAll();
            tipsHint.Display(Parent.rect, "switch_active_hint", false);
        }
        
        Skill.RequestPreview();
    }

    public override void RequestUse()
    {
        if (!Usable)
        {
            Global.prompt.dialog.Display(Skill.CostLogic);
            DelaySwitchToInitialState();
            return;
        }

        Skill.RequestUse();
    }
    
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (Global.prompt.dialog.Intercept() || SkillButtonList.IsAnimating)
            return;
        
        if (isChoosing)
        {
            RequestUse();
            return;
        }
            
        foreach (var entity in Global.diceFunction.DiceEntities)
            entity.SetSelectStatus(false);

        isChoosing = true;
        body.DOScale(Vector3.one * 1.1f, AnimateDuration);
        DelaySwitchToInitialState();

        var prevButton = Parent.prevClickButton;
        if (prevButton != this && prevButton != null)
        {
            prevButton.CancelClickStatus();
            prevButton.warningHint.Reset();
            prevButton.tipsHint.Reset();
        }
        Parent.prevClickButton = this;

        PreviewSkill();
    }

    #endregion

    #region Style Switch Methods

    public override void HideHint()
    {
        warningHint.Reset();
        tipsHint.Reset();
    }

    public override void CancelClickStatus()
    {
        warningHint.Reset();
        tipsHint.Reset();
        isChoosing = false;
        chosenEffect.gameObject.SetActive(false);
    }

    public override void SwitchToInitialState()
    {
        body.DOScale(Vector3.one, AnimateDuration);
        highlight.material = null;
        highlight.color = Color.white;
        highlight.gameObject.SetActive(false);
        chosenEffect.gameObject.SetActive(isChoosing);
        backgroundLight.color = defaultLightColor;

        backgroundLight.gameObject.SetActive(Usable);
        lockingIcon.gameObject.SetActive(Locking);
        skillIcon.color = Usable ? defaultIconColor : unableIconColor;
    }

    private void SwitchToHoveringState()
    {
        body.DOScale(Vector3.one * 1.1f, AnimateDuration);
        highlight.gameObject.SetActive(true);
        backgroundLight.gameObject.SetActive(true);

        lockingIcon.gameObject.SetActive(Locking);
        highlight.color = Usable ? enableHoverHighlightColor : unableHoverHighlightColor;
        backgroundLight.color = Usable ? defaultLightColor : unableHoverLightColor;
    }

    private void SwitchToClickingState()
    {
        body.DOScale(Vector3.one * 0.95f, AnimateDuration);
        highlight.gameObject.transform.localScale = Vector3.one * 0.975f;
        highlight.gameObject.SetActive(true);
        
        if (Usable)
            highlight.material = clickingHighlightMaterial;
        else
            highlight.color = enableHoverHighlightColor;
        
        skillIcon.color = Usable ? clickingIconColor : unableIconColor;
        lockingIcon.gameObject.SetActive(Locking);
        backgroundLight.gameObject.SetActive(true);
        backgroundLight.color = Usable ? clickingLightColor : unableHoverLightColor;
    }

    public void DelaySwitchToInitialState()
    {
        DOVirtual.DelayedCall(AnimateDuration * 1.25f, SwitchToInitialState);
    }
    
    #endregion

    #region Interactive

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (Global.prompt.dialog.isShowing)
            return;
        
        SwitchToHoveringState();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (Global.prompt.dialog.isShowing)
            return;
        
        SwitchToInitialState();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (Global.prompt.dialog.isShowing)
            return;
        
        SwitchToClickingState();
    }

    #endregion

    #region Misc

    public void DisplayWarningHint()
    {
        warningHint.Display(ParentRect, Matched.Message);
    }

    #endregion
}