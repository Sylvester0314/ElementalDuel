using Shared.Enums;
using System;
using DG.Tweening;
using UnityEngine;

public class ElementalTuning : AbstractPromptComponent
{
    [Header("Self Components")]
    public LargeDice origin;
    public LargeDice target;
    public RectTransform tuningBackground;
    public CanvasGroup tuningBackgroundCanvas;
    public CanvasGroup tuningArrows;
    public AnimationCurve tuningCurve;
    
    private const string TuningEntry = "elemental_tuning_banner";
    private const float TimeUnit = 0.04f;
    
    public override void Reset()
    {
        target.canvas.alpha = 0;
        target.transform.localScale = Vector3.one * 0.4f;
        target.transform.localPosition = Vector3.zero;
        origin.canvas.alpha = 0;
        origin.transform.localScale = Vector3.one * 0.64f;
        origin.transform.localPosition = Vector3.zero;
        tuningBackground.sizeDelta = Vector2.one * 9.55f;
        tuningBackgroundCanvas.alpha = 0;
        tuningArrows.alpha = 0;
    }

    public override async void Display<T>(T data, Action onComplete = null)
    {   
        if (data is not ValueTuple<CostType, CostType> (var typeOrigin, var typeTarget))
            return;
        
        var subEntry = typeTarget.ToString().ToLower() + "_name";
        var mainText = ResourceLoader.GetLocalizedUIText(TuningEntry);
        var subText = ResourceLoader.GetLocalizedUIText(subEntry);
        var content = mainText.Replace("$[Sub]",  subText);

        isShowing = true;
        gameObject.SetActive(true);
        prompt.banner.Animate(content);
        await target.SetStyle(typeTarget);
        await origin.SetStyle(typeOrigin);
        
        Reset();
        DOTween.Sequence()
            .Append(origin.transform.DOScale(Vector3.one * 0.8f, 3.5f * TimeUnit).SetEase(tuningCurve))
            .Join(origin.canvas.DOFade(1, 5 * TimeUnit))
            .Insert(4 * TimeUnit, tuningBackgroundCanvas.DOFade(1, 4 * TimeUnit))
            .Insert(4 * TimeUnit, tuningArrows.DOFade(1, 0))
            .Insert(5 * TimeUnit, target.canvas.DOFade(1, 3 * TimeUnit))
            .Insert(5 * TimeUnit, target.transform.DOScale(Vector3.one * 0.8f, 2 * TimeUnit))
            .Insert(5 * TimeUnit, target.transform.DOLocalMove(Vector3.right * 6, 6 * TimeUnit))
            .Insert(5 * TimeUnit, origin.transform.DOLocalMove(Vector3.right * -6, 6 * TimeUnit))
            .Insert(6 * TimeUnit, tuningBackground.DOSizeDelta(new Vector2(20.05f, 9.55f), 4 * TimeUnit))
            .Play();
    }
    
    public override void Hide()
    {
        isShowing = false;
        gameObject.SetActive(false);
        Reset();
    }
    
    public async void SetOrigin(CostType type)
    {
        await origin.SetStyle(type);
    }
}
