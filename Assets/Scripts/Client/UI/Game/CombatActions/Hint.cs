using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class Hint : MonoBehaviour
{
    public Transform parent;
    public CanvasGroup hint;
    public LocalizeStringEvent hintText;
    public RectTransform content;
    public LayoutElement layout;
    
    private Tween _tween;

    public bool Displaying => _tween != null && _tween.IsActive() && !_tween.IsComplete();

    public async void Display(RectTransform skillListRect, string entry, bool autoClose = true)
    {
        if (Displaying)
            return;
        
        gameObject.SetActive(true);
        hintText.SetEntry(entry);
        LayoutRebuilder.ForceRebuildLayoutImmediate(hintText.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        await Task.Yield();
        
        var w1 = skillListRect.sizeDelta.x;
        var x1 = w1 + parent.transform.localPosition.x;

        var x2 = w1 - x1 - hint.transform.localPosition.x;
        var w2 = content.sizeDelta.x;
        var w3 = (w2 + 5) / 2;

        hint.alpha = 1;
        if (autoClose) 
            _tween = DOVirtual.DelayedCall(2, () => Hide(false));
        if (w3 <= x2)
            return;
        
        var position = content.localPosition;
        position.x = x2 - w3 + 3.445f;
        content.localPosition = position;
    }

    public async void Display(string entry, float maxWeight = -1, bool autoClose = true)
    {
        if (Displaying)
            return;
        
        layout.preferredWidth = -1;
        gameObject.SetActive(true);
        hintText.SetEntry(entry);

        var rect = hintText.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        await Task.Yield();
        
        layout.preferredWidth = Mathf.Min(maxWeight, rect.sizeDelta.x);
        
        hint.alpha = 0;
        hint.DOFade(1, 0.3f).SetEase(Ease.OutCirc);
        
        if (autoClose) 
            _tween = DOVirtual.DelayedCall(2, () => Hide(true));
    }

    public void Hide(bool fade)
    {
        if (!fade)
        {
            gameObject.SetActive(false);
            hint.alpha = 0;
            return;
        }
        
        hint.DOFade(0, 0.3f)
            .SetEase(Ease.OutCirc)
            .OnComplete(() => gameObject.SetActive(false));
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        hint.alpha = 0;
        _tween?.Kill();
    }
}