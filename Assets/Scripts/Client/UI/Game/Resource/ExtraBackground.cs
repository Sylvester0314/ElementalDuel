using DG.Tweening;
using UnityEngine;

public class ExtraBackground : MonoBehaviour
{
    public RectTransform background;
    public CanvasGroup backgroundCanvas;
    public GameObject icon;
    public GameObject iconLight;
    
    public void Open()
    {
        background.gameObject.SetActive(true);
        background.DOAnchorPos(Vector2.zero, 0.167f);
        backgroundCanvas.DOFade(1, 0.167f).SetEase(Ease.InExpo);
    }

    public void Close()
    {
        background.gameObject.SetActive(false);
        background.anchoredPosition = Vector2.right;
        backgroundCanvas.alpha = 0;
    }

    public void ShowIcon()
    {
        icon.SetActive(true);
        iconLight.SetActive(true);
    }

    public void HideIcon()
    {
        icon.SetActive(false);
        iconLight.SetActive(false);
    }
}
