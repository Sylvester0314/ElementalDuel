using DG.Tweening;
using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    public Global global;
    public SelfCountdown selfCountdown;
    public OpponentCountdown oppoCountdown;
    public CanvasGroup canvas;

    [Header("In Game Data")]
    public bool forcedStatus = true;
    
    private const float AnimationDuration = 0.15f;
    
    public void Hide()
    {
        SetSubActive(false);
    }

    public void Display()
    {
        SetSubActive(true);
    }

    public void SetDisplayStatus(bool status)
    {
        if (status)
            Open(false);
        else
            Close(false);
    }
    
    public void Close(bool force)
    {
        if (forcedStatus && !force)
            return;
        
        forcedStatus = force;
        Animation(0);
        if (force)
            DOVirtual.DelayedCall(AnimationDuration, Hide);
    }

    public void Open(bool force)
    {
        if (forcedStatus && !force)
            return;
        
        forcedStatus = false;
        Animation(1);
    }
    
    public void Switch(bool isSelfActing)
    {
        AbstractTurnCountdown countdown = isSelfActing ? selfCountdown : oppoCountdown;
        
        var isPrevShowing = countdown.next.gameObject.activeSelf;
        countdown.next.SetActive(false);
        countdown.Switch(isPrevShowing);
    }
    
    private void Animation(float endValue)
    {
        canvas.DOFade(endValue, AnimationDuration).SetEase(Ease.OutExpo);
        transform.DOScale(endValue, AnimationDuration).SetEase(Ease.OutExpo);
    }

    private void SetSubActive(bool showDice)
    {
        selfCountdown.SetActive(false);
        oppoCountdown.SetActive(false);
        selfCountdown.diceCount.gameObject.SetActive(showDice);
        oppoCountdown.diceCount.gameObject.SetActive(showDice);
    }
}