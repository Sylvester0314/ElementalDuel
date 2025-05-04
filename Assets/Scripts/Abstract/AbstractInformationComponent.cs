using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractInformationComponent : MonoBehaviour
{
    public CanvasGroup canvas;
    public RectTransform content;
    public InformationUI ui;
    
    private Tween _tween;
    
    public void FadeIn()
    {
        canvas.alpha = 0;
        _tween = canvas.DOFade(1, 0.25f).SetEase(Ease.OutQuint);
    }

    public void FadeOut()
    {
        _tween?.Kill();
        canvas.DOFade(0, 0.2f).SetEase(Ease.OutQuint);
    }
    
    public void SetParent(InformationUI parent)
    {
        transform.SetParent(parent.transform, false);
        ui = parent;
    }
    
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent, false);
    }

    public void DestroySelf(bool immediately = false, float delay = 0.21f)
    {
        if (!immediately)
        {
            FadeOut();
            Destroy(gameObject, delay);
            return;
        }
        
        Destroy(gameObject);
    }
    
    public void ForceRebuildLayoutImmediate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public abstract void SetInformation<T>(T data);
}