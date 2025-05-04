using Shared.Classes;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractSkillButton : MonoBehaviour,
    IPointerExitHandler, IPointerEnterHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public Transform body;

    protected SkillButtonList Parent;
    protected const float AnimateDuration = 0.15f;

    public virtual string Key { set; get; }
    public RectTransform ParentRect => Parent.rect;
    protected Global Global => Parent.global;
    
    public void SetParent(SkillButtonList parent)
    {
        Parent = parent;
        transform.SetParent(parent.transform, false);
    }

    public abstract void SwitchToInitialState();

    public abstract void OnPointerEnter(PointerEventData eventData);

    public abstract void OnPointerExit(PointerEventData eventData);
    
    public abstract void OnPointerDown(PointerEventData eventData);
    
    public abstract void OnPointerUp(PointerEventData eventData);
    
    public virtual void CancelClickStatus() { }

    public virtual void NetworkSynchronous(CostMatchResult result) { }
    
    public virtual void HideHint() { }

    public abstract void RequestUse();
    
    public void ResetGameLayout()
    {
        SwitchToInitialState();

        // Reset data and UI Layout
        Global.diceFunction.ResetLayout();
        Global.prompt.CloseAll();
        Global.CancelPreview();
        Global.hand.ResetLayout();
    }
}