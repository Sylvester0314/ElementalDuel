using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Enums;
using DG.Tweening;
using Server.GameLogic;
using Shared.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class AnimationWaypoint
{
    public Vector3 position;
    public Vector3 rotation;
}

public class DeckCard : AbstractCard, IPointerClickHandler
{
    public Image switchIcon;
    public GameObject hint;
    public Global global;
    
    [Header("Components")]
    public CostSetComponent costSet;
    public Image cardFace;
    public Image cardFrame;
    public Image cardBack;
    public CardRotation cardRotation;
    public CanvasGroup canvas;
    public Material dissolve;
    
    [Header("In Game Data")]
    public Cardholder place;
    public int timestamp;
    public bool canSwitch;
    public bool isSelecting;

    [Header("Animation Configurations")]
    public AnimationConfiguration selfDeckToBuffer;
    public AnimationConfiguration bufferToSelfDeck;
    public AnimationConfiguration opponentDeckToHand;
    public AnimationConfiguration opponentHandToDeck;
    
    public CostLogic CostLogic;
    public Action CompleteAnimation;
    
    private int _forcedRotationIndex = -1;
    private Tween _animation;
    
    public void Initialize(Cardholder holder, Global g)
    {
        place = holder;
        global = g;
        timestamp = holder.timestamp;
        
        costSet.InitializeCostList("_Outline");
        RefreshStyle();
    }

    public void SetAsset(ActionCardAsset asset, int time)
    {
        place.asset = asset;
        place.timestamp = time;
        timestamp = time;
        RefreshStyle();
    }

    private void RefreshStyle()
    {
        if (place.asset == null)
            return;
        
        cardFace.sprite = place.asset.cardImage;
        CostLogic = new CostLogic(place.asset.costs);
        CostLogic.RefreshCostDisplay(costSet);
    }

    public void ForcedCardRotation(int index, bool value)
    {
        _forcedRotationIndex = index;
        cardRotation.forcedValue = value;
    }

    #region Path Animation
    
    public void MoveFromDeck(Site site, TweenCallback onComplete = null)
    {
        var configuration = site == Site.Self ? selfDeckToBuffer : opponentDeckToHand;
        var basePosition = configuration.path[^1].position;
        var z = 5 - place.sort * 0.001f;
        
        var path = PathParse(configuration, basePosition, z, out var corrected);
        var quaternions = QuaternionParse(configuration.path);

        if (site == Site.Self)
            transform.DOScale(Vector3.one * 1.235f, configuration.duration);
        PathAnimate(
            path, quaternions, configuration.duration * corrected,
            configuration.curve, onComplete
        );
    }

    public void MoveToDeck(Site site, TweenCallback onComplete = null)
    {
        var configuration = site == Site.Self ? bufferToSelfDeck : opponentHandToDeck;
        var basePosition = configuration.path[0].position;
        
        basePosition.z = transform.localPosition.z;
        var path = PathParse(configuration, basePosition, 0, out var corrected);
        var quaternions = QuaternionParse(configuration.path);

        PathAnimate(
            path, quaternions, configuration.duration * corrected,
            configuration.curve, onComplete
        );
        
        if (site != Site.Self)
            return;
        
        var half = configuration.duration / 2;
        DOVirtual.DelayedCall(half, () => transform
            .DOScale(Vector3.one, half).SetEase(Ease.InSine));
    }

    private void PathAnimate(
        Vector3[] path, Quaternion[] quaternions, 
        float duration, AnimationCurve curve,
        TweenCallback onComplete
    )
    {
        if (_forcedRotationIndex != -1)
            cardRotation.forcedSet = true;
        
        var count = path.Length * 1f;

        _animation?.Kill();
        _animation = transform.DOPath(path, duration, PathType.CatmullRom)
            .OnWaypointChange(index =>
            {
                if (index == path.Length)
                    return;
                if (index >= _forcedRotationIndex)
                    cardRotation.forcedSet = false;
                
                var p1 = curve.InverseEvaluateCurve(index / count);
                var p2 = curve.InverseEvaluateCurve((index + 1) / count);
                var deltaTime = (p2 - p1) * duration;
                transform.DORotateQuaternion(quaternions[index + 1], deltaTime);
            })
            .SetEase(curve)
            .OnComplete(onComplete);
    }
    
    private Vector3[] PathParse(
        AnimationConfiguration configuration, Vector3 basePosition, 
        float targetZ, out float corrected
    )
    {
        var baseY = basePosition.y;
        // Calculate the offset ratio between the Y position
        // of the current object and the base Y position
        var radio = (place.transform.localPosition.x + baseY) / baseY;
        var maxRatio = (configuration.maxOffset + baseY) / baseY;
        
        var currentOffsetProp = (radio - 1) / (maxRatio - 1);
        corrected = 1 + currentOffsetProp * configuration.maxCorrected;

        var waypointList = configuration.path;
        var waypointCount = waypointList.Count - 1f;
        return waypointList
            .Select((waypoint, index) =>
            {
                var (x, y, _) = waypoint.position;
                var z = Mathf.Lerp(basePosition.z, targetZ, index / waypointCount);
                var local = new Vector3(x, y * radio, z);
                return transform.parent.TransformPoint(local);
            })
            .ToArray();
    }

    private static Quaternion[] QuaternionParse(List<AnimationWaypoint> waypoints)
    {
        return waypoints
            .Select(waypoint => Quaternion.Euler(waypoint.rotation))
            .Append(Quaternion.Euler(waypoints[^1].rotation))
            .ToArray();
    }
    
    #endregion

    #region Eazy Animation
    
    public async void MoveToBufferCenter(float delay = 0.833f)
    {
        var positionInBuffer = transform.position + Vector3.back * 4;
        if (_animation != null && _animation.IsActive() && !_animation.IsComplete())
            await _animation.AsyncWaitForCompletion();
        
        _animation = transform
            .DOMove(positionInBuffer, 0.2f)
            .OnComplete(() => DOVirtual.DelayedCall(delay, () =>
            {
                CompleteAnimation?.Invoke();
                CompleteAnimation = null;
            }));
    }

    public async void MoveToSelfHand(Transform hand, bool isExtend, Vector3 target, TweenCallback onComplete = null)
    {
        var scale = Vector3.one * (isExtend ? 1.075f : 1);
        var offset = Vector3.up * (isExtend ? 8.3f : 7.95f);
        var position = hand.InverseTransformPoint(target) + offset;

        cardRotation.ForceFront();
        transform.DOScale(scale, 0.4f);
        
        if (_animation != null && _animation.IsActive() && !_animation.IsComplete())
            await _animation.AsyncWaitForCompletion();
        
        _animation = transform
            .DOMove(hand.TransformPoint(position), 0.4f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => DOVirtual.DelayedCall(0.05f, () =>
            {
                transform
                    .DOMove(target, 0.3f)
                    .OnComplete(onComplete);
            }));
    }

    public async void RotateToBack()
    {
        if (_animation != null && _animation.IsActive() && !_animation.IsComplete())
            await _animation.AsyncWaitForCompletion();
        
        _animation = transform
            .DOLocalRotate(new Vector3(0, -180, 0), 0.3f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => DOVirtual.DelayedCall(
                0.1f, () =>
                {
                    CompleteAnimation?.Invoke();
                    CompleteAnimation = null;
                })
            );
    }

    public async void Translation(float duration, Ease ease = Ease.OutSine)
    {
        var position = place.transform.localPosition;
        position.z = place.sort * 0.001f - 4;

        var parent = global.buffer.transform;
        var target = parent.TransformPoint(position);
        
        cardRotation.ForceFront();
        
        if (_animation != null && _animation.IsActive() && !_animation.IsComplete())
            await _animation.AsyncWaitForCompletion();
        
        _animation = transform
            .DOMove(target, duration)
            .SetEase(ease)
            .OnComplete(() => cardRotation.forcedSet = false);
    }

    #endregion

    public void Dissolve()
    {
        costSet.costs.gameObject.SetActive(false);
        cardFrame.material = dissolve;
        cardFace.material = dissolve;
        
        dissolve
            .DOFloat(0, "_DissolveAmount", 0.25f)
            .OnComplete(() => Destroy(gameObject));
    }
    
    public new void CloseSelectIcon()
    {
        base.CloseSelectIcon();
        switchIcon.gameObject.SetActive(false);
        hint.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canSwitch)
            return;
        
        if (isSelecting)
        {
            hint.SetActive(false);
            switchIcon.gameObject.SetActive(false);
            RotateTargetAnimation();
        }
        else
        {
            CloseSelectIcon();
            hint.SetActive(true);
            switchIcon.color = new Color(1, 1, 1, 0);
            switchIcon.transform.localEulerAngles = Vector3.back * 16f;
            switchIcon.gameObject.SetActive(true);
            const float duration = 0.28f;
            var sequence = DOTween.Sequence();
            var rotate = switchIcon.transform
                .DORotate(Vector3.forward * 16f, duration, RotateMode.LocalAxisAdd);
            sequence.Append(switchIcon.DOFade(1, duration));
            sequence.Join(rotate);
            sequence.Play();
        }
        
        global.SetSelectingCard(this);
        isSelecting = !isSelecting;
    }
}
