using System;
using System.Collections.Generic;
using DG.Tweening;
using Shared.Enums;
using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthModifyFeedback : MonoBehaviour
{
    public Image background;
    public TextMeshProUGUI valueText;
    
    [Header("References")]
    public Sprite damageBackground;
    public Sprite healBackground;

    [Header("Configurations")] 
    public AnimationCurve damageCurve;
    public List<Color> elementColors;

    public Action OnStartClose;
    public Action OnStart;
    public Action OnComplete;

    private void Reset()
    {
        transform.localScale = Vector3.one;
        background.transform.localScale = Vector3.one * 0.3f;
        valueText.transform.localScale = Vector3.one;
        gameObject.SetActive(true);   
    }
    
    public void Display(int value, Element element)
    {
        Reset();
        
        var isDamage = element != Element.None;
        background.sprite = isDamage ? damageBackground : healBackground;

        valueText.text = HealthPreview.HealthModifyString(value, isDamage);
        valueText.color = elementColors[(int)element];
        
        StaticMisc.InvokeThenClear(ref OnStart);

        if (isDamage)
        {
            valueText.transform
                .DOScale(Vector3.one * 2.3f, 0.5f)
                .SetEase(damageCurve);
            background.transform
                .DOScale(Vector3.one, 0.35f)
                .OnComplete(() => DelayClose(1));
        }
        else
        {
            valueText.transform.localScale = Vector3.zero;
            DOTween.Sequence()
                .Append(background.transform.DOScale(Vector3.one, 0.4f))
                .Insert(0.1f, valueText.transform.DOScale(Vector3.one * 0.75f, 0.4f))
                .OnComplete(() => DelayClose(0.5f));
        }
    }

    private void DelayClose(float delay) => DOVirtual.DelayedCall(delay, Close);

    private void Close()
    {
        StaticMisc.InvokeThenClear(ref OnStartClose);

        transform
            .DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InExpo)
            .OnComplete(() =>
            {
                StaticMisc.InvokeThenClear(ref OnComplete);
                gameObject.SetActive(false);
            });
    }
}