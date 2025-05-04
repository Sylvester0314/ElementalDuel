using System.Collections.Generic;
using Shared.Enums;
using DG.Tweening;
using UnityEngine;

public class SkillButtonList : MonoBehaviour
{
    public Global global;
    public CanvasGroup canvasGroup;
    public RectTransform rect;
    public UseSkillButton skillPrefab;

    [Header("In Game Data")]
    public string key;
    public UseSkillButton prevClickButton;
    public List<AbstractSkillButton> buttons;
    
    public static bool IsAnimating = false;
    public static Tween Delay;
    public const float AnimationDuration = 0.3f;
    
    public void SetInformation(CharacterCard card, Global g)
    {
        global = g;
        key = card.uniqueId;
        buttons = new List<AbstractSkillButton>();
        
        foreach (var skill in card.Skills)
        {
            var instance = Instantiate(skillPrefab);
            instance.SetSkillData(skill);
            instance.SetParent(this);
            
            buttons.Add(instance);
        }
    }

    public SkillButtonList SetInformation(string info)
    {
        key = info;
        return this;
    }

    public void Reset()
    {
        if (key == "choose_active")
            return;
        
        foreach (var button in buttons)
        {
            button.CancelClickStatus();
            button.SwitchToInitialState();
        }
    }

    public bool FadeIn()
    {
        Reset();    
        
        gameObject.SetActive(true);
        canvasGroup.DOFade(1, AnimationDuration);
        transform.DOLocalMove(Vector3.zero, AnimationDuration);
        
        return true;
    }

    public bool FadeOut()
    {
        foreach (var button in buttons)
            button.HideHint();
        
        canvasGroup.DOFade(0, AnimationDuration);
        transform.DOLocalMove(Vector3.down * 3f, AnimationDuration);
        var delay = DOVirtual.DelayedCall(
            AnimationDuration, 
            () => gameObject.SetActive(false)
        );
        var charKey = global.GetZone<CharacterZone>(Site.Self).Active?.uniqueId;
        
        if (key == charKey)
            Delay = delay;
        return true;
    }

    public AbstractSkillButton GetSubButton(int index)
    {
        return transform.GetChild(index).GetComponent<AbstractSkillButton>();
    }
}
