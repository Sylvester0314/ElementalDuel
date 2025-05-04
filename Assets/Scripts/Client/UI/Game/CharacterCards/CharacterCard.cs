using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Enums;
using DG.Tweening;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCard : AbstractCard, IPointerClickHandler, IGameEntity
{
    [TitleGroup("In Game Data")] 
    public int index;
    public bool isActiveCharacter;
    public string uniqueId;

    [Space] 
    public int currentHealth;
    public int maxHealth;
    [Space]
    public int currentEnergy;
    public int maxEnergy;
    [Space]

    public bool isPreviewing;
    public bool canSelectInPreview;
    public CharacterZone zone;

    [TitleGroup("Base Components")]
    public CharacterAsset asset;
    public Image cardFace;
    public Image cardFrame;
    public Canvas canvas;

    [TitleGroup("Information Components")] 
    public GameObject information;
    public TextMeshProUGUI healthValue;
    public EquipController weapon;
    public EquipController relic;
    public EquipController talent;
    public CharacterApplications applications;
    public StatusZone statuses;
    public GameObject energyList;
    public GameObject technique;

    [TitleGroup("Selection Components")]
    public GameObject selectIcon;
    public Image selectChecking;
    public Image selectRing;
    
    [TitleGroup("Effect Components")]
    public HealthModifyFeedback feedback;
    public HealthPreview healthPreview;
    public ReactionPreview reactionPreview;
    public EntityHitEffect hitEffect;
    public CanvasGroup defeatedIcons;
    public Image cardFlash;
    public CanvasGroup flashCanvas;
    
    [TitleGroup("References")]
    public Energy energyPrefab;
    public Material darkMaterial;
    public Material ringMaterial;

    [TitleGroup("Configurations")]
    public List<Color> flashColors;
    public AnimationCurve checkDisplayCurve;
    public AnimationCurve checkBounceCurve;
    public List<CharacterSkill> Skills;
    
    public Action PreviewingAction;
    
    private List<Energy> _energyList;
    private Material _material;
    private readonly int _flashColorProperty = Shader.PropertyToID("_EmissionColor");
    private readonly int _intensityProperty = Shader.PropertyToID("_Intensity");
    private const float SwitchActiveAnimationDuration = 0.375f;
    private const float SelectingAnimationDuration = 0.125f;
    
    public CostType Element => CostLogic.Map(asset.Properties);
    public Global Global => zone.owner.global;
    
    public void Start()
    {
        zone = transform.parent.GetComponent<CharacterZone>();
        
        // Used for debugging the main game scene
        if (FindObjectOfType<PrepareRoom>() == null)
            LoadAsset(asset);
    }

    public void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }

    public void LoadAsset(CharacterAsset characterAsset)
    {
        asset = characterAsset;
        
        maxHealth = asset.baseMaxHealth;
        currentHealth = maxHealth;
        maxEnergy = asset.baseMaxEnergy;
        currentEnergy = 0;

        _energyList = new List<Energy>();
        for (var i = 0; i < maxEnergy; i++)
        {
            var instance = Instantiate(energyPrefab, energyList.transform);
            _energyList.Add(instance);
        }
        var rectTransform = energyList.GetComponent<RectTransform>();
        var sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = maxEnergy * 80;
        rectTransform.sizeDelta = sizeDelta;

        Skills = asset.skillList
            .Select(skill => CharacterSkill.FormCharacter(this, skill))
            .ToList();
        
        cardFace.sprite = asset.cardImage;
        if (zone.owner.site == Site.Self)
            Global.combatAction.Append(this);
    }
    
    #region Display

    private void SetFlashColor(int i)
    {
        cardFlash.material.SetColor(_flashColorProperty, flashColors[i]);
        flashCanvas.DOFade(1, 0.3f).SetEase(Ease.OutExpo);
    }

    public void ModifyHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        healthValue.text = currentHealth.ToString();
    }

    public void ModifyEnergy(int amount)
    {
        if (amount == 0)
            return;
        
        EnergyIndexRange(amount, out var sign, out var min, out var max);
        currentEnergy += amount;
        
        for (var i = min; i < max; i++)
        {
            var energy = _energyList.TryGetValue(i);
            if (energy != null)
                energy.Charged = sign;
        }
    }

    #endregion
    
    #region Character Status

    public void SwitchToSelectableStatus(bool amplify = true)
    {
        SwitchToPreviewingStatus(amplify);
        canSelectInPreview = true;
        SetFlashColor(0);
    }
    
    public void SwitchToDefeatedStatus()
    {
        if (currentHealth > 0)
            return;
        
        _material = new Material(darkMaterial);
        
        information.SetActive(false);
        defeatedIcons.gameObject.SetActive(true);
        _material.SetFloat(_intensityProperty, 0);
        cardFace.material = _material;
        cardFrame.material = _material;

        SetActiveStatus(false);
        DOTween.Sequence()
            .Append(defeatedIcons.DOFade(1, 0.3f))
            .Join(_material.DOFloat(0.5f, _intensityProperty, 0.3f))
            .Play();
    }

    public void SetActiveStatus(bool active, Action onComplete = null)
    {
        isActiveCharacter = active;
        var local = transform.localPosition;
        var sign = zone.owner.site == Site.Self ? 1 : -1;
        var positionY = isActiveCharacter ? 1.1f * sign : 0;
        var position = new Vector3(local.x, positionY, local.z);
        transform
            .DOLocalMove(position, SwitchActiveAnimationDuration)
            .SetEase(Ease.OutQuart)
            .OnComplete(() => onComplete?.Invoke());

        if (isActiveCharacter)
        {
            var combatZone = zone.owner.combatStatuses;
            combatZone.transform.SetParent(information.transform, false);
            combatZone.transform.localPosition = Vector3.back * 9.85f;
            combatZone.belongs = this;
        }
        
        DOVirtual.DelayedCall(
            SwitchActiveAnimationDuration,
            DisplaySkillButtonList
        );
    }
    
    private void DisplaySkillButtonList()
    {
        if (zone.owner.site == Site.Self)
            zone.Global.combatAction.TransferStatus(CombatTransfer.Active);
    }
    
    public void SwitchToSelectedMainTargetStatus()
    {
        selectIcon.SetActive(true);
        selectChecking.DOFillAmount(1, SelectingAnimationDuration)
            .SetEase(checkDisplayCurve);
        selectChecking.transform
            .DOScale(Vector3.one, SelectingAnimationDuration * 3)
            .SetEase(checkBounceCurve);
        
        selectRing.DOFade(1, SelectingAnimationDuration * 1.5f);
        selectRing.transform.DOScale(Vector3.one, SelectingAnimationDuration * 1.5f);
        DOVirtual.DelayedCall(
            SelectingAnimationDuration * 1.75f,
            () => selectRing.material = ringMaterial
        );
    }

    private void CancelMainTargetStatus()
    {
        selectIcon.SetActive(false);
        selectRing.material = null;
        selectRing.transform.localScale = Vector3.one * 0.7f;
        selectRing.DOFade(0, 0);
        selectChecking.fillAmount = 0;
        selectChecking.transform.localScale = Vector3.one * 0.9f;
    }
    
    #endregion
    
    #region Interactive

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Global.prompt.dialog.Intercept() || zone.canvas.alpha == 0)
            return;
        
        if (Global.previewingMainTarget == this)
        {
            PreviewingAction?.Invoke();
            return;
        }

        if (canSelectInPreview)
        {
            Global.SetPreview(uniqueId);
            SelectCard();
            return;
        }
        
        Global.CancelPreview();
        Global.SetSelectingCard(this);
        RotateTargetAnimation();
    }

    public void SelectCard(AbstractSkillButton previewSkill = null)
    {
        var prevMainTarget = Global.previewingMainTarget;
        
        PreviewingAction = previewSkill != null
            ? previewSkill.RequestUse
            : prevMainTarget?.PreviewingAction;
        
        SwitchToSelectedMainTargetStatus();
        if (prevMainTarget != null && prevMainTarget != this)
            prevMainTarget.CancelMainTargetStatus();
        
        Global.previewingMainTarget = this;
    }
    
    #endregion

    #region Preview

    public void SwitchToPreviewingStatus(bool amplify)
    {
        isPreviewing = true;
        canvas.sortingOrder = 1;
        
        if (!Global.combatAction.choosing)
            applications.gameObject.SetActive(false);

        if (amplify)
            transform.DOScale(Vector3.one * 1.075f, 0.03f);
    }
    
    public void CancelPreviewStatus()
    {
        isPreviewing = false;
        canSelectInPreview = false;
        canvas.sortingOrder = 0;
        transform.DOScale(Vector3.one, 0.03f);
        flashCanvas.DOFade(0, 0.3f).SetEase(Ease.OutExpo);
        
        HidePreviewComponents();
    }

    public void HidePreviewComponents()
    {
        CancelMainTargetStatus();
        StopEnergyFlash();
        healthPreview.Reset();
        reactionPreview.Close();
        applications.gameObject.SetActive(true);
    }
    
    public void SetPreviewInformation(CharacterModification modification)
    {
        EnergyFlash(modification.EnergyModified);
        healthPreview.Open(modification);
        
        if (modification.AppliedNewApplication)
            reactionPreview.Open(modification.Applications.Unpacking());
        else
            applications.gameObject.SetActive(true);
    }

    public void StopEnergyFlash()
    {
        for (var i = currentEnergy; i < maxEnergy; i++)
            _energyList[i].CancelPlay();
    }

    private void EnergyIndexRange(int amount, out bool sign, out int min, out int max)
    {
        sign = amount > 0;
        min = sign ? currentEnergy : currentEnergy + amount;
        max = sign ? currentEnergy + amount : currentEnergy;
    }
    
    public void EnergyFlash(int amount)
    {
        StopEnergyFlash();

        if (amount == 0)
            return;

        EnergyIndexRange(amount, out var sign, out var min, out var max);
        
        for (var i = min; i < max; i++)
            _energyList.TryGetValue(i)?.PlayAnimation(sign);
    }

    #endregion

    #region Attack Animation

    [TitleGroup("Attack Animation Configurations")]
    public Transform attackPoint;
    public float prepareDuration;
    public float backswingDuration;
    public float attackDuration;
    public float backDuration;
    public float maxScale;
    
    public async void DoAction(CharacterCard character, Element element, Action feedbackAction = null)
    {
        var originPosition = transform.localPosition;
        
        var from = transform.position + Vector3.up * 8;
        var to = character.transform.position;
        var (angleX, angleY, _) = Quaternion.LookRotation(to - from).eulerAngles;

        if (element is not (Shared.Enums.Element.None or Shared.Enums.Element.Physical or
            Shared.Enums.Element.Piercing))
            SetFlashColor((int)element);
        
        // prepare
        var site = zone.owner.site == Site.Self;
        var rotation = site 
            ? new Vector3(angleX, 0, -angleY)
            : new Vector3(-angleX / 3, 0, 180 - angleY);

        var prepare = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * maxScale, prepareDuration))
            .Join(transform.DOLocalMoveZ(-8, prepareDuration))
            .Join(transform.DOLocalRotate(rotation, prepareDuration))
            .SetEase(Ease.InCirc)
            .Play();

        await prepare.AsyncWaitForCompletion();
        
        // backswing
        var difference = to + Vector3.up * 4 - transform.position;
        var direction = difference.normalized;
        var distance = difference.magnitude / 2.5f;

        var backswing = transform
            .DOMove(transform.position - direction * distance, backswingDuration)
            .SetEase(Ease.OutCirc);
            
        await backswing.AsyncWaitForCompletion();
        
        // attack
        var target = attackPoint.transform.position;
        var offset = transform.InverseTransformPoint(target) / maxScale;
        var endPos = to - transform.TransformVector(offset);
        
        var attack = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 1.05f, attackDuration))
            .Join(transform.DOMove(endPos, attackDuration))
            .SetEase(Ease.InOutQuart)
            .Play();

        var hitPunch = Quaternion.Euler(-90, 0, 0) * direction;
        
        DOVirtual.DelayedCall(attackDuration * 0.55f, () =>
        {
            character.transform.DOPunchPosition(hitPunch, 0.4f, 4, 1.4f);
            character.hitEffect.Play(element);
            
            feedbackAction?.Invoke();
        });
        
        await attack.AsyncWaitForCompletion();
        
        // back
        DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one, backDuration))
            .Join(transform.DOLocalRotate(Vector3.zero, backDuration))
            .Join(transform.DOLocalMove(originPosition, backDuration))
            .Join(flashCanvas.DOFade(0, backDuration).SetEase(Ease.OutCirc))
            .Play();
    }

    #endregion
}
