using Shared.Handler;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class InputFieldItem : MonoBehaviour, IDataInjectorHandler
{
    public TMP_InputField input;
    public LocalizeStringEvent placeholder;
    public InputFieldHide hide;

    public void Initialize(string placeholderEntry, string initValue, bool isHide)
    {
        placeholder.SetEntry(placeholderEntry);
        input.text = initValue;
        hide.enabled = isHide;
    }

    public string GetData()
    {
        return input.text;
    }
}