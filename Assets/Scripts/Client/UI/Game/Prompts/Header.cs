using System;
using UnityEngine;
using UnityEngine.Localization.Components;

public class Header : AbstractPromptComponent
{
    [Header("Self Components")]
    public LocalizeStringEvent mainEvent;
    public LocalizeStringEvent subEvent;

    public override void Display<T>(T data, Action onComplete = null)
    {
        isShowing = true;
        gameObject.SetActive(true);
        prompt?.DarkBackgroundDisplay();
        
        if (data is string single)
        {
            Show(single, mainEvent);
            subEvent.gameObject.SetActive(false);
            return;
        }
        
        if (data is not ValueTuple<string, string> multi)
            return;

        Show(multi.Item1, mainEvent);
        Show(multi.Item2, subEvent); 
    }

    private void Show(string entry, LocalizeStringEvent e)
    {
        e.SetEntry(entry);
        e.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        isShowing = false;
        Reset();
    }

    public override void Reset()
    {
        gameObject.SetActive(false);
        prompt?.DarkBackgroundHide();
        mainEvent.gameObject.SetActive(false);
        subEvent.gameObject.SetActive(false);
    }
}