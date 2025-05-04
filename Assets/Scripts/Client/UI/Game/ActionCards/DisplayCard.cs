using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour
{
    public Image cardFace;
    public CanvasGroup canvas;

    public void Display(Sprite sprite, Action onComplete = null)
    {
        canvas.alpha = 0;
        transform.localScale = Vector3.one * 0.875f;
        cardFace.sprite = sprite;
        gameObject.SetActive(true);

        DOTween.Sequence()
            .Append(canvas.DOFade(1, 0.3f).SetEase(Ease.OutExpo))
            .Join(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutCirc))
            .OnComplete(() => DOVirtual.DelayedCall(0.3f, () => Hide(onComplete)));
    }

    private void Hide(Action onComplete = null)
    {
        transform.DOScale(Vector3.one * 1.15f, 0.3f).SetEase(Ease.OutCirc);
        canvas.DOFade(0, 0.3f).SetEase(Ease.OutExpo)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
    }
}