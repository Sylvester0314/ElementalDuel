using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalClickEvent : MonoBehaviour
{
    public Global global;
    
    private float _lastClickTime;
    
    public void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        ResetUI();
    }

    private void ResetUI()
    {
        if (global.prompt.dialog.Intercept() || Time.time - _lastClickTime < 0.2f)
            return;

        _lastClickTime = Time.time;
        
        global.SetTurnInformationStatus(true);
        global.hand.ResetLayout();
        global.combatAction.TransferStatus(CombatTransfer.Active);
        global.indicator.selfCountdown.HideDeclareButton();
        global.CancelPreview();
        // global.hand.ResetLayout();
        global.SetSelectingCard(null);
        global.prompt.CloseAll();
        global.diceFunction.ResetLayout();
        global.combatAction.ResetLayout();
    }
}