using System;
using UnityEngine;

public class Signal : AbstractPromptComponent
{
    [Header("Self Components")]
    public WaitingText waitingText;

    public override void Display<T>(T data, Action onComplete = null)
    {
        if (data is not string entry)
            return;

        gameObject.SetActive(true);
        isShowing = true;
        waitingText.Active(entry);
    }

    public void Display(string content, bool showEllipsis)
    {
        gameObject.SetActive(true);
        isShowing = true;
        waitingText.Display(content, showEllipsis);
    }

    public override void Hide()
    {
        isShowing = false;
        Reset();        
    }

    public override void Reset()
    {
        gameObject.SetActive(false);
        waitingText.Inactive();
    }
}