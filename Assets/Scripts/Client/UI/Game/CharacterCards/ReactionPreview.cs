using System.Collections.Generic;
using DG.Tweening;
using Shared.Enums;
using Shared.Misc;
using UnityEngine;

public class ReactionPreview : MonoBehaviour
{
    public CanvasGroup canvas;
    public AnimationCurve curve;
    
    [Header("References")]
    public ReactionPreviewItem reactionPrefab;

    private Tween _tween;
    
    public void Close()
    {
        StaticMisc.DestroyAllChildren(transform);
        gameObject.SetActive(false);
    }
    
    public void Open(List<ElementalApplication> applications)
    {
        StaticMisc.DestroyAllChildren(transform);
        gameObject.SetActive(true);
        
        var reactions = applications.Paginate(2);
        foreach (var reaction in reactions)
            Instantiate(reactionPrefab, transform, false)
                .SetReaction(reaction);

        canvas.alpha = 0.45f; // applied ? 0.45f : 1;

        _tween?.Kill();
        _tween = canvas.DOFade(1, 1.5f).SetEase(curve)
            .SetLoops(-1, LoopType.Restart);
    }
}