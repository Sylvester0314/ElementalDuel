using UnityEngine.EventSystems;

public class OpponentCountdown : AbstractTurnCountdown
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        parent.global.prompt.dialog.Display("warning_hint_turn");
    }
}