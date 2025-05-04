using System;
using UnityEngine;
using UnityEngine.Localization.Components;

public class RuleItem : AbstractInformationComponent
{
    public GameObject divider;
    public LocalizeStringEvent title;
    public LocalizeStringEvent description;

    public override void SetInformation<T>(T data)
    {
        if (data is not ValueTuple<string, bool> tuple)
            return;

        var (ruleName, isFirst) = tuple;
        divider.SetActive(!isFirst);
        SetRuleInfo(ruleName);
    }

    public void SetRuleInfo(string ruleName)
    {
        title.SetEntry(ruleName + "_name");
        description.SetEntry(ruleName + "_description");
        ForceRebuildLayoutImmediate();
    }
}