using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class Rounds : AbstractPromptComponent
{
    public CanvasGroup canvas;
    public Transform wrapper;
    public Image icon;
    public Image dark;
    public Image background;
    public RectTransform left;
    public RectTransform right;
    public List<Image> lines;

    public LocalizeStringEvent main;
    public TextMeshProUGUI round;

    [Header("Self Style Configurations")] 
    public Sprite selfIcon;
    public Color selfRoundColor;
    public Color selfLineColor;
    public Color selfBackgroundColor;

    [Header("Opponent Style Configurations")]
    public Sprite oppoIcon;
    public Color oppoRoundColor;
    public Color oppoLineColor;
    public Color oppoBackgroundColor;
    
    private readonly Vector2 _min = new (15, 1.2f);
    private readonly Vector2 _max = new (30, 1.2f);
    
    public override void Display<T>(T data, Action onComplete = null)
    {
        if (data is not ValueTuple<string, bool> tuple)
            return;

        var (roundNumber, isSelf) = tuple;
        var pattern = ResourceLoader.GetLocalizedUIText("round_number");
        var entry = isSelf ? "hint_you_start_first" : "hint_oppo_start_first";
        
        main.SetEntry(entry);
        icon.sprite = isSelf ? selfIcon : oppoIcon;
        round.color = isSelf ? selfRoundColor : oppoRoundColor; 
        round.text = pattern.Replace("$[Number]", roundNumber);
        background.color = isSelf ? selfBackgroundColor : oppoBackgroundColor;
        foreach (var image in lines)
            image.color = isSelf ? selfLineColor : oppoLineColor;
        
        Reset();
        prompt?.global?.information.CloseAll();
        gameObject.SetActive(true);

        DOTween.Sequence()
            .Append(dark.DOFade(0.65f, 0.4f).SetEase(Ease.OutCubic))
            .Join(canvas.DOFade(1, 0.4f).SetEase(Ease.OutCubic))
            .Join(wrapper.DOScale(1, 0.4f).SetEase(Ease.OutCubic))
            .Insert(0.35f, right.DOSizeDelta(_max, 0.15f).SetEase(Ease.OutSine))
            .Insert(0.35f, left.DOSizeDelta(_max, 0.15f).SetEase(Ease.OutSine))
            .Insert(0.95f, wrapper.DOScale(1.15f, 0.375f).SetEase(Ease.OutSine))
            .Insert(0.95f, canvas.DOFade(0, 0.375f).SetEase(Ease.OutSine))
            .Insert(0.95f, dark.DOFade(0, 0.4f).SetEase(Ease.OutCubic))
            .Play()
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Hide();
            });
    }

    public override void Hide()
    {
        canvas.alpha = 0;
        gameObject.SetActive(false);
    }

    public override void Reset()
    {
        canvas.alpha = 0;
        left.sizeDelta = _min;
        right.sizeDelta = _min;
        wrapper.localScale = Vector3.one * 0.875f;
        dark.color = new Color32(0, 0, 0, 0);
    }
}