using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractCard : MonoBehaviour
{
    public Image targetIcon;

    private TweenerCore<Quaternion, Vector3, QuaternionOptions> _loop;
    
    protected void RotateTargetAnimation()
    {
        targetIcon.gameObject.SetActive(true);
        _loop = targetIcon.transform
            .DORotate(new Vector3(0, 0, -360), 16f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    public void CloseSelectIcon()
    {
        _loop?.Kill();
        if (targetIcon != null)
            targetIcon.gameObject.SetActive(false);
    }
}