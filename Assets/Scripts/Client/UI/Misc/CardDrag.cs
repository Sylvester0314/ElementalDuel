using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool IsDragging { get; private set; }
    
    [HideInInspector]
    public Canvas canvas;
    [HideInInspector]
    public DiceDisplayer diceDisplayer;
    [HideInInspector]
    public HandCards hand;

    public delegate void DragEvent();
    
    public DragEvent OnBeginDragCallback;
    public DragEvent OnEndDragCallback;
    public DragEvent OnDiceAreaEndDragCallback;
    public DragEvent OnHandAreaEndDragCallback;
    public DragEvent OnOutsideEndDragCallback;

    public DragEvent OnDragInsideDiceAreaCallback;
    public DragEvent OnDragOutsideDiceAreaCallback;
    public DragEvent OnDragInsideHandAreaCallback;
    public DragEvent OnDragOutsideHandAreaCallback;
    
    public DragEvent OnDragBetweenInHandAreaCallback;
    public DragEvent OnDragBetweenOutHandAreaCallback;

    public bool isInHandArea;
    public bool isInDiceArea;
    private Vector3 _offset;
    private Vector3 _originalPosition;
    private RectTransform _rect;

    public void Start()
    {
        _rect = GetComponent<RectTransform>();
    }

    private bool CurrentCanDrag()
    {
        return !hand.CanDrag || hand.global.prompt.dialog.isShowing;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentCanDrag())
            return;

        IsDragging = true;
        _originalPosition = transform.localPosition;
        transform.DOScale(1.36f, 0.05f);
        OnBeginDragCallback?.Invoke();
        SetDraggingPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CurrentCanDrag())
            return;
        
        SetDraggingPosition(eventData);
        
        var point = RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera,
            transform.position
        );
        var nowInsideDice = RectTransformUtility.RectangleContainsScreenPoint(
            diceDisplayer.checker, point,
            diceDisplayer.rootCanvas.worldCamera
        );
        var nowInsideHand = RectTransformUtility.RectangleContainsScreenPoint(
            hand.checkbox, point, hand.rootCanvas.worldCamera
        );

        if (nowInsideDice && !isInDiceArea)
        {
            OnDragInsideDiceAreaCallback?.Invoke();
            isInDiceArea = true;
        }
        else if (!nowInsideDice && isInDiceArea)
        {
            OnDragOutsideDiceAreaCallback?.Invoke();
            isInDiceArea = false;
        }
        
        if (nowInsideHand && !isInHandArea)
        {
            OnDragInsideHandAreaCallback?.Invoke();
            isInHandArea = true;
        }
        else if (!nowInsideHand && isInHandArea)
        {
            OnDragOutsideHandAreaCallback?.Invoke();
            isInHandArea = false;
        }

        if (isInHandArea)
        {
            if (transform.localPosition.x is <= -14 or >= 16)
                OnDragBetweenOutHandAreaCallback?.Invoke();
            else
                OnDragBetweenInHandAreaCallback?.Invoke();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CurrentCanDrag())
            return;
        
        if (isInDiceArea)
            OnDiceAreaEndDragCallback?.Invoke();
        else if (isInHandArea)
            OnHandAreaEndDragCallback?.Invoke();
        else
            OnOutsideEndDragCallback?.Invoke();
        
        IsDragging = false;
        OnEndDragCallback?.Invoke();
    }

    public void ResetPosition(float y = 4.25f)
    {
        _originalPosition.y = y;
        transform.DOLocalMove(_originalPosition, 0.1f);
        transform.DOScale(1.075f, 0.05f);
    }

    private void SetDraggingPosition(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _rect, eventData.position, 
                eventData.pressEventCamera, 
                out var mouseWorldPosition
            ))
            return;

        _rect.position = mouseWorldPosition;
    }
}