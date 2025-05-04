using Client.Logic.Request;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChooseActiveButton : AbstractSkillButton
{
    public override void RequestUse()
    {
        var uniqueId = Global.previewingMainTarget.uniqueId;
        var isStarting = Global.startingPhase;
        
        if (isStarting)
        {
            Global.combatAction.SetStatus(false);
            Global.combatAction.ForcedTransferStatus(CombatTransfer.Transparent);
            Global.startingPhase = false;
        }
        
        var request = new ChooseActiveRequest(uniqueId, isStarting);
        var wrapper = ActionRequestWrapper.Create(request);

        Global.combatAction.choosing = false;
        Global.manager.RequestServerRpc(wrapper);

        ResetGameLayout();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (Parent.global.prompt.dialog.isShowing)
            return;
        
        body.DOScale(Vector3.one * 1.1f, AnimateDuration);
    }
    
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (Parent.global.prompt.dialog.isShowing)
            return;
        
        SwitchToInitialState();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        body.DOScale(Vector3.one, AnimateDuration).SetEase(Ease.OutBack);
        RequestUse();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        body.DOScale(Vector3.one * 0.95f, AnimateDuration);
    }

    public override void SwitchToInitialState()
    {
        body.DOScale(Vector3.one, AnimateDuration);
    }
} 