using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractTurnCountdown : MonoBehaviour, IPointerClickHandler
{
    public TurnIndicator parent;
    public DiceCount diceCount;
    public Transform indicator;
    public AbstractTurnCountdown next;

    public int direction;
    
    public void Switch(bool isPrevShowing)
    {
        diceCount.SetActiveStatus(true);
        next.diceCount.SetActiveStatus(false);

        if (!isPrevShowing)
        {
            SetActive(true);
            return;
        }
        
        SwitchReset();
        
        transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutExpo);
        indicator.DOLocalMoveY(2.42f * direction, 0.3f).SetEase(Ease.OutExpo);
    }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        indicator.gameObject.SetActive(active);
    }

    public virtual void SwitchReset()
    {
        SetActive(true);
        indicator.localPosition = Vector3.up * 1.52f * direction;
        transform.localRotation = Quaternion.Euler(0, 0, 90);
    }

    public abstract void OnPointerClick(PointerEventData eventData);
}