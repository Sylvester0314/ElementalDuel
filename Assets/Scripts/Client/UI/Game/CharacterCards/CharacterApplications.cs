using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Enums;
using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CharacterApplications : AbstractAppliedManager
{
    public RectTransform reacting;
    public CanvasGroup reactingCanvas;
    public Image reactingBackground;
    public TextMeshProUGUI reactionLabel;
    public LocalizeStringEvent reactionEvent;
    public LayoutElement labelLayout;
    public List<Image> reactingIcons;
    public HorizontalLayoutGroup layoutGroup;
    public Image mergingLight;

    [Header("In Game Data")] 
    public ElementalApplication applications;
    
    private readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");
    private readonly int _outlineWidth = Shader.PropertyToID("_OutlineWidth");
    
    public void SetApplications(ElementalApplication application)
    {
        ResetIcons();

        applications = application;
        
        var index = icons.Count - 1;
        var flag = ElementalApplication.Dendro;
        
        while (flag != ElementalApplication.None)
        {
            if ((application & flag) == flag)
            {
                SetElementIcon(index, flag);
                index -= 1;
            }
            
            flag = (ElementalApplication)((int)flag >> 1);
        }
    }
    
    private List<Task> ZoomIcons(List<Image> images, float scale, float duration)
        => images
            .Select(icon => icon.transform
                .DOScale(Vector3.one * scale, duration)
                .SetEase(Ease.InSine)
            )
            .Select(tweener => tweener.AsyncWaitForCompletion())
            .ToList();

    public async void ReactionAnimation(
        ElementalApplication applied, ElementalApplication incoming, Action onComplete = null
    )
    {
        ResetIcons();
        SetElementIcon(0, applied, reactingIcons, "Pure");
        SetElementIcon(1, incoming, reactingIcons, "Pure");
        
        // Reset
        layoutGroup.gameObject.SetActive(true);
        layoutGroup.spacing = -1.6f;
        reactingIcons.ForEach(icon => icon.transform.localScale = Vector3.one * 1.1f);
        mergingLight.gameObject.SetActive(false);
        mergingLight.transform.localScale = new Vector3(2.8f, 1.6f, 0);

        // Shrink
        await Task.Delay(100);
        await Task.WhenAll(ZoomIcons(reactingIcons, 0.635f, 0.2f));

        // Merge
        DOTween.To(
            () => layoutGroup.spacing,
            x => layoutGroup.spacing = x,
            -3.2f, 0.1f
        );
        
        DOVirtual.DelayedCall(0.05f, () =>
        {
            mergingLight.gameObject.SetActive(true);
            mergingLight.transform.DOScaleY(0, 0.175f);
        });
        
        await Task.WhenAll(ZoomIcons(reactingIcons, 0, 0.1f));
        await Task.Delay(25);
        
        await DisplayReactionLabel(applied.ToReaction(incoming));
        
        layoutGroup.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    public async Task DisplayReactionLabel(ElementalReaction reaction)
    {
        var material = new Material(reactionLabel.fontMaterial);
        var reactionColor = ElementalStatic.ReactionColors[reaction];
        
        material.SetColor(_outlineColor, reactionColor.Item2);
        material.SetFloat(_outlineWidth, 0.125f);

        reactingBackground.color = reactionColor.Item2;
        reactionLabel.color = reactionColor.Item1;
        reactionLabel.fontMaterial = material;
        
        reacting.gameObject.SetActive(true);
        reactionEvent.SetEntry($"reaction_{reaction.ToSnakeCase().ToLower()}");
        var rect = reactionEvent.GetComponent<RectTransform>();
        
        labelLayout.preferredWidth = -1;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        
        await Task.Yield();
        labelLayout.preferredWidth = Mathf.Min(10.08f, rect.sizeDelta.x);
        reactingCanvas.alpha = 0;
        reacting.localScale = Vector3.one * 0.7f;
        reacting.sizeDelta = Vector2.zero;
        
        var appear = DOTween.Sequence()
            .Append(reactingCanvas.DOFade(1, 0.15f))
            .Join(reacting.DOScale(1, 0.15f))
            .SetEase(Ease.OutExpo)
            .Join(reacting.DOLocalMoveY(0.15f, 0.15f))
            .SetEase(Ease.InSine)
            .Play();

        await appear.AsyncWaitForCompletion();
        await Task.Delay(1400);
        
        // disappear
        DOTween.Sequence()
            .Append(reacting.DOScale(0, 0.1f))
            .Join(reactingCanvas.DOFade(0, 0.1f))
            .SetEase(Ease.OutExpo)
            .Play()
            .OnComplete(() => reacting.gameObject.SetActive(false));
    }
}