using System;
using UnityEngine;

public abstract class AbstractPromptComponent : MonoBehaviour
{
    public PromptUI prompt;
    public bool isShowing;

    public abstract void Reset();

    public abstract void Display<T>(T data, Action onComplete = null);

    public abstract void Hide();
}