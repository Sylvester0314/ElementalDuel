using System;
using DG.Tweening;
using UnityEngine;

public class Banner : AbstractBannerPrompt
{
    [Header("In Game Data")]
    public bool fixedStatus;
    
    private Action _onComplete;
    
    public void Animate(string content, bool showDark = true, bool autoHide = false, Action onComplete = null)
    {
        isShowing = true;
        text.text = content;
        prompt.Reset();
        if (showDark)
            prompt.DarkBackgroundDisplay();

        if (!autoHide)
        {
            BaseDisplaySequence().Play()
                .OnComplete(() => onComplete?.Invoke());
            return;
        }

        BaseDisplaySequence().Play();
        
        Action hide = fixedStatus ? FixedHide : Hide;
        _onComplete = onComplete;
        DOVirtual.DelayedCall(0.9f, () => hide.Invoke());
    }

    public void FixedAnimate(string content, bool autoHide = false, Action onComplete = null)
    {
        fixedStatus = true;
        Animate(content, false, autoHide, onComplete);
    }

    public void FixedHide()
    {
        fixedStatus = false;
        Hide();
    }
    
    public override void Display<T>(T data, Action onComplete = null)
    {
        if (data is not ValueTuple<string, string> tuple)
            return;

        var (mainText, subText) = tuple;
        var content = string.IsNullOrEmpty(subText)
            ? mainText
            : mainText.Replace("$[Sub]",  $"<color=#FFD780>{subText}</color>");
        Animate(content, onComplete: onComplete);
    }

    public override void Hide()
    {
        if (fixedStatus)
            return;
        
        isShowing = false;
        prompt.DarkBackgroundHide();
        Tween = BaseHideSequence().Play()
            .OnComplete(() =>
            {
                _onComplete?.Invoke();
                _onComplete = null;
            });
    }
}
