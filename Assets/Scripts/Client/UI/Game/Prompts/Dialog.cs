using System;
using DG.Tweening;
using Server.GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class Dialog : AbstractPromptComponent
{
    [Header("Self Components")] 
    public TextMeshProUGUI text;
    public CanvasGroup canvas;
    public LocalizeStringEvent textEvent;
    public CanvasGroup dark;

    private bool _disableShowDark;
    private Tween _hideTween;
    private Tween _inactiveTween;
    private const float DurationTime = 0.25f;
    private readonly Vector3 _scale = Vector3.one * 0.85f;

    public override void Reset()
    {
        gameObject.SetActive(false);
        transform.localScale = _scale;
        canvas.alpha = 0;
        dark.alpha = 0;
    }

    public void DisableShowDark()
    {
        _disableShowDark = true;
    }

    public override void Display<T>(T data, Action onComplete = null)
    {
        if (data is string entry)
            textEvent.SetEntry(entry);

        if (data is CostLogic costLogic)
        {
            if (!costLogic.PureDiceUsed())
                textEvent.SetEntry("warning_hint_energy");
            else
            {
                textEvent.StringReference.TableEntryReference = default;
                var contentString = ResourceLoader.GetLocalizedUIText("pay_dice_hint");
                text.text = contentString + costLogic;
            }
        }

        var alpha = _disableShowDark ? 0 : 0.25f;

        _disableShowDark = false;
        _inactiveTween?.Kill();
        _hideTween = DOVirtual.DelayedCall(2, Hide);

        gameObject.SetActive(true);
        isShowing = true;
        DOTween.Sequence()
            .Append(dark.DOFade(alpha, DurationTime))
            .Join(transform.DOScale(Vector3.one, DurationTime).SetEase(Ease.OutCubic))
            .Join(canvas.DOFade(1, DurationTime).SetEase(Ease.OutCubic))
            .Play()
            .OnComplete(() => onComplete?.Invoke());
    }

    public override void Hide()
    {
        _hideTween?.Kill();
        _inactiveTween = DOVirtual.DelayedCall(
            DurationTime, () => gameObject.SetActive(false)
        );

        isShowing = false;
        dark.DOFade(0, DurationTime);
        transform.DOScale(_scale, DurationTime).SetEase(Ease.OutCubic);
        canvas.DOFade(0, DurationTime).SetEase(Ease.OutCubic);
    }

    public bool Intercept()
    {
        var result = isShowing;
        if (isShowing)
            Hide();
        
        return result;
    }
}