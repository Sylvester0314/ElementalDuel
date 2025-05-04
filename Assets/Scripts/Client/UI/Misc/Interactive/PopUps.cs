using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;

public class PopUps : MonoBehaviour
{
    [Header("Environment")]
    public CanvasGroup canvas;
    public UISizeFitter sizeFitter;
    
    [Header("Components")]
    public Transform elements;
    public Transform content;
    public LocalizeStringEvent titleEvent;
    public MiddleButton confirmButton;

    [Header("Prefabs References")]
    public ContentText textPrefab;
    public ContentText errorPrefab;
    
    public void Initialize(string titleEntry)
    {
        titleEvent.SetEntry(titleEntry);
    }

    public PopUps AppendText(string entry)
    {
        var instance = Instantiate(textPrefab, content, false);
        instance.textEvent.SetEntry(entry);
        return this;
    }
    
    public PopUps AppendError(string message)
    {
        var instance = Instantiate(errorPrefab, content, false);
        instance.text.text = message;
        return this;
    }

    public void Display()
    {
        canvas.DOFade(1, 0.4f).SetEase(Ease.OutExpo);
        elements.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutExpo);
    }
}
