using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ActionBanner : AbstractBannerPrompt
{
    public Image arc;
    public Image ray;
    public Image icon;
    public CanvasGroup arcCanvas;
    public CanvasGroup rayCanvas;
    public CanvasGroup iconCanvas;
    public Transform arcTransform;
    public Transform iconTransform;
    public Image bannerBackground;
    public LocalizeStringEvent content;
    
    [Header("Configurations")]
    public List<Sprite> endIcons;
    public List<Sprite> actionIcons;
    public List<Color> arcColors;
    public List<Color> rayColors;
    public List<Color> textColors;
    public List<Material> backgroundMaterials;
    
    public override void Reset()
    {
        icon.gameObject.SetActive(false);
        iconCanvas.alpha = 0.35f;
        iconTransform.localPosition = Vector3.up;
        
        arc.gameObject.SetActive(false);
        arcCanvas.alpha = 0.4f;
        arcTransform.localPosition = Vector3.up * 2;
        
        rayCanvas.alpha = 0;
        
        base.Reset();
    }

    public override void Display<TB>(TB data, Action onComplete = null)
    {
        if (data is not ValueTuple<string, bool, bool> tuple)
            return;

        isShowing = true;
        prompt.Reset();
        gameObject.SetActive(true);
        
        var (entry, isSelf, isEnd) = tuple;
        var site = isSelf ? 1 : 0;
        var icons = isEnd ? endIcons : actionIcons;
        var combat = prompt.global.combatAction;
        
        content.SetEntry(entry);
        arc.color = arcColors[site];
        ray.color = rayColors[site];
        text.color = textColors[site];
        icon.sprite = icons[site];
        bannerBackground.material = backgroundMaterials[site];

        prompt.global.Acting = isSelf;
        if (!isSelf)
            combat?.TransferStatus(CombatTransfer.Transparent);
        
        Tween = BaseDisplaySequence()
            .InsertCallback(1.2f * T, () => icon.gameObject.SetActive(true))
            .Insert(1.2f * T, iconCanvas.DOFade(1, 0.4f * T).SetEase(Ease.OutCirc))
            .Insert(1.2f * T, iconTransform.DOLocalMoveY(5, 1.45f * T).SetEase(Ease.OutCirc))
            .InsertCallback(1.45f * T, () => arc.gameObject.SetActive(true))
            .Insert(1.45f * T, arcCanvas.DOFade(1, 0.4f * T).SetEase(Ease.OutCirc))
            .Insert(1.45f * T, arcTransform.DOLocalMoveY(5, 0.535f * T).SetEase(Ease.OutCirc))
            .Insert(2 * T, rayCanvas.DOFade(1, 0.35f * T).SetEase(Ease.OutCirc))
            .Play();
        
        DOVirtual.DelayedCall(0.9f, () =>
        {
            Hide();
            Tween.onComplete = () =>
            {
                onComplete?.Invoke();
                if (!isSelf)
                    combat?.TransferStatus(CombatTransfer.Active);
            };
        });
    }

    public override void Hide()
    {
        isShowing = false;
        Tween = BaseHideSequence()
            .Insert(0.6f * T, iconCanvas.DOFade(0, 0.8f * T).SetEase(Ease.OutSine))
            .Insert(0.6f * T, iconTransform.DOLocalMoveY(1, 0.8f * T).SetEase(Ease.OutSine))
            .Insert(0.6f * T, arcCanvas.DOFade(0, 0.8f * T).SetEase(Ease.OutSine))
            .Insert(0.6f * T, arcTransform.DOLocalMoveY(1, 0.8f * T).SetEase(Ease.OutSine))
            .Insert(0.6f * T, rayCanvas.DOFade(0, 0.8f * T).SetEase(Ease.OutSine))
            .Play();
    }
}