using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractDeckCard : MonoBehaviour, 
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler 
{
    public bool isInteractable = true;
    
    private float _lastLeftClickTime;
    private Tween _task;

    private const float DoubleClick = 0.2f;

    protected abstract void OnLeftClick();
    protected abstract void OnRightClick();
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable)
            return;
        
        // Right click or Double left click check
        if (eventData.button == PointerEventData.InputButton.Right ||
            Time.time - _lastLeftClickTime < DoubleClick
           )
        {
            _task?.Kill();
            OnRightClick();
            return;
        }

        _lastLeftClickTime = Time.time;
        _task = DOVirtual.DelayedCall(DoubleClick, OnLeftClick);
    }
    
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (isInteractable)
            transform.DOScale(Vector3.one * 0.96f, 0.15f).SetEase(Ease.OutExpo);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (isInteractable)
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutExpo);
    }
}