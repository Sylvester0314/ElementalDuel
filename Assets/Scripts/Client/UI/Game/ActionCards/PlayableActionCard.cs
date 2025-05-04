using System;
using Client.Logic.Request;
using Client.Logic.Response;
using Shared.Enums;
using DG.Tweening;
using Server.GameLogic;
using Shared.Classes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayableActionCard : AbstractCard, 
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IComparable<PlayableActionCard>
{
    public ActionCardAsset asset;

    [Header("Components")]
    public Image flash;
    public Image cardFace;
    public Image cardFrame;
    public CanvasGroup canvasGroup;
    public RectTransform body;
    public CardDrag drag;
    public TextMeshProUGUI countText;
    public CanvasGroup countIcon;
    public CostSetComponent costSet;

    [Header("Material References")]
    public Material meshMask;

    [Header("In Game Data")]
    public int timestamp;
    public bool isHovering;
    public bool isUsable;
    public ResourceMatchedResult matched; 
    public CostLogic SynchronousCost;
    
    private HandCards _hand;
    private bool _isInteractable;
    private bool _switchStatus;
    private bool _canTuning;
    private bool _endDragHideHandCondition;
    private float _clickStartTime;
    private PointerEventData _pointerEventData;
    private ExtraBackground _tuning;
    private Vector3 _backPosition;
    
    private Tween _tween;
    private const float HoveringAnimationDuration = 0.4f;
    public const float ExtendCardDuration = 0.4f;

    private Global Global => _hand.global;
    
    public void Update()
    {
        if (Time.time - _clickStartTime >= 0.8f && _pointerEventData != null && !drag.IsDragging)
            ExecuteEvents.Execute(gameObject, _pointerEventData, ExecuteEvents.beginDragHandler);
    }

    public void NetworkSynchronous(CostMatchResult result)
    {
        isUsable = result.Usable;
        matched = result.MatchedResult;
        SynchronousCost = result.Cost;
        
        SynchronousCost.RefreshCostDisplay(costSet);
    }
    
    public void Initialize(ActionCardAsset cardAsset, HandCards hand)
    {
        _hand = hand;
        costSet = transform.GetComponent<CostSetComponent>();
        costSet.InitializeCostList("_Outline");

        asset = cardAsset;
        cardFace.sprite = asset.cardImage;
        SynchronousCost = new CostLogic(asset.costs);
        SynchronousCost.RefreshCostDisplay(costSet);
        
        drag.canvas = _hand.rootCanvas;
        drag.diceDisplayer = Global.diceFunction.displayer;
        drag.hand = _hand;
        
        drag.OnBeginDragCallback += OnBeginDrag;
        drag.OnEndDragCallback += OnEndDrag;

        drag.OnDragInsideDiceAreaCallback += ShowMask;
        drag.OnDragOutsideDiceAreaCallback += HideMask;
        drag.OnDiceAreaEndDragCallback += TuningCard;

        drag.OnDragInsideHandAreaCallback += BackToHandArea;
        drag.OnDragOutsideHandAreaCallback += LeaveFromHandArea;
        drag.OnHandAreaEndDragCallback += ReturnToHand;
        
        drag.OnOutsideEndDragCallback += RequestPlayPreview;
        drag.OnDragBetweenInHandAreaCallback += () => _hand.HoveringOtherCardAnimation(transform);
        drag.OnDragBetweenOutHandAreaCallback += () => _hand.ResetCanvasBodyPosition(this);
    }

    private void ReturnToHand()
    {
        _endDragHideHandCondition = true;
    }
    
    private void TuningCard()
    {
        var element = Global.GetZone<CharacterZone>(Site.Self).Active.Element;
        
        var priorDice = Global.diceFunction.PrioriElementalTuning(element);
        var prior = priorDice != null;
        _endDragHideHandCondition = _canTuning && prior;
        
        if (_canTuning && prior)
        {
            _hand.usingCard = true;

            var request = new TuningRequest(TuningAction.Start, priorDice.Logic, timestamp);
            var wrapper = ActionRequestWrapper.Create(request);
            Global.manager.RequestServerRpc(wrapper);
        }
        else
        {
            var entry = prior
                ? "cannot_used_for_tuning"
                : "no_dice_can_be_converted";
            drag.ResetPosition();
            _hand.ExtendAreaLayout();
            Global.prompt.dialog.Display(entry);
        }
    }

    private void BackToHandArea()
    {
        _hand.ExtendAreaLayout(this);
        _tuning.HideIcon();
    }

    private void LeaveFromHandArea()
    {
        _tuning.ShowIcon();
        _hand.ContractAreaLayout(expected: this);
    }

    private void RequestPlayPreview()
    {
        _endDragHideHandCondition = isUsable;

        if (isUsable)
        {
            var request = PlayCardRequest.Preview(timestamp);
            var wrapper = ActionRequestWrapper.Create(request);
            Global.manager.RequestServerRpc(wrapper);
            return;
        }
        
        drag.ResetPosition();
        _hand.ExtendAreaLayout();

        var entry = matched.Success 
            ? "prerequisite_not_met" 
            : "not_enough_dice";
        Global.prompt.dialog.Display(entry);
    }

    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
    
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;
    }

    public void EaseAnimation(Vector3 position, float duration = HoveringAnimationDuration)
    {
        body.DOLocalMove(position, duration).SetEase(Ease.OutExpo);
    }

    private void ShowMask()
    {
        costSet.costs.gameObject.SetActive(false);
        cardFace.material = meshMask;
        cardFrame.material = meshMask;
        _tuning.Open();
    }

    private void HideMask()
    {
        costSet.costs.gameObject.SetActive(true);
        cardFace.material = null;
        cardFrame.material = null;
        _tuning.Close();
    }
    
    private void HoveringCardAnimation()
    {
        if (isHovering)
            return;
        
        EaseAnimation(new Vector3(0, 0.925f, -0.01f));
        _hand.HoveringOtherCardAnimation(transform);
        isHovering = true;
    }

    private void CancelHoveringCardAnimation()
    {
        if (!isHovering)
            return;
        EaseAnimation(Vector3.zero);
        isHovering = false;
    }

    private void ShowSelectIcon()
    {
        Global.SetSelectingCard(this);
        RotateTargetAnimation();
    }

    public void ShowCountIcon()
    {
        if (_tween != null && _tween.IsActive() && !_tween.IsComplete())
            return;
        
        countText.text = _hand.cards.Count.ToString();
        countIcon.gameObject.SetActive(true);

        _tween = DOTween.Sequence()
            .Append(countIcon.DOFade(1, ExtendCardDuration))
            .Play()
            .OnComplete(() => _tween = DOVirtual.DelayedCall(
                ExtendCardDuration * 2,
                HideCountIcon
            ));
    }

    public void HideCountIcon()
    {
        _tween?.Kill();
        countIcon
            .DOFade(0, ExtendCardDuration * 0.6f)
            .OnComplete(() => countIcon.gameObject.SetActive(false));
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Global.prompt.dialog.Intercept())
            return;
        if (drag.IsDragging)
            return;

        if (!_isInteractable)
        {
            _hand.ExtendArea();
            OnPointerEnter(eventData);
            return;            
        }

        ShowSelectIcon();
        _clickStartTime = Time.time;
        _pointerEventData = eventData;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (drag.IsDragging)
            ExecuteEvents.Execute(gameObject, _pointerEventData, ExecuteEvents.endDragHandler);

        _pointerEventData = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;
        
        HoveringCardAnimation();
        _hand.hasCardHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;
        
        CancelHoveringCardAnimation();
        _hand.hasCardHovering = false;
        DOVirtual.DelayedCall(0.05f, () =>
        {
            if (!_hand.hasCardHovering)
                _hand.ResetCanvasBodyPosition();
        });
    }

    private bool IsInteractable()
    {
        return _isInteractable && 
               !_hand.HasCardDragging && 
               !Global.prompt.dialog.isShowing;
    }
    
    private void OnBeginDrag()
    {
        countIcon.gameObject.SetActive(false);
        HideCountIcon();
        CloseSelectIcon();
        Global.information.CloseAll();
        _hand.hasCardHovering = false;
        isHovering = false;
        
        body.localPosition = Vector3.back;
        
        var displayer = Global.diceFunction.displayer;
        _canTuning = !asset.Properties.Contains(Property.TuningDisable);
        _tuning = _canTuning 
            ? displayer.tuningBackground 
            : displayer.tuningDisableBackground;
    }

    private void OnEndDrag()
    {
        Global.selectingCard = null;
        
        if (drag.isInHandArea)
        {
            drag.ResetPosition();
            DOVirtual.DelayedCall(0.02f, () =>
            {
                if (!_hand.hasCardHovering)
                    HoveringCardAnimation();
            });
        }
        else if (_endDragHideHandCondition)
        {
            _hand.gameObject.SetActive(false);
            _hand.ContractAreaLayout(true);
            _hand.ResetCanvasBodyPosition();
        }
        HideMask();
        _tuning.HideIcon();
        _tuning.Close();
    }

    public int CompareTo(PlayableActionCard other)
    {
        var result = other.asset.CompareTo(asset);
        if (result == 0)
            result = timestamp.CompareTo(other.timestamp);
        return result;
    }
}