using Sirenix.OdinInspector;
using UnityEngine;

public class Energy : MonoBehaviour
{
    public Animator animator;
    public GameObject active;
    public CanvasGroup lightImage;

    [Header("In Game Data"), ShowInInspector]
    private bool _charged;
    
    public bool Charged
    {
        get => _charged;
        set
        {
            _charged = value;
            
            active.SetActive(value);
            lightImage.alpha = value ? 1 : 0;
            animator.enabled = false;
        }
    }
    

    public void SetActiveDisplay(bool status)
    {
        active.SetActive(status);
        Charged = true;
    }

    public void PlayAnimation(bool sign)
    {
        if (sign)
            GainAnimation();
        else
            ConsumeAnimation();
    }
    
    private void ConsumeAnimation()
    {
        
    }

    private void GainAnimation()
    {
        active.SetActive(true);
        animator.enabled = true;
        animator.Play("Prepare");
    }

    public void CancelPlay()
    {
        active.SetActive(false);
        lightImage.alpha = 0;
        animator.enabled = false;
    }
}