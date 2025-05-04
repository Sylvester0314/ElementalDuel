using DG.Tweening;
using UnityEngine;

public abstract class AbstractBookPage : MonoBehaviour
{
    public AbstractHandbookContainer container;
    public CanvasGroup canvas;

    public virtual void Open()
    {
        canvas.alpha = 0;
        gameObject.SetActive(true);
        canvas.DOFade(1, 0.2f).SetEase(Ease.OutExpo);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}