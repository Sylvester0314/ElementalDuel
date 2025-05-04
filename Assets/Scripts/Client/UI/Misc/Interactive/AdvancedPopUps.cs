using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Shared.Handler;
using UnityEngine;

public class AdvancedPopUps : PopUps
{
    public InputFieldItem inputPrefab;

    [Header("Advanced Components")] 
    public MiddleButton cancelButton;

    private readonly List<IDataInjectorHandler> _handlers = new ();

    public AdvancedPopUps SetEntry(
        string confirmEntry = "pop_confirm",
        string cancelEntry = "pop_cancel"
    )
    {
        confirmButton.textEvent.SetEntry(confirmEntry);
        cancelButton.textEvent.SetEntry(cancelEntry);
        
        return this;
    }
    
    public AdvancedPopUps SetCallbacks(
        Action<List<string>> onConfirm,
        Action onCancel = null
    )
    {
        confirmButton.Callback += () => onConfirm?.Invoke(GetInputFieldsResult());
        cancelButton.Callback += onCancel;

        return this;
    }
    
    public AdvancedPopUps SetAsyncCallbacks(
        Func<List<string>, Task> onConfirm,
        Func<Task> onCancel = null
    )
    {
        confirmButton.AsyncCallback += 
            async () => await onConfirm.Invoke(GetInputFieldsResult());
        cancelButton.AsyncCallback += onCancel;

        return this;
    }

    public AdvancedPopUps AppendInputField(
        string placeholderEntry,
        string initValue = "",
        bool isHide = false)
    {
        var instance = Instantiate(inputPrefab, content, false);
        instance.Initialize(placeholderEntry, initValue, isHide);
        
        _handlers.Add(instance);

        return this;
    }

    private List<string> GetInputFieldsResult()
    {
        return _handlers.Select(handler => handler.GetData()).ToList();
    }
}