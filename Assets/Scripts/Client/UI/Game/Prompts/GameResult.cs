using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class GameResult : AbstractPromptComponent
{
    public Image background;
    public Image leftDecoration;
    public Image rightDecoration;
    public LocalizeStringEvent textEvent;
    public ConfirmButton button;
    public CanvasGroup buttonCanvas;
    public Transform moveableComponents;
    
    [Header("Configurations")]
    public List<Color> initialColors;
    public List<Color> backgroundColors;
    public List<Sprite> leftDecorations;
    public List<Sprite> rightDecorations;

    public void Test(bool isWinner)
    {
        Display(isWinner);
    }
    
    public override void Reset() { }
    
    public override void Display<T>(T data, Action onComplete = null)
    {
        if (data is not bool isWinner)
            return;
        
        var index = isWinner ? 1 : 0;
        
        background.color = initialColors[index];
        leftDecoration.sprite = leftDecorations[index];
        rightDecoration.sprite = rightDecorations[index];
        textEvent.SetEntry(isWinner ? "game_win" : "game_lose");
        textEvent.transform.localScale = Vector3.one * 1.25f;
            
        var global = prompt.global;
        
        global.diceFunction.gameObject.SetActive(false);
        global.combatAction.ForcedTransferStatus(CombatTransfer.Transparent);
        global.indicator.Close(true);
        button.Callback = global.BackToRoom;
        
        gameObject.SetActive(true);

        DOTween.Sequence()
            .Append(background.DOColor(backgroundColors[index], 0.3f).SetEase(Ease.OutCubic))
            .Join(textEvent.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutSine))
            .Join(moveableComponents.DOLocalMoveY(2, 0.3f))
            .Insert(0.75f, buttonCanvas.DOFade(1, 0.4f).SetEase(Ease.OutCubic))
            .Join(moveableComponents.DOLocalMoveY(5.4f, 0.4f))
            .Play();
    }
    
    public override void Hide() { }
}