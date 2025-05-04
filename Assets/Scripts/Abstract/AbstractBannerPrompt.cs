using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AbstractBannerPrompt : AbstractPromptComponent
{
    [Header("Self Components")]
    public Transform textTransform;
    public Transform bannerTransform;
    public CanvasGroup textCanvas;
    public CanvasGroup bannerCanvas;
    public TextMeshProUGUI text;
    public AnimationCurve curve;

    protected const float T = 0.12f;
    protected readonly Vector3 TextTransformOffset = Vector3.right * 1.65f;
    protected Tween Tween;

    protected Sequence BaseDisplaySequence()
    {
        Tween?.Kill();
        return DOTween.Sequence()
            .Append(bannerTransform.DOScaleY(1f, 1.5f * T).SetEase(curve))
            .Join(bannerCanvas.DOFade(1, 2 * T).SetEase(Ease.OutExpo))
            .Insert(1.5f * T, textTransform.DOLocalMove(Vector3.zero, 2.25f * T))
            .Insert(1.5f * T, textCanvas.DOFade(1, 1.5f * T));
    }
    
    protected Sequence BaseHideSequence()
    {
        Tween?.Kill();
        return DOTween.Sequence()
            .Append(textTransform.DOLocalMove(-1 * TextTransformOffset, T))
            .Join(textCanvas.DOFade(0, 1.25f * T))
            .Insert(T, bannerCanvas.DOFade(0, T))
            .Insert(T, bannerTransform.DOScaleY(0, T));
    }
    
    public override void Reset()
    {
        textTransform.localPosition = TextTransformOffset;
        bannerTransform.localScale = new Vector3(1, 0.2f, 1);
        textCanvas.alpha = 0f;
        bannerCanvas.alpha = 0f;
    }
    
    public override void Display<TD>(TD data, Action onComplete = null) { }
    
    public override void Hide() { }
}